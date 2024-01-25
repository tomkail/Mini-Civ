using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Shapes;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

public static class TilemapExtensions {
    
    public static T[] GetTiles<T>(this Tilemap tilemap) where T : TileBase {
        List<T> tiles = new List<T>();
        
        for (int y = tilemap.origin.y; y < (tilemap.origin.y + tilemap.size.y); y++) {
            for (int x = tilemap.origin.x; x < (tilemap.origin.x + tilemap.size.x); x++) {
                T tile = tilemap.GetTile<T>(new Vector3Int(x, y, 0));
                if (tile != null) tiles.Add(tile);
            }
        }
        return tiles.ToArray();
    }
    
    public static (Vector3Int position, T tileBase)[] GetTilesAndPositions<T>(this Tilemap tilemap) where T : TileBase {
        List<(Vector3Int, T)> tiles = new List<(Vector3Int, T)>();
        
        for (int y = tilemap.origin.y; y < (tilemap.origin.y + tilemap.size.y); y++) {
            for (int x = tilemap.origin.x; x < (tilemap.origin.x + tilemap.size.x); x++) {
                T tile = tilemap.GetTile<T>(new Vector3Int(x, y, 0));
                if (tile != null) tiles.Add((new Vector3Int(x,y,0), tile));
            }
        }
        return tiles.ToArray();
    }
}

[ExecuteAlways]
public class FogRenderer : ImmediateModeShapeDrawer {
    public WorldSpaceHexGrid worldSpaceHexGrid;
    
    public Color scrollingOverlayColorA;
    public Color scrollingOverlayColorB;
    public float scrollingOverlayLinesDashSize = 1f;
    public float scrollingOverlayLinesDashSpacing = 1f;
    
    [Range(-1,1)]
    public float fogExtrusion = 0;
    [Range(0,1)]
    public float smoothingRadius = 0.3f;
    [Range(0,90)]
    public float smoothingDegPerPoint = 20;
    
    public Quaternion rotation => worldSpaceHexGrid.axis;
    public Matrix4x4 worldToXYMatrix => Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);

    public override void DrawShapes( Camera cam ) {
        using (Draw.Command(cam)) {
            DrawFog();
        }
    }

    public List<HexCoord> pointsToReveal;
    void DrawFog() {
        Draw.PushMatrix();
        Draw.Matrix = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
        
        GameController.Instance.terrainTilemap.RefreshAllTiles();
        var fogTiles = GameController.Instance.fogTilemap.GetTilesAndPositions<FogTile>();
        var revealedAreasDetector = new IslandDetector<HexCoord>(fogTiles.Select(x => ((HexCoord.OffsetToAxial(x.position.x,x.position.y)) )), p => HexCoord.Directions(p), p => fogTiles.Any(x => (HexCoord)x.position == p));
        var revealedIslands = revealedAreasDetector.FindIslands();
        
        // foreach (var island in revealedIslands) CreateFogRevealIsland(island.points);
        CreateFogRevealIsland(pointsToReveal);
        
        Draw.StencilRefID = 1;
        Draw.StencilOpPass = StencilOp.Keep;
        Draw.StencilComp = CompareFunction.NotEqual;
        Draw.ColorMask = ColorWriteMask.All;
        
        Draw.UseDashes = true;
        Draw.DashOffset = Time.time;
        Draw.DashSize = scrollingOverlayLinesDashSize;
        Draw.DashSpacing = scrollingOverlayLinesDashSpacing;
        Draw.DashType = DashType.Angled;
        Draw.DashShapeModifier = -1;
        Draw.DashSpace = DashSpace.Meters;
        Draw.DashSnap = DashSnapping.Off;

        Draw.LineGeometry = LineGeometry.Flat2D;
        Draw.LineEndCaps = LineEndCap.None;
        
        var rect = RectX.CreateEncapsulating(new Vector2(-30, -30), new Vector2(30, 30));
        Draw.Thickness = rect.size.y;
        Draw.ThicknessSpace = ThicknessSpace.Meters;

        Draw.Color = scrollingOverlayColorA;
        Draw.Rectangle(new Vector3(rect.center.x, rect.center.y, 0), new Vector2(rect.size.x, rect.size.y));
        Draw.Color = scrollingOverlayColorB;
        Draw.Line(new Vector3(rect.center.x - rect.size.x * 0.5f, rect.center.y, 0), new Vector3(rect.center.x + rect.size.x * 0.5f, rect.center.y, 0));
        Draw.ResetStyle();
        

        // foreach (var island in revealedIslands) {
            // var outlineCoords = OutlineDetector.GetOutlinePoly(island.points, HexCoord.GetBestCornerIndex, HexCoord.Corner, HexCoord.GetPointsOnRing).ToArray();
            // foreach (var coord in island.points) {
            //     Draw.Disc(coord.Position(), 0.5f);    
            // }
        // }
        // foreach (var cell in gameModel.GetCells()) {
        //     DrawFogTile(cell);
        // }
        Draw.PopMatrix();
    }

    public float scaleFactor = 1;
    Matrix4x4 axialToWorldMatrix2D;
    void CreateFogRevealIsland(List<HexCoord> islandPoints) {
        axialToWorldMatrix2D = Matrix4x4.TRS(worldSpaceHexGrid.hexCoordPositionToWorldMatrix.GetPosition(), Quaternion.identity, worldSpaceHexGrid.hexCoordPositionToWorldMatrix.lossyScale);
        var outline = OutlineDetector.GetOutlinePoly(islandPoints, HexCoord.GetTouchingCornerPointIndex, (coord, i) => axialToWorldMatrix2D.MultiplyPoint3x4(HexCoord.Corner(coord, i)), 6).ToArray();
        // worldSpaceHexGrid.
        // var outline = OutlineDetector.GetOutlinePoly(islandPoints, HexCoord.GetTouchingCornerPointIndex, (coord, i) => worldSpaceHexGrid.GetCornerPosition(coord, i), 6).ToArray();

        var polygon = new Polygon(outline);
        Vector2[] extrudedPoints = Polygon.GetExtruded(polygon, fogExtrusion);
        var smoothedPoints = Polygon.GetSmoothed(extrudedPoints, smoothingRadius, smoothingDegPerPoint);
            
        Draw.Color = Color.black;
            
        Draw.PolygonTriangulation = PolygonTriangulation.EarClipping;
        var polygonPath = new PolygonPath();
        polygonPath.AddPoints(smoothedPoints);
            
        Draw.StencilRefID = 1;
        Draw.StencilOpPass = StencilOp.Replace;
        Draw.StencilComp = CompareFunction.Always;
        Draw.ColorMask = (ColorWriteMask) 0;
            
        Draw.Polygon(polygonPath);
        Draw.ResetStyle();
    }
}
