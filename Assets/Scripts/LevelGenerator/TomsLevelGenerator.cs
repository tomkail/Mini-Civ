using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
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
			var islands = detector.FindIslands().ToArray();
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
					if(adjacentPoints.Any(adjacentPoint => gameModel.board.landLayer.GetValueAtGridPoint<TerrainModel>(adjacentPoint)?.type == TerrainType.Grass)) continue;
					clearingTiles.Add(point);
				}

				foreach (var clearingTile in clearingTiles) {
					gameModel.board.landLayer.GetValueAtGridPoint<TerrainModel>(clearingTile).type = TerrainType.Grass;
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
				gameModel.board.landLayer.GetValueAtGridPoint<TerrainModel>(samples[i]).type = TerrainType.Mountain;
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
			foreach(var land in gameModel.board.landLayer.entities) {
				GridEntity.CreateAndAddEntity(() => new FogModel(land.Key), gameModel.board.fogLayer);
			}
			gameModel.board.fogLayer.ResetFog();

			var randomPos = HexUtils.HexagonPoints(3).Random();
			var radialCoords = HexUtils.HexagonPoints(2);
			foreach(var radialCoord in radialCoords) {
				gameModel.board.fogLayer.RevealFog(randomPos+radialCoord);
			}
			
			// This tests a bug where the fog has a hole in it
// 			var text = @"
// [{""q"":0,""r"":0},{""q"":1,""r"":0},{""q"":2,""r"":0},{""q"":2,""r"":1},{""q"":3,""r"":1},{""q"":3,""r"":2},{""q"":4,""r"":2},{""q"":5,""r"":2},{""q"":5,""r"":3},{""q"":4,""r"":4},{""q"":4,""r"":3},{""q"":3,""r"":3},{""q"":2,""r"":3},{""q"":1,""r"":4},{""q"":0,""r"":4},{""q"":0,""r"":3},{""q"":1,""r"":3},{""q"":1,""r"":2},{""q"":1,""r"":1},{""q"":0,""r"":1},{""q"":-1,""r"":2},{""q"":-1,""r"":3},{""q"":-2,""r"":4},{""q"":-3,""r"":5},{""q"":-2,""r"":3},{""q"":-2,""r"":2},{""q"":-3,""r"":2},{""q"":-1,""r"":1},{""q"":5,""r"":1},{""q"":6,""r"":1},{""q"":5,""r"":0},{""q"":5,""r"":-1},{""q"":6,""r"":-1},{""q"":6,""r"":-2},{""q"":5,""r"":-2},{""q"":4,""r"":-1},{""q"":3,""r"":-1},{""q"":2,""r"":-1},{""q"":1,""r"":-1},{""q"":0,""r"":-1},{""q"":-1,""r"":-1},{""q"":-2,""r"":0},{""q"":-3,""r"":0},{""q"":-2,""r"":-1},{""q"":-2,""r"":-2},{""q"":-1,""r"":-2},{""q"":0,""r"":-2},{""q"":0,""r"":-3},{""q"":1,""r"":-3},{""q"":2,""r"":-3},{""q"":3,""r"":-3},{""q"":4,""r"":-3},{""q"":5,""r"":-3},{""q"":6,""r"":-3},{""q"":6,""r"":-4},{""q"":5,""r"":-4},{""q"":4,""r"":-4},{""q"":3,""r"":-4},{""q"":2,""r"":-4},{""q"":1,""r"":-4},{""q"":2,""r"":-5},{""q"":4,""r"":-2},{""q"":3,""r"":-2}]
// ";
// 			var coords = JsonConvert.DeserializeObject<List<HexCoord>>(text);
// 			foreach(var radialCoord in coords) {
// 				gameModel.board.fogLayer.RevealFog(randomPos+radialCoord);
// 			}
			// var rect = RectX.CreateEncapsulating(coords.Select(x => x.Position()));
			// var hexCoordsInRect = HexCoord.CartesianRectangleBounds(rect.min, rect.max);
			
			
			// Find islands in hexCoordsInRect
			// Find convex hull
			// var islandHoleDetector = new IslandDetector<HexCoord>(coords, p => HexCoord.Directions(p), p => !coords.Contains(p));
			// var islands = islandHoleDetector.FindIslands();
			// foreach (var island in islands) {
			// 	var outlineCoords = OutlineDetector.GetOutlineCoords(island.points, 0, HexCoord.GetPointsOnRing).ToArray();
			// }
			// for (int i = 0; i < 3; i++) {
			// 	var pos = landTiles.Random();
			// 	var radialCoords = HexUtils.HexagonPoints(2);
			// 	foreach(var radialCoord in radialCoords) {
			// 		gameModel.board.fogLayer.RevealFog(pos+radialCoord);
			// 	}
			// }

		}
		return gameModel;
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
