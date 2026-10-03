using System.Collections.Generic;
using UnityX.HexGrid;

public static class PathFinder {
    public class Path {
        // Every cell from the origin to the destination, inclusive.
        public List<HexCoord> solution;
        // The summed cost of the steps after the origin.
        public float totalCost;
    }

    // Cheapest path between two cells, stepping between any neighbours (on or off the island) at the cost
    // `stepCost` gives; null if there isn't one.
    public static Path PathFind(HexCoord originPoint, HexCoord destinationPoint, System.Func<HexCoord, HexCoord, float> stepCost) {
        var solution = HexSearch.FindPath(originPoint, destinationPoint, _ => true, stepCost);
        if (solution == null) return null;
        float totalCost = 0;
        for (int i = 1; i < solution.Count; i++) totalCost += stepCost(solution[i - 1], solution[i]);
        return new Path { solution = solution, totalCost = totalCost };
    }
}
