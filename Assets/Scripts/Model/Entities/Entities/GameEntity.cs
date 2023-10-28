using System;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class GameEntity : GridEntity {
	[SerializeField]
	bool _flooded;
	[JsonIgnore]
	public bool flooded {
		get {
			return _flooded;
		} set {
			if(_flooded == value) return;
			_flooded = value;
			if(OnChangeFlooded != null) OnChangeFlooded(this);
		}
	}

	[JsonIgnore]
	public Action<GameEntity> OnChangeFlooded;
	public GameEntity (HexCoord gridPoint) : base (gridPoint) {}
	protected GameEntity (GameEntity model) : base (model) {
		this._flooded = model._flooded;
	}
	public override GridEntity Clone () {
		return new GameEntity(this);
	}
}
