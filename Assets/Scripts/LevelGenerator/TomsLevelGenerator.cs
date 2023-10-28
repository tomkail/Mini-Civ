using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class TomsLevelGenerator : LevelGenerator {
	public int numLandTiles = 100;
	public TomsLevelGeneratorSettings settings;
	public NoiseSamplerProperties properties;
	
	public float landRoundness = 1f;
	
	[Space]
	public float clearingRoundness = 1f;
	public float clearingPoissonRadius = 3;
	public int numClearings = 10;
	public float clearingSize = 3;
	
	[Space]
	public float mountainPoissonRadius = 3;
	public int numMountains = 6;
	
	public static GameModel CreateGameModel () {
		var gameModel = new GameModel();
		gameModel.Init();
		return gameModel;
	}

	// Algorithm to generate the island:
	// Pick a random tile on the periphery, where tiles are weighted by a noise layer, and add it to the map
	// Do this about 100 times
	// Mountains are the default
	// Pick points via poisson to put mountains on
	// Do the same thing with plains, except that it should also make them in chunks of size 3-7 (using the same technique as for island generation).
	// Plains should be common enough that you find them ever 5 turns or so.
	// Make rivers by connecting mountains to the sea
	// Place caves randomly.
	// Replace some trees with fruit trees
	public override GameModel GenerateLevel () {
		if(settings.useSeed) Random.InitState(settings.seed);
		else Random.InitState((Time.realtimeSinceStartup * 100).RoundToInt());
		
		var gameModel = CreateGameModel();

		HashSet<HexCoord> landTiles = new HashSet<HexCoord>();
		for (int i = 0; i < numLandTiles; i++) {
			var point = GetNewPointForIsland(landTiles, landRoundness);
			landTiles.Add(point);
		}

		static HexCoord GetNewPointForIsland(HashSet<HexCoord> existingTiles, float roundness) {
			if(existingTiles.Count == 0) return HexCoord.zero;
			var detector = new IslandDetector<HexCoord>(existingTiles, p => HexCoord.Directions(p), p => existingTiles.Contains(p));
			var islands = detector.FindIslands();
			foreach(var island in islands) {
				var outlineCoords = OutlineDetector.GetOutlineCoords(island.points, 1, HexCoord.GetPointsOnRing).ToArray();
				var weights = new List<float>();
				foreach (var coord in outlineCoords) {
					var adjacent = HexCoord.GetPointsOnRing(coord, 1);
					weights.Add(Mathf.Pow(adjacent.Count(x => existingTiles.Contains(x)) / 6f, roundness));
					// weights.Add(Mathf.InverseLerp(-1,1,NoiseSampler.SampleAtPosition(coord.Position(), properties).value));
				}
				var index = RandomX.WeightedIndex(weights);
				
				// blended.targetPoint = WeightedBlends.WeightedBlend(allProperties, p => p.targetPoint, weights);
				return outlineCoords[index];
				foreach(var outlineCoord in outlineCoords) {
					// GridEntity.CreateAndAddEntity(() => {
					// 	return new WaterModel(outlineCoord);
					// }, gameModel.board.waterLayer);
					return outlineCoord;
				}
			}
			Debug.LogError("Failed!");
			return HexCoord.zero;
		}
		foreach (var point in landTiles) {
			GridEntity.CreateAndAddEntity(() => new TerrainModel(point, TerrainType.Forest), gameModel.board.landLayer);
		}
		
		

		{
			var detector = new IslandDetector<HexCoord>(landTiles, p => HexCoord.Directions(p), p => landTiles.Contains(p));
			var islands = detector.FindIslands();
			foreach (var island in islands) {
				var outlineCoords = OutlineDetector.GetOutlineCoords(island.points, 1, HexCoord.GetPointsOnRing).ToArray();
				var outlineIslandDetector = new IslandDetector<HexCoord>(outlineCoords, p => HexCoord.Directions(p), p => outlineCoords.Contains(p));
				var outlineIslands = outlineIslandDetector.FindIslands();

				foreach (var outline in outlineIslands) {
					var outlineDetector = new IslandDetector<HexCoord>(outline.points, p => HexCoord.Directions(p), p => !landTiles.Contains(p));
					// var outlineIslandCoords = outlineDetector.FindIslands();
					// foreach (var coord in outlineIslandCoords.First().points) {
					// 	GridEntity.CreateAndAddEntity(() => new TerrainModel(coord, TerrainType.River), gameModel.board.landLayer);
					// }
				}
				//
				// var riverPoint = outlineCoords.Random();
				// foreach (var coord in outlineCoords) {
				// 	if(gameModel.GetCell(coord).onGrid) gameModel.board.landLayer.GetValuesAtGridPoint(coord).OfType<TerrainModel>().First().type = TerrainType.River;
				// 	else GridEntity.CreateAndAddEntity(() => new TerrainModel(coord, TerrainType.River), gameModel.board.landLayer);
				// }
			}
		}

		var mapBounds = RectX.CreateEncapsulating(landTiles.Select(x => x.Position()));
		{
			var poissonDiscSampler = new PoissonDiscSampler(mapBounds.width, mapBounds.height, clearingPoissonRadius);
			var samples = poissonDiscSampler.Samples().Select(x => HexCoord.AtPosition(x+mapBounds.position)).Where(x => landTiles.Contains(x)).ToList();
			samples.Shuffle();
			
			for (int i = 0; i < Mathf.Min(numClearings, samples.Count); i++) {
				clearingSize = Random.Range(3, 7);
				
				var clearingTiles = new HashSet<HexCoord>();
				clearingTiles.Add(samples[i]);
				// gameModel.board.landLayer.GetValuesAtGridPoint(samples[i]).OfType<TerrainModel>().First().type = TerrainType.Grass;
				
				for (int j = 1; j < clearingSize; j++) {
					var point = GetNewPointForIsland(clearingTiles, clearingRoundness);
					if (!landTiles.Contains(point)) continue;
					var adjacentPoints = HexCoord.GetPointsOnRing(point, 1);
					if(adjacentPoints.Any(adjacentPoint => gameModel.board.landLayer.GetValuesAtGridPoint(adjacentPoint).OfType<TerrainModel>().FirstOrDefault()?.type == TerrainType.Grass)) continue;
					clearingTiles.Add(point);
				}

				foreach (var clearingTile in clearingTiles) {
					gameModel.board.landLayer.GetValuesAtGridPoint(clearingTile).OfType<TerrainModel>().First().type = TerrainType.Grass;
				}
			}
			
			// Dictionary<HexCoord, float> weights = new Dictionary<HexCoord, float>();
			
		}

		{
			var poissonDiscSampler = new PoissonDiscSampler(mapBounds.width, mapBounds.height, mountainPoissonRadius);
			var samples = poissonDiscSampler.Samples().Select(x => HexCoord.AtPosition(x+mapBounds.position)).Where(x => landTiles.Contains(x)).ToList();
			samples.Shuffle();
			
			List<HexCoord> mountainTiles = new List<HexCoord>();
			for (int i = 0; i < Mathf.Min(numMountains, samples.Count); i++) {
				mountainTiles.Add(samples[i]);
				gameModel.board.landLayer.GetValuesAtGridPoint(samples[i]).OfType<TerrainModel>().First().type = TerrainType.Mountain;
			}
		}


		// Rivers
		{
			var detector = new IslandDetector<HexCoord>(landTiles, p => HexCoord.Directions(p), p => landTiles.Contains(p));
			var islands = detector.FindIslands();
			foreach (var island in islands) {
				var outlineCoords = OutlineDetector.GetOutlineCoords(island.points, 0, HexCoord.GetPointsOnRing).ToArray();
			
				// var riverPoint = outlineCoords.Random();
				// foreach (var coord in outlineCoords) {
				// 	if(gameModel.GetCell(coord).onGrid) gameModel.board.landLayer.GetValuesAtGridPoint(coord).OfType<TerrainModel>().First().type = TerrainType.River;
				// 	else GridEntity.CreateAndAddEntity(() => new TerrainModel(coord, TerrainType.River), gameModel.board.landLayer);
				// }
			}
		}
		
		
		//
		// var radialCoords = HexUtils.HexagonPoints(settings.levelDiameter);
		// foreach(var radialCoord in radialCoords) {
		// 	GridEntity.CreateAndAddEntity(() => new TerrainModel(radialCoord, TerrainType.Grass), gameModel.board.landLayer);
		// }

		{
			// foreach(var land in gameModel.board.landLayer.entities) {
			// 	GridEntity.CreateAndAddEntity(() => new FogModel(land.gridPoint), gameModel.board.fogLayer);
			// }
			// gameModel.board.fogLayer.ResetFog();
			// var radialCoords = HexUtils.HexagonPoints(2);
			// foreach(var radialCoord in radialCoords) {
			// 	gameModel.board.fogLayer.RevealFog(radialCoord);
			// }
		}
		return gameModel;
	}
}



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
	
	public static List<HexCoord> PathFind(PathFinderSearchParams searchParams) {
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

		
		Utils.Algorithms.AStar<HexCoord> aStar = new Utils.Algorithms.AStar<HexCoord> (initData);

		IList<HexCoord> results = aStar.Calculate (searchParams.originPoint, searchParams.destinationPoint).solution;
		// Debug.Log("Pathfinder result: "+DebugX.ListAsString(results)+"\nOptions: "+options);
		if(results == null) return null;
		return results.ToList();
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


[System.Serializable]
public class RoadModel : GridEntity, IEquatable<RoadModel> {
	[SerializeField]
	HexEdgeDirections _roadDirections;
	public HexEdgeDirections roadDirections {
		get {
			return _roadDirections;
		} private set {
			_roadDirections = value;
		}
	}

	public RoadModel (HexCoord gridPoint, HexEdgeDirections roadDirections) : base(gridPoint) {
		this.roadDirections = roadDirections;
	}
	protected RoadModel (RoadModel model) : base (model) {
		roadDirections = model.roadDirections.Clone();
	}
	public override GridEntity Clone () {
		return new RoadModel(this);
	}
	
	public bool IsConnectedTo (RoadModel other) {
		return HexEdgeDirections.Connected(other.gridPoint, other.roadDirections, gridPoint, roadDirections);
	}

	public bool Equals(RoadModel obj) {
		return base.Equals(obj);
	}
}
