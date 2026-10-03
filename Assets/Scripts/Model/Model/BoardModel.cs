using System.Collections.Generic;
using UnityX.HexGrid;

[System.Serializable]
public class BoardModel {
	[System.NonSerialized, Newtonsoft.Json.JsonIgnore]
	public GameModel gameModel;

	// The island: which cells exist and what terrain each has. Cells off the island aren't in the map.
	public HexMap<TerrainType> terrain = new HexMap<TerrainType>();
	// Fog over the island, per cell: true once revealed. Cells without fog count as revealed.
	// Change it through the methods below, which fire OnFogChanged.
	public HexMap<bool> fog = new HexMap<bool>();
	// Fired once per change to the fog, however many cells changed.
	[field: System.NonSerialized]
	public event System.Action OnFogChanged;
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
	// The cells whose fog has been revealed (not the cells that never had fog).
	public IEnumerable<HexCoord> RevealedFogCells () {
		foreach (var cell in fog) if (cell.Value) yield return cell.Key;
	}
	public bool IsFogRevealed (HexCoord coord) => fog.TryGetValue(coord, out var revealed) && revealed;

	// Puts unrevealed fog over `coords`.
	public void CoverWithFog (IEnumerable<HexCoord> coords) {
		bool changed = false;
		foreach (var coord in coords) changed |= SetFog(coord, false, addIfMissing: true);
		if (changed) OnFogChanged?.Invoke();
	}
	public void ResetFog () => SetAllFog(false);
	public void RevealAllFog () => SetAllFog(true);
	void SetAllFog (bool revealed) {
		bool changed = false;
		foreach (var coord in new List<HexCoord>(fog.Coords)) changed |= SetFog(coord, revealed, addIfMissing: false);
		if (changed) OnFogChanged?.Invoke();
	}
	// Reveals the fogged cells within `radius` of `point`.
	public void RevealFog (HexCoord point, int radius = 0) => RevealFog(HexShapes.Hexagon(point, radius));
	// Reveals the fogged cells among `coords`.
	public void RevealFog (IEnumerable<HexCoord> coords) {
		bool changed = false;
		foreach (var coord in coords) changed |= SetFog(coord, true, addIfMissing: false);
		if (changed) OnFogChanged?.Invoke();
	}

	// True if the cell changed.
	bool SetFog (HexCoord coord, bool revealed, bool addIfMissing) {
		if (fog.TryGetValue(coord, out var current) ? current == revealed : !addIfMissing) return false;
		fog[coord] = revealed;
		return true;
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
