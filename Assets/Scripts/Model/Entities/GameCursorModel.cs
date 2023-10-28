using UnityEngine;
using System.Linq;

[System.Serializable]
public class GameCursorModel : GridEntity {
	[SerializeField]
	bool _enabled = true;
	public bool enabled {
		get {
			return _enabled;
		} set {
			if(_enabled == value) return;
			_enabled = value;
			if(_enabled) OnEnable();
			else OnDisable();
			if(OnChangeEnabled != null) OnChangeEnabled(_enabled);
		}
	}

	[Newtonsoft.Json.JsonConstructor]
	protected GameCursorModel () : base () {}

	public GameCursorModel (GameModel gameModel, HexCoord gridPoint) : base (gridPoint) {}
	protected GameCursorModel (GameCursorModel model) : base (model) {}
	public override GridEntity Clone () {
		return new GameCursorModel(this);
	}

	public delegate void OnChangeEnabledDelegate(bool enabled);
	public event OnChangeEnabledDelegate OnChangeEnabled;

	void OnEnable () {
		Refresh();
	}

	void OnDisable () {
		Refresh();	
	}

	public void Update () {
		Refresh();
	}

	protected override void OnChangeGridPointInternal (HexCoord lastGridPoint, HexCoord newGridPoint) {
		base.OnChangeGridPointInternal (lastGridPoint, newGridPoint);
		Refresh();
	}

	void Refresh () {
		// bool hoveringValidCoord = GameController.Instance.player.validMoves.Contains(gridPoint);
	}

	public void SetToDefaultPoint() {
		gridPoint = HexCoord.zero;
	}
}
