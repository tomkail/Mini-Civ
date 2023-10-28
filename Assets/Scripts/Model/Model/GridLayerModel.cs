using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityX.Geometry;
using Newtonsoft.Json;
using System.Runtime.Serialization;


[System.Serializable]
public class GridLayerModel {
	[OnDeserialized]
    internal void OnDeserializedMethod(StreamingContext context) {
		foreach(var entity in entities) {
			entity.OnDie += OnEntityDie;
		}
    }


	public string name;
	[System.NonSerialized, Newtonsoft.Json.JsonIgnore]
	public BoardModel board;
	[Newtonsoft.Json.JsonProperty]
	public HashSet<GridEntity> entities = new HashSet<GridEntity>();
	
	public delegate void OnAddEntityEvent(GridEntity newEntity);
	public event OnAddEntityEvent OnAddEntity;
	
	public delegate void OnRemoveEntityEvent(GridEntity removedEntity);
	public event OnRemoveEntityEvent OnRemoveEntity;
	
	protected GridLayerModel () {}
	public GridLayerModel (BoardModel board, string name) {
		this.board = board;
		this.name = name;
	}

	protected GridLayerModel (GridLayerModel layerToClone) {
		name = layerToClone.name;
		foreach(var entity in layerToClone.entities) {
			entities.Add(entity.Clone());
		}
	}
	public virtual GridLayerModel Clone () {
		return new GridLayerModel(this);
	}
	
	public virtual GridEntity AddEntity(GridEntity newEntity) {
		Debug.Assert(!entities.Contains(newEntity));
		entities.Add(newEntity);
		newEntity.OnDie += OnEntityDie;
		if(OnAddEntity != null)
			OnAddEntity(newEntity);
		return newEntity;
	}

	void OnEntityDie (GridEntity entity) {
		entity.OnDie -= OnEntityDie;
		Debug.Assert(RemoveEntity(entity));
	}

	public virtual bool RemoveEntity(GridEntity entity) {
		if(entities.Remove(entity)) {
			if(OnRemoveEntity != null)
				OnRemoveEntity(entity);
			return true;
		} else {
			return false;
		}
	}

	/// <summary>
	/// Clears the layer.
	/// </summary>
	public virtual void Clear () {
		if(entities == null) return;
		foreach(var entity in entities) {
			Debug.Assert(RemoveEntity(entity));
		}
	}

	public IEnumerable<T> GetValuesAtGridPoint<T>(HexCoord coord) where T : GridEntity {
		foreach(var entity in GetValuesAtGridPoint(coord)) {
			if(entity != null && entity is T) yield return (T)entity;
		}
	}
	public IEnumerable<GridEntity> GetValuesAtGridPoint(HexCoord coord) {
		foreach(var entity in entities) {
			if(entity.gridPoint == coord) yield return entity;
		}
	}
	public IEnumerable<T> OfType<T>() {
		foreach (object item in entities) { 
        	if (item is T) yield return (T)item; 
    	}
	}

	// private void HandleOnEntityDie (GridEntity entity) {
	// 	RemoveEntity(entity);
	// }

	public HexCoord GetRandomPoint () {
		return entities.Random().gridPoint;
	}
}