using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        }
    }
    void UpdatePath () {
        if(!currentPathPoints.IsEmpty() && currentPathPoints.First() != playerHexCoord) currentPathPoints.Clear();
        if(currentPathPoints.IsEmpty()) currentPathPoints.Add(playerHexCoord);
        UpdatePath(gameModel.board, currentPathPoints, gameModel.cursor.gridPoint, playerMovementRange);
	}

    public static void UpdatePath (BoardModel board, List<HexCoord> currentPathPoints, HexCoord targetPoint, int movementRange) {
        int indexOfPoint = currentPathPoints.IndexOf(targetPoint);
        // Try to reach this point without crossing any existing path points. 
        // If we can't reach it, "rewind" the path until it becomes viable.
        if(indexOfPoint == -1) {
            var opts = PathFinder.PathFinderOptions.standard;
            
            List<HexCoord> newPoints = null;
            var startIndex = currentPathPoints.Count;
            while(newPoints == null && startIndex > 0) {
                startIndex--;
                newPoints = PathFinder.PathFind(board, currentPathPoints[startIndex], targetPoint, opts);
            }
            if(!newPoints.IsNullOrEmpty()) {
                // remove the first point since it's the same as the last one in our existing list
                newPoints.RemoveAt(0);
                int num = (currentPathPoints.Count-1)-startIndex;
                currentPathPoints.RemoveRange(startIndex+1, num);
                currentPathPoints.AddRange(newPoints);
            }

            // Enforce max movement
            newPoints = null;
            startIndex = currentPathPoints.Count;
            int pathLength = currentPathPoints.Count;
            if(pathLength - 1 > movementRange) {
                var bestPath = PathFinder.PathFind(board, currentPathPoints.First(), targetPoint, opts);
                if(bestPath == null || bestPath.Count - 1 > movementRange) {
                    // if no path can make this distance, clear the path
                    currentPathPoints.Clear();
                } else {
                    // Step back one point at a time until we can pathfind to the target point.
                    while(startIndex > 0 && pathLength-1 > movementRange) {
                        startIndex--;
                        
                        newPoints = PathFinder.PathFind(board, currentPathPoints[startIndex], targetPoint, opts);
                        if(newPoints == null) pathLength = currentPathPoints.Count;
                        else pathLength = (currentPathPoints.Count - ((currentPathPoints.Count-1)-startIndex)) + (newPoints.Count-1);
                    }
                    if(!newPoints.IsNullOrEmpty()) {
                        // remove the first point since it's the same as the last one in our existing list
                        newPoints.RemoveAt(0);
                        int num = (currentPathPoints.Count-1)-startIndex;
                        currentPathPoints.RemoveRange(startIndex+1, num);
                        currentPathPoints.AddRange(newPoints);
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
