using System;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable] public struct InputSettings
{
    [Min(0f)] public float scrollFactor;
    [Min(0f)] public float sensitivity;
}

[Serializable] public struct SelectionTiles
{
    public TileBase hover;
    public TileBase selected;
}

public class GridEditor : MonoBehaviour
{
    public TMP_InputField widthInput;
    public TMP_InputField heightInput;
    public Tilemap selection;
    public InputSettings inputSettings;
    public SelectionTiles selectionTiles;

    private Camera cam;
    private Tilemap output;
    private Vector3Int prevHoveredTilePos;
    private Vector3Int? selectedPos;

    private void Awake() {
        selectedPos = null;
    }

    private void Start() {
        output = CelesteWFC.I.output;
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
        selection.SetTile(prevHoveredTilePos, null);

        if (selectedPos.HasValue) {
            var gb = 0.1f * Mathf.Sin(7f * Time.time) + 0.3f;
            selection.SetTile(selectedPos.Value, selectionTiles.selected);
            selection.SetColor(selectedPos.Value, new Color(1f, gb, gb));
        }

        cam.orthographicSize += -Input.GetAxis("Mouse ScrollWheel") * inputSettings.scrollFactor;
        if (Input.GetMouseButton(1) || Input.GetMouseButton(2)) {
            cam.transform.Translate(-Input.GetAxis("Mouse X") * inputSettings.sensitivity,
                                    -Input.GetAxis("Mouse Y") * inputSettings.sensitivity,
                                    0f);
        }

        var mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = 0f;
        var mouseWorldPos = cam.ScreenToWorldPoint(mouseScreenPos);
        var tilePos = output.WorldToCell(mouseWorldPos);

        if (tilePos.x >= 0 && tilePos.x < CelesteWFC.I.gridSettings.width
                           && tilePos.y >= 0 && tilePos.y < CelesteWFC.I.gridSettings.height) {
            if (tilePos == selectedPos) {
                if (Input.GetMouseButtonDown(0)) {
                    selection.SetTile(selectedPos.Value, null);
                    selectedPos = null;
                }

                return;
            }

            if (Input.GetMouseButtonDown(0)) {
                selection.SetTile(selectedPos ?? new Vector3Int(-1, -1, 0), null);
                selectedPos = tilePos;
            }
            else {
                selection.SetTile(tilePos, selectionTiles.hover);
                selection.SetColor(tilePos, new Color(1f, 1f, 1f, 0.3f));
                prevHoveredTilePos = tilePos;
            }
        }
    }
}