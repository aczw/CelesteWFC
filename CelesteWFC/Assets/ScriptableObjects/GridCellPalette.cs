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

[Serializable] public struct CellData
{
    public string name;
    public SymmetryType symmetry;
    public TileBase tile;
}

/// <summary>
///     Each GridCellPalette defines a set of tiles that we should consider for this grid. The list of possible cell
///     states represents all possible tiles that a cell could display.
/// </summary>
[CreateAssetMenu(fileName = "Grid Cell Palette", menuName = "Scriptable Objects/Grid Cell Palette")]
public class GridCellPalette : ScriptableObject
{
    public string paletteName;
    [SerializeField] public List<CellData> possibleCellStates;
}