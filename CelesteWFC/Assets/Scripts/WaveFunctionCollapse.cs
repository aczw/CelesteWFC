using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

public enum NeighborType
{
    Up,
    Down,
    Left,
    Right
}

public class WaveFunctionCollapse
{
    public readonly Cell[,] grid;
    public readonly int width;
    public readonly int height;

    // Up, down, left, right neighbors. Note to self: I treat coords == indices. Since row index increases
    // going "down" this means to go up, we subtract by 1 and vice versa.
    private static readonly List<(NeighborType nbType, Vector2Int vec)> Offsets = new() {
        (NeighborType.Up, new Vector2Int(0, -1)), (NeighborType.Down, new Vector2Int(0, 1)),
        (NeighborType.Left, new Vector2Int(-1, 0)), (NeighborType.Right, new Vector2Int(1, 0))
    };

    public WaveFunctionCollapse(int width, int height, Palette palette) {
        grid = new Cell[height, width];
        this.width = width;
        this.height = height;

        for (var y = 0; y < height; ++y) {
            for (var x = 0; x < width; ++x) {
                grid[y, x] = new Cell(palette.tiles);
            }
        }
    }

    private void Collapse(in Vector2Int coords) {
        var currentPossibleStates = grid[coords.y, coords.x].states;
        var randomState = currentPossibleStates[Random.Range(0, currentPossibleStates.Count)];

        // Clear possible tiles and only add back randomly chosen one
        currentPossibleStates.Clear();
        currentPossibleStates.Add(randomState);
    }

    private Vector2Int PickLowestEntropyCell() {
        var currMin = int.MaxValue;
        var coords = (-1, -1);

        for (var y = 0; y < height; ++y) {
            for (var x = 0; x < width; ++x) {
                var cell = grid[y, x];
                var numberOfPossibleStates = cell.states.Count;

                // If this cell has a smaller number of states than the previous *and* it's not fully collapsed
                if (numberOfPossibleStates < currMin && !cell.IsCollapsed) {
                    currMin = numberOfPossibleStates;
                    coords = (x, y);
                }
            }
        }

        return new Vector2Int(coords.Item1, coords.Item2);
    }

    private void Propagate(Vector2Int coords) {
        var stack = new Stack<Vector2Int>();
        stack.Push(coords);

        // Retrieve the singular state left in the cell at this coordinate (because it was just collapsed)
        var collapsedState = grid[coords.y, coords.x].states[0];

        while (stack.Count > 0) {
            var currCoords = stack.Pop();

            // Check which of the four neighbors are valid, because we might be on the grid edge
            var validNeighbors = new List<(NeighborType nbType, Vector2Int vec)>();
            Offsets.ForEach(offset => {
                var newX = currCoords.x + offset.vec.x;
                var newY = currCoords.y + offset.vec.y;

                var withinBounds = newX >= 0 && newX <= width - 1 && newY >= 0 && newY <= height - 1;
                if (withinBounds && !grid[newY, newX].IsCollapsed) {
                    validNeighbors.Add((offset.nbType, new Vector2Int(newX, newY)));
                }
            });

            // For each up, down, left, and right (in that order) neighbor of the collapsed cell
            foreach (var (nbType, neighborCoords) in validNeighbors) {
                var nbCell = grid[neighborCoords.y, neighborCoords.x];

                if (nbCell.states.Count == 0) continue;

                // Get valid states that are available in this neighbor, and match the socket for this cell
                var validNeighborStates = new List<State>();
                nbCell.states.ForEach(nbState => {
                    switch (nbType) {
                    case NeighborType.Up:
                        if (collapsedState.socket.up == nbState.socket.down) validNeighborStates.Add(nbState);
                        break;

                    case NeighborType.Down:
                        if (collapsedState.socket.down == nbState.socket.up) validNeighborStates.Add(nbState);
                        break;

                    case NeighborType.Left:
                        if (collapsedState.socket.left == nbState.socket.right) validNeighborStates.Add(nbState);
                        break;

                    case NeighborType.Right:
                        if (collapsedState.socket.right == nbState.socket.left) validNeighborStates.Add(nbState);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                    }
                });

                if (validNeighborStates.Count == 0) Debug.LogError("Propagate(): valid neighbor states == zero");

                // If the lists have the same number of states then nothing was removed, which means this neighbor
                // cell has not been updated, and therefore we do not need to propagate any changes. So, we only enter
                // this block if the opposite is true.
                if (validNeighborStates.Count != nbCell.states.Count) {
                    nbCell.states = new List<State>(validNeighborStates);
                    if (!stack.Contains(neighborCoords)) {
                        stack.Push(neighborCoords);
                    }
                }
            }
        }
    }

    public void Iterate() {
        var nextCellToCollapse = PickLowestEntropyCell();
        Collapse(nextCellToCollapse);
        Propagate(nextCellToCollapse);
    }

    public bool IsCollapsed() {
        var collapsed = true;

        for (var y = 0; y < height; ++y) {
            for (var x = 0; x < width; ++x) {
                // Everything needs to be true
                collapsed = collapsed && grid[y, x].IsCollapsed;
            }
        }

        return collapsed;
    }
}