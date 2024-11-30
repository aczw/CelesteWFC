using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
///     A tile can be symmetric with respect to the following types. This affects how we rotate/transform
///     the tile to get all possible neighbor combinations.
/// </summary>
public enum SymmetryType
{
    X,
    T,
    I,
    L
}

[Serializable] public record Socket
{
    public bool up;
    public bool down;
    public bool left;
    public bool right;

    public override string ToString() {
        return $"up: {up}, down: {down}, left: {left}, right: {right}";
    }
}

/// <summary>
///     Stores the Tile along with additional metadata to help the algorithm collapse and propagate this tile.
/// </summary>
[Serializable] public struct TileInfo
{
    public string tileName;
    public TileBase tile;
    public SymmetryType symmetry;
    public bool disabled;
    public Socket originalSocket;

    public static TileInfo Blank = new() {
        tileName = "Blank",
        tile = null,
        symmetry = SymmetryType.X,
        disabled = false,
        originalSocket = new Socket {
            up = false,
            down = false,
            left = false,
            right = false
        }
    };
}

/// <summary>
///     Used to store a set of tiles that come from a single source/spritesheet. This becomes important later for
///     grouping/socket purposes.
/// </summary>
[CreateAssetMenu(fileName = "Palette", menuName = "ScriptableObjects/Palette")]
public class Palette : ScriptableObject
{
    public string paletteName;
    [SerializeField] public List<TileInfo> tiles;
}