using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class SetCameraDepthTextureMode : MonoBehaviour {
	public DepthTextureMode mode;

	void Awake () {
		GetComponent<Camera>().depthTextureMode = mode;
	}

	void Update () {
		GetComponent<Camera>().depthTextureMode = mode;
	}
}