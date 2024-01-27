using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class MatchWidth : MonoBehaviour {

    // Set this to the in-world distance between the left & right edges of your scene.
    public float sceneWidth = 10;

    private Camera cam;

    private void Start() {
        cam = GetComponent<Camera>();
    }

    // Adjust the camera's height so the desired scene width fits in view
    // even if the screen/window size changes dynamically.
    private void Update() {
        var unitsPerPixel = sceneWidth / Screen.width;

        var desiredHalfHeight = 0.5f * unitsPerPixel * Screen.height;

        cam.orthographicSize = desiredHalfHeight;
    }
}