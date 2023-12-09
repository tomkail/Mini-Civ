using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Shapes;
using TMPro;
using UnityEngine.Rendering;

[ExecuteAlways]
public class ShapesHexTest : ImmediateModeShapeDrawer {
    public WorldSpaceHexGrid worldSpaceHexGrid;
    public HexCoord coord;
    public float corner;
    public override void DrawShapes( Camera cam ) {
        using (Draw.Command(cam)) {
            DrawFloor();
        }
    }
    
    void DrawFloor() {
        HexCoord.offsetLayout = HexCoord.Layout.OddR;
        
        // Debug.Log(UnityEngine.Grid.Swizzle(worldSpaceHexGrid.grid.cellSwizzle, Vector3.forward) +" "+ UnityEngine.Grid.Swizzle(worldSpaceHexGrid.grid.cellSwizzle, Vector3.up));
        // Debug.Log(Quaternion.LookRotation(UnityEngine.Grid.Swizzle(worldSpaceHexGrid.grid.cellSwizzle, Vector3.forward), UnityEngine.Grid.Swizzle(worldSpaceHexGrid.grid.cellSwizzle, Vector3.up)));
        var polygonPath = new PolygonPath();
        polygonPath.AddPoints(MasterGrid.HexCornerVectors2D());
        
        Draw.PushMatrix();
        Draw.Matrix = Matrix4x4.TRS(worldSpaceHexGrid.AxialToWorld(coord), worldSpaceHexGrid.axis, Vector3.one);
        Draw.Polygon(polygonPath);
        Draw.Color = Color.black;
        Draw.Line(Vector3.zero, worldSpaceHexGrid.GetWorldPositionOnCoordInEdgeDirection(HexCoord.zero, corner));
        Draw.PopMatrix();
        
        foreach (var axial in HexCoord.GetPointsInRing(0, 3)) {
            Draw.PushMatrix();
            Draw.Matrix = Matrix4x4.TRS(worldSpaceHexGrid.AxialToWorld(axial), worldSpaceHexGrid.axis, Vector3.one);
            
            Draw.Color = Color.white.WithAlpha(0.5f);
            Draw.Polygon(polygonPath);
            Draw.Color = Color.black;

            
            var offsetCoord = HexCoord.AxialToOffset(axial);
            if (offsetCoord == new Point(0, 1)) {
            //     var x = axial.q + (axial.r + (axial.r&1)) / 2;
            //     var x2 = axial.q + (axial.r - (axial.r&1)) / 2;
            //     Debug.Log(x+" "+x2);
            }
            Debug.Assert(HexCoord.AxialToOffset(worldSpaceHexGrid.WorldToAxial(Draw.Matrix.GetPosition())) == new Point(offsetCoord.x, offsetCoord.y));
            Debug.Assert(axial == worldSpaceHexGrid.WorldToAxial(Draw.Matrix.GetPosition()));
            
            var debugText = "Axial: "+axial.ToString();
            debugText += $"\nOffset ({HexCoord.offsetLayout}) Pos: {offsetCoord}";
            debugText += "\nWorld Pos: "+Draw.Matrix.GetPosition();
            Draw.Text(Vector3.zero, Quaternion.identity, debugText);

            Draw.PopMatrix();
            for (int i = 0; i < 6; i++) {
                var cornerPosition = worldSpaceHexGrid.GetWorldPositionOnCoordInCornerDirection(axial, i, 0.8f);
                Draw.PushMatrix();
                Draw.Matrix = Matrix4x4.TRS(cornerPosition, worldSpaceHexGrid.axis, Vector3.one);
                
                // var cornerVector = HexCoord.CornerVector(coord, i);
                Draw.Ring(Vector3.zero, 0.1f, 0.025f);
                Draw.Text(Vector3.zero, Quaternion.identity, i.ToString());
                
                Draw.PopMatrix();
            }
        }
        
        
    //     Draw.Color = ((cell.point.x + cell.point.y) % 2) == 0 ? settings.floorColorA : settings.floorColorB;
        //     // var rect = gameController.gridRenderer.cellCenter.GridToWorldRect(RectX.CreateFromCenter(cell.point, Vector2.one));
        //     // Draw.Quad(rect[0], rect[1], rect[2], rect[3]);
        // Draw.Texture(settings.floorTextures.Random(), new Rect(0, 0, 1, 1));
        Draw.ResetMatrix();
        Draw.ResetStyle();
    }
}