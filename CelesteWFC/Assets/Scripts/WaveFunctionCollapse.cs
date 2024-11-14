using System.Collections.Generic;
using System.Linq;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public struct State
{
    public TileBase tile;
    public Socket socket;
    public int timesRotatedClockwise;

    public static State RotateClockwise(ref State state) {
        ref var socket = ref state.socket;
        var temp = socket.up;

        return new State {
            tile = state.tile,
            socket = new Socket {
                up = socket.left,
                left = socket.down,
                down = socket.right,
                right = temp
            },
            timesRotatedClockwise = (state.timesRotatedClockwise + 1) % 4
        };
    }
}

public class Cell
{
    public List<State> states;

    public Cell(in List<TileInfo> tileInfo) {
        states = tileInfo.SelectMany(ti => {
            var currState = new State {
                tile = ti.tile,
                socket = ti.originalSocket,
                timesRotatedClockwise = 0
            };

            // Add original state to list first
            var statesFromTile = new List<State> { currState };

            switch (ti.symmetry) {
            case SymmetryType.T:
            case SymmetryType.L:
                for (var i = 0; i < 3; ++i) {
                    currState = State.RotateClockwise(ref currState);
                    statesFromTile.Add(currState);
                }

                break;

            case SymmetryType.I:
                currState = State.RotateClockwise(ref currState);
                statesFromTile.Add(currState);
                break;

            // X-type tiles don't need to be rotated
            case SymmetryType.X:
            default:
                break;
            }

            return statesFromTile;
        }).ToList();
    }

    public bool IsCollapsed => states.Count == 1;
}

public class WaveFunctionCollapse
{
    public readonly Cell[,] grid;

    public WaveFunctionCollapse(int width, int height, Palette palette) {
        grid = new Cell[width, height];

        for (var y = 0; y < grid.GetLength(0); ++y) {
            for (var x = 0; x < grid.GetLength(1); ++x) {
                grid[x, y] = new Cell(palette.tiles);
            }
        }
    }

    private void Collapse((int, int) coords) {
        var currentPossibleStates = grid[coords.Item1, coords.Item2].states;
        var randomState = currentPossibleStates[Random.Range(0, currentPossibleStates.Count)];

        // Clear possible tiles and only add back randomly chosen one
        currentPossibleStates.Clear();
        currentPossibleStates.Add(randomState);
    }

    private (int, int) PickLowestEntropyCell() {
        var currMin = int.MaxValue;
        var coords = (-1, -1);

        for (var y = 0; y < grid.GetLength(0); ++y) {
            for (var x = 0; x < grid.GetLength(1); ++x) {
                var cell = grid[x, y];
                var numberOfPossibleStates = cell.states.Count;

                // If this cell has a smaller number of states than the previous *and* it's not fully collapsed
                if (numberOfPossibleStates < currMin && !cell.IsCollapsed) {
                    currMin = numberOfPossibleStates;
                    coords = (x, y);
                }
            }
        }

        return coords;
    }

    private void Propagate(Cell collapsedCell) {
        var stack = new Stack<Cell>();
        stack.Push(collapsedCell);

        while (stack.Count > 0) {
            var cell = stack.Pop();
        }
    }

    public void Iterate() {
        var nextCellToCollapse = PickLowestEntropyCell();
        Collapse(nextCellToCollapse);
    }

    // 1. get neighbors of this cell
    // 2. remove invalid states from neighbors
    // 3. if neighbors have been updated, add them to the stack, so we can propagate
}