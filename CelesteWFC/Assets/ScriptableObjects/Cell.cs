using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum Test
{
    Cross,
    Blank,
    Line,
    Corner,
    T
}

public enum Symmetry
{
    X,
    T,
    I,
    L
}

[CreateAssetMenu(fileName = "Cell", menuName = "Scriptable Objects/Cell")]
public class Cell : ScriptableObject
{
    public Test cellEnum;
    public Symmetry Symmetry;
    public TileBase tile;

    public List<Cell> validUp;
    public List<Cell> validDown;
    public List<Cell> validLeft;
    public List<Cell> validRight;
}