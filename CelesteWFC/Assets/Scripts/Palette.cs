using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
///     A tile can be symmetric with respect to the following types. This affects how we rotate/transform
///     the tile to get all possible neighbor combinations. Sometimes the tile simply can't be rotated (not symmetrical
///     art), hence we need the first entry.
/// </summary>
public enum SymmetryType
{
    CanNotRotate,
    X,
    T,
    I,
    L
}

/// <summary>
///     Stores the valid socket IDs on each of the four sides of the current cell. A value of <c>-1</c> means that this
///     direction does not have a valid socket, i.e. no connections are possible.
/// </summary>
/// <seealso cref="NbLocation" />
[Serializable] public record Socket
{
    [Min(-1)] public int up = -1;
    [Min(-1)] public int down = -1;
    [Min(-1)] public int left = -1;
    [Min(-1)] public int right = -1;

    public override string ToString() {
        return $"up: {up}, down: {down}, left: {left}, right: {right}";
    }
}

/// <summary>
///     Stores the <see cref="Tile">Tile</see> along with additional metadata to help the algorithm collapse and propagate
///     this tile.
/// </summary>
[Serializable] public struct TileInfo
{
    public Tile tile;
    public SymmetryType symmetry;
    public Socket originalSocket;
}

/// <summary>
///     Used to store a set of tiles that come from a single source/spritesheet. This becomes important later for
///     grouping/socket purposes.
/// </summary>
/// <seealso cref="PaletteSet" />
[CreateAssetMenu(fileName = "Palette", menuName = "ScriptableObjects/Palette")]
public class Palette : ScriptableObject
{
    /// <summary>
    ///     <para>
    ///         Most spritesheets are not compatible with tiles being allowed to place two invalid sockets next to each other,
    ///         because the art was not designed that way. Use <see cref="Type.Unfold" /> for that.
    ///     </para>
    ///     <para>
    ///         Some simpler spritesheets (like simple pipes) can "fold" upon themselves. So <see cref="Type.Fold" /> works
    ///         best.
    ///     </para>
    /// </summary>
    public enum Type
    {
        Unfold,
        Fold
    }

    public string paletteName;
    public Type type;
    [SerializeField] public List<TileInfo> tiles;
}