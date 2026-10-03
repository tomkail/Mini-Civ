using UnityEngine;
using UnityX.HexGrid;

public static class WorldSpaceHexGridExtensions {
    // Rotation that lays the 2D XY plane (HexCoord.Position/Corner space, and the plane Shapes draws in) onto
    // the board. This is what WorldSpaceHexGrid.axis meant before UnityX.HexGrid redefined axis as a frame
    // whose up is the floor normal; on Board's grid (XYZ swizzle, rotated 90 degrees about X) the new axis
    // would stand the drawing on its edge.
    public static Quaternion XYPlaneRotation(this WorldSpaceHexGrid hexGrid) {
        var swizzle = hexGrid.grid.cellSwizzle;
        var swizzleRotation = Quaternion.LookRotation(UnityEngine.Grid.Swizzle(swizzle, Vector3.forward), UnityEngine.Grid.Swizzle(swizzle, Vector3.up));
        return swizzleRotation * hexGrid.transform.rotation;
    }
}
