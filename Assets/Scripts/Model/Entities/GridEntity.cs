using System;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public abstract class GridEntity {

	public virtual void SetEntityID (GameModel gameModel) {
		entityTypeIDIndex = gameModel.idDictionary.ReserveEntityID(GetType());
		entityID = GetEntityID(GetType(), entityTypeIDIndex);
	}
	public static string GetEntityID (System.Type type, int index) {
		return type.Name+index.ToString();
	}

	[SerializeField, Disable, JsonProperty]
	int _entityTypeIDIndex;
	[JsonIgnore]
	public int entityTypeIDIndex {
		get {
			return _entityTypeIDIndex;
		} private set {
			_entityTypeIDIndex = value;
		}
	}
	[SerializeField, Disable, JsonProperty]
	string _entityID;
	[JsonIgnore]
	public string entityID {
		get {
			return _entityID;
		} private set {
			_entityID = value;
		}
	}


		
	[SerializeField, Disable, JsonProperty]
	HexCoord _gridPoint;
	[JsonIgnore]
	public HexCoord gridPoint {
		get {
			return _gridPoint;
		} set {
			var lastGridPoint = _gridPoint;
			_gridPoint = value;
			OnChangeGridPointInternal(lastGridPoint, gridPoint);
			if(OnChangeGridPoint != null) OnChangeGridPoint(this, lastGridPoint, gridPoint);
		}
	}

	public delegate void OnChangeGridPointEvent(GridEntity entity, HexCoord lastPoint, HexCoord newPoint);
	public event OnChangeGridPointEvent OnChangeGridPoint;
	[JsonIgnore]
	public Action<GridEntity> OnDie;
	
	public static T CreateAndAddEntity<T> (System.Func<T> CreateEntity, GridLayerModel layer) where T : GridEntity {
		var entity = CreateEntity();
		layer.AddEntity(entity);
		return entity;
	}

	[JsonConstructor]
	protected GridEntity () {}

	public GridEntity (HexCoord gridPoint) {
		this.gridPoint = gridPoint;
	}

	public GridEntity (GridEntity model) {
		gridPoint = model.gridPoint;
	}
	public abstract GridEntity Clone ();
	protected virtual void OnChangeGridPointInternal (HexCoord lastGridPoint, HexCoord newGridPoint) {}
	public virtual void OnAdvanceTurn (int lastPlayerIndex) {}
	public virtual void Die () {
		if(OnDie != null) OnDie(this);
	}

	public override string ToString () {
        return string.Format("[{0}] gridPoint:{1}", GetType().Name, gridPoint);
	}
}