using Shapes;
using UnityEngine;

public static class ShapesUtils {
    public static void DrawArrowPolyLine(Vector3 startPosition, Vector3 endPosition, Vector3 normal, float arrowBodyWidth, float arrowHeadWidth, float arrowHeadLength, float arrowHeadRadius, float arrowHeadPointsPerTurn) {
        Quaternion rotation = Quaternion.LookRotation((endPosition - startPosition).normalized, normal).Rotate(new Vector3(90, 0,0));
        
        Draw.PolygonTriangulation = PolygonTriangulation.EarClipping;
        
        Draw.PushMatrix();
        Draw.Matrix = Matrix4x4.TRS(startPosition, rotation, Vector3.one);
        
        
        Vector2 startPos2D = Draw.Matrix.inverse.MultiplyPoint3x4(startPosition);
        Vector2 endPos2D = Draw.Matrix.inverse.MultiplyPoint3x4(endPosition);
        Vector2 direction = (endPos2D - startPos2D).normalized;
        var bodyWidthVector = Vector2.right * arrowBodyWidth * 0.5f;
        var arrowWidthVector = Vector2.right * arrowHeadWidth * 0.5f;
        
        var bodyBottomLeft = startPos2D - bodyWidthVector;
        var bodyTopLeft = endPos2D - direction * arrowHeadLength - bodyWidthVector;
        var arrowHeadBottomLeft = endPos2D - direction * arrowHeadLength - arrowWidthVector;
        var arrowHeadBottomRight = endPos2D - direction * arrowHeadLength + arrowWidthVector;
        var bodyTopRight = endPos2D - direction * arrowHeadLength + bodyWidthVector;
        var bodyBottomRight = startPos2D + bodyWidthVector;
        
        var linePath = new PolylinePath();
        linePath.AddPoint(startPos2D);
        linePath.ArcTo(bodyBottomLeft, bodyTopLeft, arrowHeadRadius, arrowHeadPointsPerTurn);
        linePath.AddPoint(bodyTopLeft);
        linePath.ArcTo(arrowHeadBottomLeft, endPos2D, arrowHeadRadius, arrowHeadPointsPerTurn);
        linePath.ArcTo(endPos2D, arrowHeadBottomRight, arrowHeadRadius, arrowHeadPointsPerTurn);
        linePath.ArcTo(arrowHeadBottomRight, bodyTopRight, arrowHeadRadius, arrowHeadPointsPerTurn);
        linePath.AddPoint(bodyTopRight);
        linePath.ArcTo(bodyBottomRight, startPos2D, arrowHeadRadius, arrowHeadPointsPerTurn);
        linePath.AddPoint(startPos2D);
        
        Draw.Polyline(linePath);
        Draw.PopMatrix();
    }

    public static void DrawArrowHeadPolygon(Vector3 position, Quaternion rotation, float arrowHeadWidth, float arrowHeadLength, float arrowHeadRadius, float arrowHeadPointsPerTurn) {
        Draw.PolygonTriangulation = PolygonTriangulation.EarClipping;
        
        Draw.PushMatrix();
        Draw.Matrix = Matrix4x4.TRS(position, rotation, Vector3.one);


        Vector2 arrowBase = Vector2.zero;
        Vector2 arrowTip = arrowBase + Vector2.up * arrowHeadLength; 
        var arrowWidthVector = Vector2.right * arrowHeadWidth * 0.5f;
        
        var arrowHeadBottomLeft = arrowBase - arrowWidthVector;
        var arrowHeadBottomRight = arrowBase + arrowWidthVector;
        
        var gonPath = new PolygonPath();
        gonPath.AddPoint(arrowBase);
        gonPath.ArcTo(arrowHeadBottomLeft, arrowTip, arrowHeadRadius, arrowHeadPointsPerTurn);
        gonPath.ArcTo(arrowTip, arrowHeadBottomRight, arrowHeadRadius, arrowHeadPointsPerTurn);
        gonPath.ArcTo(arrowHeadBottomRight, arrowBase, arrowHeadRadius, arrowHeadPointsPerTurn);
        // gonPath.AddPoint(endPos2D);

        Draw.Polygon(gonPath);
        Draw.PopMatrix();
    }

    public static void DrawArrowPolygon(Vector3 startPosition, Vector3 endPosition, Vector3 normal, float arrowBodyWidth, float arrowHeadWidth, float arrowHeadLength, float arrowHeadRadius, float arrowHeadPointsPerTurn) {
        Quaternion rotation = Quaternion.LookRotation((endPosition - startPosition).normalized, normal).Rotate(new Vector3(90, 0,0));
        
        Draw.PolygonTriangulation = PolygonTriangulation.EarClipping;
        
        Draw.PushMatrix();
        Draw.Matrix = Matrix4x4.TRS(startPosition, rotation, Vector3.one);
        
        
        Vector2 startPos2D = Draw.Matrix.inverse.MultiplyPoint3x4(startPosition);
        Vector2 endPos2D = Draw.Matrix.inverse.MultiplyPoint3x4(endPosition);
        Vector2 direction = (endPos2D - startPos2D).normalized;
        var bodyWidthVector = Vector2.right * arrowBodyWidth * 0.5f;
        var arrowWidthVector = Vector2.right * arrowHeadWidth * 0.5f;
        
        var bodyBottomLeft = startPos2D - bodyWidthVector;
        var bodyTopLeft = endPos2D - direction * arrowHeadLength - bodyWidthVector;
        var arrowHeadBottomLeft = endPos2D - direction * arrowHeadLength - arrowWidthVector;
        var arrowHeadBottomRight = endPos2D - direction * arrowHeadLength + arrowWidthVector;
        var bodyTopRight = endPos2D - direction * arrowHeadLength + bodyWidthVector;
        var bodyBottomRight = startPos2D + bodyWidthVector;
        
        var gonPath = new PolygonPath();
        gonPath.AddPoint(startPos2D);
        gonPath.ArcTo(bodyBottomLeft, bodyTopLeft, arrowHeadRadius, arrowHeadPointsPerTurn);
        gonPath.AddPoint(bodyTopLeft);
        gonPath.ArcTo(arrowHeadBottomLeft, endPos2D, arrowHeadRadius, arrowHeadPointsPerTurn);
        gonPath.ArcTo(endPos2D, arrowHeadBottomRight, arrowHeadRadius, arrowHeadPointsPerTurn);
        gonPath.ArcTo(arrowHeadBottomRight, bodyTopRight, arrowHeadRadius, arrowHeadPointsPerTurn);
        gonPath.AddPoint(bodyTopRight);
        gonPath.ArcTo(bodyBottomRight, startPos2D, arrowHeadRadius, arrowHeadPointsPerTurn);
        gonPath.AddPoint(startPos2D);

        Draw.Polygon(gonPath);
        Draw.PopMatrix();
    }
    
    public static void DrawSpikedCircle(Vector3 startPoint, int numSpikes, float innerRadius, float outerRadius) {
        int NumVertices = numSpikes * 2;
        Vector2[] vertices = new Vector2[NumVertices];
        for(int i = 0; i < NumVertices; i++) {
            var radians = i/(float)NumVertices * Mathf.PI * 2;
            radians += Mathf.Deg2Rad;
            vertices[i] = new Vector2(Mathf.Sin(radians), Mathf.Cos(radians)) * (i % 2 == 0 ? innerRadius : outerRadius);
        }
        
        Quaternion rotation = Quaternion.LookRotation(Vector3.up, Vector3.forward);
        
        Draw.PolygonTriangulation = PolygonTriangulation.FastConvexOnly;
        
        Draw.PushMatrix();
        Draw.Matrix = Matrix4x4.TRS(startPoint, rotation, Vector3.one);
        
        var gonPath = new PolygonPath();
        foreach (var vertex in vertices) {
            gonPath.AddPoint(vertex);
        }

        Draw.Polygon(gonPath);
        Draw.PopMatrix();
    }
}