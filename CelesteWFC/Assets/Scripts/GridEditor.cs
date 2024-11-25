using System;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable] public struct InputSettings
{
    [Min(0f)] public float scrollFactor;
    [Min(0f)] public float sensitivity;
}

[Serializable] public struct PlaceholderTiles
{
    public TileBase fill;
    public TileBase line;
    public TileBase corner;
}

[Serializable] public struct SelectionTiles
{
    public TileBase hover;
    public TileBase selected;
}

public class GridEditor : MonoBehaviour
{
    private bool IsEditingGridSize { get; set; }

    public TMP_InputField widthInput;
    public TMP_InputField heightInput;

    public Tilemap placeholder;
    public Tilemap selection;

    public InputSettings inputSettings;
    public SelectionTiles selectionTiles;
    public PlaceholderTiles placeholderTiles;

    private Camera cam;
    private Tilemap output;
    private Vector3Int prevHoveredTilePos;
    private Vector3Int? selectedPos;

    private void Awake() {
        selectedPos = null;
    }

    private void Start() {
        // Clears any placeholders for the placeholder in the editor....
        RedrawPlaceholder();

        output = CelesteWFC.I.output;
        cam = Camera.main;

        widthInput.onSelect.AddListener(_ => IsEditingGridSize = true);
        heightInput.onSelect.AddListener(_ => IsEditingGridSize = true);

        widthInput.onEndEdit.AddListener(width => {
            if (string.IsNullOrWhiteSpace(width)) {
                widthInput.text = CelesteWFC.I.gridSettings.width.ToString();
            }
            else {
                CelesteWFC.I.ResizeWidth(int.Parse(width));
            }

            IsEditingGridSize = false;
            SetDefaultPlaceholderFillColor();
        });
        heightInput.onEndEdit.AddListener(height => {
            if (string.IsNullOrWhiteSpace(height)) {
                heightInput.text = CelesteWFC.I.gridSettings.height.ToString();
            }
            else {
                CelesteWFC.I.ResizeHeight(int.Parse(height));
            }

            IsEditingGridSize = false;
            SetDefaultPlaceholderFillColor();
        });
    }

    private void Update() {
        if (IsEditingGridSize) {
            var color = new Color(1f, 1f, 1f, 0.15f * Mathf.Sin(7f * Time.time) + 0.3f);
            for (var y = 0; y < CelesteWFC.I.gridSettings.height; ++y) {
                for (var x = 0; x < CelesteWFC.I.gridSettings.width; ++x) {
                    if (CelesteWFC.I.IsCollapsed(x, y)) continue;
                    placeholder.SetColor(new Vector3Int(x, y, 0), color);
                }
            }
        }

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

    private void SetDefaultPlaceholderFillColor() {
        var transparent = new Color(1f, 1f, 1f, 0.3f);
        for (var y = 0; y < CelesteWFC.I.gridSettings.height; ++y) {
            for (var x = 0; x < CelesteWFC.I.gridSettings.width; ++x) {
                var position = new Vector3Int(x, y, 0);
                placeholder.SetTile(position, placeholderTiles.fill);
                placeholder.SetColor(position, transparent);
            }
        }
    }

    public void RedrawPlaceholder() {
        placeholder.ClearAllTiles();
        SetDefaultPlaceholderFillColor();

        var width = CelesteWFC.I.gridSettings.width;
        var height = CelesteWFC.I.gridSettings.height;

        // Draw top and bottom borders
        var bottomRotMat = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, 180f));
        for (var x = 0; x < width; ++x) {
            placeholder.SetTile(new Vector3Int(x, height, 0), placeholderTiles.line);

            var bottomPosition = new Vector3Int(x, -1, 0);
            placeholder.SetTile(bottomPosition, placeholderTiles.line);
            placeholder.SetTransformMatrix(bottomPosition, bottomRotMat);
        }

        // Draw left and right borders
        var leftRotMat = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, 90f));
        var rightRotMat = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, -90f));
        for (var y = 0; y < height; ++y) {
            var leftPosition = new Vector3Int(-1, y, 0);
            var rightPosition = new Vector3Int(width, y, 0);

            placeholder.SetTile(leftPosition, placeholderTiles.line);
            placeholder.SetTransformMatrix(leftPosition, leftRotMat);
            placeholder.SetTile(rightPosition, placeholderTiles.line);
            placeholder.SetTransformMatrix(rightPosition, rightRotMat);
        }

        // Draw corners
        var tl = new Vector3Int(-1, height, 0);
        var bl = new Vector3Int(-1, -1, 0);
        var tr = new Vector3Int(width, height, 0);
        var br = new Vector3Int(width, -1, 0);
        placeholder.SetTile(tl, placeholderTiles.corner);
        placeholder.SetTile(bl, placeholderTiles.corner);
        placeholder.SetTile(tr, placeholderTiles.corner);
        placeholder.SetTile(br, placeholderTiles.corner);
        placeholder.SetTransformMatrix(bl, leftRotMat);
        placeholder.SetTransformMatrix(tr, Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, 270f)));
        placeholder.SetTransformMatrix(br, bottomRotMat);
    }
}