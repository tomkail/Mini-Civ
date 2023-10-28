using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class TerrainGridLayerModel : GridLayerModel {
	[Newtonsoft.Json.JsonIgnore]
	public new List<TerrainModel> entities {
		get {
			return base.entities.Cast<TerrainModel>().ToList();
		}
	}

	protected TerrainGridLayerModel () : base () {}
	public TerrainGridLayerModel (BoardModel board, string name) : base(board, name) {}
	protected TerrainGridLayerModel (TerrainGridLayerModel model) : base (model) {}
	public override GridLayerModel Clone () {
		return new TerrainGridLayerModel(this);
	}
}