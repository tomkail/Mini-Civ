using UnityEngine;

public class GameInputController : MonoSingleton<GameInputController> {
    public bool hoveringOverLegacyGUI => GUIUtility.hotControl != 0;

    public bool hoveringOverUI => EventSystemX.Raycast(Input.mousePosition).isValid;

    public bool hoveringOverGameView => ScreenX.screenRect.Contains(Input.mousePosition);

    [Disable]
    public bool lastClickStartedOverLegacyGUI;
    [Disable]
    public bool lastClickStartedOverUI;

    public Vector3 worldPosition => GetWorldPosition(Input.mousePosition);

    public HexCoord gridPoint => GetGridPoint(Input.mousePosition);

    public Vector3 GetWorldPosition (Vector2 mousePosition) => ScreenToFloorPoint(Camera.main, mousePosition);

    public HexCoord GetGridPoint (Vector2 mousePosition) => GameController.Instance.hexGrid.WorldToAxial(GetWorldPosition(mousePosition));
    

    public Vector3 ScreenToFloorPoint (Camera camera, Vector2 screenPoint) {
        var ray = camera.ScreenPointToRay(screenPoint);
        return GameController.Instance.hexGrid.GetRayHitPoint(ray);
    }
	
    void Update () {
        // Debug.Log(actions.Dash.IsPressed+" "+ actions.Move.Vector.normalized);
        if (Input.GetMouseButtonDown(0)) {
            lastClickStartedOverLegacyGUI = hoveringOverLegacyGUI;
            lastClickStartedOverUI = hoveringOverUI;
        }
        if (Input.GetMouseButtonUp(0)) {
            lastClickStartedOverLegacyGUI = false;
            lastClickStartedOverUI = false;
        }
    }

    void OnDrawGizmosSelected () {
        Gizmos.DrawLine(Camera.main.transform.position, ScreenToFloorPoint(Camera.main, Input.mousePosition));
    }
}