using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public struct State
{
    public Tile tile;
    public string paletteName;

    public Socket socket;
    public int timesRotatedClockwise;

    public static State RotateClockwise(in State state) {
        var socket = state.socket;
        var temp = socket.up;

        return new State {
            tile = state.tile,
            paletteName = state.paletteName,
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

/// <summary>
///     <para>
///         This class checks whether sockets between the current and neighbor cell are "valid." Validity can mean
///         different things depending on what <see cref="PaletteType" /> we're using.
///     </para>
///     <para>
///         Currently, there are two types:
///         <ul>
///             <li>
///                 <see cref="PaletteType.Single" />: true if both socket IDs are equal. Note this means two invalid
///                 sockets will also return true. This is fine for <see cref="PaletteSet" />s that only have one
///                 <see cref="Palette" />.
///             </li>
///             <li>
///                 <see cref="PaletteType.Multiple" />: assumes we're dealing with multiple palettes. Therefore, there are
///                 two ways to be valid: if both are from the palette and have valid, equal socket IDs, or they're from
///                 different palettes and both have invalid socket IDs (i.e. facing "away" from each other).
///             </li>
///         </ul>
///     </para>
/// </summary>
public class NeighborValidator
{
    private enum PaletteType
    {
        Single,
        Multiple
    }

    private readonly PaletteType type;

    public NeighborValidator(bool includeFalseSockets) {
        type = includeFalseSockets ? PaletteType.Single : PaletteType.Multiple;
    }

    public bool IsValid(in NeighborLocation nbLoc, in State curr, in State nb) {
        var fromSamePalette = curr.paletteName == nb.paletteName;

        var currSocket = curr.socket;
        var nbSocket = nb.socket;
        var (currID, nbID) = nbLoc switch {
            NeighborLocation.Up => (currSocket.up, nbSocket.down),
            NeighborLocation.Down => (currSocket.down, nbSocket.up),
            NeighborLocation.Left => (currSocket.left, nbSocket.right),
            NeighborLocation.Right => (currSocket.right, nbSocket.left),
            _ => throw new ArgumentOutOfRangeException()
        };

        var equalSocketIDs = currID == nbID;
        var validSocketIDs = currID >= 0 && nbID >= 0;

        return type switch {
            PaletteType.Single => equalSocketIDs,
            PaletteType.Multiple => (fromSamePalette && equalSocketIDs && validSocketIDs) ||
                                    (!fromSamePalette && equalSocketIDs && !validSocketIDs),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public class Cell
{
    public List<State> states;

    public Cell(in PaletteSet paletteSet) {
        states = new List<State>();

        paletteSet.palettes.ForEach(p => {
            states.AddRange(p.tiles.SelectMany(ti => {
                if (ti.disabled) return new List<State>();

                var currState = new State {
                    tile = ti.tile,
                    paletteName = p.paletteName,
                    socket = ti.originalSocket,
                    timesRotatedClockwise = 0
                };

                // Add original state to list first
                var statesFromTile = new List<State> { currState };

                // If this tile can't be rotated then we immediately return here
                if (!ti.canRotate) return statesFromTile;

                switch (ti.symmetry) {
                case SymmetryType.T:
                case SymmetryType.L:
                    for (var i = 0; i < 3; ++i) {
                        currState = State.RotateClockwise(currState);
                        statesFromTile.Add(currState);
                    }

                    break;

                case SymmetryType.I:
                    currState = State.RotateClockwise(currState);
                    statesFromTile.Add(currState);
                    break;

                // X-type tiles don't need to be rotated
                case SymmetryType.X:
                default:
                    break;
                }

                return statesFromTile;
            }).ToList());
        });
    }

    public bool IsCollapsed => states.Count == 1;
}

/// <summary>
///     Denotes on which of the four sides does the neighbor cell lie relative to the current cell.
/// </summary>
public enum NeighborLocation
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

    private readonly NeighborValidator nbValidator;

    // Up, down, left, right neighbors. Note to self: I treat coords == indices. Since row index increases
    // going "down" this means to go up, we subtract by 1 and vice versa.
    private static readonly List<(NeighborLocation nbType, Vector2Int vec)> Offsets = new() {
        (NeighborLocation.Up, new Vector2Int(0, -1)), (NeighborLocation.Down, new Vector2Int(0, 1)),
        (NeighborLocation.Left, new Vector2Int(-1, 0)), (NeighborLocation.Right, new Vector2Int(1, 0))
    };

    public WaveFunctionCollapse(int width, int height, in PaletteSet paletteSet) {
        grid = new Cell[height, width];
        this.width = width;
        this.height = height;
        nbValidator = new NeighborValidator(paletteSet.includeFalseSockets);

        for (var y = 0; y < height; ++y) {
            for (var x = 0; x < width; ++x) {
                grid[y, x] = new Cell(paletteSet);
            }
        }
    }

    private void Collapse(in Vector2Int coords) {
        var currentPossibleStates = grid[coords.y, coords.x].states;
        var randomState = currentPossibleStates[Random.Range(0, currentPossibleStates.Count)];

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

        while (stack.Count > 0) {
            var currCoords = stack.Pop();

            // Check which of the four neighbors are valid, because we might be on the grid edge
            var validNeighbors = new List<(NeighborLocation nbType, Vector2Int vec)>();
            Offsets.ForEach(offset => {
                var newX = currCoords.x + offset.vec.x;
                var newY = currCoords.y + offset.vec.y;

                var withinBounds = newX >= 0 && newX <= width - 1 && newY >= 0 && newY <= height - 1;
                if (withinBounds && !grid[newY, newX].IsCollapsed) {
                    validNeighbors.Add((offset.nbType, new Vector2Int(newX, newY)));
                }
            });

            // For each up, down, left, and right (in that order) neighbor of the collapsed cell
            foreach (var (nbLoc, neighborCoords) in validNeighbors) {
                var nbCell = grid[neighborCoords.y, neighborCoords.x];

                // Not sure when this will happen. The video I was following has this though.
                if (nbCell.states.Count == 0) {
                    Debug.LogWarning("Propagate(): number of states in neighbor cell is zero. Skipping this iteration");
                    continue;
                }

                // Get all valid neighbor states for all possible states of the current cell. In other words, if a
                // neighbor socket aligns with at least *one* of the current cell's possible states, then we include it.
                var validNeighborStates = new HashSet<State>();
                foreach (var nbState in nbCell.states) {
                    foreach (var currState in grid[currCoords.y, currCoords.x].states) {
                        if (nbValidator.IsValid(nbLoc, currState, nbState)) {
                            validNeighborStates.Add(nbState);
                        }
                    }
                }

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

    // Let algorithm decide coordinate and state
    public void Iterate() {
        var nextCellToCollapse = PickLowestEntropyCell();
        Collapse(nextCellToCollapse);
        Propagate(nextCellToCollapse);
    }

    // Let algorithm decide the state
    public void Iterate(int x, int y) {
        var nextCellToCollapse = new Vector2Int(x, height - 1 - y);
        Collapse(nextCellToCollapse);
        Propagate(nextCellToCollapse);
    }

    // Specifically pick a coordinate and state to collapse to
    public void Iterate(int x, int y, State state) {
        var pos = new Vector2Int(x, height - 1 - y);
        var cellStates = grid[height - 1 - y, x].states;

        cellStates.Clear();
        cellStates.Add(state);
        Propagate(pos);
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