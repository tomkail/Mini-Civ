using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Shapes;
using TMPro;
using UnityEngine.Rendering;

[ExecuteAlways]
public class GameRenderer : ImmediateModeShapeDrawer {
    public WorldSpaceHexGrid worldSpaceHexGrid;
    public Color grassColor;
    public Color forestColor;
    public Color mountainColor;
    public Color riverColor;
    public Color roadColor;
    public Color fogColor;
    public Color cursorFillColor;
    public Color cursorOutlineColor;
    public Texture2D groundTexture;
    public float textureScale = 1;
    public float corner;

    GameModel gameModel => GameController.Instance.gameModel;

    public bool generateEachFrame;
    public override void DrawShapes( Camera cam ) {
        if (gameModel == null) return;
        
        using (Draw.Command(cam)) {
            DrawFloor(gameModel);
            DrawFog(gameModel);
            DrawOwnership(gameModel);
            DrawCursor(gameModel.cursor);
            DrawMovementPath(GameController.Instance.currentPathPoints);
        }
    }

    void DrawFloor(GameModel gameModel) {
        foreach (var cell in gameModel.GetCells()) {
            if(cell.terrain.type == TerrainType.Grass) Draw.Color = grassColor;
            else if(cell.terrain.type == TerrainType.Mountain) Draw.Color = mountainColor;
            else if(cell.terrain.type == TerrainType.Forest) Draw.Color = forestColor;
            else if(cell.terrain.type == TerrainType.River) Draw.Color = riverColor;
            else if(cell.terrain.type == TerrainType.Road) Draw.Color = roadColor;
            
            DrawPolygonTile(cell.coord, () => {
                Draw.RegularPolygon(6);
            });
            DrawTextureTile(cell.coord, () => {
                Draw.Texture(groundTexture, RectX.CreateFromCenter(Vector3.zero, Vector2.one * textureScale));
            });
        }
    }

    void DrawTextureTile(HexCoord coord, Action draw) {
        Draw.PushMatrix();
        Draw.Matrix = Matrix4x4.TRS(worldSpaceHexGrid.CellToWorld(coord), worldSpaceHexGrid.axis.Rotate(new Vector3(90, 0, 0)), Vector3.one);
        draw();
        Draw.PopMatrix();
    }

    void DrawPolygonTile(HexCoord coord, Action draw) {
        Draw.PushMatrix();
        Draw.Matrix = Matrix4x4.TRS(worldSpaceHexGrid.CellToWorld(coord), worldSpaceHexGrid.axis.Rotate(new Vector3(90, 0, 0)).Rotate(new Vector3(0,0,30)), Vector3.one);
        draw();
        Draw.PopMatrix();
    }

    void DrawCursor(GameCursorModel cursorModel) {
        DrawPolygonTile(cursorModel.gridPoint, () => {
            Draw.Color = cursorFillColor;
            Draw.RegularPolygon(6);
            Draw.Color = cursorOutlineColor;
            Draw.RegularPolygonBorder(6, 1, 0.15f);
        });
    }

    void DrawOwnership(GameModel gameModel) {
        
    }

    public Quaternion rotation => worldSpaceHexGrid.axis * Quaternion.Euler(90, 0, 0);
    
    void DrawFog(GameModel gameModel) {
        Draw.PushMatrix();
        Draw.Matrix = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
        
        var revealedAreasDetector = new IslandDetector<HexCoord>(gameModel.GetCells().Select(x => x.coord), p => HexCoord.Directions(p), p => gameModel.GetCell(p).onGrid && (gameModel.GetCell(p).fog == null || gameModel.GetCell(p).fog.revealed));
        var revealedIslands = revealedAreasDetector.FindIslands();
        
        foreach (var island in revealedIslands) {
            // var outlineCoords = OutlineDetector.GetOutlinePoly(island.points, HexCoord.GetBestCornerIndex, HexCoord.Corner, HexCoord.GetPointsOnRing).ToArray();
            
            System.Func<HexCoord, int, Vector2> GetCornerPoint = (HexCoord coord, int corner) => {
                Vector3 pos = (HexCoord.CornerVector(corner) + coord.Position()).ToVector3XZY();
                pos = (Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0,90,0), Vector3.one).MultiplyPoint(pos));
                return pos.XZ();
            };
            var outline = OutlineDetector.GetOutlinePoly(island.points, HexCoord.GetTouchingCornerPointIndex, HexCoord.Corner, 6).ToArray();
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
            
        }
        
        
        Draw.StencilRefID = 1;
        Draw.StencilOpPass = StencilOp.Keep;
        Draw.StencilComp = CompareFunction.NotEqual;
        Draw.ColorMask = ColorWriteMask.All;
        
        Draw.UseDashes = true;
        Draw.DashOffset = Time.time;
        Draw.DashSize = 0.5f;
        Draw.DashSpacing = 0.5f;
        Draw.DashType = DashType.Angled;
        Draw.DashShapeModifier = -1;
        Draw.DashSpace = DashSpace.Meters;
        Draw.DashSnap = DashSnapping.Off;

        Draw.LineGeometry = LineGeometry.Flat2D;
        Draw.LineEndCaps = LineEndCap.None;
        
        var rect = RectX.CreateEncapsulating(new Vector2(-10, -10), new Vector2(10, 10));
        Draw.Thickness = rect.size.y;
        Draw.ThicknessSpace = ThicknessSpace.Meters;

        Draw.Color = scrollingOverlayColorA;
        Draw.Rectangle(new Vector3(rect.center.x, rect.center.y, 0), new Vector2(rect.size.x, rect.size.y));
        Draw.Color = scrollingOverlayColorB;
        Draw.Line(new Vector3(rect.center.x - rect.size.x * 0.5f, rect.center.y, 0), new Vector3(rect.center.x + rect.size.x * 0.5f, rect.center.y, 0));
        Draw.ResetStyle();
        
        Draw.PopMatrix();
        
        // foreach (var cell in gameModel.GetCells()) {
        //     DrawFogTile(cell);
        // }
    }

    public Color scrollingOverlayColorA;
    public Color scrollingOverlayColorB;
    
    [Range(-1,1)]
    public float fogExtrusion = 0;
    [Range(0,1)]
    public float smoothingRadius = 10;
    [Range(0,90)]
    public float smoothingDegPerPoint = 5;
    
    void DrawFogTile(GridCellModel cell) {
        if(cell.fog == null || cell.fog.revealed) return;
        DrawPolygonTile(cell.coord, () => {
            Draw.Color = fogColor;
            Draw.RegularPolygon(6);
        });
    }


    public Color arrowColor;
    public float arrowThickness = 1;
    public float arrowHeadThickness = 2;
    public float arrowHeadLength = 2;
    public float arrowHeadPivot = 0.5f;
    public float arrowHeadRadius = 2;

    void DrawMovementPath(List<HexCoord> instanceCurrentPathPoints) {
        Draw.Color = arrowColor;
        
        Draw.PushMatrix();
        Draw.Matrix = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
        var polylinePath = new PolylinePath();
        List<Vector2> points = new List<Vector2>();
        Vector2 arrowDir = Vector2.zero;
        foreach (var pathPoint in instanceCurrentPathPoints) {
            var point = (Vector2)Draw.Matrix.inverse.MultiplyPoint3x4(GameController.Instance.hexGrid.CellToWorld(pathPoint));
            if(points.Count == instanceCurrentPathPoints.Count-1) {
                arrowDir = (point-points[^1]);
                point = points.Last() + arrowDir.normalized * (arrowDir.magnitude - (arrowHeadLength * arrowHeadPivot));
            }
            points.Add(point);
        }
        polylinePath.AddPoints(points);
        Draw.PolylineGeometry = PolylineGeometry.Flat2D;
        Draw.Polyline(polylinePath, false, arrowThickness, PolylineJoins.Round);

        if (instanceCurrentPathPoints.Count > 1) {
            // var arrowDir = GameController.Instance.hexGrid.CellToWorld(instanceCurrentPathPoints[instanceCurrentPathPoints.Count-1])-GameController.Instance.hexGrid.CellToWorld(instanceCurrentPathPoints[instanceCurrentPathPoints.Count-2]);
            var arrowRot = Quaternion.LookRotation(arrowDir, worldSpaceHexGrid.floorNormal);
            ShapesUtils.DrawArrowHeadPolygon(points.Last(), Qua, arrowHeadThickness, arrowHeadLength, arrowHeadRadius, 16);
        }
        Draw.PopMatrix();
    }
}