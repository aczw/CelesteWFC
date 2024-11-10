using System;
using System.Collections.Generic;
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
    [SerializeField] private List<Cell> possibleStates;

    private WaveFunctionCollapse wfc;

    private void Awake() {
        wfc = new WaveFunctionCollapse(gridSettings.width, gridSettings.height);
    }

    private void Start() {
        for (var y = 0; y < gridSettings.height; ++y) {
            for (var x = 0; x < gridSettings.width; ++x) {
                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }
}