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

        widthInput.onSelect.AddListener(_ => CelesteWFC.I.IsEditingGridSize = true);
        heightInput.onSelect.AddListener(_ => CelesteWFC.I.IsEditingGridSize = true);

        widthInput.onEndEdit.AddListener(width => {
            if (string.IsNullOrWhiteSpace(width)) {
                widthInput.text = CelesteWFC.I.gridSettings.width.ToString();
            }
            else {
                CelesteWFC.I.ResizeWidth(int.Parse(width));
            }

            CelesteWFC.I.IsEditingGridSize = false;
            CelesteWFC.I.SetDefaultPlaceholderFillColor();
        });
        heightInput.onEndEdit.AddListener(height => {
            if (string.IsNullOrWhiteSpace(height)) {
                heightInput.text = CelesteWFC.I.gridSettings.height.ToString();
            }
            else {
                CelesteWFC.I.ResizeHeight(int.Parse(height));
            }

            CelesteWFC.I.IsEditingGridSize = false;
            CelesteWFC.I.SetDefaultPlaceholderFillColor();
        });
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