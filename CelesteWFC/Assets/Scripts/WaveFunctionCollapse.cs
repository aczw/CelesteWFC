using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class GridCell
{
    private readonly HashSet<TileBase> possibleStates;

    public GridCell(HashSet<TileBase> allPossibleStates) {
        possibleStates = allPossibleStates;
    }

    /// <summary>
    ///     Checks if the current cell has only one possible state.
    /// </summary>
    /// <returns>true if this cell has completely collapsed to a single possible state, and false otherwise.</returns>
    public bool IsCollapsed() {
        return possibleStates.Count == 1;
    }
}

public class WaveFunctionCollapse
{
    private GridCell[,] grid;

    public WaveFunctionCollapse(int width, int height) {
        grid = new GridCell[width, height];
    }

    public void Collapse(int x, int y) { }
}