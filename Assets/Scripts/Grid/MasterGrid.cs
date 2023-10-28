using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityX.Geometry;


public class MasterGrid : MonoSingleton<MasterGrid> {
    public WorldSpaceHexGrid hexGrid;
    public UnityEngine.Grid grid;
    public HexCoord.Layout layout;

    void OnValidate () {
    	HexCoord.offsetLayout = layout;
    }

    public Polygon tilePolygon {
		get {
			return new Polygon(HexCornerVectors2D());
		}
	}
    
    public IEnumerable<Vector2> HexCornerVectors2D(int first = 0, float hexSize = 1) {
		// foreach(var direction in HexCoord.Directions()) {
		// 	var worldVector = CellToWorldVector(direction);
		// 	yield return worldVector.XZ() * 0.5f;
		// }

		for(int corner = 0; corner < 6; corner++) {
			yield return CornerVector2D(corner) * hexSize;
		}
	}

	public Vector2 CornerVector2D (int corner) {
		var angle = 2f * Mathf.PI * (0f - corner) / 6f;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
		// var angle = 2f * Mathf.PI * (-0.5f - corner) / 6f;
        // return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

		// var angle = 2f * Mathf.PI * ((orientation == HexCoord.Orientation.Flat ? 0 : -0.5f) - corner) / 6f;
        // return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

		// for(int corner = 0; corner < 6; corner++) {
		// 	var angle = 2f * Mathf.PI * (/*(orientation == HexCoord.Orientation.Flat ? 0 : -0.5f)*/ - corner) / 6f;
		// 	yield return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
		// }
	}
}