using System.Collections.Generic;
using System.Linq;
using Clipper2Lib;
using NUnit.Framework;
using UnityEngine;
using UnityX.HexGrid;

public class FogTests {
    static float HexArea() {
        var corners = Enumerable.Range(0, 6).Select(i => HexCoord.zero.Corner(i)).ToArray();
        return Mathf.Abs(PolygonArea(corners));
    }

    static float PolygonArea(IReadOnlyList<Vector2> points) {
        float area = 0;
        for (int i = 0; i < points.Count; i++) {
            var a = points[i];
            var b = points[(i + 1) % points.Count];
            area += a.x * b.y - b.x * a.y;
        }
        return area * 0.5f;
    }

    static float TrianglesArea(List<Vector2> triangles) {
        float area = 0;
        for (int i = 0; i < triangles.Count; i += 3) area += Mathf.Abs(PolygonArea(new[] { triangles[i], triangles[i + 1], triangles[i + 2] }));
        return area;
    }

    static bool InsideTriangles(List<Vector2> triangles, Vector2 point) {
        for (int i = 0; i < triangles.Count; i += 3) {
            float d1 = Cross(triangles[i], triangles[i + 1], point);
            float d2 = Cross(triangles[i + 1], triangles[i + 2], point);
            float d3 = Cross(triangles[i + 2], triangles[i], point);
            bool hasNegative = d1 < 0 || d2 < 0 || d3 < 0;
            bool hasPositive = d1 > 0 || d2 > 0 || d3 > 0;
            if (!(hasNegative && hasPositive)) return true;
        }
        return false;
    }

    static float Cross(Vector2 a, Vector2 b, Vector2 p) => (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);

    // Cells outside `cells` that can't reach the far outside without crossing `cells`.
    static HashSet<HexCoord> FindPockets(HashSet<HexCoord> cells) {
        const int bound = 20;
        var outside = new HashSet<HexCoord>();
        var start = new HexCoord(bound, 0);
        var frontier = new Queue<HexCoord>();
        frontier.Enqueue(start);
        outside.Add(start);
        while (frontier.Count > 0) {
            var cell = frontier.Dequeue();
            for (int direction = 0; direction < 6; direction++) {
                var neighbour = cell.Neighbor(direction);
                if (HexCoord.Distance(HexCoord.zero, neighbour) > bound || cells.Contains(neighbour) || !outside.Add(neighbour)) continue;
                frontier.Enqueue(neighbour);
            }
        }
        var pockets = new HashSet<HexCoord>();
        foreach (var cell in HexUtils.HexagonPoints(bound)) {
            if (!cells.Contains(cell) && !outside.Contains(cell)) pockets.Add(cell);
        }
        return pockets;
    }

    static HexShapeBuilder Build(HashSet<HexCoord> cells, HexShapeBuilder.Style style) {
        var builder = new HexShapeBuilder();
        builder.Build(cells, cells.Contains, style);
        return builder;
    }

    [Test]
    public void SingleHexTracesOneCounterClockwiseLoopCoveringTheHex() {
        var cells = new HashSet<HexCoord> { new HexCoord(2, -1) };
        var builder = Build(cells, new HexShapeBuilder.Style(0, 0, 20));
        Assert.AreEqual(1, builder.tracedLoops.Count);
        Assert.AreEqual(6, builder.tracedLoops[0].Count);
        Assert.Greater(Clipper.Area(builder.tracedLoops[0]), 0);
        Assert.AreEqual(HexArea(), TrianglesArea(builder.triangles), HexArea() * 0.001f);
    }

    [Test]
    public void SeparateAreasTraceSeparateLoops() {
        var cells = new HashSet<HexCoord> { HexCoord.zero, new HexCoord(3, 0) };
        var builder = Build(cells, new HexShapeBuilder.Style(0, 0, 20));
        Assert.AreEqual(2, builder.tracedLoops.Count);
        Assert.AreEqual(2 * HexArea(), TrianglesArea(builder.triangles), HexArea() * 0.001f);
    }

    [Test]
    public void UnrevealedPocketStaysOutOfTheShape() {
        var cells = new HashSet<HexCoord>(FogTestPatterns.revealedAreaWithHole);
        var pockets = FindPockets(cells);
        Assert.IsNotEmpty(pockets, "The test pattern should enclose a pocket");

        var builder = Build(cells, new HexShapeBuilder.Style(0, 0, 20));
        Assert.AreEqual(1, builder.tracedLoops.Count(loop => Clipper.Area(loop) > 0), "outer loops");
        Assert.GreaterOrEqual(builder.tracedLoops.Count(loop => Clipper.Area(loop) < 0), 1, "hole loops");
        Assert.AreEqual(cells.Count * HexArea(), TrianglesArea(builder.triangles), HexArea() * 0.01f);
        foreach (var cell in cells) Assert.IsTrue(InsideTriangles(builder.triangles, cell.Position()), $"{cell} should be revealed");
        foreach (var cell in pockets) Assert.IsFalse(InsideTriangles(builder.triangles, cell.Position()), $"{cell} should stay fogged");
    }

    [Test]
    public void RoundedAndExtrudedShapeKeepsThePocket() {
        var cells = new HashSet<HexCoord>(FogTestPatterns.revealedAreaWithHole);
        var pockets = FindPockets(cells);
        var builder = Build(cells, new HexShapeBuilder.Style(0.2f, 0.38f, 20));
        Assert.IsNotEmpty(builder.triangles);
        foreach (var cell in cells) Assert.IsTrue(InsideTriangles(builder.triangles, cell.Position()), $"{cell} should be revealed");
        foreach (var cell in pockets) Assert.IsFalse(InsideTriangles(builder.triangles, cell.Position()), $"{cell} should stay fogged");
    }

    static BoardModel CreateFoggedIsland(int radius) {
        var board = TomsLevelGenerator.CreateGameModel().board;
        foreach (var coord in HexShapes.Hexagon(HexCoord.zero, radius)) board.terrain[coord] = TerrainType.Grass;
        board.CoverWithFog(board.terrain.Coords);
        return board;
    }

    [Test]
    public void RevealingABatchNotifiesOnce() {
        var board = CreateFoggedIsland(3);
        int changes = 0;
        board.OnFogChanged += () => changes++;

        board.RevealFog(HexCoord.zero, 2);
        Assert.AreEqual(1, changes);
        Assert.AreEqual(HexShapes.Hexagon(HexCoord.zero, 2).Count(), board.RevealedFogCells().Count());

        board.RevealFog(HexCoord.zero, 1);
        Assert.AreEqual(1, changes, "Revealing cells that are already revealed changes nothing");
    }

    [Test]
    public void CellsWithoutFogAreNotPartOfTheRevealedShape() {
        var board = CreateFoggedIsland(1);
        board.RevealFog(new HexCoord(5, 0));
        Assert.IsFalse(board.IsFogRevealed(new HexCoord(5, 0)));
        Assert.IsTrue(board.IsRevealed(new HexCoord(5, 0)), "Cells without fog still count as revealed for gameplay");
        board.RevealFog(HexCoord.zero);
        Assert.IsTrue(board.IsFogRevealed(HexCoord.zero));
    }
}
