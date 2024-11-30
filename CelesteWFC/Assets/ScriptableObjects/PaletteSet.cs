using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     A set of (assumed to be unique) palettes. It's designed to be "plug and chug" as you can switch out a
///     ScriptableObject for another so that CelesteWFC uses a different set of tiles with different rules.
/// </summary>
[CreateAssetMenu(fileName = "Palette Set", menuName = "ScriptableObjects/Palette Set")]
public class PaletteSet : ScriptableObject
{
    public string setName;
    public bool fillFalseSockets;
    [SerializeField] public List<Palette> palettes;
}