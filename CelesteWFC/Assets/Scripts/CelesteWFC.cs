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
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Palette palette;
    [SerializeField] private GridSize gridSettings;

    private WaveFunctionCollapse wfc;

    private void Awake() {
        wfc = new WaveFunctionCollapse(gridSettings.width, gridSettings.height, palette);
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

                    tilemap.SetTile(position, state.tile);

                    var angle = -90f * state.timesRotatedClockwise;
                    var rotMat = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, angle), Vector3.one);

                    tilemap.SetTransformMatrix(position, rotMat);
                }
            }
        }
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
        tilemap.ClearAllTiles();
        Paint();
    }
}