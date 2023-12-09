using UnityEngine;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

public class FogGridLayerModel : StaticGridLayerModel {
    protected FogGridLayerModel () : base () {}
    public FogGridLayerModel (BoardModel board, string name) : base(board, name) {}
	protected FogGridLayerModel (FogGridLayerModel model) : base (model) {}
	public override GridLayerModel Clone () {
		return new FogGridLayerModel(this);
	}
    
    public void ResetFog () {
        foreach(var fog in OfType<FogModel>()) {
            fog.revealed = false;
        }
    }
    public void RevealAllFog () {
        foreach(var fog in OfType<FogModel>()) {
            fog.revealed = true;
        }
    }
    public void RevealFog (HexCoord point, int radius = 0) {
        var pointsToReveal = HexCoord.GetPointsInRing(point, 0, radius);
        foreach(var fog in OfType<FogModel>()) {
            if(pointsToReveal.Contains(fog.gridPoint)) {
                ((FogModel)fog).revealed = true;
            }
        }
    }
}