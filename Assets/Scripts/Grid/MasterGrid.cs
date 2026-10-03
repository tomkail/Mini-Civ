using System.Collections.Generic;
using UnityEngine;
using UnityX.HexGrid;


public class MasterGrid : MonoSingleton<MasterGrid> {
    public WorldSpaceHexGrid hexGrid;
    public UnityEngine.Grid grid;
    public HexCoord.Layout layout;

    public static Polygon tilePolygon => new(HexCornerVectors2D());

    // Pointy-top corner offsets of a unit hex (corner 0 at 30 degrees below +X, then clockwise).
    public static IEnumerable<Vector2> HexCornerVectors2D(int first = 0, float hexSize = 1) => HexCoord.CornerVectors(first, hexSize);

    public static Vector2 CornerVector2D (int corner) => HexCoord.CornerVector(corner);
}