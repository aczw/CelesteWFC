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

[Serializable] public struct Socket
{
    public bool up;
    public bool down;
    public bool left;
    public bool right;
}

[Serializable] public struct TileInfo
{
    public string tileName;
    public TileBase tile;
    public SymmetryType symmetry;
    public Socket originalSocket;
}

/// <summary>
///     Used to store a set of tiles that a grid can output along with additional cell information to help the algorithm
///     collapse and propagate. It's designed to be "plug and chug" as you can switch out a ScriptableObject for another
///     so that CelesteWFC uses a different set of tiles with different rules.
/// </summary>
[CreateAssetMenu(fileName = "Palette", menuName = "ScriptableObjects/Palette")]
public class Palette : ScriptableObject
{
    public string paletteName;
    [SerializeField] public List<TileInfo> tiles;
}