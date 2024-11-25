using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable] public struct GridSize
{
    [Min(1)] public int width;
    [Min(1)] public int height;
}

[Serializable] public struct PlaceholderTiles
{
    public TileBase fill;
    public TileBase line;
    public TileBase corner;
}

public class CelesteWFC : MonoBehaviour
{
    public static CelesteWFC I { get; private set; }

    [SerializeField] private Tilemap output;
    [SerializeField] private Tilemap placeholder;

    [SerializeField] private Palette palette;
    [SerializeField] private GridEditor editor;

    public PlaceholderTiles placeholderTiles;
    public GridSize gridSettings;

    private WaveFunctionCollapse wfc;

    private void Awake() {
        if (I == null) {
            I = this;
        }
        else {
            Destroy(gameObject);
        }

        wfc = new WaveFunctionCollapse(gridSettings.width, gridSettings.height, palette);
        editor.widthInput.text = gridSettings.width.ToString();
        editor.heightInput.text = gridSettings.height.ToString();

        // Clears any placeholders for the placeholder in the editor....
        RedrawPlaceholder();
    }

    private void Update() {
        var color = new Color(1f, 1f, 1f, 0.1f * Mathf.Sin(5f * Time.time) + 0.3f);
        for (var y = 0; y < wfc.height; ++y) {
            for (var x = 0; x < wfc.width; ++x) {
                if (wfc.grid[wfc.height - y - 1, x].IsCollapsed) continue;
                placeholder.SetColor(new Vector3Int(x, y, 0), color);
            }
        }
    }

    /// <summary>
    ///     Draws currently collapsed tiles to the tilemap, if any. Note: because tiles are arranged in a
    ///     2D array, the topmost row has an index of 0; value of Y increases going *down.* This means we should
    ///     draw the rows top down!
    /// </summary>
    private void Paint() {
        for (var y = 0; y < wfc.height; ++y) {
            for (var x = 0; x < wfc.width; ++x) {
                var cell = wfc.grid[y, x];

                if (cell.IsCollapsed) {
                    var state = cell.states[0];
                    var position = new Vector3Int(x, wfc.height - 1 - y, 0);

                    output.SetTile(position, state.tile);

                    var angle = -90f * state.timesRotatedClockwise;
                    var rotMat = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, angle));

                    output.SetTransformMatrix(position, rotMat);
                }
            }
        }
    }

    private void RedrawPlaceholder() {
        placeholder.ClearAllTiles();

        var transparent = new Color(1f, 1f, 1f, 0.3f);
        for (var y = 0; y < wfc.height; ++y) {
            for (var x = 0; x < wfc.width; ++x) {
                var position = new Vector3Int(x, y, 0);
                placeholder.SetTile(position, placeholderTiles.fill);
                placeholder.SetColor(position, transparent);
            }
        }

        // Draw top and bottom borders
        var bottomRotMat = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, 180f));
        for (var x = 0; x < wfc.width; ++x) {
            placeholder.SetTile(new Vector3Int(x, wfc.height, 0), placeholderTiles.line);

            var bottomPosition = new Vector3Int(x, -1, 0);
            placeholder.SetTile(bottomPosition, placeholderTiles.line);
            placeholder.SetTransformMatrix(bottomPosition, bottomRotMat);
        }

        // Draw left and right borders
        var leftRotMat = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, 90f));
        var rightRotMat = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, -90f));
        for (var y = 0; y < wfc.height; ++y) {
            var leftPosition = new Vector3Int(-1, y, 0);
            var rightPosition = new Vector3Int(wfc.width, y, 0);

            placeholder.SetTile(leftPosition, placeholderTiles.line);
            placeholder.SetTransformMatrix(leftPosition, leftRotMat);
            placeholder.SetTile(rightPosition, placeholderTiles.line);
            placeholder.SetTransformMatrix(rightPosition, rightRotMat);
        }

        // Draw corners
        var tl = new Vector3Int(-1, wfc.height, 0);
        var bl = new Vector3Int(-1, -1, 0);
        var tr = new Vector3Int(wfc.width, wfc.height, 0);
        var br = new Vector3Int(wfc.width, -1, 0);
        placeholder.SetTile(tl, placeholderTiles.corner);
        placeholder.SetTile(bl, placeholderTiles.corner);
        placeholder.SetTile(tr, placeholderTiles.corner);
        placeholder.SetTile(br, placeholderTiles.corner);
        placeholder.SetTransformMatrix(bl, leftRotMat);
        placeholder.SetTransformMatrix(tr, Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, 270f)));
        placeholder.SetTransformMatrix(br, bottomRotMat);
    }

    public void Iterate() {
        if (wfc.IsCollapsed()) {
            Debug.Log("WFC is done!");
            return;
        }

        wfc.Iterate();
        Paint();
    }

    public void Solve() {
        while (!wfc.IsCollapsed()) {
            wfc.Iterate();
        }

        Paint();
    }

    public void Reset() {
        wfc = new WaveFunctionCollapse(gridSettings.width, gridSettings.height, palette);

        output.ClearAllTiles();
        Paint();
    }

    public void ResizeWidth(int width) {
        wfc = new WaveFunctionCollapse(width, gridSettings.height, palette);
        gridSettings.width = width;

        output.ClearAllTiles();
        RedrawPlaceholder();
        Paint();
    }

    public void ResizeHeight(int height) {
        wfc = new WaveFunctionCollapse(gridSettings.width, height, palette);
        gridSettings.height = height;

        output.ClearAllTiles();
        RedrawPlaceholder();
        Paint();
    }
}