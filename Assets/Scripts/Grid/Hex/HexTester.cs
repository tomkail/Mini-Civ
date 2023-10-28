using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTester : MonoBehaviour
{
    public HexCoord dir;
    public int dirIndex;
    public int dist;
    public HexCoord a;
    public HexCoord b;
    public int dirBetween;
    public float edgeA;
    public int cornerA1;
    public int cornerA2;
    public int cornerB1;
    public int cornerB2;

    public Vector3 gridPos;
    public Vector3 worldPos;
    public Vector3 worldPos2;
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnDrawGizmosSelected ()
    {
        dirIndex = HexCoord.ClosestDirectionIndex(dir);
        // worldPos = BoardView.Instance.GridToWorldPoint(gridPos, gridPos.z);
        worldPos2 = transform.localToWorldMatrix.MultiplyPoint3x4(gridPos);

        // Gizmos.DrawSphere(GetEdgePositionInGridSpace(a, BoardView.EdgeToCornerSpace(edgeA)), 0.15f);
        Gizmos.DrawSphere(HexCoord.Corner(a, cornerA1), 0.25f);
        Gizmos.DrawSphere(HexCoord.Corner(a, cornerA2), 0.25f);
        Gizmos.DrawSphere(HexCoord.Corner(b, cornerB1), 0.25f);
        Gizmos.DrawSphere(HexCoord.Corner(b, cornerB2), 0.25f);
        Gizmos.DrawSphere(b.Position(), 0.5f);

        Gizmos.DrawSphere(a.Position(), 0.5f);
        Gizmos.DrawSphere(b.Position(), 0.5f);
        Gizmos.DrawLine(a.Position(), b.Position());
        dist = HexCoord.Distance(a,b);
        if(dist == 1) dirBetween = HexCoord.GetClosestDirectionIndex(a,b);
        else dirBetween = -1000;
    }

    public Vector2 GetEdgePositionInGridSpace (HexCoord coord, float cornerEdge) {
		cornerEdge = Mathf.Repeat(cornerEdge, 6);
		float frac = cornerEdge % 1;
		int start = Mathf.FloorToInt(cornerEdge);
		int end = Mathf.CeilToInt(cornerEdge);
		return Vector2.Lerp(coord.Corner(start), coord.Corner(end), frac);
	}
}
