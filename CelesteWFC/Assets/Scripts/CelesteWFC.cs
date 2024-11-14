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
    [SerializeField] private TileBase tile;
    [SerializeField] private Palette palette;

    private WaveFunctionCollapse wfc;

    private void Awake() {
        wfc = new WaveFunctionCollapse(gridSettings.width, gridSettings.height, palette);
    }

    private void Start() {
        var x = 0;

        foreach (var state in wfc.grid[0, 0].states) {
            var position = new Vector3Int(x, 0, 0);
            tilemap.SetTile(position, state.tile);

            var angle = -90f * state.timesRotatedClockwise;
            var rotMat = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, angle), Vector3.one);
            tilemap.SetTransformMatrix(position, rotMat);

            x += 1;
        }
    }
}