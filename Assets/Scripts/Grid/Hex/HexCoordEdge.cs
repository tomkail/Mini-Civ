using System.Linq;

public struct HexCoordEdge {
    public HexCoordVert start;
    public HexCoordVert end;
    
    public bool SeparatesCoords (HexCoord coordA, HexCoord coordB) {
		var sharedVerts = HexCoord.GetCornerIndiciesSharedWithOther(coordA, coordB);
        var thisCoord = this;
        return sharedVerts.Length == 2 && sharedVerts.All((vertIndex) => {
            var c = new HexCoordVert(coordA, vertIndex);
            return thisCoord.start == c || thisCoord.end == c;
        });
	}
}