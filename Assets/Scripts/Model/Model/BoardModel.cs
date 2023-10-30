using System.Collections;
using System.Collections.Generic;
using UnityX.Geometry;
using System.Linq;
using Newtonsoft.Json;
using System.Runtime.Serialization;

// Remove most the helper funcitons
// Consider moving the grid layers into this class, else moving most of the code related to them into their class.
[System.Serializable]
public class BoardModel {
	// public static BoardModel current {
	// 	get {
	// 		if(!GameController.IsInitialized) return null;
	// 		if(GameController.Instance.state == GameController.State.NoGame) return null;
	// 		return GameController.Instance.gameModel.board;
	// 	}
	// }
	//
	[System.NonSerialized, Newtonsoft.Json.JsonIgnore]
	public GameModel gameModel;
	// public List<GridEntity> entities;

	[Newtonsoft.Json.JsonIgnore]
	public IEnumerable<GridLayerModel> gridLayers {
		get {
			yield return landLayer;
			yield return gameEntityLayer;
			yield return fogLayer;
		}
	}
	
	public TerrainGridLayerModel landLayer;
	public DynamicGridLayerModel gameEntityLayer;
	public FogGridLayerModel fogLayer;

	public HexCoord.Layout offsetLayout;

	internal void OnDeserializedMethod() {
		foreach(var gridLayer in gridLayers) gridLayer.board = this;
	}
	

	public BoardModel (GameModel gameModel) {
		this.gameModel = gameModel;
		landLayer = new TerrainGridLayerModel(this, "Land");
		gameEntityLayer = new DynamicGridLayerModel(this, "Unit");
		fogLayer = new FogGridLayerModel(this, "Fog");
	}
	protected BoardModel (GameModel gameModel, BoardModel modelToClone) {
		this.gameModel = gameModel;
		landLayer = modelToClone.landLayer.Clone() as TerrainGridLayerModel;
		gameEntityLayer = modelToClone.gameEntityLayer.Clone() as DynamicGridLayerModel;
		fogLayer = modelToClone.fogLayer.Clone() as FogGridLayerModel;
		offsetLayout = modelToClone.offsetLayout;
	}
	public BoardModel Clone (GameModel gameModel) {
		return new BoardModel(gameModel, this);
	}

	public virtual void Clear () {
		foreach(var gridLayer in gridLayers)
			gridLayer.Clear();
	}

	public GridEntity AddEntity(GridEntity newEntity) {
		if(newEntity is TerrainModel) landLayer.AddEntity(newEntity);
		else if(newEntity is FogModel) fogLayer.AddEntity(newEntity);
		else gameEntityLayer.AddEntity(newEntity);
		return newEntity;
	}

	public IEnumerable<GridEntity> AllEntities () {
		foreach(var layer in gridLayers) {
			foreach(var entity in layer.GetAllEntities()) {
				yield return entity;
			}
		}
	}
	public IEnumerable<T> AllEntitiesOfType<T> () {
		foreach(var layer in gridLayers) {
			if(layer == null) continue;
			foreach(var entity in layer.OfType<T>()) {
				yield return entity;
			}
		}
	}
	
	public IEnumerable<GridCellModel> GetCells () {
		foreach(var land in landLayer.entities) {
			yield return GetCell(land.Key);
		}
	}
	public GridCellModel GetCell (HexCoord coord) {
		return new GridCellModel(gameModel, coord);
	}
}