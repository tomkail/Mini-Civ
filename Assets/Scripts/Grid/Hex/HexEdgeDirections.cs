using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

[System.Serializable]
public class HexEdgeDirections {
	const int numEdges = 6;
	[SerializeField, JsonProperty]
	bool[] _edgeDirections = new bool[numEdges];
	[JsonIgnore]
	public ReadOnlyCollection<bool> directions {
		get {
			return _edgeDirections.ToList().AsReadOnly();
		}
	}


	[JsonIgnore]
	public bool this[int directionIndex] { 
		get {
			return this._edgeDirections[directionIndex];
		} set { 
			if(this._edgeDirections[directionIndex] == value) return;
			this._edgeDirections[directionIndex] = value;
			if(OnChangeDirections != null) OnChangeDirections(this);
		}
	}
	public delegate void OnChangeDirectionsDelegate(HexEdgeDirections edgeDirections);
	[JsonIgnore]
	public OnChangeDirectionsDelegate OnChangeDirections;

	[JsonIgnore]
	public int directionCount {
		get {
			int num = 0;
			foreach(var x in _edgeDirections)
				if(x) num++;
			return num;
		}
	}

	public static HexEdgeDirections All {
		get {
			return new HexEdgeDirections(new bool[]{true,true,true,true,true,true});
		}
	}
	public HexEdgeDirections () {}
	public HexEdgeDirections (IList<bool> _roadDirections) {
		Set(_roadDirections);
	}

	protected HexEdgeDirections (HexEdgeDirections model)  {
		Set(model._edgeDirections);
	}
	public HexEdgeDirections Clone () {
		return new HexEdgeDirections(this);
	}

	public void Set(HexEdgeDirections roadDirections) {
		Set(roadDirections._edgeDirections);
	}
	public void Set(IList<bool> newRoadDirections) {
		Debug.Assert(newRoadDirections.Count == numEdges);
		for(int i = 0; i < newRoadDirections.Count; i++) {
			this[i] = newRoadDirections[i];
		}
	}
	public void Add(HexEdgeDirections toAdd) {
		foreach(var d in toAdd.GetValidDirectionIndicies()) {
			this[d] = true;
		}
	}

	public void Rotate (int offset) {
		_edgeDirections = ArrayX.GetShiftedRepeating(_edgeDirections, offset);
		if(OnChangeDirections != null) OnChangeDirections(this);
	}
	public void Reverse () {
		// _edgeDirections = 
		System.Array.Reverse(_edgeDirections);
		if(OnChangeDirections != null) OnChangeDirections(this);
	}

	public IEnumerable<int> GetValidDirectionIndicies () {
		return _edgeDirections.IndexesWhere(x => x);
	}

	public bool IsSetInDirection (int directionIndex) {
		return this[directionIndex.Mod(directions.Count)];
	}

	public static bool Connected (HexCoord coord, IEnumerable<int> validDirections, HexCoord targetCoord) {
        if(HexCoord.Distance(coord, targetCoord) != 1) return false;
		var directionBetweenCoords = HexCoord.GetClosestDirectionIndex(coord, targetCoord);
        foreach(var otherDirection in validDirections) {
			var otherAbsDeltaDirection = Mathf.Abs(HexUtils.SignedDeltaDirection(otherDirection, directionBetweenCoords));
			if(otherAbsDeltaDirection == 0) return true;
		}
        return false;
    }
	
	public static bool Connected (HexCoord coordA, HexEdgeDirections directionsA, HexCoord coordB, HexEdgeDirections directionsB) {
        return Connected(coordA, directionsA.GetValidDirectionIndicies(), coordB, directionsB.GetValidDirectionIndicies());
    }

	public static bool Connected (HexCoord coordA, IEnumerable<int> validDirectionsA, HexCoord coordB, IEnumerable<int> validDirectionsB) {
        if(HexCoord.Distance(coordA, coordB) != 1) return false;
		var directionBetweenCoords = HexCoord.GetClosestDirectionIndex(coordA, coordB);
        foreach(var direction in validDirectionsB) {
			var absDeltaDirection = Mathf.Abs(HexUtils.SignedDeltaDirection(direction, directionBetweenCoords));
			if(absDeltaDirection != 3) continue;
            foreach(var otherDirection in validDirectionsA) {
				var otherAbsDeltaDirection = Mathf.Abs(HexUtils.SignedDeltaDirection(otherDirection, directionBetweenCoords));
				if(otherAbsDeltaDirection == 0) return true;
            }
        }
        return false;
    }

    public override string ToString() {
        return string.Format("[{0}] Valid Directions:{1}", GetType().Name, DebugX.ListAsString(GetValidDirectionIndicies()));
    }
}