using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityX.Geometry;

public interface IGridLayerModel {
	Grid grid {get;set;}
	void Loop ();
	void Clear ();
	
	List<GridEntity> entities {get;set;}
	void AddEntity(GridEntity newEntity);
	void RemoveEntity(GridEntity newEntity);
	GridEntity Get(int x, int y);
	GridEntity Get(Vector2Int gridPosition);
	
	IList<Vector2Int> GetEmptyGridPositions();
	bool GetEmptyGridPosition(out Vector2Int gridPosition);
}