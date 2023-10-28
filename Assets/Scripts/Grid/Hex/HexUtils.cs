using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class HexUtils {
    public static float HexCoordDirectionToDegrees (HexCoord coord) {
		return Vector2X.Degrees(coord.DirectionVector());
	}

    // Gets the difference between the forward direction of unit and the direction to a target point
    public static int DeltaDirectionBetweenForwardAndDirectionToTarget (HexCoord gridPoint, int directionIndex, HexCoord targetPoint) {
        var directionIndexToTarget = HexCoord.GetClosestDirectionIndex(gridPoint, targetPoint);
		return HexUtils.SignedDeltaDirection(directionIndex, directionIndexToTarget);
    }
    public static int SignedDeltaDirection (int dirIndex1, int dirIndex2) {
        var deltaDirection = dirIndex2 - dirIndex1;
		return MathX.Mod((deltaDirection + 3), 6) - 3;
    }
    public static int RotateTowards (int currentDirectionIndex, int targetDirectionIndex, int maxRotationSteps) {
        var deltaDirection = SignedDeltaDirection(currentDirectionIndex, targetDirectionIndex);
        return currentDirectionIndex + Mathf.Clamp(deltaDirection, -maxRotationSteps, maxRotationSteps);
    }
    
	public static List<HexCoord> OffsetRectPoints (Point rectSize) {
        return OffsetRectPoints(new PointRect(new Point(0,0), rectSize));
    }

    public static List<HexCoord> OffsetRectPoints (PointRect gridSize) {
        List<HexCoord> hexCoords = new List<HexCoord>();
        for (int r = gridSize.yMin; r < gridSize.yMax; r++) {
            for (int q = gridSize.xMin; q < gridSize.xMax; q++) {
                var hex = HexCoord.OffsetToHex(new Point(q,r));
                hexCoords.Add(hex);
            }
        }
        return hexCoords;
    }

    public static IEnumerable<HexCoord> HexagonPoints (int circumference) {
        int smallRadius = Mathf.FloorToInt(circumference * 0.5f);
        int largeRadius = Mathf.CeilToInt(circumference * 0.5f);
        for (int q = -smallRadius; q <= largeRadius; q++) {
            int r1 = Mathf.Max(-smallRadius, -q - smallRadius);
            int r2 = Mathf.Min(largeRadius, -q + largeRadius);
            for (int r = r1; r <= r2; r++) {
                var coord = new HexCoord(q, r);
                yield return coord;
            }
        }
    }
}