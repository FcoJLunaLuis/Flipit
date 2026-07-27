using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Flipit.CityTerrain.Tests
{
    /// <summary>
    /// Property-based tests for PropPlacer algorithms (Properties 13–17).
    /// Uses PropertyTestUtility for seed-controlled random generation.
    /// Feature: city-terrain-generation
    /// </summary>
    [TestFixture]
    public class PropPlacerPropertyTests
    {
        #region Test Grid Helpers

        private static (Grid_Layout grid, int streetInterval, Random rng) CreateTestGrid(
            Random random)
        {
            int width = random.Next(5, 21);
            int depth = random.Next(5, 21);
            int streetInterval = random.Next(3, 9);
            int seed = random.Next(1, 100000);

            var grid = new Grid_Layout(width, depth, 4f);
            GridGenerator.AssignCellTypes(grid, streetInterval);
            var rng = new Random(seed);
            return (grid, streetInterval, rng);
        }

        private static (Grid_Layout grid, int streetInterval, Random rng, float density) CreateTestGridWithDensity(
            Random random)
        {
            var (grid, streetInterval, rng) = CreateTestGrid(random);
            float density = random.Next(5, 81) / 100f;
            return (grid, streetInterval, rng, density);
        }

        /// <summary>
        /// Creates a grid with some Building cells converted to Park cells
        /// to enable bench placement testing.
        /// </summary>
        private static (Grid_Layout grid, int streetInterval, Random rng) CreateGridWithParks(
            Random random)
        {
            int width = random.Next(8, 21);
            int depth = random.Next(8, 21);
            int streetInterval = random.Next(3, 7);
            int seed = random.Next(1, 100000);

            var grid = new Grid_Layout(width, depth, 4f);
            GridGenerator.AssignCellTypes(grid, streetInterval);

            var rng = new Random(seed);

            // Convert some Building cells to Park (simulating Stage 5 cell conversion)
            var buildingCells = grid.GetCellsOfType(CellType.Building);
            int parkCount = Math.Max(1, buildingCells.Count / 4);
            for (int i = 0; i < Math.Min(parkCount, buildingCells.Count); i++)
            {
                int idx = rng.Next(buildingCells.Count);
                var cell = buildingCells[idx];
                grid.SetCellType(cell.Row, cell.Col, CellType.Park);
                buildingCells.RemoveAt(idx);
            }

            return (grid, streetInterval, rng);
        }

        #endregion

        #region Property 13: Cell Occupancy Invariant

        // Feature: city-terrain-generation, Property 13: Cell Occupancy Invariant
        /// <summary>
        /// **Validates: Requirements 7.1, 7.2, 7.4, 8.1, 8.2, 8.5, 9.1, 9.2, 9.3, 9.7**
        /// After placing all prop types, no cell has more than one occupant.
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property13_CellOccupancyInvariant_NoCellHasMoreThanOneOccupant()
        {
            PropertyTestUtility.ForAll(random =>
            {
                var (grid, streetInterval, rng) = CreateTestGrid(random);

                // Place all prop types sequentially (as done in the real pipeline)
                var treesResult = PropPlacer.PlaceTrees(grid, rng, 0.33f, 0.5f);
                var carsResult = PropPlacer.PlaceCars(grid, rng, 0.2f);
                var lampResult = PropPlacer.PlaceLampposts(grid, streetInterval);
                var benchResult = PropPlacer.PlaceBenches(grid, rng, 1.0f);
                var trashResult = PropPlacer.PlaceTrashCans(grid, rng, 0.125f, 3);

                // Collect all placed positions and verify no duplicates
                var allPlaced = new HashSet<(int, int)>();

                foreach (var c in treesResult.PlacedCells)
                {
                    Assert.IsTrue(allPlaced.Add((c.Row, c.Col)),
                        $"Cell ({c.Row},{c.Col}) was placed more than once (tree duplicate)");
                }
                foreach (var c in carsResult.PlacedCells)
                {
                    Assert.IsTrue(allPlaced.Add((c.Row, c.Col)),
                        $"Cell ({c.Row},{c.Col}) was placed more than once (car overlaps previous)");
                }
                foreach (var c in lampResult.PlacedCells)
                {
                    Assert.IsTrue(allPlaced.Add((c.Row, c.Col)),
                        $"Cell ({c.Row},{c.Col}) was placed more than once (lamppost overlaps previous)");
                }
                foreach (var c in benchResult.PlacedCells)
                {
                    Assert.IsTrue(allPlaced.Add((c.Row, c.Col)),
                        $"Cell ({c.Row},{c.Col}) was placed more than once (bench overlaps previous)");
                }
                foreach (var c in trashResult.PlacedCells)
                {
                    Assert.IsTrue(allPlaced.Add((c.Row, c.Col)),
                        $"Cell ({c.Row},{c.Col}) was placed more than once (trashcan overlaps previous)");
                }

                // Also verify grid state consistency: occupied cells have exactly one tag
                for (int row = 0; row < grid.Depth; row++)
                {
                    for (int col = 0; col < grid.Width; col++)
                    {
                        var cell = grid.Cells[row, col];
                        if (cell.IsOccupied)
                        {
                            Assert.IsNotNull(cell.OccupantTag,
                                $"Occupied cell at ({row},{col}) has null OccupantTag");
                        }
                    }
                }
            });
        }

        #endregion

        #region Property 14: Prop Placement on Valid Cell Types

        // Feature: city-terrain-generation, Property 14: Prop Placement on Valid Cell Types
        /// <summary>
        /// **Validates: Requirements 7.1, 8.1, 9.1, 9.2, 9.3**
        /// Trees are only on Sidewalk or Park cells.
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property14_TreesOnlyOnSidewalkOrPark()
        {
            PropertyTestUtility.ForAll(random =>
            {
                var (grid, streetInterval, rng) = CreateTestGrid(random);
                var result = PropPlacer.PlaceTrees(grid, rng, 0.33f, 0.5f);

                foreach (var cell in result.PlacedCells)
                {
                    var currentCell = grid.Cells[cell.Row, cell.Col];
                    Assert.IsTrue(
                        currentCell.Type == CellType.Sidewalk || currentCell.Type == CellType.Park,
                        $"Tree placed on invalid cell type {currentCell.Type} at ({cell.Row},{cell.Col})");
                }
            });
        }

        /// <summary>
        /// Cars only on non-intersection Street cells adjacent to Sidewalk.
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property14_CarsOnEligibleStreetCells()
        {
            PropertyTestUtility.ForAll(random =>
            {
                var (grid, streetInterval, rng) = CreateTestGrid(random);
                var result = PropPlacer.PlaceCars(grid, rng, 0.3f);

                foreach (var cell in result.PlacedCells)
                {
                    var currentCell = grid.Cells[cell.Row, cell.Col];

                    // Must be a Street cell
                    Assert.AreEqual(CellType.Street, currentCell.Type,
                        $"Car placed on non-Street cell at ({cell.Row},{cell.Col}), type={currentCell.Type}");

                    // Must not be an intersection
                    Assert.IsFalse(currentCell.IsIntersection,
                        $"Car placed on intersection at ({cell.Row},{cell.Col})");

                    // Must be adjacent to at least one Sidewalk
                    var neighbors = grid.GetAdjacentCells(cell.Row, cell.Col);
                    bool adjacentToSidewalk = neighbors.Any(n => n.Type == CellType.Sidewalk);
                    Assert.IsTrue(adjacentToSidewalk,
                        $"Car at ({cell.Row},{cell.Col}) is not adjacent to any Sidewalk cell");
                }
            });
        }

        /// <summary>
        /// Lampposts on intersection-corner Sidewalk cells (diagonally adjacent to intersection).
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property14_LamppostsOnIntersectionCornerSidewalkCells()
        {
            PropertyTestUtility.ForAll(random =>
            {
                var (grid, streetInterval, rng) = CreateTestGrid(random);
                var result = PropPlacer.PlaceLampposts(grid, streetInterval);

                foreach (var cell in result.PlacedCells)
                {
                    var currentCell = grid.Cells[cell.Row, cell.Col];

                    // Must be a Sidewalk cell
                    Assert.AreEqual(CellType.Sidewalk, currentCell.Type,
                        $"Lamppost placed on non-Sidewalk cell at ({cell.Row},{cell.Col}), type={currentCell.Type}");

                    // Must be diagonally adjacent to an intersection
                    bool isCornerOfIntersection = false;
                    int[] rowOffsets = { -1, -1, 1, 1 };
                    int[] colOffsets = { -1, 1, -1, 1 };

                    for (int i = 0; i < 4; i++)
                    {
                        int checkRow = cell.Row + rowOffsets[i];
                        int checkCol = cell.Col + colOffsets[i];

                        if (checkRow < 0 || checkRow >= grid.Depth) continue;
                        if (checkCol < 0 || checkCol >= grid.Width) continue;

                        if (GridGenerator.IsIntersection(checkRow, checkCol, streetInterval))
                        {
                            isCornerOfIntersection = true;
                            break;
                        }
                    }

                    Assert.IsTrue(isCornerOfIntersection,
                        $"Lamppost at ({cell.Row},{cell.Col}) is not diagonally adjacent to any intersection");
                }
            });
        }

        /// <summary>
        /// Benches on Sidewalk cells adjacent to Park cells.
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property14_BenchesOnSidewalkAdjacentToPark()
        {
            PropertyTestUtility.ForAll(random =>
            {
                var (grid, streetInterval, rng) = CreateGridWithParks(random);
                var result = PropPlacer.PlaceBenches(grid, rng, 1.0f);

                foreach (var cell in result.PlacedCells)
                {
                    var currentCell = grid.Cells[cell.Row, cell.Col];

                    // Must be a Sidewalk cell
                    Assert.AreEqual(CellType.Sidewalk, currentCell.Type,
                        $"Bench placed on non-Sidewalk cell at ({cell.Row},{cell.Col}), type={currentCell.Type}");

                    // Must be adjacent to at least one Park cell
                    var neighbors = grid.GetAdjacentCells(cell.Row, cell.Col);
                    bool adjacentToPark = neighbors.Any(n => n.Type == CellType.Park);
                    Assert.IsTrue(adjacentToPark,
                        $"Bench at ({cell.Row},{cell.Col}) is not adjacent to any Park cell");
                }
            });
        }

        /// <summary>
        /// Trash cans only on Sidewalk cells.
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property14_TrashCansOnSidewalkCells()
        {
            PropertyTestUtility.ForAll(random =>
            {
                var (grid, streetInterval, rng) = CreateTestGrid(random);
                var result = PropPlacer.PlaceTrashCans(grid, rng, 0.125f, 3);

                foreach (var cell in result.PlacedCells)
                {
                    var currentCell = grid.Cells[cell.Row, cell.Col];
                    Assert.AreEqual(CellType.Sidewalk, currentCell.Type,
                        $"Trash can placed on non-Sidewalk cell at ({cell.Row},{cell.Col}), type={currentCell.Type}");
                }
            });
        }

        #endregion

        #region Property 15: Prop Density Count

        // Feature: city-terrain-generation, Property 15: Prop Density Count
        /// <summary>
        /// **Validates: Requirements 7.2, 8.2**
        /// Placed count equals floor(eligible × density) or max available, whichever is smaller.
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property15_TreeDensityCount()
        {
            PropertyTestUtility.ForAll(random =>
            {
                var (grid, streetInterval, rng, density) = CreateTestGridWithDensity(random);

                var sidewalkCells = grid.GetUnoccupiedCellsOfType(CellType.Sidewalk);
                var parkCells = grid.GetUnoccupiedCellsOfType(CellType.Park);

                int expectedSidewalk = (int)Math.Floor(sidewalkCells.Count * density);
                int expectedPark = (int)Math.Floor(parkCells.Count * density);
                int totalExpected = expectedSidewalk + expectedPark;

                var result = PropPlacer.PlaceTrees(grid, rng, density, density);

                Assert.AreEqual(totalExpected, result.RequestedCount,
                    $"RequestedCount mismatch: expected {totalExpected}, got {result.RequestedCount}");
                Assert.LessOrEqual(result.ActualCount, result.RequestedCount,
                    "ActualCount exceeds RequestedCount");
            });
        }

        [Test]
        [Category("Property")]
        public void Property15_CarDensityCount()
        {
            PropertyTestUtility.ForAll(random =>
            {
                var (grid, streetInterval, rng, density) = CreateTestGridWithDensity(random);

                // Count eligible cells before placement
                int eligibleCount = 0;
                for (int row = 0; row < grid.Depth; row++)
                {
                    for (int col = 0; col < grid.Width; col++)
                    {
                        var cell = grid.Cells[row, col];
                        if (cell.Type != CellType.Street) continue;
                        if (cell.IsIntersection) continue;
                        if (cell.IsOccupied) continue;

                        var neighbors = grid.GetAdjacentCells(row, col);
                        if (neighbors.Any(n => n.Type == CellType.Sidewalk))
                            eligibleCount++;
                    }
                }

                int expectedCount = (int)Math.Floor(eligibleCount * density);
                var result = PropPlacer.PlaceCars(grid, rng, density);

                Assert.AreEqual(expectedCount, result.RequestedCount,
                    $"Car RequestedCount mismatch: expected {expectedCount}, got {result.RequestedCount}");
                Assert.LessOrEqual(result.ActualCount, result.RequestedCount,
                    "Car ActualCount exceeds RequestedCount");
                Assert.LessOrEqual(result.ActualCount, eligibleCount);
            });
        }

        [Test]
        [Category("Property")]
        public void Property15_TrashCanDensityCount()
        {
            PropertyTestUtility.ForAll(random =>
            {
                var (grid, streetInterval, rng, density) = CreateTestGridWithDensity(random);

                var eligibleCells = grid.GetUnoccupiedCellsOfType(CellType.Sidewalk);
                int expectedCount = (int)Math.Floor(eligibleCells.Count * density);

                var result = PropPlacer.PlaceTrashCans(grid, rng, density, 3);

                Assert.AreEqual(expectedCount, result.RequestedCount,
                    $"TrashCan RequestedCount mismatch: expected {expectedCount}, got {result.RequestedCount}");
                // Due to spacing constraints, actual may be less than requested
                Assert.LessOrEqual(result.ActualCount, result.RequestedCount,
                    "TrashCan ActualCount exceeds RequestedCount");
            });
        }

        #endregion

        #region Property 16: Car Orientation Matches Street Direction

        // Feature: city-terrain-generation, Property 16: Car Orientation Matches Street Direction
        /// <summary>
        /// **Validates: Requirements 8.4**
        /// For any placed car, if on a horizontal street (row % interval == 0), orientation
        /// should be 0°/180°. If on a vertical street (col % interval == 0), orientation
        /// should be 90°/270°. This test verifies car cells are on determinable street directions.
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property16_CarCellsOnDeterminableStreetDirection()
        {
            PropertyTestUtility.ForAll(random =>
            {
                var (grid, streetInterval, rng) = CreateTestGrid(random);
                var result = PropPlacer.PlaceCars(grid, rng, 0.3f);

                foreach (var cell in result.PlacedCells)
                {
                    bool isOnHorizontalStreet = cell.Row % streetInterval == 0;
                    bool isOnVerticalStreet = cell.Col % streetInterval == 0;

                    // Car should not be on intersection (both row and col are street positions)
                    Assert.IsFalse(isOnHorizontalStreet && isOnVerticalStreet,
                        $"Car at ({cell.Row},{cell.Col}) is on an intersection");

                    // Car must be on either a horizontal or vertical street
                    Assert.IsTrue(isOnHorizontalStreet || isOnVerticalStreet,
                        $"Car at ({cell.Row},{cell.Col}) is not on a street row or column");
                }
            });
        }

        #endregion

        #region Property 17: Trash Can Minimum Spacing

        // Feature: city-terrain-generation, Property 17: Trash Can Minimum Spacing
        /// <summary>
        /// **Validates: Requirements 9.3**
        /// For any two placed trash cans in the city, their Manhattan distance is ≥ 3 cells.
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property17_TrashCanMinimumSpacing_ManhattanDistanceAtLeast3()
        {
            PropertyTestUtility.ForAll(random =>
            {
                var (grid, streetInterval, rng) = CreateTestGrid(random);
                var result = PropPlacer.PlaceTrashCans(grid, rng, 0.2f, 3);

                var placedCells = result.PlacedCells;
                for (int i = 0; i < placedCells.Count; i++)
                {
                    for (int j = i + 1; j < placedCells.Count; j++)
                    {
                        int distance = placedCells[i].ManhattanDistance(placedCells[j]);
                        Assert.GreaterOrEqual(distance, 3,
                            $"Trash cans at ({placedCells[i].Row},{placedCells[i].Col}) and " +
                            $"({placedCells[j].Row},{placedCells[j].Col}) have Manhattan distance " +
                            $"{distance} < 3");
                    }
                }
            });
        }

        /// <summary>
        /// Trash can spacing holds with various density values.
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property17_TrashCanSpacing_WithVariousDensities()
        {
            PropertyTestUtility.ForAll(random =>
            {
                var (grid, streetInterval, rng, density) = CreateTestGridWithDensity(random);
                var result = PropPlacer.PlaceTrashCans(grid, rng, density, 3);

                var placedCells = result.PlacedCells;
                for (int i = 0; i < placedCells.Count; i++)
                {
                    for (int j = i + 1; j < placedCells.Count; j++)
                    {
                        int distance = placedCells[i].ManhattanDistance(placedCells[j]);
                        Assert.GreaterOrEqual(distance, 3,
                            $"Trash cans at ({placedCells[i].Row},{placedCells[i].Col}) and " +
                            $"({placedCells[j].Row},{placedCells[j].Col}) have Manhattan distance " +
                            $"{distance} < 3 (density={density})");
                    }
                }
            });
        }

        #endregion
    }
}
