using UnityEngine;
using System.Collections.Generic;


public abstract class LevelGenerator : MonoBehaviour {
	public abstract GameModel GenerateLevel ();
}
