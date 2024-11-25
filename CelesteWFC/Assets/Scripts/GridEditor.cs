using System;
using TMPro;
using UnityEngine;

[Serializable] public struct InputSettings
{
    [Min(0f)] public float scrollFactor;
    [Min(0f)] public float sensitivity;
}

public class GridEditor : MonoBehaviour
{
    public TMP_InputField widthInput;
    public TMP_InputField heightInput;
    public InputSettings inputSettings;

    private Camera cam;

    private void Start() {
        cam = Camera.main;
        widthInput.onEndEdit.AddListener(width => CelesteWFC.I.ResizeWidth(int.Parse(width)));
        heightInput.onEndEdit.AddListener(height => CelesteWFC.I.ResizeHeight(int.Parse(height)));
    }

    private void Update() {
        cam.orthographicSize += -Input.GetAxis("Mouse ScrollWheel") * inputSettings.scrollFactor;

        if (Input.GetMouseButton(1) || Input.GetMouseButton(2)) {
            cam.transform.Translate(-Input.GetAxis("Mouse X") * inputSettings.sensitivity,
                                    -Input.GetAxis("Mouse Y") * inputSettings.sensitivity,
                                    0f);
        }
    }
}