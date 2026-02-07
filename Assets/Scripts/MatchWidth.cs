using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class MatchWidth : MonoBehaviour {

    public float sceneWidth = 10;

    private Camera cam;
    private int cachedScreenWidth;
    private int cachedScreenHeight;

    private void Start() {
        cam = GetComponent<Camera>();
    }

    private void Update() {
        if (Screen.width == cachedScreenWidth && Screen.height == cachedScreenHeight) return;

        cachedScreenWidth = Screen.width;
        cachedScreenHeight = Screen.height;

        var unitsPerPixel = sceneWidth / cachedScreenWidth;
        var desiredHalfHeight = 0.5f * unitsPerPixel * cachedScreenHeight;
        cam.orthographicSize = desiredHalfHeight;
    }
}
