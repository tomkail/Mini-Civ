using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Shapes;
using UnityEngine.Rendering;
using UnityX.HexGrid;

[ExecuteAlways]
public class GameRenderer : ImmediateModeShapeDrawer {
    public WorldSpaceHexGrid worldSpaceHexGrid;
    public Color grassColor;
    public Color forestColor;
    public Color mountainColor;
    public Color riverColor;
    public Color roadColor;
    public Color cursorFillColor;
    public Color cursorOutlineColor;
    public Texture2D groundTexture;
    public float textureScale = 1;
    public float corner;

    GameModel gameModel => GameController.Instance.gameModel;

    public override void DrawShapes( Camera cam ) {
        if (gameModel == null) return;
        
        // Shapes defaults to CameraEvent.BeforeImageEffects, which never runs on this camera: the Post
        // Processing v2 PostProcessLayer takes over image effects. Draw before post-processing instead.
        using (Draw.Command(cam, CameraEvent.AfterForwardAlpha)) {
            DrawFloor(gameModel);
            DrawOwnership(gameModel);
            DrawCursor(gameModel.cursor);
            DrawMovementPath(GameController.Instance.currentPathPoints);
        }
    }

    void DrawFloor(GameModel gameModel) {
        foreach (var cell in gameModel.GetCells()) {
            var terrainType = cell.terrainType;
            if(terrainType == TerrainType.Grass) Draw.Color = grassColor;
            else if(terrainType == TerrainType.Mountain) Draw.Color = mountainColor;
            else if(terrainType == TerrainType.Forest) Draw.Color = forestColor;
            else if(terrainType == TerrainType.River) Draw.Color = riverColor;
            else if(terrainType == TerrainType.Road) Draw.Color = roadColor;
            
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
        Draw.Matrix = Matrix4x4.TRS(worldSpaceHexGrid.AxialToWorld(coord), worldSpaceHexGrid.XYPlaneRotation(), Vector3.one);
        draw();
        Draw.PopMatrix();
    }

    void DrawPolygonTile(HexCoord coord, Action draw) {
        Draw.PushMatrix();
        Draw.Matrix = Matrix4x4.TRS(worldSpaceHexGrid.AxialToWorld(coord), worldSpaceHexGrid.XYPlaneRotation().Rotate(new Vector3(0,0,30)), Vector3.one);
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

    public Quaternion rotation => worldSpaceHexGrid.XYPlaneRotation();
    public Matrix4x4 worldToXYMatrix => Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);

    public Color arrowColor;
    public float arrowThickness = 1;
    public float arrowHeadThickness = 2;
    public float arrowHeadLength = 2;
    public float arrowHeadPivot = 0.5f;
    public float arrowHeadRadius = 2;

    void DrawMovementPath(List<HexCoord> instanceCurrentPathPoints) {
        Draw.Color = arrowColor;
        if (instanceCurrentPathPoints.Count < 2) {
            Draw.PushMatrix();
            Draw.Matrix = worldToXYMatrix;
            Draw.Disc(0.3f);
            Draw.PopMatrix();
        } else {
            Draw.PushMatrix();
            Draw.Matrix = worldToXYMatrix;
            var polylinePath = new PolylinePath();
            List<Vector2> points2D = new List<Vector2>();
            List<Vector3> points3D = new List<Vector3>();
            Vector2 arrowDir = Vector2.zero;
            foreach (var pathPoint in instanceCurrentPathPoints) {
                var worldPoint = GameController.Instance.hexGrid.AxialToWorld(pathPoint);
                var point = (Vector2)Draw.Matrix.inverse.MultiplyPoint3x4(worldPoint);
                if(points2D.Count == instanceCurrentPathPoints.Count-1) {
                    arrowDir = (point-points2D[^1]);
                    point = points2D.Last() + arrowDir.normalized * (arrowDir.magnitude - (arrowHeadLength * arrowHeadPivot));
                }

                points2D.Add(point);
                points3D.Add(Draw.Matrix.MultiplyPoint3x4(point));
            }
            polylinePath.AddPoints(points2D);
            Draw.PolylineGeometry = PolylineGeometry.Flat2D;
            Draw.Polyline(polylinePath, false, arrowThickness, PolylineJoins.Round);
            Draw.PopMatrix();

            if (instanceCurrentPathPoints.Count > 1) {
                // var arrowDir = GameController.Instance.hexGrid.AxialToWorld(instanceCurrentPathPoints[instanceCurrentPathPoints.Count-1])-GameController.Instance.hexGrid.AxialToWorld(instanceCurrentPathPoints[instanceCurrentPathPoints.Count-2]);
                var arrowRot = Quaternion.Inverse(Quaternion.LookRotation(arrowDir, Vector3.forward)) * worldSpaceHexGrid.XYPlaneRotation();
                ShapesUtils.DrawArrowHeadPolygon(points3D.Last(), arrowRot, arrowHeadThickness, arrowHeadLength, arrowHeadRadius, 16);
            }
        }
        
    }
}