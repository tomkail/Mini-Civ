using System.Collections.Generic;
using UnityX.HexGrid;

[System.Serializable]
public class BoardModel {
	[System.NonSerialized, Newtonsoft.Json.JsonIgnore]
	public GameModel gameModel;

	// The island: which cells exist and what terrain each has. Cells off the island aren't in the map.
	public HexMap<TerrainType> terrain = new HexMap<TerrainType>();
	// Fog over the island, per cell: true once revealed. Cells without fog count as revealed.
	public HexMap<bool> fog = new HexMap<bool>();
	// Things that move around (units); they own their position.
	public DynamicGridLayerModel gameEntityLayer;

	public HexCoord.Layout offsetLayout;

	internal void OnDeserializedMethod() {
		gameEntityLayer.board = this;
	}

	public BoardModel (GameModel gameModel) {
		this.gameModel = gameModel;
		gameEntityLayer = new DynamicGridLayerModel(this, "Unit");
	}
	protected BoardModel (GameModel gameModel, BoardModel modelToClone) {
		this.gameModel = gameModel;
		terrain = new HexMap<TerrainType>(modelToClone.terrain.Coords, coord => modelToClone.terrain[coord]);
		fog = new HexMap<bool>(modelToClone.fog.Coords, coord => modelToClone.fog[coord]);
		gameEntityLayer = modelToClone.gameEntityLayer.Clone() as DynamicGridLayerModel;
		offsetLayout = modelToClone.offsetLayout;
	}
	public BoardModel Clone (GameModel gameModel) {
		return new BoardModel(gameModel, this);
	}

	public virtual void Clear () {
		terrain.Clear();
		fog.Clear();
		gameEntityLayer.Clear();
	}

	public GridEntity AddEntity(GridEntity newEntity) {
		return gameEntityLayer.AddEntity(newEntity);
	}

	public IEnumerable<GridEntity> AllEntities () => gameEntityLayer.GetAllEntities();
	public IEnumerable<T> AllEntitiesOfType<T> () => gameEntityLayer.OfType<T>();

	public bool IsRevealed (HexCoord coord) => !fog.TryGetValue(coord, out var revealed) || revealed;

	public void ResetFog () => SetAllFog(false);
	public void RevealAllFog () => SetAllFog(true);
	void SetAllFog (bool revealed) {
		foreach (var coord in new List<HexCoord>(fog.Coords)) fog[coord] = revealed;
	}
	// Reveals the fogged cells within `radius` of `point`.
	public void RevealFog (HexCoord point, int radius = 0) {
		foreach (var coord in HexShapes.Hexagon(point, radius))
			if (fog.Contains(coord)) fog[coord] = true;
	}

	public IEnumerable<GridCellModel> GetCells () {
		foreach(var coord in terrain.Coords) {
			yield return GetCell(coord);
		}
	}
	public GridCellModel GetCell (HexCoord coord) {
		return new GridCellModel(gameModel, coord);
	}
}
