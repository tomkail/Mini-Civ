using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
[System.Serializable]
public class PlayerModel {
	[JsonIgnore]
	[System.NonSerialized]
	public GameModel gameModel;
	[SerializeField, Disable]
	int _money;
	public int money {
		get => _money;
		set {
			if(_money == value) return;
			_money = value;
			if(OnChangeMoney != null) OnChangeMoney(_money);
		}
	}
	[JsonIgnore]
	public Action<int> OnChangeMoney;
	
	public PlayerModel (GameModel gameModel) {
		this.gameModel = gameModel;
	}
	protected PlayerModel (GameModel gameModel, PlayerModel playerModel) {
		this.gameModel = gameModel;
		money = playerModel.money;
	}
	public PlayerModel Clone (GameModel gameModel) {
		return new PlayerModel(gameModel, this);
	}
}