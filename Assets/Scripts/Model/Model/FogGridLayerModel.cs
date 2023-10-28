using UnityEngine;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

public class FogGridLayerModel : GridLayerModel {
    [Newtonsoft.Json.JsonIgnore]
	public new List<FogModel> entities {
		get {
			return base.entities.Cast<FogModel>().ToList();
		}
	}

    protected FogGridLayerModel () : base () {}
    public FogGridLayerModel (BoardModel board, string name) : base(board, name) {}
	protected FogGridLayerModel (FogGridLayerModel model) : base (model) {}
	public override GridLayerModel Clone () {
		return new FogGridLayerModel(this);
	}
    
    public void ResetFog () {
        foreach(var fog in entities) {
            ((FogModel)fog).revealed = false;
        }
    }
    public void RevealAllFog () {
        foreach(var fog in entities) {
            ((FogModel)fog).revealed = true;
        }
    }
    public void RevealFog (HexCoord point, int radius = 1) {
        var pointsToReveal = HexCoord.GetPointsInRing(point, 0, radius);
        foreach(var fog in entities) {
            if(pointsToReveal.Contains(fog.gridPoint)) {
                ((FogModel)fog).revealed = true;
            }
        }
    }
}