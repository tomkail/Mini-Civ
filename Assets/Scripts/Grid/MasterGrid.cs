using System.Collections.Generic;
using UnityEngine;


public class MasterGrid : MonoSingleton<MasterGrid> {
    public WorldSpaceHexGrid hexGrid;
    public UnityEngine.Grid grid;
    public HexCoord.Layout layout;

    public static Polygon tilePolygon => new(HexCornerVectors2D());

    public static IEnumerable<Vector2> HexCornerVectors2D(int first = 0, float hexSize = 1) {
		// foreach(var direction in HexCoord.Directions()) {
		// 	var worldVector = CellToWorldVector(direction);
		// 	yield return worldVector.XZ() * 0.5f;
		// }

		for(int corner = 0; corner < 6; corner++) {
			yield return CornerVector2D(corner) * hexSize;
		}
	}

	public static Vector2 CornerVector2D (int corner) {
		// var angle = 2f * Mathf.PI * (0f - corner) / 6f;
		// return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

		return HexCoord.HexCornerOffset(HexCoord.Orientation.Pointy, corner);
	}
}