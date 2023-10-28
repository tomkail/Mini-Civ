using UnityEngine;

[System.Serializable]
public struct HexCoordVert {
    public int x, y;
    public HexCoordVert(int _x, int _y) {
		x = _x;
		y = _y;
	}
	public HexCoordVert(Point point) {
		x = point.x;
		y = point.y;
	}

	public static bool Valid (HexCoord cornerPointTriad1, HexCoord cornerPointTriad2, HexCoord cornerPointTriad3) {
		var sharedIndicies = HexCoord.GetCornerIndiciesSharedWithOthers(cornerPointTriad1, cornerPointTriad2, cornerPointTriad3);
        return sharedIndicies.Length == 1;
	}

	public HexCoordVert (HexCoord cornerPointTriad1, HexCoord cornerPointTriad2, HexCoord cornerPointTriad3) {
		// Debug.Assert(HexCoord.Distance(cornerPointTriad1, cornerPointTriad2) == 1 && HexCoord.Distance(cornerPointTriad1, cornerPointTriad3) == 1 && HexCoord.Distance(cornerPointTriad2, cornerPointTriad3) == 1);
		var sharedIndicies = HexCoord.GetCornerIndiciesSharedWithOthers(cornerPointTriad1, cornerPointTriad2, cornerPointTriad3);
        if(sharedIndicies.Length == 1) {
            var corner = cornerPointTriad1.Corner(sharedIndicies[0]);
            var point = RoundFrom(PositionToHexCornerPoint(corner));
            x = point.x;
            y = point.y;
        } else {
			x = 0; y = 0;
			Debug.LogError("Points not adjacent "+cornerPointTriad1+" "+cornerPointTriad2+" "+cornerPointTriad3);
		}
	}
	
	public HexCoordVert (HexCoord hex, int cornerIndex) {
        var position = hex.Corner(cornerIndex);
		var point = RoundFrom(PositionToHexCornerPoint(position));
		x = point.x;
		y = point.y;
	}

    public static HexCoordVert RoundFrom (Vector2 pos) {
        return new HexCoordVert(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
    }


	public Vector2 Position () {
		return HexCornerPointToPosition(new Vector2(x,y));
	}

	public static Vector2 HexCornerPointToPosition (Vector2 point) {
		// Vector2 cellSize;
		// pos = Vector2.zero;
		// if(HexCoord.orientation == HexCoord.Orientation.Flat) {
		// 	// cellSize = new Vector2(2f/4f, HexCoord.SQRT3*0.5f);
		// 	// offset = new Vector2(-cellSize.x * 2, -cellSize.y);
		// } else {
		// 	pos += point.x * new Vector2(HexCoord.SQRT3 * 0.5f, 0);
		// 	pos += Mathf.FloorToInt(point.y * 0.5f) * new Vector2(0, 1);
		// 	if(point.x % 2 == 0) {
		// 		pos += new Vector2(0, 1 * 0.5f);
		// 	}
		// }
		Vector2 pos = point.x*Q_XY + point.y*R_XY;
		return pos + offset;
	}

    public static Vector2 PositionToHexCornerPoint (Vector2 position) {
		position -= offset;
        Vector2 vPoint = position.x*X_QR + position.y*Y_QR;
        // This SHOULD round in a special way.
        return new Point(vPoint);
	}

    static Vector2 offset {
        get {
            if(HexCoord.orientation == HexCoord.Orientation.Flat) {
                return new Vector2(-0.25f,-HexCoord.SQRT3 * 0.75f);
            } else {
                return new Vector2(-HexCoord.SQRT3 * 0.5f, -1f);
            }    
        }
    }
	
    static Vector2 Q_XY {
		get {
			if(HexCoord.orientation == HexCoord.Orientation.Flat) return Q_XY_Flat;
			else return Q_XY_Pointy;
		}
	}
	static Vector2 R_XY {
		get {
			if(HexCoord.orientation == HexCoord.Orientation.Flat) return R_XY_Flat;
			else return R_XY_Pointy;
		}
	}
	static readonly Vector2 Q_XY_Pointy = new Vector2(HexCoord.SQRT3/2, 0);
	static readonly Vector2 R_XY_Pointy = new Vector2(0, 0.5f);
	static readonly Vector2 Q_XY_Flat = new Vector2(0.75f, HexCoord.SQRT3/4);
	static readonly Vector2 R_XY_Flat = new Vector2(-HexCoord.SQRT3/8, 0.75f/2);

    static Vector2 X_QR {
		get {
			if(HexCoord.orientation == HexCoord.Orientation.Flat) return X_QR_Flat;
			else return X_QR_Pointy;
		}
	}
	static Vector2 Y_QR {
		get {
			if(HexCoord.orientation == HexCoord.Orientation.Flat) return Y_QR_Flat;
			else return Y_QR_Pointy;
		}
	}
	public static Vector2 X_QR_Pointy = new Vector2(HexCoord.SQRT3/1.5f, 0);
	public static Vector2 Y_QR_Pointy = new Vector2(0, 2);
	public static Vector2 X_QR_Flat = new Vector2(1, -HexCoord.SQRT3/1.5f);
	public static Vector2 Y_QR_Flat = new Vector2(HexCoord.SQRT3/3f, 2);






    public static Point ToPoint(HexCoordVert point) {
		return new Point(point.x, point.y);
	}

    public Point ToPoint() {
		return ToPoint(this);
	}

    public static HexCoordVert FromPoint(Point vector) {
		return new HexCoordVert(vector.x, vector.y);
	}

    public override bool Equals(System.Object obj) {
		// If parameter is null return false.
		if (obj == null) {
			return false;
		}

		// If parameter cannot be cast to Point return false.
		Point p = (Point)obj;
		if ((System.Object)p == null) {
			return false;
		}

		// Return true if the fields match:
		return Equals(p);
	}

	public bool Equals(Point p) {
		// If parameter is null return false:
		if ((object)p == null) {
			return false;
		}

		// Return true if the fields match:
		return (x == p.x) && (y == p.y);
	}

	public override int GetHashCode() {
		unchecked // Overflow is fine, just wrap
		{
			int hash = 27;
			hash = hash * x.GetHashCode();
			hash = hash * y.GetHashCode();
			return hash;
		}
	}

	public static bool operator == (HexCoordVert left, HexCoordVert right) {
		if (System.Object.ReferenceEquals(left, right))
		{
			return true;
		}

		// If one is null, but not both, return false.
		if (((object)left == null) || ((object)right == null))
		{
			return false;
		}

		return left.Equals(right);
	}

	public static bool operator != (HexCoordVert left, HexCoordVert right) {
		return !(left == right);
	}

	public static HexCoordVert operator +(HexCoordVert left, HexCoordVert right) {
		return Point.Add(left, right);
	}

	public static HexCoordVert operator -(HexCoordVert left) {
		return new HexCoordVert(-left.x, -left.y);
	}

	public static HexCoordVert operator -(HexCoordVert left, HexCoordVert right) {
		return Point.Subtract(left, right);
	}


	public static HexCoordVert operator *(HexCoordVert left, HexCoordVert right) {
		return Point.Multiply(left, right);
	}


	public static HexCoordVert operator /(HexCoordVert left, HexCoordVert right) {
		return Point.Divide(left, right);
	}

	public static implicit operator HexCoordVert(Point src) {
		return FromPoint(src);
	}
	
	public static implicit operator Point(HexCoordVert src) {
		return src.ToPoint();
	}




	public override string ToString() {
		return "(" + x + ", " + y+")";
	}
}
