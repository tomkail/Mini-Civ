using UnityEngine;
using System.Linq;

[RequireComponent(typeof(UnityEngine.Grid))]
public class WorldSpaceHexGrid : MonoSingleton<WorldSpaceHexGrid> {
    public UnityEngine.Grid grid;

    public Quaternion gridSwizzleRotation => Quaternion.LookRotation(UnityEngine.Grid.Swizzle(grid.cellSwizzle, Vector3.forward), UnityEngine.Grid.Swizzle(grid.cellSwizzle, Vector3.up));
    public Quaternion axis => gridSwizzleRotation * transform.rotation;
    // public Quaternion axis => Quaternion.LookRotation(Vector3.forward, Vector3.up);

    public Vector3 floorNormal => axis * Vector3.forward;

    public Plane floorPlane => new(floorNormal, transform.position);

	
    // NOTES ON UNITY'S HEX GRID SYSTEM
    // Unity's coordinate system is Offset OddR; which is a pointy orientation
    // The various Swizzle functions rotate their grid 90 degrees in various directions, which can give the illusion of a flat orientation
    // If you need a flat orientation you should rotate the grid manually or use the Swizzle functions; but leave the as Offset OddR, since that's what Unity's LocalToCell/WorldToCell return.
    void OnValidate () {
	    // Unity uses OddR (pointy) or EvenQ (flat)
	    // If we want to convert between axial and offset automatically we need to set the correct layout in HexCoord
	    HexCoord.offsetLayout = HexCoord.Layout.OddR;
    }
    
    public HexCoord LocalToAxial (Vector3 localPosition) {
	    var offsetPos = grid.LocalToCell(localPosition);
        return HexCoord.OffsetToAxial(offsetPos.x, offsetPos.y);
    }
    public HexCoord WorldToAxial (Vector3 worldPosition) {
        var offsetPos = grid.WorldToCell(worldPosition);
        return HexCoord.OffsetToAxial(offsetPos.x, offsetPos.y);
    }
    public Vector3 AxialToWorldInterpolated (Vector2 fractionalCell) {
        var offsetPos = HexCoord.ToOddRInterpolated(fractionalCell);
        return grid.LocalToWorld(grid.CellToLocalInterpolated(offsetPos));
    }
    public Vector3 AxialToWorldVectorInterpolated (Vector2 fractionalCell) {
		return AxialToWorldInterpolated(fractionalCell) - AxialToWorldInterpolated(Vector2.zero);
    }
    public Vector3 AxialToWorld (HexCoord coord) {
		var offsetPos = HexCoord.AxialToOffset(coord);
		return grid.CellToWorld(new Vector3Int(offsetPos.x, offsetPos.y, 0));
    }
    public Vector3 AxialToWorldVector (HexCoord coord) {
		return AxialToWorld(coord) - AxialToWorld(HexCoord.zero);
    }
    public Vector3 AxialToLocal (HexCoord coord) {
	    var offsetPos = HexCoord.AxialToOffset(coord);
		return grid.CellToLocal(new Vector3Int(offsetPos.x, offsetPos.y, 0));
    }
    public Vector3 AxialToLocalVector (HexCoord coord) {
		return AxialToLocal(coord) - AxialToLocal(HexCoord.zero);
    }

    public float HexCoordDirectionToDegreesAgainstNormal (HexCoord coord) {
		// return Vector2X.Degrees(AxialToLocalVector(coord).XZ());
		return Vector2X.Degrees(Quaternion.Inverse(gridSwizzleRotation) * AxialToLocalVector(coord));
	}
    public Quaternion HexCoordDirectionIndexToRotation (int directionIndex) {
		return HexCoordDirectionToRotation(HexCoord.Direction(directionIndex));
	}
    public Quaternion HexCoordDirectionToRotation (HexCoord coord) {
		return DegreesToRotation(HexCoordDirectionToDegreesAgainstNormal(coord));
	}

	public Quaternion DegreesToRotation (float degrees) {
		return axis.Rotate(Vector3.up * degrees);
	}

	public HexCoord RotationToHexCoordDirection (Quaternion rotation) {
		return LocalToAxial(rotation * Vector3.forward);
		// var degrees = Vector3X.SignedDegreesAgainstDirection(rotation * Vector3.forward, axis * Vector3.forward, axis * Vector3.up);
		// return HexCoord.AtPosition();
	}
	
	
	
	// public Quaternion Rotation () {
	// 	return Rotation(HexCoord.ClosestDirectionIndex(this));
	// }
	// public static Quaternion Rotation (int directionIndex) {
	// 	Quaternion rotation = Quaternion.LookRotation(Vector3.forward, HexCoord.Direction(directionIndex).DirectionVector());
	// 	if(HexCoord.orientation == HexCoord.Orientation.Flat) rotation = rotation.Rotate(new Vector3(0,0,30));
	// 	// HexCoord.orientation == HexCoord.Orientation.Flat ? 30 : 0
	// 	return rotation;
	// }

	public Vector3 GetCornerPosition (HexCoord coord, int corner) {
		var position = AxialToWorld(coord);
		// var cornerPosition = (MasterGrid.CornerVector2D(corner).ToVector3XZY());
		var cornerPosition = axis * HexCoord.HexCornerOffset(HexCoord.orientation, corner);
		return position + cornerPosition;
	}
	
	public Vector3 GetCornerPositionFloat (HexCoord coord, float corner) {
		corner = Mathf.Repeat(corner, 6);
		float frac = corner % 1;
		if (frac == 0) return GetCornerPosition(coord, Mathf.RoundToInt(corner));
		int start = Mathf.FloorToInt(corner);
		int end = Mathf.CeilToInt(corner);
		return Vector3.Lerp(GetCornerPosition(coord, start), GetCornerPosition(coord, end), frac);
	}

	public Vector3 GetEdgePosition (HexCoord coord, int edge) {
		var position = AxialToWorld(coord);
		return position + AxialToWorld(HexCoord.Direction(edge)) * 0.5f;
		// var cornerPosition = (MasterGrid.Instance.CornerVector2D(corner).ToVector3XZY());
		// return position + cornerPosition;
	}
	public Vector3 GetEdgePositionFloat (HexCoord coord, float edge) {
		edge = Mathf.Repeat(edge, 6);
		float frac = edge % 1;
		int start = Mathf.FloorToInt(edge);
		int end = Mathf.CeilToInt(edge);
		return AxialToWorld(coord) + Vector3.Lerp(AxialToWorld(HexCoord.Direction(start)), AxialToWorld(HexCoord.Direction(end)), frac) * 0.5f;
	}
	
	public Vector3 GetWorldPositionOnCoordInCornerDirection (HexCoord coord, float corner, float normalizedDistanceFromCenterTowardsEdge = 1) {
		var centerPos = AxialToWorld(coord);
		var edgePos = GetCornerPositionFloat(coord, corner);
		return Vector3.Lerp(centerPos, edgePos, normalizedDistanceFromCenterTowardsEdge);
	}
	public Vector3 GetWorldPositionOnCoordInEdgeDirection (HexCoord coord, float direction, float normalizedDistanceFromCenterTowardsEdge = 1) {
		return GetWorldPositionOnCoordInCornerDirection(coord, DirectionToCorner(direction), normalizedDistanceFromCenterTowardsEdge);
	}



	public Vector3 GetRayHitPoint (Ray ray) {
		float distance;
		floorPlane.Raycast(ray, out distance);
		return ray.GetPoint(distance);
	}

	

	public static float DirectionToCorner (float direction) {
		return direction-1.5f;
		// 4.5f-direction;
	}
	// public float EdgeToDirection (float edge) {
		
	// }
}