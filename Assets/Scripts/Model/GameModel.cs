using UnityEngine;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using UnityX.Geometry;
using Newtonsoft.Json;
using System.Runtime.Serialization;

[System.Serializable]
public class GameModel {
	static int GetAndIncrementGameID () {
		gameIDCount++;
		return gameIDCount;
	}
	[OnDeserialized]
	internal void OnDeserializedMethod(StreamingContext context) {
		gameID = GetAndIncrementGameID();
		board.gameModel = this;
		player.gameModel = this;
		cursor = new GameCursorModel(this, HexCoord.zero);
		board.OnDeserializedMethod();
	}

	public static GameModel current;
	static int gameIDCount;
	public bool isLookahead = false;
	public int gameID;
	
	public delegate void TurnEvent (int turnNumber);
	public event TurnEvent OnStartTurn;
	public event TurnEvent OnEndTurn;
	[SerializeField, Disable, Newtonsoft.Json.JsonProperty]
	int _currentTurn = -1;
	[Newtonsoft.Json.JsonIgnore]
	public int currentTurn {
		get {
			return _currentTurn;
		} private set {
			_currentTurn = value;
		}
	}

	public BoardModel board;
	[Newtonsoft.Json.JsonIgnore]
	public GameCursorModel cursor;
	public PlayerModel player;

	[Newtonsoft.Json.JsonIgnore]
	public string turnAndPhaseLogPrefix {
		get {
			return "Turn:"+currentTurn;
		}
	}
	public EntityIDDictionary idDictionary = new EntityIDDictionary();
	
	public GameModel () {}

	public void Init () {
		gameID = GetAndIncrementGameID();
		// inkInteractableUtils = new InkInteractableUtils();
		board = new BoardModel(this);
		cursor = new GameCursorModel(this, HexCoord.zero);
		player = new PlayerModel(this);
	}

	protected GameModel (GameModel gameModel) : this () {
		Init();
		board = gameModel.board.Clone(this);
		isLookahead = gameModel.isLookahead;
		_currentTurn = gameModel.currentTurn;
		cursor = new GameCursorModel(this, HexCoord.zero);
		player = gameModel.player.Clone(this);
		idDictionary = gameModel.idDictionary.Clone();
		
        
        OnDeserializedMethod(new StreamingContext());
	}
	public GameModel Clone () {
		return new GameModel(this);
	}

	public void StartGame () {
        currentTurn = 0;
		StartTurn();
	}

	public void Update () {
		cursor.Update();
	}


	public void AdvanceTurn () {
		EndTurn();
		currentTurn++;
		StartTurn();
	}

	void StartTurn () {
		Debug.Log("Start turn "+currentTurn);
  		if(OnStartTurn != null) OnStartTurn(currentTurn);
	}
	
	void EndTurn () {
		if(OnEndTurn != null) OnEndTurn(currentTurn);
	}
	
	public IEnumerable<GridCellModel> GetCells () {
		foreach(var floor in board.landLayer.entities) {
			yield return GetCell(floor.gridPoint);
		}
	}
	public GridCellModel GetCell (HexCoord coord) {
		return new GridCellModel(this, coord);
	}
}