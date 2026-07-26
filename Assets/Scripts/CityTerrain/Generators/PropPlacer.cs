using System;
using System.Collections.Generic;

namespace Flipit.CityTerrain
{
    /// <summary>
    /// Places trees, cars, lampposts, benches, and trash cans on the grid.
    /// Uses seeded random for deterministic placement.
    /// </summary>
    public static class PropPlacer
    {
        public struct PlacementResult
        {
            public List<CellData> PlacedCells;
            public int RequestedCount;
            public int ActualCount;
            public string PropType;
        }

        /// <summary>
        /// Places trees on unoccupied Sidewalk and Park cells using separate density values.
        /// Trees on sidewalks use sidewalkDensity, trees in parks use parkDensity.
        /// </summary>
        public static PlacementResult PlaceTrees(Grid_Layout grid, Random rng, float sidewalkDensity, float parkDensity)
        {
            var placedCells = new List<CellData>();

            // Place trees on sidewalk cells
            var sidewalkCells = grid.GetUnoccupiedCellsOfType(CellType.Sidewalk);
            int sidewalkTarget = (int)Math.Floor(sidewalkCells.Count * sidewalkDensity);
            var sidewalkPlaced = PlaceWithDensity(grid, sidewalkCells, sidewalkTarget, rng, 0, "tree");
            placedCells.AddRange(sidewalkPlaced);

            // Place trees on park cells
            var parkCells = grid.GetUnoccupiedCellsOfType(CellType.Park);
            int parkTarget = (int)Math.Floor(parkCells.Count * parkDensity);
            var parkPlaced = PlaceWithDensity(grid, parkCells, parkTarget, rng, 0, "tree");
            placedCells.AddRange(parkPlaced);

            int totalRequested = sidewalkTarget + parkTarget;

            return new PlacementResult
            {
                PlacedCells = placedCells,
                RequestedCount = totalRequested,
                ActualCount = placedCells.Count,
                PropType = "tree"
            };
        }

        /// <summary>
        /// Places cars on non-intersection Street cells that are adjacent to at least one Sidewalk cell.
        /// </summary>
        public static PlacementResult PlaceCars(Grid_Layout grid, Random rng, float density)
        {
            var eligibleCells = new List<CellData>();

            for (int row = 0; row < grid.Depth; row++)
            {
                for (int col = 0; col < grid.Width; col++)
                {
                    CellData cell = grid.Cells[row, col];
                    if (cell.Type != CellType.Street)
                        continue;
                    if (cell.IsIntersection)
                        continue;
                    if (cell.IsOccupied)
                        continue;
                    // Skip edge cells to prevent cars from sticking out of the map
                    if (row == 0 || row == grid.Depth - 1 || col == 0 || col == grid.Width - 1)
                        continue;
                    // Also skip cells one row/col from the edge for safety margin
                    if (row == 1 || row == grid.Depth - 2 || col == 1 || col == grid.Width - 2)
                        continue;

                    // Check if adjacent to at least one Sidewalk cell
                    var neighbors = grid.GetAdjacentCells(row, col);
                    bool adjacentToSidewalk = false;
                    foreach (var neighbor in neighbors)
                    {
                        if (neighbor.Type == CellType.Sidewalk)
                        {
                            adjacentToSidewalk = true;
                            break;
                        }
                    }

                    if (adjacentToSidewalk)
                    {
                        eligibleCells.Add(cell);
                    }
                }
            }

            int targetCount = (int)Math.Floor(eligibleCells.Count * density);
            var placedCells = PlaceWithDensity(grid, eligibleCells, targetCount, rng, 0, "car");

            return new PlacementResult
            {
                PlacedCells = placedCells,
                RequestedCount = targetCount,
                ActualCount = placedCells.Count,
                PropType = "car"
            };
        }

        /// <summary>
        /// Places lampposts on the 4 corner Sidewalk cells diagonally adjacent to each intersection.
        /// </summary>
        public static PlacementResult PlaceLampposts(Grid_Layout grid, int streetInterval)
        {
            if (streetInterval < 3)
                streetInterval = 3;

            var placedCells = new List<CellData>();
            int requestedCount = 0;

            for (int row = 0; row < grid.Depth; row++)
            {
                for (int col = 0; col < grid.Width; col++)
                {
                    if (row % streetInterval != 0 || col % streetInterval != 0)
                        continue;

                    // This is an intersection. Find 4 diagonal sidewalk cells.
                    int[] rowOffsets = { -1, -1, 1, 1 };
                    int[] colOffsets = { -1, 1, -1, 1 };

                    for (int i = 0; i < 4; i++)
                    {
                        int diagRow = row + rowOffsets[i];
                        int diagCol = col + colOffsets[i];

                        if (diagRow < 0 || diagRow >= grid.Depth)
                            continue;
                        if (diagCol < 0 || diagCol >= grid.Width)
                            continue;

                        requestedCount++;

                        CellData cell = grid.Cells[diagRow, diagCol];
                        if (cell.Type != CellType.Sidewalk)
                            continue;
                        if (cell.IsOccupied)
                            continue;

                        grid.MarkOccupied(diagRow, diagCol, "lamppost");
                        // Re-read cell after marking occupied
                        placedCells.Add(grid.Cells[diagRow, diagCol]);
                    }
                }
            }

            return new PlacementResult
            {
                PlacedCells = placedCells,
                RequestedCount = requestedCount,
                ActualCount = placedCells.Count,
                PropType = "lamppost"
            };
        }

        /// <summary>
        /// Places benches on Sidewalk cells adjacent to Park cells using density algorithm.
        /// </summary>
        public static PlacementResult PlaceBenches(Grid_Layout grid, Random rng, float density)
        {
            var eligibleCells = new List<CellData>();

            for (int row = 0; row < grid.Depth; row++)
            {
                for (int col = 0; col < grid.Width; col++)
                {
                    CellData cell = grid.Cells[row, col];
                    if (cell.Type != CellType.Sidewalk)
                        continue;
                    if (cell.IsOccupied)
                        continue;

                    // Check if adjacent to at least one Park cell
                    var neighbors = grid.GetAdjacentCells(row, col);
                    bool adjacentToPark = false;
                    foreach (var neighbor in neighbors)
                    {
                        if (neighbor.Type == CellType.Park)
                        {
                            adjacentToPark = true;
                            break;
                        }
                    }

                    if (adjacentToPark)
                    {
                        eligibleCells.Add(cell);
                    }
                }
            }

            int targetCount = (int)Math.Floor(eligibleCells.Count * density);
            var placedCells = PlaceWithDensity(grid, eligibleCells, targetCount, rng, 0, "bench");

            return new PlacementResult
            {
                PlacedCells = placedCells,
                RequestedCount = targetCount,
                ActualCount = placedCells.Count,
                PropType = "bench"
            };
        }

        /// <summary>
        /// Places trash cans on unoccupied Sidewalk cells with minimum Manhattan distance spacing.
        /// </summary>
        public static PlacementResult PlaceTrashCans(Grid_Layout grid, Random rng, float density, int minSpacing)
        {
            var eligibleCells = grid.GetUnoccupiedCellsOfType(CellType.Sidewalk);
            int targetCount = (int)Math.Floor(eligibleCells.Count * density);
            var placedCells = PlaceWithDensity(grid, eligibleCells, targetCount, rng, minSpacing, "trashcan");

            return new PlacementResult
            {
                PlacedCells = placedCells,
                RequestedCount = targetCount,
                ActualCount = placedCells.Count,
                PropType = "trashcan"
            };
        }

        /// <summary>
        /// Core density-based placement algorithm.
        /// Shuffles eligible cells, places up to targetCount respecting occupancy and spacing.
        /// </summary>
        private static List<CellData> PlaceWithDensity(
            Grid_Layout grid, List<CellData> eligibleCells, int targetCount,
            Random rng, int minSpacing, string propType)
        {
            var placed = new List<CellData>();

            if (eligibleCells.Count == 0 || targetCount <= 0)
                return placed;

            // Fisher-Yates shuffle
            Shuffle(eligibleCells, rng);

            for (int i = 0; i < eligibleCells.Count; i++)
            {
                if (placed.Count >= targetCount)
                    break;

                CellData cell = eligibleCells[i];

                // Re-check occupancy (may have been occupied by a previous placement pass)
                if (grid.Cells[cell.Row, cell.Col].IsOccupied)
                    continue;

                // Check minimum spacing constraint
                if (minSpacing > 0)
                {
                    bool tooClose = false;
                    foreach (var placedCell in placed)
                    {
                        if (cell.ManhattanDistance(placedCell) < minSpacing)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    if (tooClose)
                        continue;
                }

                grid.MarkOccupied(cell.Row, cell.Col, propType);
                placed.Add(grid.Cells[cell.Row, cell.Col]);
            }

            return placed;
        }

        /// <summary>
        /// Fisher-Yates shuffle for a list using the provided RNG.
        /// </summary>
        private static void Shuffle<T>(List<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
