using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable] public struct GridSize
{
    [Min(1)] public int width;
    [Min(1)] public int height;
}

public class CelesteWFC : MonoBehaviour
{
    public static CelesteWFC I { get; private set; }

    public Tilemap output;
    public PaletteSet paletteSet;
    public GridEditor editor;
    public GridSize gridSettings;

    private WaveFunctionCollapse wfc;

    private void Awake() {
        if (I == null) {
            I = this;
        }
        else {
            Destroy(gameObject);
        }

        wfc = new WaveFunctionCollapse(gridSettings.width, gridSettings.height, paletteSet);
        editor.widthInput.text = gridSettings.width.ToString();
        editor.heightInput.text = gridSettings.height.ToString();
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

    public void Iterate() {
        if (wfc.IsCollapsed()) {
            Debug.Log("WFC is done!");
            return;
        }

        if (editor.SelectedPos.HasValue) {
            var pos = editor.SelectedPos.Value;
            wfc.Iterate(pos.x, pos.y);
            editor.ClearContent();
            editor.ClearSelectedTile();
            editor.step.text = "Step / Iterate";
        }
        else {
            wfc.Iterate();
        }

        editor.solve.text = "Finish solving!";
        Paint();
    }

    public void Iterate(int x, int y, State state) {
        wfc.Iterate(x, y, state);
        editor.ClearContent();
        editor.ClearSelectedTile();
        editor.solve.text = "Finish solving!";
        editor.step.text = "Step / Iterate";
        Paint();
    }

    public void Solve() {
        while (!wfc.IsCollapsed()) {
            wfc.Iterate();
        }

        Paint();
    }

    public void Reset() {
        wfc = new WaveFunctionCollapse(gridSettings.width, gridSettings.height, paletteSet);

        editor.ClearContent();
        editor.ClearSelectedTile();
        editor.solve.text = "Generate!";
        editor.step.text = "Step / Iterate";
        output.ClearAllTiles();
        Paint();
    }

    public void ResizeWidth(int width) {
        wfc = new WaveFunctionCollapse(width, gridSettings.height, paletteSet);
        gridSettings.width = width;

        output.ClearAllTiles();
        editor.RedrawPlaceholder();
        Paint();
    }

    public void ResizeHeight(int height) {
        wfc = new WaveFunctionCollapse(gridSettings.width, height, paletteSet);
        gridSettings.height = height;

        output.ClearAllTiles();
        editor.RedrawPlaceholder();
        Paint();
    }

    public void ResizeCeleste() {
        wfc = new WaveFunctionCollapse(40, 23, paletteSet);
        gridSettings.width = 40;
        gridSettings.height = 23;
        editor.widthInput.text = "40";
        editor.heightInput.text = "23";

        output.ClearAllTiles();
        editor.RedrawPlaceholder();
        editor.CameraCeleste();
        Paint();
    }

    public bool IsCollapsed(int x, int y) {
        return wfc.grid[y, x].IsCollapsed;
    }

    public Cell GetCell(int x, int y) {
        return wfc.grid[wfc.height - 1 - y, x];
    }
}