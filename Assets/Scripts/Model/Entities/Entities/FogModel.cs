using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class FogModel : GridEntity {
	public FogModel (HexCoord gridPoint) : base (gridPoint) {}
	protected FogModel (FogModel fog) : base (fog) {
		_revealed = fog._revealed;
	}
	public override GridEntity Clone () {
		return new FogModel(this);
	}
	bool _revealed;
	public bool revealed {
		get {
			return _revealed;
		} set {
			if(_revealed == value) return;
			_revealed = value;
			if(OnChangeRevealed != null) OnChangeRevealed(_revealed);
		}
	}

	public delegate void OnChangeRevealedState(bool revealed);
	public event OnChangeRevealedState OnChangeRevealed;


	public override string ToString () {
		return string.Format("[{0}] gridPoint {1}, revealed {2}", GetType().Name, gridPoint, revealed);
	}
}