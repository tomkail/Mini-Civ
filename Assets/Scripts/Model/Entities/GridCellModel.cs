using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityX.Geometry;

public struct GridCellModel : IEquatable<GridCellModel> {

	public GameModel gameModel;
	public BoardModel board => gameModel.board;
	public HexCoord coord;

	public bool valid => gameModel != null && board != null;
	public bool onGrid => terrain != null;

	public TerrainModel terrain => board.landLayer.GetValuesAtGridPoint(coord).OfType<TerrainModel>().FirstOrDefault();
	public FogModel fog => board.fogLayer.GetValuesAtGridPoint(coord).OfType<FogModel>().FirstOrDefault();

	public IEnumerable<GridEntity> entities {
		get {
			foreach(var land in board.landLayer.GetValuesAtGridPoint(coord)) yield return land;
			foreach(var gameEntity in board.gameEntityLayer.GetValuesAtGridPoint(coord)) yield return gameEntity;
			foreach(var fog in board.fogLayer.GetValuesAtGridPoint(coord)) yield return fog;
		}
	}
	
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