using System.Collections.Generic;
using UnityEngine;

public class GridCell
{
    public List<CellData> states;

    public GridCell(List<CellData> possibleCellStates) {
        states = new List<CellData>(possibleCellStates);
    }

    public bool IsCollapsed => states.Count == 1;
}

public class WaveFunctionCollapse
{
    private readonly GridCell[,] grid;

    public WaveFunctionCollapse(int width, int height, GridCellPalette gridCellPalette) {
        grid = new GridCell[width, height];
        var possibleCellStates = gridCellPalette.possibleCellStates;

        for (var y = 0; y < grid.GetLength(0); ++y) {
            for (var x = 0; x < grid.GetLength(1); ++x) {
                grid[x, y] = new GridCell(possibleCellStates);
            }
        }
    }

    public void Collapse(GridCell gridCell) {
        var possibleStates = gridCell.states;
        var randomIdx = Random.Range(0, possibleStates.Count);
        var randomTile = possibleStates[randomIdx];

        // Clear possible tiles and only add back randomly chosen one
        possibleStates.Clear();
        possibleStates.Add(randomTile);
    }

    public GridCell PickLowestEntropyCell() {
        var currMin = int.MaxValue;
        GridCell currGridCell = null;

        for (var y = 0; y < grid.GetLength(0); ++y) {
            for (var x = 0; x < grid.GetLength(1); ++x) {
                var gridCell = grid[x, y];
                var possibleStatesCount = gridCell.states.Count;

                // If this cell has a smaller number of states than the previous *and* it's not fully collapsed
                if (possibleStatesCount < currMin && !gridCell.IsCollapsed) {
                    currMin = possibleStatesCount;
                    currGridCell = gridCell;
                }
            }
        }

        return currGridCell;
    }

    // 1. get neighbors of this cell
    // 2. remove invalid states from neighbors
    // 3. if neighbors have been updated, add them to the stack, so we can propagate
}