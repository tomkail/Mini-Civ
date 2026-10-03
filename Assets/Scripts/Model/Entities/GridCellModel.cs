using System;
using System.Collections.Generic;
using UnityX.HexGrid;

public struct GridCellModel : IEquatable<GridCellModel> {

	public GameModel gameModel;
	public BoardModel board => gameModel.board;
	public HexCoord coord;

	public bool valid => gameModel != null && board != null;
	public bool onGrid => board.terrain.Contains(coord);

	// Null off the island.
	public TerrainType? terrainType => board.terrain.TryGetValue(coord, out var type) ? type : null;
	public bool hasFog => board.fog.Contains(coord);
	// True once the fog here is revealed, or if there's no fog here.
	public bool revealed => board.IsRevealed(coord);

	public IEnumerable<GridEntity> entities => board.gameEntityLayer.GetValuesAtGridPoint(coord);

	public IEnumerable<T> GetEntitiesOfType<T>() {
		foreach (object item in entities) {
			if (item is T) yield return (T)item;
		}
	}

	public GridCellModel (GameModel gameModel, HexCoord gridPoint) {
		this.gameModel = gameModel;
		this.coord = gridPoint;
	}


	public override string ToString () {
		return string.Format ("[GridCellModel: gridPoint={0}]", coord);
	}

	public override bool Equals(System.Object obj) {
		if (obj == null) return false;
		GridCellModel p = (GridCellModel)obj;
		if ((System.Object)p == null) return false;
		return Equals(p);
	}

	public bool Equals(GridCellModel p) {
		// if ((object)p == null) return false;
		return coord == p.coord && gameModel == p.gameModel;
	}
}
