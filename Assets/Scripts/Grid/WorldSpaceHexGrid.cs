using UnityEngine;
using System.Linq;

[RequireComponent(typeof(UnityEngine.Grid))]
public class WorldSpaceHexGrid : MonoSingleton<WorldSpaceHexGrid> {
    public UnityEngine.Grid grid;

    public Quaternion axis {
		get {
			return Quaternion.LookRotation(Vector3.forward, Vector3.up);
		}
	}
	public Vector3 floorNormal {
		get {
			return axis * Vector3.up;
		}
	}
	public Plane floorPlane {
		get {
			return new Plane(floorNormal, transform.position);
		}
	}
    
    public HexCoord LocalToCell (Vector3 localPosition) {
        var offsetPos = grid.LocalToCell(localPosition);
        return HexCoord.OffsetToHex(offsetPos.x, offsetPos.y);
    }
    public HexCoord WorldToCell (Vector3 worldPosition) {
        var offsetPos = grid.WorldToCell(worldPosition);
        return HexCoord.OffsetToHex(offsetPos.x, offsetPos.y);
    }
    public Vector3 FractionalCellToWorld (Vector2 fractionalCell) {
        var offsetPos = Vector2.zero;
		if(HexCoord.orientation == HexCoord.Orientation.Flat) {
			offsetPos = HexCoord.ToOddQInterpolated(fractionalCell);
		} else {
			offsetPos = HexCoord.ToOddRInterpolated(fractionalCell);
		}
        return grid.LocalToWorld(grid.CellToLocalInterpolated(offsetPos));
    }
    public Vector3 FractionalCellToWorldVector (Vector2 fractionalCell) {
		return FractionalCellToWorld(fractionalCell) - FractionalCellToWorld(Vector2.zero);
    }
    public Vector3 CellToWorld (HexCoord cell) {
		var offsetPos = HexCoord.ToOddR(cell);
		return grid.CellToWorld(new Vector3Int(offsetPos.x, offsetPos.y, 0));
    }
    public Vector3 CellToWorldVector (HexCoord cell) {
		return CellToWorld(cell) - CellToWorld(HexCoord.zero);
    }
    public Vector3 CellToLocal (HexCoord cell) {
		var offsetPos = HexCoord.ToOddR(cell);
		return grid.CellToLocal(new Vector3Int(offsetPos.x, offsetPos.y, 0));
    }
    public Vector3 CellToLocalVector (HexCoord cell) {
		return CellToLocal(cell) - CellToLocal(HexCoord.zero);
    }

    public float HexCoordDirectionToDegreesAgainstNormal (HexCoord coord) {
		return Vector2X.Degrees(CellToLocalVector(coord).XZ());
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
		return LocalToCell(rotation * Vector3.forward);
		// var degrees = Vector3X.SignedDegreesAgainstDirection(rotation * Vector3.forward, axis * Vector3.forward, axis * Vector3.up);
		// return HexCoord.AtPosition();
	}

	public Vector3 GetCornerPosition (HexCoord coord, int corner) {
		var position = CellToWorld(coord);
		var cornerPosition = (MasterGrid.Instance.CornerVector2D(corner).ToVector3XZY());
		return position + cornerPosition;
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

	
	public Vector3 GetCornerPositionFloat (HexCoord coord, float corner) {
		corner = Mathf.Repeat(corner, 6);
		float frac = corner % 1;
		int start = Mathf.FloorToInt(corner);
		int end = Mathf.CeilToInt(corner);
		return Vector3.Lerp(GetCornerPosition(coord, start), GetCornerPosition(coord, end), frac);
	}

	public Vector3 GetEdgePosition (HexCoord coord, int edge) {
		var position = CellToWorld(coord);
		return position + CellToWorld(HexCoord.Direction(edge)) * 0.5f;
		// var cornerPosition = (MasterGrid.Instance.CornerVector2D(corner).ToVector3XZY());
		// return position + cornerPosition;
	}
	public Vector3 GetEdgePositionFloat (HexCoord coord, float edge) {
		edge = Mathf.Repeat(edge, 6);
		float frac = edge % 1;
		int start = Mathf.FloorToInt(edge);
		int end = Mathf.CeilToInt(edge);
		return CellToWorld(coord) + Vector3.Lerp(CellToWorld(HexCoord.Direction(start)), CellToWorld(HexCoord.Direction(end)), frac) * 0.5f;
	}
	

	public Vector3 GetPositionOnCoordInCornerDirection (HexCoord coord, float corner, float normalizedDistanceFromCenterTowardsEdge = 1) {
		var centerPos = CellToWorld(coord);
		var edgePos = GetCornerPositionFloat(coord, corner);
		return Vector3.Lerp(centerPos, edgePos, normalizedDistanceFromCenterTowardsEdge);
	}
	public Vector3 GetPositionOnCoordInEdgeDirection (HexCoord coord, float direction, float normalizedDistanceFromCenterTowardsEdge = 1) {
		return GetPositionOnCoordInCornerDirection(coord, DirectionToCorner(direction), normalizedDistanceFromCenterTowardsEdge);
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