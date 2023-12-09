using System.Collections.Generic;
using System.Linq;
using Utils.Algorithms;

public static class PathFinder {

    public struct PathFinderOptions {
        public static PathFinderOptions standard {
            get {
                var opts = new PathFinderOptions();
                opts.getActualCostForMovementBetweenElementsFunc = null;
                opts.getElementsConnectedToElementFunc = null;
                return opts;
            }
        }
        public System.Func<HexCoord, HexCoord, float> getActualCostForMovementBetweenElementsFunc;
        public System.Func<HexCoord, IEnumerable<HexCoord>> getElementsConnectedToElementFunc;

        public override string ToString() {
            return string.Format("[{0}]", GetType());
        }
    }
	
    [System.Serializable]
    public class PathFinderSearchParams {
        public BoardModel board;
        public HexCoord originPoint;
        public HexCoord destinationPoint;
        public PathFinderOptions options;

        public PathFinderSearchParams (BoardModel board, HexCoord originPoint, HexCoord destinationPoint, PathFinderOptions options) {
            this.board = board;
            this.originPoint = originPoint;
            this.destinationPoint = destinationPoint;
            this.options = options;
        }
    }
	
    public static AStar<HexCoord>.PathfinderSolution PathFind(BoardModel board, HexCoord originPoint, HexCoord destinationPoint, PathFinderOptions options) {
        return PathFind(new PathFinderSearchParams(board, originPoint, destinationPoint, options));
    }
    
    public static AStar<HexCoord>.PathfinderSolution PathFind(PathFinderSearchParams searchParams) {
        if(searchParams == null) return null;

        Utils.Algorithms.AStar<HexCoord>.LazyGraph initData = new Utils.Algorithms.AStar<HexCoord>.LazyGraph ();
        initData.getLowestCostEstimateForMovementBetweenElementsFunc = (HexCoord originPoint, HexCoord destinationPoint) => {
            return HexCoord.Distance(originPoint, destinationPoint);
        };
		
        if(searchParams.options.getActualCostForMovementBetweenElementsFunc == null) {
            initData.getActualCostForMovementBetweenElementsFunc = (HexCoord originPoint, HexCoord destinationPoint) => {
                return HexCoord.Distance(originPoint, destinationPoint);
            };
        } else {
            initData.getActualCostForMovementBetweenElementsFunc = searchParams.options.getActualCostForMovementBetweenElementsFunc;
        }

        if(searchParams.options.getElementsConnectedToElementFunc == null) {
            initData.getElementsConnectedToElementFunc = (HexCoord _originPoint) => {
                return HexCoord.GetPointsOnRing(_originPoint, 1).Where(targetPoint => AreConnected (searchParams.board.gameModel.GetCell(_originPoint), searchParams.board.gameModel.GetCell(targetPoint), searchParams.options));
            };
        } else {
            initData.getElementsConnectedToElementFunc = searchParams.options.getElementsConnectedToElementFunc;
        }

		
        var aStar = new Utils.Algorithms.AStar<HexCoord> (initData);

        var results = aStar.Calculate (searchParams.originPoint, searchParams.destinationPoint);
        // Debug.Log("Pathfinder result: "+DebugX.ListAsString(results)+"\nOptions: "+options);
        return results;
    }

    public static bool AreConnected (GridCellModel gridCellA, GridCellModel gridCellB, PathFinderOptions options) {
        if(HexCoord.Distance(gridCellA.coord, gridCellB.coord) != 1) return false;
        // if(gridCellA.city != null && gridCellB.city != null) return true;
        // if(gridCellA.city != null && gridCellB.road != null) {
        //     var directionIndex = HexCoord.GetClosestDirectionIndex(gridCellA.coord, gridCellB.coord);
        //     return gridCellB.road.roadDirections[directionIndex];
        // }
        // if(gridCellB.city != null && gridCellA.road != null) {
        //     var directionIndex = HexCoord.GetClosestDirectionIndex(gridCellB.coord, gridCellA.coord);
        //     return gridCellA.road.roadDirections[directionIndex];
        // }
        // if(gridCellA.road != null && gridCellB.road != null) return gridCellA.road.IsConnectedTo(gridCellB.road);
        
        return true;
    }
}