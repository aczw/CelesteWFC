using System;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

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
    public Vector3Int? SelectedPos { get; private set; }

    public TMP_InputField widthInput;
    public TMP_InputField heightInput;
    public TMP_Text solve;
    public TMP_Text step;

    [SerializeField] private Tilemap placeholder;
    [SerializeField] private Tilemap selection;
    [SerializeField] private MouseDetection UI;
    [SerializeField] private GameObject content;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private InputSettings inputSettings;
    [SerializeField] private SelectionTiles selectionTiles;
    [SerializeField] private PlaceholderTiles placeholderTiles;

    private Camera cam;
    private Tilemap output;
    private Vector3Int prevHoveredTilePos;

    private void Awake() {
        SelectedPos = null;
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
                    placeholder.SetColor(new Vector3Int(x, CelesteWFC.I.gridSettings.height - 1 - y, 0), color);
                }
            }
        }

        selection.SetTile(prevHoveredTilePos, null);

        if (SelectedPos.HasValue) {
            var gb = 0.1f * Mathf.Sin(7f * Time.time) + 0.3f;
            selection.SetTile(SelectedPos.Value, selectionTiles.selected);
            selection.SetColor(SelectedPos.Value, new Color(1f, gb, gb));
        }

        // Only do input stuff if mouse is not currently over the side UI
        if (UI.IsHoveringOver) return;

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
            if (tilePos == SelectedPos) {
                // Unselect currently selected tile
                if (Input.GetMouseButtonDown(0)) {
                    ClearContent();
                    ClearSelectedTile();
                    step.text = "Step / Iterate";
                }

                return;
            }

            if (Input.GetMouseButtonDown(0)) {
                selection.SetTile(SelectedPos ?? new Vector3Int(-1, -1, 0), null);
                SelectedPos = tilePos;
                step.text = "Collapse this tile!";
                Populate(tilePos.x, tilePos.y);
            }
            else {
                selection.SetTile(tilePos, selectionTiles.hover);
                selection.SetColor(tilePos, new Color(1f, 1f, 1f, 0.3f));
                prevHoveredTilePos = tilePos;
            }
        }
    }

    private void Populate(int x, int y) {
        // Clears the children from the content GameObject before repopulating it.
        ClearContent();

        var cell = CelesteWFC.I.GetCell(x, y);
        foreach (var state in cell.states) {
            var tile = (Tile)state.tile;
            var obj = Instantiate(buttonPrefab, content.transform);

            obj.GetComponent<RectTransform>().Rotate(0f, 0f, -90f * state.timesRotatedClockwise);
            obj.GetComponent<Image>().sprite = tile.sprite;

            var button = obj.GetComponent<Button>();
            if (cell.IsCollapsed) {
                button.enabled = false;
                obj.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.35f);
            }
            else {
                button.onClick.AddListener(() => CelesteWFC.I.Iterate(x, y, state));
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

    public void ClearContent() {
        foreach (Transform child in content.transform) Destroy(child.gameObject);
    }

    public void ClearSelectedTile() {
        // If the selected tile is already cleared, just clear something outside the grid
        selection.SetTile(SelectedPos ?? new Vector3Int(-1, -1, 0), null);
        SelectedPos = null;
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

    public void CameraCeleste() {
        cam.transform.position = new Vector3(15.5f, 11.2f, -10);
        cam.orthographicSize = 14.5f;
    }
}