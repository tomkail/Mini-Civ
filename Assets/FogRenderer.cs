using System.Collections.Generic;
using Shapes;
using UnityEngine;
using UnityEngine.Rendering;
using UnityX.HexGrid;

// Draws the fog of war from the board's fog map: scrolling angled dashes everywhere outside the revealed area.
// The revealed area is a cached mesh (rounded, offset, with holes) that writes a stencil mask before the dashes are drawn.
// The mesh is only rebuilt when the fog changes, the level is regenerated, or these settings change.
[ExecuteAlways]
public class FogRenderer : ImmediateModeShapeDrawer {
    const int MaskStencilRef = 1;
    // Before Shapes' draws at AfterForwardAlpha (below), so the mask is in place when the dashes test against it.
    const CameraEvent MaskCameraEvent = CameraEvent.BeforeForwardAlpha;

    public WorldSpaceHexGrid worldSpaceHexGrid;

    public Color scrollingOverlayColorA;
    public Color scrollingOverlayColorB;
    public float scrollingOverlayLinesDashSize = 1f;
    public float scrollingOverlayLinesDashSpacing = 1f;

    [Range(-1,1)]
    public float fogExtrusion = 0;
    [Range(0,1)]
    public float smoothingRadius = 0.3f;
    [Range(1,90)]
    public float smoothingDegPerPoint = 20;

    public Quaternion rotation => worldSpaceHexGrid.XYPlaneRotation();

    readonly HexShapeBuilder shapeBuilder = new HexShapeBuilder();
    readonly List<Vector3> maskVertices = new List<Vector3>();
    readonly List<int> maskIndices = new List<int>();
    Mesh maskMesh;
    Material maskMaterial;
    CommandBuffer maskCommandBuffer;
    readonly HashSet<Camera> camerasWithMask = new HashSet<Camera>();

    BoardModel board;
    bool maskDirty = true;

    public override void OnEnable() {
        base.OnEnable();
        maskDirty = true;
    }

    public override void OnDisable() {
        base.OnDisable();
        SetBoard(null);
        foreach (var cam in camerasWithMask) {
            if (cam != null) cam.RemoveCommandBuffer(MaskCameraEvent, maskCommandBuffer);
        }
        camerasWithMask.Clear();
    }

    void OnDestroy() {
        maskCommandBuffer?.Release();
        maskCommandBuffer = null;
        if (maskMesh != null) ObjectX.DestroyAutomatic(maskMesh);
        if (maskMaterial != null) ObjectX.DestroyAutomatic(maskMaterial);
    }

    void OnValidate() {
        maskDirty = true;
    }

    public override void DrawShapes( Camera cam ) {
        if (worldSpaceHexGrid == null || !GameController.IsInitialized) return;
        var currentBoard = GameController.Instance.gameModel?.board;
        if (currentBoard == null) return;

        // A regenerated level comes with a new board.
        if (currentBoard != board) SetBoard(currentBoard);
        if (maskDirty) RebuildMask();
        DrawMask(cam);

        // Shapes defaults to CameraEvent.BeforeImageEffects, which never runs on this camera: the Post
        // Processing v2 PostProcessLayer takes over image effects. Draw before post-processing instead.
        using (Draw.Command(cam, CameraEvent.AfterForwardAlpha)) {
            DrawFog();
        }
    }

    void SetBoard(BoardModel newBoard) {
        if (board != null) board.OnFogChanged -= OnFogChanged;
        board = newBoard;
        if (board != null) board.OnFogChanged += OnFogChanged;
        maskDirty = true;
    }

    void OnFogChanged() {
        maskDirty = true;
    }

    void RebuildMask() {
        maskDirty = false;
        if (maskMesh == null) {
            maskMesh = new Mesh { name = "Fog Mask", hideFlags = HideFlags.HideAndDontSave };
            maskMesh.MarkDynamic();
        }

        maskVertices.Clear();
        maskIndices.Clear();
        if (board != null) {
            // Only cells that had fog and lost it: the sea never had fog, but is still drawn fogged.
            shapeBuilder.Build(board.RevealedFogCells(), board.IsFogRevealed, new HexShapeBuilder.Style(fogExtrusion, smoothingRadius, smoothingDegPerPoint));
            foreach (var point in shapeBuilder.triangles) {
                maskIndices.Add(maskVertices.Count);
                maskVertices.Add(point);
            }
        }
        maskMesh.Clear();
        maskMesh.indexFormat = maskVertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
        maskMesh.SetVertices(maskVertices);
        maskMesh.SetTriangles(maskIndices, 0);
        maskMesh.RecalculateBounds();
    }

    // The mask is drawn by a command buffer on each camera that renders the fog; only the grid's transform is re-recorded per frame.
    void DrawMask(Camera cam) {
        if (maskMaterial == null) {
            maskMaterial = new Material(Shader.Find("Hidden/Mini-Civ/FogStencilMask")) { name = "Fog Mask", hideFlags = HideFlags.HideAndDontSave };
        }
        if (maskCommandBuffer == null) maskCommandBuffer = new CommandBuffer { name = "Fog Mask" };
        maskCommandBuffer.Clear();
        maskCommandBuffer.DrawMesh(maskMesh, HexPositionToWorldMatrix(), maskMaterial);
        if (camerasWithMask.Add(cam)) cam.AddCommandBuffer(MaskCameraEvent, maskCommandBuffer);
    }

    // Maps HexCoord.Position space (which the mask is built in) onto the grid in world space, by matching up cell centres.
    Matrix4x4 HexPositionToWorldMatrix() {
        var origin = worldSpaceHexGrid.AxialToWorld(HexCoord.zero);
        var worldQ = worldSpaceHexGrid.AxialToWorld(new HexCoord(1, 0)) - origin;
        var worldR = worldSpaceHexGrid.AxialToWorld(new HexCoord(0, 1)) - origin;
        Vector2 positionQ = new HexCoord(1, 0).Position(), positionR = new HexCoord(0, 1).Position();
        // Solve [worldX worldY] * [positionQ positionR] = [worldQ worldR] for the world vectors of the position axes.
        float determinant = positionQ.x * positionR.y - positionR.x * positionQ.y;
        var worldX = (worldQ * positionR.y - worldR * positionQ.y) / determinant;
        var worldY = (worldR * positionQ.x - worldQ * positionR.x) / determinant;
        var worldZ = Vector3.Cross(worldX, worldY).normalized;
        var matrix = Matrix4x4.identity;
        matrix.SetColumn(0, worldX);
        matrix.SetColumn(1, worldY);
        matrix.SetColumn(2, worldZ);
        matrix.SetColumn(3, new Vector4(origin.x, origin.y, origin.z, 1));
        return matrix;
    }

    void DrawFog() {
        Draw.PushMatrix();
        Draw.Matrix = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);

        Draw.StencilRefID = MaskStencilRef;
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

        Draw.PopMatrix();
    }
}
