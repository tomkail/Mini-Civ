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
    public LevelGenerator levelGenerator;
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
    public override void DrawShapes( Camera cam ) {
        var gameModel = levelGenerator.GenerateLevel();
        using (Draw.Command(cam)) {
            DrawFloor(gameModel);
            DrawFog(gameModel);
            DrawOwnership(gameModel);
            DrawCursor(gameModel.cursor);
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

    void DrawFog(GameModel gameModel) {
        // var revealedAreasDetector = new IslandDetector<HexCoord>(gameModel.GetCells().Select(x => x.coord), p => HexCoord.Directions(p), p => gameModel.GetCell(p).fog == null || gameModel.GetCell(p).fog.revealed);
        // var revealedIslands = revealedAreasDetector.FindIslands();
        //
        // foreach (var island in revealedIslands) {
        //     // var outlineCoords = OutlineDetector.GetOutlinePoly(island.points, HexCoord.GetBestCornerIndex, HexCoord.Corner, HexCoord.GetPointsOnRing).ToArray();
        //     
        //     System.Func<HexCoord, int, Vector2> GetCornerPoint = (HexCoord coord, int corner) => {
        //         Vector3 pos = (HexCoord.CornerVector(corner) + coord.Position()).ToVector3XZY();
        //         pos = (Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0,90,0), Vector3.one).MultiplyPoint(pos));
        //         return pos.XZ();
        //     };
        //     var outline = OutlineDetector.GetOutlinePoly(new List<HexCoord>{HexCoord.zero}, HexCoord.GetTouchingCornerPointIndex, GetCornerPoint, 6).ToArray();
        //
        //     Draw.PolygonTriangulation = PolygonTriangulation.EarClipping;
        //     var polygonPath = new PolygonPath();
        //     polygonPath.AddPoints(outline);
        //     Draw.Polygon(polygonPath);
        // }
        
        // foreach (var cell in gameModel.GetCells()) {
        //     DrawFogTile(cell);
        // }
    }
    void DrawFogTile(GridCellModel cell) {
        if(cell.fog == null || cell.fog.revealed) return;
        DrawPolygonTile(cell.coord, () => {
            Draw.Color = fogColor;
            Draw.RegularPolygon(6);
        });
    }
}