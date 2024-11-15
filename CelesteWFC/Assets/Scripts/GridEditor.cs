using UnityEngine;

public class GridEditor : MonoBehaviour
{
    [Min(0f)] public float scrollFactor;
    [Min(0f)] public float sensitivity;

    private Camera cam;

    private void Start() {
        cam = Camera.main;
    }

    private void Update() {
        cam.orthographicSize += -Input.GetAxis("Mouse ScrollWheel") * scrollFactor;

        if (Input.GetMouseButton(1) || Input.GetMouseButton(2)) {
            cam.transform.Translate(-Input.GetAxis("Mouse X") * sensitivity,
                                    -Input.GetAxis("Mouse Y") * sensitivity,
                                    0f);
        }
    }
}