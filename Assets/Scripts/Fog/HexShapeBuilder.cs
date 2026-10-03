using System.Collections.Generic;
using Clipper2Lib;
using UnityEngine;
using UnityX.HexGrid;

// Turns a set of hex cells into a rounded, offset shape (holes included) as a triangle soup in HexCoord.Position space.
// Adapted from Resource-Claiming-Game's TerritoryMeshBuilder, which does the same for square cells.
// Pipeline: trace the edges between cells in and out of the set into closed loops (O(cells)), offset them with one arc-free
// Clipper2 pass (which also merges or splits loops that the offset makes overlap), fillet every corner, then triangulate
// with Clipper2. Hex boundaries turn by 60 degrees at every corner, so the fillet handles any turn angle, not just right angles.
public class HexShapeBuilder {
    // Hex units -> Clipper's integer units.
    const double Scale = 1024;
    const double InvScale = 1.0 / Scale;
    // Corners are snapped to this grid (per hex unit) to match up the corners that neighbouring cells share.
    const float CornerKeyScale = 16;

    [System.Serializable]
    public struct Style {
        [Tooltip("Net outward offset of the shape from its cells, in hex units. Negative insets.")]
        public float extrusion;
        [Tooltip("Rounding applied to both outward and inward corners, in hex units.")]
        public float cornerRadius;
        [Tooltip("Angle covered by each segment of a rounded corner.")]
        public float degreesPerPoint;

        public Style(float extrusion, float cornerRadius, float degreesPerPoint) {
            this.extrusion = extrusion;
            this.cornerRadius = cornerRadius;
            this.degreesPerPoint = degreesPerPoint;
        }
    }

    // Triangle soup (every 3 points is a triangle) from the last Build.
    public readonly List<Vector2> triangles = new List<Vector2>();
    // Boundary loops traced by the last Build, before offsetting: outer loops counter-clockwise, holes clockwise.
    public Paths64 tracedLoops { get; private set; } = new Paths64();

    readonly ClipperOffset offsetter = new ClipperOffset();

    // Edge-tracing scratch.
    readonly Dictionary<long, Point64> cornerPoints = new Dictionary<long, Point64>();
    readonly Dictionary<long, int> outgoingEdge = new Dictionary<long, int>();
    readonly List<long> edgeStart = new List<long>(), edgeEnd = new List<long>();
    readonly List<bool> edgeUsed = new List<bool>();

    // For each direction, the two corner indices of the edge a cell shares with its neighbour in that direction.
    // Found geometrically (the corners nearest the midpoint between the two centres) so it doesn't depend on index conventions.
    static readonly int[,] edgeCorners = FindEdgeCorners();

    static int[,] FindEdgeCorners() {
        var result = new int[6, 2];
        for (int direction = 0; direction < 6; direction++) {
            var midpoint = HexCoord.zero.Neighbor(direction).Position() * 0.5f;
            int best = -1, secondBest = -1;
            float bestDistance = float.MaxValue, secondBestDistance = float.MaxValue;
            for (int corner = 0; corner < 6; corner++) {
                float distance = (HexCoord.zero.Corner(corner) - midpoint).sqrMagnitude;
                if (distance < bestDistance) {
                    secondBest = best; secondBestDistance = bestDistance;
                    best = corner; bestDistance = distance;
                } else if (distance < secondBestDistance) {
                    secondBest = corner; secondBestDistance = distance;
                }
            }
            result[direction, 0] = best;
            result[direction, 1] = secondBest;
        }
        return result;
    }

    // Builds the shape of `cells`. `contains` must be true for exactly the cells in `cells`.
    public void Build(IEnumerable<HexCoord> cells, System.Func<HexCoord, bool> contains, Style style) {
        tracedLoops = TraceLoops(cells, contains);
        var shape = RoundCorners(Offset(tracedLoops, style.extrusion), Mathf.Max(0, style.cornerRadius), style.degreesPerPoint);
        if (!AppendTriangles(shape)) {
            // Fillets are clamped to half their edges so this shouldn't happen, but if two ever cross, let Clipper untangle them.
            shape = Clipper.Union(shape, FillRule.NonZero);
            AppendTriangles(shape);
        }
    }

    // Clears `triangles` and fills it with the triangulation of `paths`. False if the paths cross each other.
    bool AppendTriangles(Paths64 paths) {
        triangles.Clear();
        if (paths.Count == 0) return true;
        var result = Clipper.Triangulate(paths, out var solution, false);
        if (result == TriangulateResult.pathsIntersect) return false;
        if (result != TriangulateResult.success) return true;
        foreach (var tri in solution) {
            if (tri.Count != 3) continue;
            for (int i = 0; i < 3; i++) triangles.Add(new Vector2((float)(tri[i].X * InvScale), (float)(tri[i].Y * InvScale)));
        }
        return true;
    }

    // Walks the boundary between cells in and out of the set into closed loops of cell-corner points, with the set on the left.
    // Three cells meet at every hex corner, so each boundary corner has exactly one way out and there are no pinch points.
    Paths64 TraceLoops(IEnumerable<HexCoord> cells, System.Func<HexCoord, bool> contains) {
        cornerPoints.Clear();
        outgoingEdge.Clear();
        edgeStart.Clear(); edgeEnd.Clear(); edgeUsed.Clear();

        foreach (var cell in cells) {
            var centre = cell.Position();
            for (int direction = 0; direction < 6; direction++) {
                if (contains(cell.Neighbor(direction))) continue;
                var a = cell.Corner(edgeCorners[direction, 0]);
                var b = cell.Corner(edgeCorners[direction, 1]);
                // Keep the cell on the left of the edge.
                var along = b - a;
                var toCentre = centre - a;
                if (along.x * toCentre.y - along.y * toCentre.x < 0) (a, b) = (b, a);
                AddEdge(CornerKey(a), CornerKey(b));
            }
        }

        var loops = new Paths64();
        for (int first = 0; first < edgeStart.Count; first++) {
            if (edgeUsed[first]) continue;
            var loop = new Path64();
            int current = first;
            while (true) {
                edgeUsed[current] = true;
                loop.Add(cornerPoints[edgeStart[current]]);
                if (!outgoingEdge.TryGetValue(edgeEnd[current], out int next) || edgeUsed[next]) break;
                current = next;
            }
            if (loop.Count >= 3) loops.Add(loop);
        }
        return loops;
    }

    long CornerKey(Vector2 corner) {
        long key = ((long)Mathf.RoundToInt(corner.x * CornerKeyScale) << 32) ^ (uint)Mathf.RoundToInt(corner.y * CornerKeyScale);
        if (!cornerPoints.ContainsKey(key)) cornerPoints.Add(key, new Point64(System.Math.Round(corner.x * Scale), System.Math.Round(corner.y * Scale)));
        return key;
    }

    void AddEdge(long start, long end) {
        Debug.Assert(!outgoingEdge.ContainsKey(start), "Hex boundary corner with more than one way out");
        outgoingEdge[start] = edgeStart.Count;
        edgeStart.Add(start);
        edgeEnd.Add(end);
        edgeUsed.Add(false);
    }

    // Mitred offset: corners stay sharp (and keep their angles) for RoundCorners, and Clipper merges or splits loops that the offset makes overlap.
    Paths64 Offset(Paths64 loops, float extrusion) {
        if (extrusion == 0 || loops.Count == 0) return loops;
        offsetter.Clear();
        offsetter.AddPaths(loops, JoinType.Miter, EndType.Polygon);
        var result = new Paths64();
        offsetter.Execute(extrusion * Scale, result);
        return result;
    }

    // Replaces each corner with a circular arc tangent to both of its edges (outward and inward corners alike).
    // The tangent length is clamped to half of each adjacent edge so neighbouring arcs on an edge never overlap.
    Paths64 RoundCorners(Paths64 paths, float radius, float degreesPerPoint) {
        if (radius <= 0) return paths;
        double radiansPerPoint = Mathf.Clamp(degreesPerPoint, 1, 90) * Mathf.Deg2Rad;
        double r = radius * Scale;
        var result = new Paths64(paths.Count);
        foreach (var path in paths) {
            int count = path.Count;
            var rounded = new Path64(count * 4);
            for (int i = 0; i < count; i++) {
                Point64 prev = path[(i + count - 1) % count], corner = path[i], next = path[(i + 1) % count];
                double inX = corner.X - prev.X, inY = corner.Y - prev.Y, outX = next.X - corner.X, outY = next.Y - corner.Y;
                double inLength = System.Math.Sqrt(inX * inX + inY * inY), outLength = System.Math.Sqrt(outX * outX + outY * outY);
                if (inLength == 0 || outLength == 0) {
                    rounded.Add(corner);
                    continue;
                }
                double ux = inX / inLength, uy = inY / inLength, vx = outX / outLength, vy = outY / outLength;
                // Signed turn from the incoming to the outgoing edge; positive turns left.
                double turn = System.Math.Atan2(ux * vy - uy * vx, ux * vx + uy * vy);
                double tanHalfTurn = System.Math.Tan(System.Math.Abs(turn) * 0.5);
                if (tanHalfTurn < 1e-6) {
                    rounded.Add(corner);
                    continue;
                }
                double tangent = System.Math.Min(r * tanHalfTurn, 0.5 * System.Math.Min(inLength, outLength));
                double cornerRadius = tangent / tanHalfTurn;
                // The arc starts on the incoming edge, `tangent` back from the corner; its centre is off to the side the edge turns towards.
                double startX = corner.X - ux * tangent, startY = corner.Y - uy * tangent;
                double side = System.Math.Sign(turn);
                double centreX = startX - uy * cornerRadius * side, centreY = startY + ux * cornerRadius * side;
                double fromCentreX = startX - centreX, fromCentreY = startY - centreY;
                int segments = System.Math.Max(1, (int)System.Math.Ceiling(System.Math.Abs(turn) / radiansPerPoint));
                for (int s = 0; s <= segments; s++) {
                    double angle = turn * s / segments;
                    double cos = System.Math.Cos(angle), sin = System.Math.Sin(angle);
                    rounded.Add(new Point64(
                        System.Math.Round(centreX + fromCentreX * cos - fromCentreY * sin),
                        System.Math.Round(centreY + fromCentreX * sin + fromCentreY * cos)));
                }
            }
            result.Add(rounded);
        }
        return result;
    }
}
