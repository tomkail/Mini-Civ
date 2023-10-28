using System.Linq;

[System.Serializable]
public class TerrainModel : GridEntity {
	public TerrainType type = TerrainType.Grass;
	
	[Newtonsoft.Json.JsonConstructor]
	protected TerrainModel () : base () {}

	public TerrainModel(HexCoord gridPoint, TerrainType terrainType) : base(gridPoint) {
		type = terrainType;
	}
	protected TerrainModel (TerrainModel floor) : base (floor) {
		
	}

	public override GridEntity Clone () {
		return new TerrainModel(this);
	}
	
	public override string ToString () {
		return string.Format ("[LandModel] GridPoint={0}, Type={1}", gridPoint, type);
	}
}