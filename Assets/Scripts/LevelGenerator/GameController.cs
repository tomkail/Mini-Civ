using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityX.Islands;
using UnityX.HexGrid;

[ExecuteAlways]
public class GameController : MonoSingleton<GameController> {
    public LevelGenerator levelGenerator;
    [SerializeReference]
    public GameModel gameModel;
    public WorldSpaceHexGrid hexGrid;
    public Tilemap terrainTilemap;

    [Space]
    public HexCoord playerHexCoord;
    public List<HexCoord> currentPathPoints = new List<HexCoord>();
    public int playerMovementRange = 1;

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

    // Fog is revealed by rule, per terrain type: mountains reveal radius 1, grass reveals its whole connected grass area,
    // and everything else reveals just the clicked hex.
    void Reveal(HexCoord cursorGridPoint) {
        gameModel.board.RevealFog(GetCellsRevealedFrom(gameModel.board, cursorGridPoint));
    }

    public static IEnumerable<HexCoord> GetCellsRevealedFrom(BoardModel board, HexCoord point) {
        if (!board.terrain.TryGetValue(point, out var land)) return new[] { point };
        if (land == TerrainType.Mountain) return HexShapes.Hexagon(point, 1);
        if (land == TerrainType.Grass) {
            var grassDetector = new IslandDetector<HexCoord>(new List<HexCoord>(){point}, p => HexCoord.Directions(p), p => board.terrain.TryGetValue(p, out var type) && type == TerrainType.Grass);
            return grassDetector.FindIslands().SelectMany(x => x.points);
        }
        return new[] { point };
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
        if(!board.terrain.TryGetValue(destinationPoint, out var terrain)) return 10000;
        else return GetMovementCostForTerrainType(terrain);
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
            System.Func<HexCoord, HexCoord, float> stepCost = (originPoint, destinationPoint) => GetCostForAdjacentTileMovement(board, originPoint, destinationPoint);
            
            // Step back one point at a time until we can pathfind to the target point.
            PathFinder.Path newPath = null;
            var startIndex = currentPathPoints.Count;
            while(newPath == null && startIndex > 0) {
                startIndex--;
                newPath = PathFinder.PathFind(currentPathPoints[startIndex], targetPoint, stepCost);
            }
            if(newPath != null && !newPath.solution.IsNullOrEmpty()) {
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
                var bestPath = PathFinder.PathFind(currentPathPoints.First(), targetPoint, stepCost);
                if(bestPath == null || bestPath.totalCost > movementRange) {
                    // if no path can make this distance, clear the path
                    currentPathPoints.Clear();
                } else {
                    // Step back one point at a time until we can pathfind to the target point.
                    while(startIndex > 0 && pathLength-1 > movementRange) {
                        startIndex--;
                        
                        newPath = PathFinder.PathFind(currentPathPoints[startIndex], targetPoint, stepCost);
                        if(newPath == null) pathLength = currentPathPoints.Count;
                        else pathLength = (currentPathPoints.Count - ((currentPathPoints.Count-1)-startIndex)) + (newPath.solution.Count-1);
                    }
                    if(newPath != null && !newPath.solution.IsNullOrEmpty()) {
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
