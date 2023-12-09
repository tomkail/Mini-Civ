using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.Algorithms;

[ExecuteAlways]
public class GameController : MonoSingleton<GameController> {
    public LevelGenerator levelGenerator;
    [SerializeReference]
    public GameModel gameModel;
    public WorldSpaceHexGrid hexGrid;

    [Space]
    public HexCoord playerHexCoord;
    public List<HexCoord> currentPathPoints = new List<HexCoord>();
    public int playerMovementRange = 3;

    void OnEnable() {
        CreateGame();
    }
    
    [EasyButtons.Button]
    void CreateGame() {
        gameModel = levelGenerator.GenerateLevel();
    }
    void Update() {
        // gameModel = levelGenerator.GenerateLevel();
        if (Application.isPlaying) {
            gameModel.cursor.gridPoint = GameInputController.Instance.gridPoint;
            UpdatePath();
            if (Input.GetMouseButtonDown(0) && currentPathPoints.Any()) {
                playerHexCoord = currentPathPoints.Last();
                currentPathPoints.Clear();
            }

            if (Input.GetMouseButtonDown(0)) {
                Reveal(gameModel.cursor.gridPoint);
            }
        }
    }

    void Reveal(HexCoord cursorGridPoint) {
        var land = gameModel.board.landLayer.GetValueAtGridPoint<TerrainModel>(cursorGridPoint);
        if (land == null) {
            gameModel.board.fogLayer.RevealFog(gameModel.cursor.gridPoint);
        } else if (land.type == TerrainType.Mountain) {
            var radialCoords = HexUtils.HexagonPoints(2);
            foreach(var radialCoord in radialCoords) {
                gameModel.board.fogLayer.RevealFog(gameModel.cursor.gridPoint+radialCoord);
            }
        } else if (land.type == TerrainType.River) {
            gameModel.board.fogLayer.RevealFog(gameModel.cursor.gridPoint);
            
        } else if (land.type == TerrainType.Forest) {
            gameModel.board.fogLayer.RevealFog(gameModel.cursor.gridPoint);
        } else if (land.type == TerrainType.Grass) {
            var emptyLandDetector = new IslandDetector<HexCoord>(new List<HexCoord>(){cursorGridPoint}, p => HexCoord.Directions(p), p => gameModel.board.landLayer.GetValueAtGridPoint<TerrainModel>(p)?.type == TerrainType.Grass);
            var islands = emptyLandDetector.FindIslands().ToArray();
            foreach(var coord in islands.SelectMany(x => x.points)) gameModel.board.fogLayer.RevealFog(coord);
        }
        
    }

    void UpdatePath () {
        if(!currentPathPoints.IsEmpty() && currentPathPoints.First() != playerHexCoord) currentPathPoints.Clear();
        if(currentPathPoints.IsEmpty()) currentPathPoints.Add(playerHexCoord);
        UpdatePath(gameModel.board, currentPathPoints, gameModel.cursor.gridPoint, playerMovementRange);
	}

    public static int GetMovementCostForTerrainType (TerrainType terrainType) {
        return terrainType switch {
            TerrainType.Grass => 1,
            TerrainType.Forest => 2,
            TerrainType.Mountain => 3,
            TerrainType.Road => 1,
            TerrainType.River => 10000,
            _ => 1
        };
    }
    public static int GetCostForAdjacentTileMovement (BoardModel board, HexCoord originPoint, HexCoord destinationPoint) {
        var terrain = board.landLayer.GetValueAtGridPoint<TerrainModel>(destinationPoint);
        if(terrain == null) return 10000;
        else return GetMovementCostForTerrainType(terrain.type);
    }

    public int GetCostForPath(BoardModel board, List<HexCoord> currentPathPoints) {
        var totalCost = 0;
        if (currentPathPoints.Count > 1) {
            for (var index = 0; index < currentPathPoints.Count-1; index++) {
                GetCostForAdjacentTileMovement(board, currentPathPoints[index], currentPathPoints[index + 1]);
            }
        }
        return totalCost;
    }
    
    public static void UpdatePath (BoardModel board, List<HexCoord> currentPathPoints, HexCoord targetPoint, int movementRange) {
        int indexOfPoint = currentPathPoints.IndexOf(targetPoint);
        // Try to reach this point without crossing any existing path points. 
        // If we can't reach it, "rewind" the path until it becomes viable.
        if(indexOfPoint == -1) {
            var pathfinderOpts = PathFinder.PathFinderOptions.standard;
            pathfinderOpts.getActualCostForMovementBetweenElementsFunc = (HexCoord originPoint, HexCoord destinationPoint) => GetCostForAdjacentTileMovement(board, originPoint, destinationPoint);
            
            // Step back one point at a time until we can pathfind to the target point.
            AStar<HexCoord>.PathfinderSolution newPath = null;
            var startIndex = currentPathPoints.Count;
            while(newPath == null && startIndex > 0) {
                startIndex--;
                newPath = PathFinder.PathFind(board, currentPathPoints[startIndex], targetPoint, pathfinderOpts);
            }
            if(!newPath.solution.IsNullOrEmpty()) {
                // remove the first point since it's the same as the last one in our existing list
                newPath.solution.RemoveAt(0);
                int num = (currentPathPoints.Count-1)-startIndex;
                currentPathPoints.RemoveRange(startIndex+1, num);
                currentPathPoints.AddRange(newPath.solution);
            }

            // Enforce max movement
            newPath = null;
            startIndex = currentPathPoints.Count;
            int pathLength = currentPathPoints.Count;
            if(pathLength - 1 > movementRange) {
                var bestPath = PathFinder.PathFind(board, currentPathPoints.First(), targetPoint, pathfinderOpts);
                if(bestPath == null || bestPath.totalCost - 1 > movementRange) {
                    // if no path can make this distance, clear the path
                    currentPathPoints.Clear();
                } else {
                    // Step back one point at a time until we can pathfind to the target point.
                    while(startIndex > 0 && pathLength-1 > movementRange) {
                        startIndex--;
                        
                        newPath = PathFinder.PathFind(board, currentPathPoints[startIndex], targetPoint, pathfinderOpts);
                        if(newPath == null) pathLength = currentPathPoints.Count;
                        else pathLength = (currentPathPoints.Count - ((currentPathPoints.Count-1)-startIndex)) + (newPath.solution.Count-1);
                    }
                    if(!newPath.solution.IsNullOrEmpty()) {
                        // remove the first point since it's the same as the last one in our existing list
                        newPath.solution.RemoveAt(0);
                        int num = (currentPathPoints.Count-1)-startIndex;
                        currentPathPoints.RemoveRange(startIndex+1, num);
                        currentPathPoints.AddRange(newPath.solution);
                    }
                }
            }
        }
        // If the target point appears earlier in the list, rewind to that point
        else if(currentPathPoints.Count-1 != indexOfPoint) {
            var startIndex = indexOfPoint+1;
            var numToRemove = currentPathPoints.Count-startIndex;
            currentPathPoints.RemoveRange(startIndex, numToRemove);
            Debug.Assert(currentPathPoints.Last() == targetPoint);
        }
    }
}
