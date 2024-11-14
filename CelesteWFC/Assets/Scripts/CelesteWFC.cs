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
    [SerializeField] private GridSize gridSettings;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Palette palette;

    private WaveFunctionCollapse wfc;

    private void Awake() {
        wfc = new WaveFunctionCollapse(gridSettings.width, gridSettings.height, palette);
    }

    private void Start() {
        while (!wfc.IsCollapsed()) {
            wfc.Iterate();
        }

        Debug.Log("DONE WITH WFC!!!");

        for (var y = 0; y < wfc.height; ++y) {
            for (var x = 0; x < wfc.width; ++x) {
                var state = wfc.grid[y, x].states[0];
                var position = new Vector3Int(x, y, 0);

                tilemap.SetTile(position, state.tile);

                var angle = -90f * state.timesRotatedClockwise;
                var rotMat = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, angle), Vector3.one);

                tilemap.SetTransformMatrix(position, rotMat);
            }
        }
    }
}