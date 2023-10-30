using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityX.Geometry;
using Newtonsoft.Json;
using System.Runtime.Serialization;


public class StaticGridLayerModel : GridLayerModel {
	[Newtonsoft.Json.JsonProperty]
	public Dictionary<HexCoord, GridEntity> entities = new Dictionary<HexCoord, GridEntity>();
	
	[OnDeserialized]
	internal void OnDeserializedMethod(StreamingContext context) {
		foreach(var entity in entities.Values) {
			entity.OnDie += OnEntityDie;
		}
	}
	
	protected StaticGridLayerModel () {}
	public StaticGridLayerModel (BoardModel board, string name) {
		this.board = board;
		this.name = name;
	}
	protected StaticGridLayerModel (StaticGridLayerModel layerToClone) {
		name = layerToClone.name;
		foreach(var entity in layerToClone.entities) {
			entities.Add(entity.Key, entity.Value.Clone());
		}
	}
	public override GridLayerModel Clone () {
		return new StaticGridLayerModel(this);
	}
	
	
	
	public override GridEntity AddEntity(GridEntity newEntity) {
		Debug.Assert(!entities.ContainsKey(newEntity.gridPoint));
		entities.Add(newEntity.gridPoint, newEntity);
		newEntity.OnDie += OnEntityDie;
		FireOnAddCallback(newEntity);
		return newEntity;
	}
	
	protected override void OnEntityDie (GridEntity entity) {
		entity.OnDie -= OnEntityDie;
		Debug.Assert(RemoveEntity(entity));
	}

	public override bool RemoveEntity(GridEntity entity) {
		if(entities.Remove(entity.gridPoint)) {
			FireOnRemoveCallback(entity);
			return true;
		} else {
			return false;
		}
	}
	
	
	/// <summary>
	/// Clears the layer.
	/// </summary>
	public override void Clear () {
		if(entities == null) return;
		foreach(var entity in entities) {
			Debug.Assert(RemoveEntity(entity.Value));
		}
	}

	public override T GetValueAtGridPoint<T>(HexCoord coord) {
		if (entities.TryGetValue(coord, out GridEntity val)) return val as T;
		return null;
	}

	
	public override HexCoord GetRandomPoint () {
		return entities.Keys.Random();
	}

	public override IEnumerable<GridEntity> GetAllEntities() {
		return entities.Values;
	}
}

public class DynamicGridLayerModel : GridLayerModel {
	[Newtonsoft.Json.JsonProperty]
	public HashSet<GridEntity> entities = new HashSet<GridEntity>();
	
	[OnDeserialized]
	internal void OnDeserializedMethod(StreamingContext context) {
		foreach(var entity in entities) {
			entity.OnDie += OnEntityDie;
		}
	}
	
	protected DynamicGridLayerModel () {}
	public DynamicGridLayerModel (BoardModel board, string name) {
		this.board = board;
		this.name = name;
	}
	protected DynamicGridLayerModel (DynamicGridLayerModel layerToClone) {
		name = layerToClone.name;
		foreach(var entity in layerToClone.entities) {
			entities.Add(entity.Clone());
		}
	}
	public override GridLayerModel Clone () {
		return new DynamicGridLayerModel(this);
	}
	
	public override GridEntity AddEntity(GridEntity newEntity) {
		Debug.Assert(!entities.Contains(newEntity));
		entities.Add(newEntity);
		newEntity.OnDie += OnEntityDie;
		FireOnAddCallback(newEntity);
		return newEntity;
	}
	
	protected override void OnEntityDie (GridEntity entity) {
		entity.OnDie -= OnEntityDie;
		Debug.Assert(RemoveEntity(entity));
	}

	public override bool RemoveEntity(GridEntity entity) {
		if(entities.Remove(entity)) {
			FireOnRemoveCallback(entity);
			return true;
		} else {
			return false;
		}
	}

	/// <summary>
	/// Clears the layer.
	/// </summary>
	public override void Clear () {
		if(entities == null) return;
		foreach(var entity in entities) {
			Debug.Assert(RemoveEntity(entity));
		}
	}


	public override T GetValueAtGridPoint<T>(HexCoord coord) {
		foreach(var entity in entities) {
			if(entity.gridPoint == coord && entity is T) return (T)entity;
		}
		return null;
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
	
	public override IEnumerable<GridEntity> GetAllEntities() {
		return entities;
	}

	// private void HandleOnEntityDie (GridEntity entity) {
	// 	RemoveEntity(entity);
	// }

	public override HexCoord GetRandomPoint () {
		return entities.Random().gridPoint;
	}
}

[System.Serializable]
public abstract class GridLayerModel {
	


	public string name;
	[System.NonSerialized, Newtonsoft.Json.JsonIgnore]
	public BoardModel board;
	
	public delegate void OnAddEntityEvent(GridEntity newEntity);
	public event OnAddEntityEvent OnAddEntity;
	
	public delegate void OnRemoveEntityEvent(GridEntity removedEntity);
	public event OnRemoveEntityEvent OnRemoveEntity;
	
	protected GridLayerModel () {}
	public GridLayerModel (BoardModel board, string name) {
		this.board = board;
		this.name = name;
	}

	public abstract GridLayerModel Clone();
	
	

	protected void FireOnAddCallback(GridEntity entity) {
		if(OnAddEntity != null)
			OnAddEntity(entity);
	}

	protected void FireOnRemoveCallback(GridEntity entity) {
		if(OnRemoveEntity != null)
			OnRemoveEntity(entity);
	}
	
	public abstract GridEntity AddEntity(GridEntity newEntity);

	protected abstract void OnEntityDie (GridEntity entity);

	public abstract bool RemoveEntity(GridEntity entity);

	/// <summary>
	/// Clears the layer.
	/// </summary>
	public abstract void Clear ();

	public abstract HexCoord GetRandomPoint();

	public IEnumerable<T> OfType<T>() {
		foreach (object item in GetAllEntities()) { 
			if (item is T) yield return (T)item; 
		}
	}


	public abstract T GetValueAtGridPoint<T>(HexCoord coord) where T : GridEntity;
	public abstract IEnumerable<GridEntity> GetAllEntities();
}