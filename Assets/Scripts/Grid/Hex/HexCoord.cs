using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Axial Hexagon grid coordinate.
/// </summary>
/// <remarks>
/// Uses the q,r axial system detailed at http://www.redblobgames.com/grids/hexagons/.
/// These are "pointy topped" hexagons. The q axis points right, and the r axis points up-right.
/// When converting to and from Unity coordinates, the length of a hexagon side is 1 unit.
/// </remarks>
[System.Serializable]
public struct HexCoord : IEquatable<HexCoord> {
	// return values of turns() method
   public static int LEFT = 1;
   public static int RIGHT = -1;
   public static int STRAIGHT = 0;
   
   // returns one of the 3 above constants, depending on whether the
   // three vertices constitute a left turn or a right turn.
   public static int turns(Vector2 v0,Vector2 v1,Vector2 v2) {
      var cross = (v1.x-v0.x)*(v2.y-v0.y) - (v2.x-v0.x)*(v1.y-v0.y);
      return((cross>0.0f) ? LEFT : ((cross==0.0f) ? STRAIGHT : RIGHT));
   }
	/// <summary>
	/// Position on the q axis.
	/// </summary>
	[SerializeField]
	public int q;
	/// <summary>
	/// Position on the r axis.
	/// </summary>
	[SerializeField]
	public int r;

	[Newtonsoft.Json.JsonIgnore]
	public int s {
		get {
			return -q-r;
		}
	}

	public static readonly HexCoord zero = default(HexCoord);

	/// <summary>
	/// Initializes a new instance of the <see cref="Settworks.Hexagons.HexCoord"/> struct.
	/// </summary>
	/// <param name="q">Position on the q axis.</param>
	/// <param name="r">Position on the r axis.</param>
	public HexCoord(int q, int r) : this() {
		this.q = q;
		this.r = r;
	}

	// public Vector2 Position() {
	// 	var dir = q*Q_XY + r*R_XY;
	// 	if(dir.sqrMagnitude != 1) dir.Normalize();
	// 	return dir;
	// }
	public Vector2 DirectionVector() {
		var dir = q*Q_XY + r*R_XY;
		if(dir.sqrMagnitude != 1) dir.Normalize();
		return dir;
	}
	public Vector2 Direction(Vector2 fractionalHex) {
		var dir = fractionalHex.x*Q_XY + fractionalHex.y*R_XY;
		if(dir.sqrMagnitude != 1) dir.Normalize();
		return dir;
	}

	/// <summary>
	/// Unity position of this hex.
	/// </summary>
	public Vector2 Position() {
		// var offsetPos = ToOddR(this);
		// return GridController.Instance.room.grid.CellToWorld(new Vector3Int(offsetPos.x, offsetPos.y, 0));
		
		
		// var offsetPos = ToOffset();
		// return GridController.Instance.room.grid.CellToLocal(new Vector3Int(offsetPos.x, offsetPos.y, 0));
		// if(orientation == Orientation.Flat) {
			// var offsetPos = ToOddQ(this);
			// return GridController.Instance.room.grid.CellToLocal(new Vector3Int(offsetPos.x, offsetPos.y, 0));
		// } else {
		// }
		// var cubeCoord = HexToCube();
		// return GridController.Instance.room.grid.CellToLocal(new Vector3Int(cubeCoord.y, cubeCoord.x, cubeCoord.z));
		// return GridController.Instance.room.grid.CellToLocal(new Vector3Int(cubeCoord.x, cubeCoord.y, cubeCoord.z));
		return q*Q_XY + r*R_XY;
	}

	/// <summary>
	/// Get the maximum absolute cubic coordinate.
	/// </summary>
	/// <remarks>
	/// In hexagonal space this is the polar radius, i.e. distance from 0,0.
	/// </remarks>
	public int AxialLength() {
		if (q == 0 && r == 0) return 0;
		if (q > 0 && r >= 0) return q + r;
		if (q <= 0 && r > 0) return (-q < r)? r: -q;
		if (q < 0) return -q - r;
		return (-r > q)? -r: q;
	}

	/// <summary>
	/// Get the minimum absolute cubic coordinate.
	/// </summary>
	/// <remarks>
	/// This is the number of hexagon steps from 0,0 which are not along the maximum axis.
	/// </remarks>
	public int AxialSkew() {
		if (q == 0 && r == 0) return 0;
		if (q > 0 && r >= 0) return (q < r)? q: r;
		if (q <= 0 && r > 0) return (-q < r)? Mathf.Min(-q, q + r): Mathf.Min(r, -q - r);
		if (q < 0) return (q > r)? -q: -r;
		return (-r > q)? Mathf.Min(q, -q -r): Mathf.Min(-r, q + r);
	}



	/// <summary>
	/// Get the counterclockwise position of this hex in the ring at its distance from 0,0.
	/// </summary>
	public int PolarIndex() {
		if (q == 0 && r == 0) return 0;
		if (q > 0 && r >= 0) return r;
		if (q <= 0 && r > 0) return (-q < r)? r - q: -3 * q - r;
		if (q < 0) return -4 * (q + r) + q;
		return (-r > q)? -4 * r + q: 6 * q + r;
	}

	/// <summary>
	/// Get a neighboring hex.
	/// </summary>
	/// <remarks>
	/// Neighbor 0 is to the right, others proceed counterclockwise.
	/// </remarks>
	/// <param name="index">Index of the desired neighbor. Cyclically constrained 0..5.</param>
	public HexCoord Neighbor(int index) {
		return Direction(index) + this;
	}
	
	public HexCoord PolarNeighbor(bool CCW = false) {
		if (q > 0) {
			if (r < 0) {
				if (q > -r) return this + directions[CCW? 1: 4];
				if (q < -r) return this + directions[CCW? 0: 3];
				return this + directions[CCW? 1: 3];
			}
			if (r > 0) return this + directions[CCW? 2: 5];
			return this + directions[CCW? 2: 4];
		}
		if (q < 0) {
			if (r > 0) {
				if (r > -q) return this + directions[CCW? 3: 0];
				if (r < -q) return this + directions[CCW? 4: 1];
				return this + directions[CCW? 4: 0];
			}
			if (r < 0) return this + directions[CCW? 5: 2];
			return this + directions[CCW? 5: 1];
		}
		if (r > 0) return this + directions[CCW? 3: 5];
		if (r < 0) return this + directions[CCW? 0: 2];
		return this;
	}

	public int NeighborIndexOf(HexCoord normalizedDirection) {
		return directions.IndexOf(normalizedDirection);
	}
	/// <summary>
	/// Enumerate this hex's six neighbors.
	/// </summary>
	/// <remarks>
	/// Neighbor 0 is to the right, others proceed counterclockwise.
	/// </remarks>
	/// <param name="first">Index of the first neighbor to enumerate.</param>
	public IEnumerable<HexCoord> Neighbors(int first = 0) {
		foreach (HexCoord hex in Directions(first))
			yield return hex + this;
	}

	/// <summary>
	/// Get the Unity position of a corner vertex.
	/// </summary>
	/// <remarks>
	/// Corner 0 is at the upper right, others proceed counterclockwise.
	/// </remarks>
	/// <param name="index">Index of the desired corner. Cyclically constrained 0..5.</param>
	public Vector2 Corner(int index) {
		return CornerVector(index) + Position();
	}

	public static Vector2 Corner(HexCoord coord, int index) {
		return coord.Corner(index);
	}

	/// <summary>
	/// Enumerate this hex's six corners.
	/// </summary>
	/// <remarks>
	/// Corner 0 is at the upper right, others proceed counterclockwise.
	/// </remarks>
	/// <param name="first">Index of the first corner to enumerate.</param>
	public IEnumerable<Vector2> Corners(int first = 0, float hexSize = 1) {
		Vector2 pos = Position();
		foreach (Vector2 v in CornerVectors(first, hexSize))
			yield return v + pos;
	}

	/// <summary>
	/// Get the polar angle to a corner vertex.
	/// </summary>
	/// <remarks>
	/// This is the angle in radians from the center of 0,0 to the selected corner of this hex.
	/// </remarks>
	/// <param name="index">Index of the desired corner.</param>
	public float CornerPolarAngle(int index) {
		Vector2 pos = Corner(index);
		return (float)Mathf.Atan2(pos.y, pos.x);
	}

	/// <summary>
	/// Get the polar angle to the clockwise bounding corner.
	/// </summary>
	/// <remarks>
	/// The two polar bounding corners are those whose polar angles form the widest arc.
	/// </remarks>
	/// <param name="CCW">If set to <c>true</c>, gets the counterclockwise bounding corner.</param>
	public float PolarBoundingAngle(bool CCW = false) {
		return CornerPolarAngle(PolarBoundingCornerIndex(CCW));
	}

	/// <summary>
	/// Get the XY position of the clockwise bounding corner.
	/// </summary>
	/// <remarks>
	/// The two polar bounding corners are those whose polar angles form the widest arc.
	/// </remarks>
	/// <param name="CCW">If set to <c>true</c>, gets the counterclockwise bounding corner.</param>
	public Vector2 PolarBoundingCorner(bool CCW = false) {
		return Corner(PolarBoundingCornerIndex(CCW));
	}

	/// <summary>
	/// Get the index of the clockwise bounding corner.
	/// </summary>
	/// <remarks>
	/// The two polar bounding corners are those whose polar angles form the widest arc.
	/// </remarks>
	/// <param name="CCW">If set to <c>true</c>, gets the counterclockwise bounding corner.</param>
	/// <param name="neighbor">If set to <c>true</c>, gets the other corner shared by the same ring-neighbor as normal return.</param>
	public int PolarBoundingCornerIndex(bool CCW = false) {
		if (q == 0 && r == 0) return 0;
		if (q > 0 && r >= 0) return CCW?
			(q > r)? 1: 2:
			(q < r)? 5: 4;
		if (q <= 0 && r > 0) return (-q < r)?
			CCW?
				(r > -2 * q)? 2: 3:
				(r < -2 * q)? 0: 5:
			CCW?
				(q > -2 * r)? 3: 4:
				(q < -2 * r)? 1: 0;
		if (q < 0) return CCW?
			(q < r)? 4: 5:
			(q > r)? 2: 1;
		return (-r > q)?
			CCW?
				(r < -2 * q)? 5: 0:
				(r > -2 * q)? 3: 2:
			CCW?
				(q < -2 * r)? 0: 1:
				(q > -2 * r)? 4: 3;
	}

	/// <summary>
	/// Get the half sextant of origin containing this hex.
	/// </summary>
	/// <remarks>
	/// CornerSextant is HalfSextant/2. NeighborSextant is (HalfSextant+1)/2.
	/// </remarks>
	public int HalfSextant() {
		if (q > 0 && r >= 0 || q == 0 && r == 0)
			return (q > r)? 0 : 1;
		if (q <= 0 && r > 0)
			return (-q < r)?
				(r > -2 * q)? 2: 3:
				(q > -2 * r)? 4: 5;
		if (q < 0)
			return (q < r)? 6: 7;
		return (-r > q)?
			(r < -2 * q)? 8: 9:
			(q < -2 * r)? 10: 11;
	} 

	/// <summary>
	/// Get the corner index of 0,0 closest to this hex's polar vector.
	/// </summary>
	public int CornerSextant() {
		if (q > 0 && r >= 0 || q == 0 && r == 0) return 0;
		if (q <= 0 && r > 0) return (-q < r)? 1: 2;
		if (q < 0) return 3;
		return (-r > q)? 4: 5;
	}

	/// <summary>
	/// Get the neighbor index of 0,0 through which this hex's polar vector passes.
	/// </summary>
	public int NeighborSextant() {
		if (q == 0 && r == 0) return 0;
		if (q > 0 && r >= 0) return (q <= r)? 1: 0;
		if (q <= 0 && r > 0) return (-q <= r)?
			(r <= -2 * q)? 2: 1:
			(q <= -2 * r)? 3: 2;
		if (q < 0) return (q >= r)? 4: 3;
		return (-r > q)?
			(r >= -2 * q)? 5: 4:
			(q >= -2 * r)? 0: 5;
	}

	/// <summary>
	/// Rotate around 0,0 in sextant increments.
	/// </summary>
	/// <returns>
	/// A new <see cref="Settworks.Hexagons.HexCoord"/> representing this one after rotation.
	/// </returns>
	/// <param name="sextants">How many sextants to rotate by.</param>
	public HexCoord SextantRotation(int sextants) {
		if (this == zero) return this;
		sextants = NormalizeRotationIndex(sextants, 6);
		if (sextants == 0) return this;
		if (sextants == 1) return new HexCoord(-r, -s);
		if (sextants == 2) return new HexCoord(s, q);
		if (sextants == 3) return new HexCoord(-q, -r);
		if (sextants == 4) return new HexCoord(r, s);
		return new HexCoord(-s, -q);
	}

	/// <summary>
	/// Mirror across a cubic axis.
	/// </summary>
	/// <remarks>
	/// The cubic axes are "diagonal" to the hexagons, passing through two opposite corners.
	/// </remarks>
	/// <param name="axis">A corner index through which the axis passes.</param>
	/// <returns>A new <see cref="Settworks.Hexagons.HexCoord"/> representing this one after mirroring.</returns>
	public HexCoord Mirror(int axis = 1) {
		if (this == zero) return this;
		axis = NormalizeRotationIndex(axis, 3);
		if (axis == 0) return new HexCoord(r, q);
		if (axis == 1) return new HexCoord(s, r);
		return new HexCoord(q, s);
	}

	/// <summary>
	/// Scale as a vector, truncating result.
	/// </summary>
	/// <returns>This <see cref="Settworks.Hexagons.HexCoord"/> after scaling.</returns>
	public HexCoord Scale(float factor) {
		q = (int)(q * factor);
		r = (int)(r * factor);
		return this;
	}
	/// <summary>
	/// Scale as a vector.
	/// </summary>
	/// <returns>This <see cref="Settworks.Hexagons.HexCoord"/> after scaling.</returns>
	public HexCoord Scale(int factor) {
		q *= factor;
		r *= factor;
		return this;
	}

	/*
	/// <summary>
	/// Scale as a vector.
	/// </summary>
	/// <returns><see cref="UnityEngine.Vector2"/> representing the scaled vector.</returns>
	public Vector2 ScaleToVector(float factor)
	{ return new Vector2(q * factor, r * factor); }

	/// <summary>
	/// Determines whether this hex is within a specified rectangle.
	/// </summary>
	/// <returns><c>true</c> if this instance is within the specified rectangle; otherwise, <c>false</c>.</returns>
	public bool IsWithinRectangle(HexCoord cornerA, HexCoord cornerB) {
		if (r > cornerA.r && r > cornerB.r || r < cornerA.r && r < cornerB.r)
			return false;
		bool reverse = cornerA.O > cornerB.O;	// Travel right to left.
		bool offset = cornerA.r % 2 != 0;	// Starts on an odd row, bump alternate rows left.
		bool trim = Mathf.Abs(cornerA.r - cornerB.r) % 2 == 0;	// Even height, trim alternate rows.
		bool odd = (r - cornerA.r) % 2 != 0; // This is an alternate row.
		int width = Mathf.Abs(cornerA.O - cornerB.O);
		bool hasWidth = width != 0;
		if (reverse && (odd && (trim || !offset) || !(trim || offset || odd))
		    || !reverse && (trim && odd || offset && !trim && hasWidth))
			width -= 1;
		int x = (O - cornerA.O) * (reverse? -1: 1);
		if (reverse && odd && !offset
		    || !reverse && offset && odd && hasWidth)
			x -= 1;
		return (x <= width && x >= 0);
	}
	*/
	
	
	/// <summary>
	/// Determines whether this hex is on the ray starting at origin in a direction
	/// </summary>
	public bool IsOnLine(HexCoord origin, HexCoord direction) {
		var offsetOriginC = (this-origin).HexToCube();
		if(offsetOriginC == Point3.zero) return true;
		var dirC = direction.HexToCube();
		// If I need this to work in either direction, then check if dirC.x is 0 AND this condition before returning false
		if(MathX.Sign(dirC.x, true) != MathX.Sign(offsetOriginC.x, true)) {
			return false;
		} else if(MathX.Sign(dirC.y, true) != MathX.Sign(offsetOriginC.y, true)) {
			return false;
		} else if(MathX.Sign(dirC.z, true) != MathX.Sign(offsetOriginC.z, true)) {
			return false;
		}
		return true;
	}



	/// <summary>
	/// Determines whether this hex is on the infinite line passing through points a and b.
	/// </summary>
	public bool IsOnCartesianLine(Vector2 a, Vector2 b) {
		Vector2 AB = b - a;
		bool bias = Vector3.Cross(AB, Corner(0) - a).z > 0;
		for (int i = 1; i < 6; i++) {
			if (bias != (Vector3.Cross(AB, Corner(i) - a).z > 0))
				return true;
		}
		return false;
	}

	/// <summary>
	/// Determines whether this the is on the line segment between points a and b.
	/// </summary>
	public bool IsOnCartesianLineSegment(Vector2 a, Vector2 b) {
		Vector2 AB = b - a;
		float mag = AB.sqrMagnitude;
		Vector2 AC = Corner(0) - a;
		bool within = AC.sqrMagnitude <= mag && Vector2.Dot(AB, AC) >= 0;
		int sign = Mathf.Sign(Vector3.Cross(AB, AC).z).RoundToInt();
		for (int i = 1; i < 6; i++) {
			AC = Corner(i) - a;
			bool newWithin = AC.sqrMagnitude <= mag && Vector2.Dot(AB, AC) >= 0;
			int newSign =	Mathf.Sign(Vector3.Cross(AB, AC).z).RoundToInt();
			if ((within || newWithin) && (sign * newSign <= 0))
				return true;
			within = newWithin;
			sign = newSign;
		}
		return false;
	}


	// A routine to tell if a hexagon has any part of it is "left" of a given
   // arc boundary (clockwise arm of the cone)
   public bool leftOfArc(HexCoord hc,Vector2 ac) {
      int i,carm;
      for (i=0;i<6;i++) {
		carm = turns(hc.Position(),ac,Corner(i));
		if (carm==LEFT) return true;
      }
      return false;
   }
   
   // A routine to tell if a hexagon has any part of it is "right" of a given
   // arc boundary (counter-clockwise arm of the cone)
   public bool rightOfArc(HexCoord hc,Vector2 acc) {
      int i,ccarm;
      for (i=0;i<6;i++) {
		ccarm = turns(hc.Position(),acc,Corner(i));
		if (ccarm==RIGHT) return true;
      }
      return false;
   }
   

	/// <summary>
	/// Returns a <see cref="System.String"/> that represents the current <see cref="Settworks.Hexagons.HexCoord"/>.
	/// </summary>
	/// <remarks>
	/// Matches the formatting of <see cref="UnityEngine.Vector2.ToString()"/>.
	/// </remarks>
	public override string ToString () {
		return string.Format("({0},{1})", q, r);
	}
	public string ToDirectionString () {
		return Position()+" "+Position().DirectionName();
	}
	
	/*
	 * Static Methods
	 */

	/// <summary>
	/// Distance between two hexes.
	/// </summary>
	public static int Distance(HexCoord a, HexCoord b) {
		return (a - b).AxialLength();
	}

	static public Vector2 HexLerp(HexCoord a, HexCoord b, float t) {
		return Vector2.Lerp((Vector2)a, (Vector2)b, t);
    }


	public static HexCoord[] GetPointsOnLine (HexCoord direction, int startLineDistance, int endLineDistance) {
		HexCoord[] results = new HexCoord[(endLineDistance - startLineDistance) + 1];
		for(int d = startLineDistance; d <= endLineDistance; d++) results[d - startLineDistance] = direction * d;
		return results;
	}

	public static HexCoord[] GetPointsOnLine (HexCoord gridPoint, HexCoord direction, int lineDistance) {
		var points = GetPointsOnLine(direction, 0, lineDistance-1);
		for (int i = 0; i < points.Length; i++) points [i] += gridPoint;
		return points;
	}


	public static HexCoord[] GetPointsInRing(int minRadius, int maxRadius) {
		int length = 0;
		for(int d = minRadius; d <= maxRadius; d++)
			length += d == 0 ? 1 : (d * 6);
		HexCoord[] results = new HexCoord[length];
		length = 0;
		for(int d = minRadius; d <= maxRadius; d++) {
			GetPointsOnRingNonAlloc(results, length, d);
			length += d == 0 ? 1 : (d * 6);
		}
		return results;
	}

	public static HexCoord[] GetPointsInRing(HexCoord gridPoint, int minRadius, int maxRadius) {
		var points = GetPointsInRing(minRadius, maxRadius);
		for(int i = 0; i < points.Length; i++) points[i] += gridPoint;
		return points;
	}

	public static HexCoord[] GetPointsOnRing(int ringDistance, int startIndex = 0) {
		if(ringDistance == 0) return new HexCoord[] {HexCoord.zero};
		HexCoord[] results = new HexCoord[ringDistance * 6];
		GetPointsOnRingNonAlloc(results, 0, ringDistance, startIndex);
	    return results;
	}
	public static void GetPointsOnRingNonAlloc(HexCoord[] array, int arrayOffset, int ringDistance, int startIndex = 0) {
		if(ringDistance == 0) {
            array[arrayOffset] = HexCoord.zero;
        } else {
            //    # this code doesn't work for ringDistance == 0; can you see why?
            var cube = (Direction(4)) * ringDistance;
            int k = arrayOffset;
            for(int i = startIndex; i < startIndex+6; i++) {
                for(int j = 0; j < ringDistance; j++) {
                    array[k] = cube;
                    cube = cube.Neighbor(i);
                    k++;
                }
            }
        }
	}

	public static HexCoord[] GetPointsOnRing(HexCoord gridPoint, int ringDistance) {
		var points = GetPointsOnRing(ringDistance);
		for(int i = 0; i < points.Length; i++) points[i] += gridPoint;
		return points;
	}

    public static HexCoord[] GetPointsOnArc(int ringDirectionIndexA, int ringDirectionIndexB, int ringDistance) {
		List<HexCoord> results = new List<HexCoord>();
        while(ringDirectionIndexB < ringDirectionIndexA) ringDirectionIndexB += 6;
	    for(int directionIndex = ringDirectionIndexA; directionIndex <= ringDirectionIndexB; directionIndex++) {
		    var cube = Direction(directionIndex) * ringDistance;
            results.Add(cube);
	    }
	    return results.ToArray();
	}

	public static HexCoord[] GetPointsOnArc(HexCoord gridPoint, int ringDirectionIndexA, int ringDirectionIndexB, int ringDistance) {
		var points = GetPointsOnArc(ringDirectionIndexA, ringDirectionIndexB, ringDistance);
		for(int i = 0; i < points.Length; i++) points[i] += gridPoint;
		return points;
	}


	public static HexCoord[] LineDraw(HexCoord a, HexCoord b) {
		var distance = Distance(a, b);
		var results = new HexCoord[distance + 1];
		var n = 1f/(distance);
	    for (int i = 0; i <= distance; i++) {
			results[i] = RoundFromQRVector(HexCoord.HexLerp(a, b, n * i));
	    }
	    return results;
    }

    /// <summary>
    /// If a straight line can be drawn from a which passes through b
    /// </summary>
    /// <returns>The <see cref="System.Boolean"/>.</returns>
    /// <param name="a">The alpha component.</param>
    /// <param name="b">The blue component.</param>
	public static bool StraightLineExistsBetween (HexCoord a, HexCoord b) {
		var ac = a.HexToCube();
		var bc = b.HexToCube();
		return ac.x == bc.x || ac.y == bc.y || ac.z == bc.z;
    }

	// The closest distance to either of the grid-aligned lines from the origin to the target
	public static int GetMinDistanceToCoordOnStraightLine (HexCoord linesOrigin, HexCoord targetCoord) {
		var coords = HexCoord.GetClosestPointsToCoordOnStraightLine(linesOrigin, targetCoord);
		return Mathf.Min(HexCoord.Distance(linesOrigin, coords.First()), HexCoord.Distance(targetCoord, coords.First()));
	}
    /// <summary>
	/// Gets the points on a straight line from a closest to b. Note there can two lines!
	/// Returns all the points between the start and end for each line
    /// </summary>
    /// <returns>The closest points to coordinate on straight line.</returns>
    /// <param name="linesOrigin">The alpha component.</param>
    /// <param name="targetCoord">The blue component.</param>
	public static List<HexCoord> GetClosestPointsToCoordOnStraightLine (HexCoord linesOrigin, HexCoord targetCoord) {

		//NOTE THERES AN ISSUE HERE - Depending on the distance from the line, theres a correlated number of points of equal proximity from that line - not just 2!
		// It should be easily solvable, but I dont know what the intended function is.
		var p = new List<HexCoord>();
		var ac = linesOrigin.HexToCube();
		var bc = targetCoord.HexToCube();
		if(ac.x == bc.x || ac.y == bc.y || ac.z == bc.z) p.Add(targetCoord);
		else {
			var xMin = MathX.Difference(ac.x, bc.x);
			var yMin = MathX.Difference(ac.y, bc.y);
			var zMin = MathX.Difference(ac.z, bc.z);
			// Then it's on the x axis
			if(xMin <= yMin && xMin <= zMin) {
				// p.Add(HexCoord.CubeToHex(new Point3(ac.x, bc.y+(bc.x-ac.x), bc.z)));
				// p.Add(HexCoord.CubeToHex(new Point3(ac.x, bc.y, bc.z+(bc.x-ac.x))));
				p.AddRange(HexCoord.LineDraw(HexCoord.CubeToHex(new Point3(ac.x, bc.y+(bc.x-ac.x), bc.z)), HexCoord.CubeToHex(new Point3(ac.x, bc.y, bc.z+(bc.x-ac.x)))));
			}
			if(yMin <= xMin && yMin <= zMin) {
				// p.Add(HexCoord.CubeToHex(new Point3(bc.x+(bc.y-ac.y), ac.y, bc.z)));
				// p.Add(HexCoord.CubeToHex(new Point3(bc.x, ac.y, bc.z+(bc.y-ac.y))));
				p.AddRange(HexCoord.LineDraw(HexCoord.CubeToHex(new Point3(bc.x+(bc.y-ac.y), ac.y, bc.z)), HexCoord.CubeToHex(new Point3(bc.x, ac.y, bc.z+(bc.y-ac.y)))));
			}
			if(zMin <= xMin && zMin <= yMin) {
				p.AddRange(HexCoord.LineDraw(HexCoord.CubeToHex(new Point3(bc.x+(bc.z-ac.z), bc.y, ac.z)), HexCoord.CubeToHex(new Point3(bc.x, bc.y+(bc.z-ac.z), ac.z))));
				// p.Add(HexCoord.CubeToHex(new Point3(bc.x+(bc.z-ac.z), bc.y, ac.z)));
				// p.Add(HexCoord.CubeToHex(new Point3(bc.x, bc.y+(bc.z-ac.z), ac.z)));
			}
		}
		return p;
    }

	/// <summary>
	/// Normalize a rotation index within 0 <= index < cycle.
	/// </summary>
	public static int NormalizeRotationIndex(int index, int cycle = 6) {
		if (index < 0 ^ cycle < 0)
			return (index % cycle + cycle) % cycle;
		else
			return index % cycle;
	}

	/// <summary>
	/// Determine the equality of two rotation indices for a given cycle.
	/// </summary>
	public static bool IsSameRotationIndex(int a, int b, int cycle = 6) {
		return 0 == NormalizeRotationIndex(a - b, cycle);
	}

	public static HexCoord GetClosestDirection (HexCoord a, HexCoord b) {
		if(Distance(a, b) <= 1) return (b - a);
		var direction = Vector2X.NormalizedDirection(a.Position(), b.Position());
		return HexCoord.AtPosition(direction * 1.5f);
	}
	
	public static IEnumerable<HexCoord> GetClosestDirections (HexCoord a, HexCoord b) {
		var fromTo = b - a;
		var qSign = MathX.Sign(fromTo.q, true);
		var rSign = MathX.Sign(fromTo.r, true);
		var sSign = MathX.Sign(fromTo.s, true);
		foreach(var direction in directions) {
			int numSameAxis = 0;
			// This could be micro-optimised by caching the sign of each direction
			if(MathX.Sign(direction.q, true) == qSign) numSameAxis++;
			if(MathX.Sign(direction.r, true) == rSign) numSameAxis++;
			if(MathX.Sign(direction.s, true) == sSign) numSameAxis++;
			if(numSameAxis >= 2) {
				yield return direction;
			}
		}
	}

	public static IEnumerable<int> GetClosestDirectionIndicies (HexCoord a, HexCoord b) {
		foreach(var direction in GetClosestDirections(a,b)) {
			yield return ClosestDirectionIndex(direction);
		}
	}
	public static int GetClosestDirectionIndex (HexCoord a, HexCoord b) {
		return ClosestDirectionIndex(GetClosestDirection(a,b));
	}

	/// <summary>
	/// Vector from a hex to a neighbor.
	/// </summary>
	/// <remarks>
	/// Neighbor 0 is to the right, others proceed counterclockwise.
	/// </remarks>
	/// <param name="index">Index of the desired neighbor vector. Cyclically constrained 0..5.</param>
	public static HexCoord Direction(int index)
	{ return directions[NormalizeRotationIndex(index, 6)]; }

	/// <summary>
	/// Enumerate the six neighbor vectors.
	/// </summary>
	/// <remarks>
	/// Neighbor 0 is to the right, others proceed counterclockwise.
	/// </remarks>
	/// <param name="first">Index of the first neighbor vector to enumerate.</param>
	public static IEnumerable<HexCoord> Directions(int first = 0) {
		first = NormalizeRotationIndex(first, 6);
		for (int i = first; i < 6; i++)
			yield return directions[i];
		for (int i = 0; i < first; i++)
	     yield return directions[i];
	}

	public static IEnumerable<HexCoord> Directions(HexCoord c, int first = 0) {
		foreach(var d in Directions(first)) {
			yield return c+d;
		}
	}

	public static int ClosestDirectionIndex (HexCoord direction) {
		var hexCoord = RoundFromQRVector(new Vector2(direction.q, direction.r).normalized);
		return directions.IndexOf(hexCoord);
	}

	public static HexCoord RotateDirection (HexCoord direction, int numRotationSteps) {
		int index = HexCoord.ClosestDirectionIndex(direction) + numRotationSteps;
		return HexCoord.Direction(index);
	}

	
	/// <summary>
	/// Neighbor index of 0,0 through which a polar angle passes.
	/// </summary>
	public static int AngleToNeighborIndex(float angle)
	{ return Mathf.RoundToInt(angle / SEXTANT); }
	
	/// <summary>
	/// Polar angle for a neighbor of 0,0.
	/// </summary>
	public static float NeighborIndexToAngle(int index)
	{ return index * SEXTANT; }

	/// <summary>
	/// Unity position vector from hex center to a corner.
	/// </summary>
	/// <remarks>
	/// Corner 0 is at the upper right, others proceed counterclockwise.
	/// </remarks>
	/// <param name="index">Index of the desired corner. Cyclically constrained 0..5.</param>
	public static Vector2 CornerVector(int index) {
		return corners[NormalizeRotationIndex(index, 6)];
	}

	/// <summary>
	/// Enumerate the six corner vectors.
	/// </summary>
	/// <remarks>
	/// Corner 0 is at the upper right, others proceed counterclockwise.
	/// </remarks>
	/// <param name="first">Index of the first corner vector to enumerate.</param>
	public static IEnumerable<Vector2> CornerVectors(int first = 0, float hexSize = 1) {
		var cachedCorners = corners;
		if (first == 0) {
            for (int i = 0; i < cachedCorners.Length; i++)
				yield return cachedCorners[i] * hexSize;
        } else {
			first = NormalizeRotationIndex(first, 6);
			for (int i = first; i < 6; i++)
				yield return cachedCorners[i];
			for (int i = 0; i < first; i++)
				yield return cachedCorners[i];
		}
	}

	/// <summary>
	/// Corner of 0,0 closest to a polar angle.
	/// </summary>
	public static int AngleToCornerIndex(float angle)
	{ return Mathf.FloorToInt(angle / SEXTANT); }

	/// <summary>
	/// Polar angle for a corner of 0,0.
	/// </summary>
	public static float CornerIndexToAngle(int index)
	{ return (index + 0.5f) * SEXTANT; }

	/// <summary>
	/// Half sextant of 0,0 through which a polar angle passes.
	/// </summary>
	public static int AngleToHalfSextant(float angle)
	{ return Mathf.RoundToInt(2 * angle / SEXTANT); }

	/// <summary>
	/// Polar angle at which a half sextant begins.
	/// </summary>
	public static float HalfSextantToAngle(int index)
	{ return index * SEXTANT / 2; }
	
	/// <summary>
	/// <see cref="Settworks.Hexagons.HexCoord"/> containing a Unity position.
	/// </summary>
	public static HexCoord AtPosition(Vector2 position) {
		var qr = VectorXYtoQR(position);
		return RoundFromQRVector(qr);
	}
	
	/// <summary>
	/// <see cref="Settworks.Hexagons.HexCoord"/> from hexagonal polar coordinates.
	/// </summary>
	/// <remarks>
	/// Hexagonal polar coordinates approximate a circle to a hexagonal ring.
	/// </remarks>
	/// <param name="radius">Hex distance from 0,0.</param>
	/// <param name="index">Counterclockwise index.</param>
	public static HexCoord AtPolar(int radius, int index) {
		if (radius == 0) return zero;
		if (radius < 0) radius = -radius;
		index = NormalizeRotationIndex(index, radius * 6);
		int sextant = index / radius;
		index %= radius;
		if (sextant == 0) return new HexCoord(radius - index, index);
		if (sextant == 1) return new HexCoord(-index, radius);
		if (sextant == 2) return new HexCoord(-radius, radius - index);
		if (sextant == 3) return new HexCoord(index - radius, -index);
		if (sextant == 4) return new HexCoord(index, -radius);
		return new HexCoord(radius, index - radius);
	}

	/// <summary>
	/// Find the hexagonal polar index closest to angle at radius.
	/// </summary>
	/// <remarks>
	/// Hexagonal polar coordinates approximate a circle to a hexagonal ring.
	/// </remarks>
	/// <param name="radius">Hex distance from 0,0.</param>
	/// <param name="angle">Desired polar angle.</param>
	public static int FindPolarIndex(int radius, float angle) {
		return (int)Mathf.Round(angle * radius * 3 / Mathf.PI);
	}
//
//	/// <summary>
//	/// <see cref="Settworks.Hexagons.HexCoord"/> from offset coordinates.
//	/// </summary>
//	/// <remarks>
//	/// Offset coordinates are a common alternative for hexagons, allowing pseudo-square grid operations.
//	/// This conversion assumes an offset of x = q + r/2.
//	/// </remarks>
//	public static HexCoord AtOffset(int x, int y) {
//		return new HexCoord(x - (y>>1), y);
//	}

	/// <summary>
	/// <see cref="Settworks.Hexagons.HexCoord"/> containing a floating-point q,r vector.
	/// </summary>
	/// <remarks>
	/// Hexagonal geometry makes normal rounding inaccurate. If working with floating-point
	/// q,r vectors, use this method to accurately convert them back to
	/// <see cref="Settworks.Hexagons.HexCoord"/>.
	/// </remarks>
	public static HexCoord RoundFromQRVector(Vector2 QRvector) {
		float z = -QRvector.x -QRvector.y;
		int ix = (int)Mathf.Round(QRvector.x);
		int iy = (int)Mathf.Round(QRvector.y);
		int iz = (int)Mathf.Round(z);
		if (ix + iy + iz != 0) {
			float dx = Mathf.Abs(ix - QRvector.x);
			float dy = Mathf.Abs(iy - QRvector.y);
			float dz = Mathf.Abs(iz - z);
			if (dx >= dy && dx >= dz && (-iy-iz != 0))
				ix = -iy-iz;
			else
			if (MathX.NearlyEqual(dy, dz) || dy >= dz)
				iy = -ix-iz;
		}
		return new HexCoord(ix, iy);
	}

	/// <summary>
	/// Convert an x,y vector to a q,r vector.
	/// </summary>
	public static Vector2 VectorXYtoQR(Vector2 XYvector) {
		return XYvector.x*X_QR + XYvector.y*Y_QR;
	}
	
	/// <summary>
	/// Convert a q,r vector to an x,y vector.
	/// </summary>
	public static Vector2 VectorQRtoXY(Vector2 QRvector) {
		return QRvector.x*Q_XY + QRvector.y*R_XY;
	}

	/// <summary>
	/// Get the corners of a QR-space rectangle containing every cell touching an XY-space rectangle.
	/// </summary>
	public static HexCoord[] CartesianRectangleBounds(Vector2 cornerA, Vector2 cornerB) {
		Vector2 min = new Vector2(Mathf.Min(cornerA.x, cornerB.x), Mathf.Min(cornerA.y, cornerB.y));
		Vector2 max = new Vector2(Mathf.Max(cornerA.x, cornerB.x), Mathf.Max(cornerA.y, cornerB.y));
		HexCoord[] results = {
			HexCoord.AtPosition(min),
			HexCoord.AtPosition(max)
		};
		Vector2 pos = results[0].Position();
		if (pos.y - 0.5f >= min.y)
			results[0] += directions[4];
		else if (pos.x >= min.x)
			results[0] += directions[3];
		pos = results[1].Position();
		if (pos.y + 0.5f <= max.y)
			results[1] += directions[1];
		else if (pos.x <= max.x)
			results[1] += directions[0];
		return results;
	}

	public static int[] GetCornerIndiciesSharedWithOther (HexCoord coord, HexCoord otherCoord) {
		var distance = Distance(coord, otherCoord);
		if(distance == 0) {
			return new int[] {0,1,2,3,4,5};
		} else if(distance == 1) {
			List<int> sharedIndicies = new List<int>();
			for(int i = 0; i < 6; i++) {
				if(GetTouchingCornerPointIndex(coord, i, otherCoord) == -1) continue;
				sharedIndicies.Add(i);
			}
			return sharedIndicies.ToArray();
		}
		return null;
	}

	public static int[] GetCornerIndiciesSharedWithOthers (HexCoord coord, params HexCoord[] otherCoords) {
		List<int> sharedIndicies = new List<int>() {0,1,2,3,4,5};
		foreach(var otherCoord in otherCoords) {
			var newShared = GetCornerIndiciesSharedWithOther(coord, otherCoord);
			if(newShared == null) sharedIndicies.Clear();
			else sharedIndicies = newShared.Intersect(sharedIndicies).ToList();
		}
		return sharedIndicies.ToArray();
	}

	// Given a coord and corner index, find the corner index of another coord that shares the same vert.
	public static int GetTouchingCornerPointIndex (HexCoord coord, int coordCornerIndex, HexCoord otherCoord) {
		if(HexCoord.Distance(coord, otherCoord) != 1) return -1;
		int numCorners = 6;
		var cachedCorners = corners;
		
		var coordPosition = coord.Position();
		var otherCoordPosition = otherCoord.Position();

		if(coordCornerIndex < 0 || coordCornerIndex > 5) 
			coordCornerIndex = NormalizeRotationIndex(coordCornerIndex);

		var cornerPoint = coordPosition + cachedCorners[coordCornerIndex];
		for(int otherCornerIndex = 0; otherCornerIndex < numCorners; otherCornerIndex++) {
			var otherCornerPoint = otherCoordPosition + cachedCorners[otherCornerIndex];
			if(Vector2X.SqrDistance(cornerPoint, otherCornerPoint) < 0.1f) {
				return otherCornerIndex;
			}
		}
		return -1;
	}

	// Finds the corner 
	public static int GetBestCornerIndex (HexCoord coord, Vector2 position) {
		var direction = Vector2X.NormalizedDirection(coord.Position(), position);
		
		int index = 0;
		int bestIndex = -1;
		float bestDot = -1f;

		foreach(var corner in HexCoord.CornerVectors()) {
			var dot = Vector2.Dot(corner, direction);
			if(dot > bestDot) {
				bestDot = dot;
				bestIndex = index;
			}
			index++;
		}
		return bestIndex;
	}

	public static HexCoord[] HexCoordsSharingCornerIndex (HexCoord coord, int cornerIndex) {
		cornerIndex = NormalizeRotationIndex(cornerIndex);
		return new HexCoord[2] {coord.Neighbor(cornerIndex+1), coord.Neighbor(cornerIndex+2)};
	}

	// It's often handy to consider where the edge is in relation to the corners, 
	// but this code has corners that go clockwise (this is fine) but directions which go counterclockwise. 
	// I'm not sure why this is, but I don't dare mess with it. This allows conversion between the two.
	// In this model, direction with index 0 is clockwise of 
	public static float ConvertCornerIndexToDirectionIndex (float cornerIndex) {
		return 5-cornerIndex;
	}

	/*
	 * Constants
	 */

	/// <summary>
	/// One sixth of a full rotation (radians).
	/// </summary>
	public static readonly float SEXTANT = Mathf.PI / 3;
	
	/// <summary>
	/// Square root of 3.
	/// </summary>
	public static readonly float SQRT3 = Mathf.Sqrt(3);

	// The directions array. These are private to prevent overwriting elements.
	static readonly HexCoord[] directions = {
		new HexCoord(1, 0),
		new HexCoord(0, 1),
		new HexCoord(-1, 1),
		new HexCoord(-1, 0),
		new HexCoord(0, -1),
		new HexCoord(1, -1)
	};




	public enum Orientation {
		Flat,
		Pointy,
	}
	public static Orientation orientation {
		get {
			return HexCoord.LayoutToOrientation(offsetLayout);
		}
	}

	// Corner locations in XY space. Private for same reason as neighbors.
	static Vector2[] corners {
		get {
			if(orientation == Orientation.Flat) return corners_flat;
			else return corners_pointy;
		}
	}
	// Corner locations in XY space. Private for same reason as neighbors.
	static readonly Vector2[] corners_pointy = {
		HexCornerOffset(Orientation.Pointy, 0),
		HexCornerOffset(Orientation.Pointy, 1),
		HexCornerOffset(Orientation.Pointy, 2),
		HexCornerOffset(Orientation.Pointy, 3),
		HexCornerOffset(Orientation.Pointy, 4),
		HexCornerOffset(Orientation.Pointy, 5),
	};
	static readonly Vector2[] corners_flat = {
		HexCornerOffset(Orientation.Flat, 0),
		HexCornerOffset(Orientation.Flat, 1),
		HexCornerOffset(Orientation.Flat, 2),
		HexCornerOffset(Orientation.Flat, 3),
		HexCornerOffset(Orientation.Flat, 4),
		HexCornerOffset(Orientation.Flat, 5),
	};

	public static Vector2 HexCornerOffset(HexCoord.Orientation orientation, int corner) {
        var angle = 2f * Mathf.PI * ((orientation == HexCoord.Orientation.Flat ? 0 : -0.5f) - corner) / 6f;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

	// Vector transformations between QR and XY space.
	// Private to keep IntelliSense tidy. Safe to make public, but sensible uses are covered above.
	static Vector2 Q_XY {
		get {
			if(orientation == Orientation.Flat) return Q_XY_Flat;
			else return Q_XY_Pointy;
		}
	}
	static Vector2 R_XY {
		get {
			if(orientation == Orientation.Flat) return R_XY_Flat;
			else return R_XY_Pointy;
		}
	}

	static readonly Vector2 Q_XY_Pointy = new Vector2(SQRT3, 0);
	static readonly Vector2 R_XY_Pointy = new Vector2(SQRT3/2, 1.5f);
	static readonly Vector2 Q_XY_Flat = new Vector2(1.5f, SQRT3/2);
	static readonly Vector2 R_XY_Flat = new Vector2(0, SQRT3);

	static Vector2 X_QR {
		get {
			if(orientation == Orientation.Flat) return X_QR_Flat;
			else return X_QR_Pointy;
		}
	}
	static Vector2 Y_QR {
		get {
			if(orientation == Orientation.Flat) return Y_QR_Flat;
			else return Y_QR_Pointy;
		}
	}
	static readonly Vector2 X_QR_Pointy = new Vector2(SQRT3/3, 0);
	static readonly Vector2 Y_QR_Pointy = new Vector2(-1/3f, 2/3f);
	static readonly Vector2 X_QR_Flat = new Vector2(2/3f, -1/3f);
	static readonly Vector2 Y_QR_Flat = new Vector2(0, SQRT3/3);

	public enum Layout {
		// Pointy topped
		OddR,
		EvenR,
		// Flat topped
		OddQ,
		EvenQ
	}
	static Layout _offsetLayout = Layout.OddR;
	public static Layout offsetLayout {
		get {
			return _offsetLayout;
		} set {
			if(_offsetLayout == value) return;
			// Debug.Log(_offsetLayout+" "+value);
			
			_offsetLayout = value;
			if(OnChangeOffsetLayout != null) OnChangeOffsetLayout(_offsetLayout);
		}
	}
	public static Action<Layout> OnChangeOffsetLayout;

	public static HexCoord.Orientation LayoutToOrientation(Layout layout) {
		switch (layout) {
		case Layout.OddR: 
			return HexCoord.Orientation.Pointy;
		case Layout.EvenR: 
			return HexCoord.Orientation.Pointy;
		case Layout.OddQ: 
			return HexCoord.Orientation.Flat;
		default:
			return HexCoord.Orientation.Flat;
		}
	}

	public static HexCoord OffsetToHex(int x, int y) {
		return OffsetToHex(new Point(x, y));
	}
	public static HexCoord OffsetToHex(Point offsetCoord) {
		return OffsetToHex(offsetCoord, offsetLayout);
	}
	public static HexCoord OffsetToHex(Point offsetCoord, Layout offsetLayout) {
		switch (offsetLayout) {
		case Layout.OddR: 
			return OddRToHex(offsetCoord);
		case Layout.EvenR: 
			return EvenRToHex(offsetCoord);
		case Layout.OddQ: 
			return OddQToHex(offsetCoord);
		default:
			return EvenQToHex(offsetCoord);
		}
	}

	public static HexCoord OddRToHex(Point hex) {
		var q = hex.x - (hex.y - (hex.y&1)) / 2;
		var r = hex.y;
		return new HexCoord(q, r);
	}
	static HexCoord EvenRToHex(Point hex) {
		var q = hex.x - (hex.y + (hex.y&1)) / 2;
		var r = hex.y;
		return new HexCoord(q, r);
	}
	static HexCoord OddQToHex(Point hex) {
		var q = hex.x;
		var r = hex.y - (hex.x - (hex.x&1)) / 2;
		return new HexCoord(q, r);
	}
	static HexCoord EvenQToHex(Point hex) {
		var q = hex.x;
		var r = hex.y - (hex.x + (hex.x&1)) / 2;
		return new HexCoord(q, r);
	}


	public Point3 HexToCube() {
		return HexToCube(this);
	}

	public static Point3 HexToCube(HexCoord coord) {
		return new Point3(coord.q, coord.s, coord.r);
	}

	public static HexCoord CubeToHex(Point3 cubeCoord) {
		var q = cubeCoord.x;
		var r = cubeCoord.z;
		return new HexCoord(q, r);
    }

	public Point ToOffset() {
		return HexToOffset(this);
	}

	public static Point HexToOffset(HexCoord coord) {
		return HexToOffset(coord, HexCoord.offsetLayout);
	}

	static Point HexToOffset(HexCoord coord, HexCoord.Layout mode) {
		switch (mode) {
		case Layout.OddR: return ToOddR(coord);
		case Layout.EvenR: return ToEvenR(coord);
		case Layout.OddQ: return ToOddQ(coord);
		default: return ToEvenQ(coord);
		}
	}

	public static Point ToOddR(HexCoord coord) {
		var x = coord.q + (coord.r - (coord.r&1)) / 2;
		var y = coord.r;
		return new Point(x, y);
	}

	public static Point ToEvenR(HexCoord coord) {
		var x = coord.q + (coord.r + (coord.r&1)) / 2;
		var y = coord.r;
		return new Point(x, y);
	}

	public static Point ToOddQ(HexCoord coord) {
		var x = coord.q;
		var y = coord.r + (coord.q - (coord.q&1)) / 2;
		return new Point(x, y);
	}

	public static Point ToEvenQ(HexCoord coord) {
		var x = coord.q;
		var y = coord.r + (coord.q + (coord.q&1)) / 2;
		return new Point(x, y);
	}
	
	public static Vector2 ToOddRInterpolated(Vector2 coord) {
		var x = coord.x + (coord.y - ((coord.y < 0 ? Mathf.CeilToInt(coord.y) : Mathf.FloorToInt(coord.y))&1)) / 2f;
		var y = coord.y;
		return new Vector2(x, y);
	}

	public static Vector2 ToEvenRInterpolated(Vector2 coord) {
		var x = coord.x + (coord.y + ((coord.y < 0 ? Mathf.CeilToInt(coord.y) : Mathf.FloorToInt(coord.y))&1)) / 2f;
		var y = coord.y;
		return new Vector2(x, y);
	}

	public static Vector2 ToOddQInterpolated(Vector2 coord) {
		var x = coord.x;
		var y = coord.y + (coord.x - ((coord.x < 0 ? Mathf.CeilToInt(coord.x) : Mathf.FloorToInt(coord.x))&1)) / 2f;
		return new Vector2(x, y);
	}

	public static Vector2 ToEvenQInterpolated(Vector2 coord) {
		var x = coord.x;
		var y = coord.y + (coord.x + ((coord.x < 0 ? Mathf.CeilToInt(coord.x) : Mathf.FloorToInt(coord.x))&1)) / 2f;
		return new Vector2(x, y);
	}


	public static explicit operator Vector2(HexCoord src) {
		return new Vector2(src.q, src.r);
	}



	#region Operators

	/*
	 * Operators
	 */
	 /*
	// Cast to Vector2 in QR space. Explicit to avoid QR/XY mix-ups.
	public static explicit operator Vector2(HexCoord h)
	{ return new Vector2(h.q, h.r); }
	// +, -, ==, !=
	public static HexCoord operator +(HexCoord a, HexCoord b)
	{ return new HexCoord(a.q+b.q, a.r+b.r); }
	public static HexCoord operator -(HexCoord a, HexCoord b)
	{ return new HexCoord(a.q-b.q, a.r-b.r); }
	public static bool operator ==(HexCoord a, HexCoord b)
	{ return a.q == b.q && a.r == b.r; }
	public static bool operator !=(HexCoord a, HexCoord b)
	{ return a.q != b.q || a.r != b.r; }
	// Mandatory overrides: Equals(), GetHashCode()
	public override bool Equals(object o)
	{ return (o is HexCoord) && this == (HexCoord)o; }
	public override int GetHashCode() {
		return q & (int)0xFFFF | r<<16;
	}*/

	public static HexCoord Add(HexCoord left, HexCoord right){
		return new HexCoord(left.q+right.q, left.r+right.r);
	}

	public static HexCoord Add(HexCoord left, int right){
		return new HexCoord(left.q+right, left.r+right);
	}

	public static HexCoord Add(int left, HexCoord right){
		return new HexCoord(left+right.q, left+right.r);
	}


	public static HexCoord Subtract(HexCoord left, HexCoord right){
		return new HexCoord(left.q-right.q, left.r-right.r);
	}

	public static HexCoord Subtract(HexCoord left, int right){
		return new HexCoord(left.q-right, left.r-right);
	}

	public static HexCoord Subtract(int left, HexCoord right){
		return new HexCoord(left-right.q, left-right.r);
	}


	public static HexCoord Multiply(HexCoord left, HexCoord right){
		return new HexCoord(left.q*right.q, left.r*right.r);
	}

	public static HexCoord Multiply(HexCoord left, int right){
		return new HexCoord(left.q*right, left.r*right);
	}

	public static HexCoord Multiply(int left, HexCoord right){
		return new HexCoord(left*right.q, left*right.r);
	}


	public static HexCoord Divide(HexCoord left, HexCoord right){
		return new HexCoord(left.q/right.q, left.r/right.r);
	}

	public static HexCoord Divide(HexCoord left, int right){
		return new HexCoord(left.q/right, left.r/right);
	}

	public static HexCoord Divide(int left, HexCoord right){
		return new HexCoord(left/right.q, left/right.r);
	}


	public static HexCoord operator +(HexCoord left, HexCoord right) {
		return Add(left, right);
	}

	
	public static HexCoord operator -(HexCoord left) {
		return new HexCoord(-left.q, -left.r);
	}

	public static HexCoord operator -(HexCoord left, HexCoord right) {
		return Subtract(left, right);
	}


	public static HexCoord operator *(HexCoord left, HexCoord right) {
		return Multiply(left, right);
	}

	public static HexCoord operator *(HexCoord left, int right) {
		return Multiply(left, right);
	}


	public static HexCoord operator /(HexCoord left, HexCoord right) {
		return Divide(left, right);
	}
	
	public static HexCoord operator /(HexCoord left, int right) {
		return Divide(left, right);
	}

	public override bool Equals(System.Object obj) {
		return obj is HexCoord && this == (HexCoord)obj;
	}

	public bool Equals(HexCoord p) {
		return q == p.q && r == p.r;
	}

	public override int GetHashCode() {
		unchecked // Overflow is fine, just wrap
		{
			int hash = 27;
			hash = hash * 31 + q.GetHashCode();
			hash = hash * 31 + r.GetHashCode();
			return hash;
		}
	}

	public static bool operator == (HexCoord left, HexCoord right) {
		return left.Equals(right);
	}

	public static bool operator != (HexCoord left, HexCoord right) {
		return !(left == right);
	}
	#endregion


	#region Grids
	// Triangles

//	unordered_set<Hex> map;
//	for (int q = 0; q <= map_size; q++) {
//	    for (int r = 0; r <= map_size - q; r++) {
//	        map.insert(Hex(q, r, -q-r));
//	    }
//	}
//	unordered_set<Hex> map;
//	for (int q = 0; q <= map_size; q++) {
//	    for (int r = map_size - q; r <= map_size; r++) {
//	        map.insert(Hex(q, r, -q-r));
//	    }
//	}

	// Hexagons 

//	unordered_set<Hex> map;
//	for (int q = -map_radius; q <= map_radius; q++) {
//	    int r1 = max(-map_radius, -q - map_radius);
//	    int r2 = min(map_radius, -q + map_radius);
//	    for (int r = r1; r <= r2; r++) {
//	        map.insert(Hex(q, r, -q-r));
//	    }
//	}

	// Rectangles

//	unordered_set<Hex> map;
//	for (int r = 0; r < map_height; r++) {
//	    int r_offset = floor(r/2); // or r>>1
//	    for (int q = -r_offset; q < map_width - r_offset; q++) {
//	        map.insert(Hex(q, r, -q-r));
//	    }
//	}

//	unordered_set<Hex> map;
//	for (int q = 0; q < map_width; q++) {
//	    int q_offset = floor(q/2); // or q>>1
//	    for (int r = -q_offset; r < map_height - q_offset; r++) {
//	        map.insert(Hex(q, r, -q-r));
//	    }
//	}
	#endregion
}
