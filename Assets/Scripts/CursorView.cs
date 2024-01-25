using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class CursorView : MonoBehaviour {
    public Shapes.Polygon polygon;

    void Update() {
        // polygon.points = HexCoord.CornerVectors().ToList();
        transform.position = GameController.Instance.hexGrid.AxialToWorld(GameController.Instance.gameModel.cursor.gridPoint);
        // GameController.Instance.terrainTilemap.GetTile(GameController.Instance.gameModel.cursor.gridPoint).GetTileData();
        // Debug.Log();
        // m_Tilemap.SwapTile(pair.Key, pair.Value);
    }
}
