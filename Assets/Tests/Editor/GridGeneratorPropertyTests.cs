using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Flipit.CityTerrain;

namespace Flipit.CityTerrain.Tests
{
    /// <summary>
    /// Property-based tests for GridGenerator.AssignCellTypes.
    /// Validates correctness properties 1–5 from the city-terrain-generation spec.
    /// Uses FsCheck-style generators with random width 5–30, depth 5–30, interval 3–8.
    /// Feature: city-terrain-generation
    /// </summary>
    [TestFixture]
    public class GridGeneratorPropertyTests
    {
        #region Generators

        /// <summary>
        /// Generates random grid parameters within valid ranges.
        /// Width: 5–30, Depth: 5–30, Interval: 3–8
        /// </summary>
        private static (int width, int depth, int interval) RandomGridParams(System.Random rng)
        {
            int width = rng.Next(5, 31);
            int depth = rng.Next(5, 31);
            int interval = rng.Next(3, 9);
            return (width, depth, interval);
        }

        /// <summary>
        /// Creates a grid and assigns cell types using GridGenerator.
        /// </summary>
        private static Grid_Layout CreateAndAssignGrid(int width, int depth, int interval)
        {
            var grid = new Grid_Layout(width, depth, 4f);
            GridGenerator.AssignCellTypes(grid, interval);
            return grid;
        }

        #endregion

        // ===================================================================
        // Property 1: Grid Cell Partition
        // Feature: city-terrain-generation, Property 1: Grid Cell Partition
        // ===================================================================

        /// <summary>
        /// Property 1: Grid Cell Partition — Every cell is assigned exactly one type
        /// from {Street, Sidewalk, Building} and total equals width × depth.
        /// **Validates: Requirements 2.1, 2.3, 2.4**
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property1_GridCellPartition_AllCellsAssignedExactlyOneType()
        {
            PropertyTestUtility.ForAll(rng =>
            {
                var (width, depth, interval) = RandomGridParams(rng);
                var grid = CreateAndAssignGrid(width, depth, interval);

                int streetCount = 0;
                int sidewalkCount = 0;
                int buildingCount = 0;
                int otherCount = 0;

                for (int row = 0; row < depth; row++)
                {
                    for (int col = 0; col < width; col++)
                    {
                        var cellType = grid.Cells[row, col].Type;
                        switch (cellType)
                        {
                            case CellType.Street:
                                streetCount++;
                                break;
                            case CellType.Sidewalk:
                                sidewalkCount++;
                                break;
                            case CellType.Building:
                                buildingCount++;
                                break;
                            default:
                                otherCount++;
                                break;
                        }
                    }
                }

                int totalAssigned = streetCount + sidewalkCount + buildingCount;
                int expectedTotal = width * depth;

                Assert.AreEqual(0, otherCount,
                    $"No cells should have Park/Empty type. Grid: {width}x{depth}, interval: {interval}");
                Assert.AreEqual(expectedTotal, totalAssigned,
                    $"Total assigned ({totalAssigned}) should equal width*depth ({expectedTotal}). " +
                    $"Grid: {width}x{depth}, interval: {interval}");
            });
        }

        // ===================================================================
        // Property 2: Street Connectivity
        // Feature: city-terrain-generation, Property 2: Street Connectivity
        // ===================================================================

        /// <summary>
        /// Property 2: Street Connectivity — All Street_Cells form a connected graph
        /// via orthogonal adjacency (any street cell is reachable from any other
        /// street cell through other street cells).
        /// **Validates: Requirements 2.1**
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property2_StreetConnectivity_AllStreetCellsFormConnectedGraph()
        {
            PropertyTestUtility.ForAll(rng =>
            {
                var (width, depth, interval) = RandomGridParams(rng);
                var grid = CreateAndAssignGrid(width, depth, interval);

                // Collect all street cell positions
                var streetCells = new HashSet<(int row, int col)>();
                for (int row = 0; row < depth; row++)
                {
                    for (int col = 0; col < width; col++)
                    {
                        if (grid.Cells[row, col].Type == CellType.Street)
                            streetCells.Add((row, col));
                    }
                }

                Assert.IsTrue(streetCells.Count > 0,
                    $"Grid {width}x{depth} with interval {interval} should have street cells");

                // BFS from the first street cell to verify connectivity
                var start = streetCells.First();
                var visited = new HashSet<(int row, int col)> { start };
                var queue = new Queue<(int row, int col)>();
                queue.Enqueue(start);

                while (queue.Count > 0)
                {
                    var (r, c) = queue.Dequeue();
                    var neighbors = new (int row, int col)[]
                    {
                        (r - 1, c), (r + 1, c), (r, c - 1), (r, c + 1)
                    };

                    foreach (var neighbor in neighbors)
                    {
                        if (streetCells.Contains(neighbor) && !visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }

                Assert.AreEqual(streetCells.Count, visited.Count,
                    $"BFS visited {visited.Count} of {streetCells.Count} street cells. " +
                    $"Streets are not fully connected. Grid: {width}x{depth}, interval: {interval}");
            });
        }

        // ===================================================================
        // Property 3: Sidewalk Adjacency Invariant
        // Feature: city-terrain-generation, Property 3: Sidewalk Adjacency Invariant
        // ===================================================================

        /// <summary>
        /// Property 3: Sidewalk Adjacency Invariant — Every cell that is orthogonally
        /// adjacent to a Street_Cell and is not itself a Street_Cell must be Sidewalk.
        /// **Validates: Requirements 2.3**
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property3_SidewalkAdjacencyInvariant_NonStreetNeighborsOfStreetsAreSidewalk()
        {
            PropertyTestUtility.ForAll(rng =>
            {
                var (width, depth, interval) = RandomGridParams(rng);
                var grid = CreateAndAssignGrid(width, depth, interval);

                for (int row = 0; row < depth; row++)
                {
                    for (int col = 0; col < width; col++)
                    {
                        if (grid.Cells[row, col].Type != CellType.Street)
                            continue;

                        // Check all orthogonal neighbors of this street cell
                        var neighbors = new (int r, int c)[]
                        {
                            (row - 1, col), (row + 1, col), (row, col - 1), (row, col + 1)
                        };

                        foreach (var (r, c) in neighbors)
                        {
                            if (r < 0 || r >= depth || c < 0 || c >= width)
                                continue;

                            var neighborType = grid.Cells[r, c].Type;
                            if (neighborType != CellType.Street)
                            {
                                Assert.AreEqual(CellType.Sidewalk, neighborType,
                                    $"Cell ({r},{c}) is {neighborType} but adjacent to Street at ({row},{col}). " +
                                    $"Should be Sidewalk. Grid: {width}x{depth}, interval: {interval}");
                            }
                        }
                    }
                }
            });
        }

        // ===================================================================
        // Property 4: Street Placement at Intervals
        // Feature: city-terrain-generation, Property 4: Street Placement at Intervals
        // ===================================================================

        /// <summary>
        /// Property 4: Street Placement at Intervals — Cell at (row, col) is Street
        /// if and only if (row % I == 0) or (col % I == 0).
        /// **Validates: Requirements 2.2**
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property4_StreetPlacementAtIntervals_StreetIffRowOrColModIntervalIsZero()
        {
            PropertyTestUtility.ForAll(rng =>
            {
                var (width, depth, interval) = RandomGridParams(rng);
                var grid = CreateAndAssignGrid(width, depth, interval);

                for (int row = 0; row < depth; row++)
                {
                    for (int col = 0; col < width; col++)
                    {
                        bool shouldBeStreet = (row % interval == 0) || (col % interval == 0);
                        bool isStreet = grid.Cells[row, col].Type == CellType.Street;

                        Assert.AreEqual(shouldBeStreet, isStreet,
                            $"Cell ({row},{col}): shouldBeStreet={shouldBeStreet}, isStreet={isStreet}. " +
                            $"Grid: {width}x{depth}, interval: {interval}");
                    }
                }
            });
        }

        // ===================================================================
        // Property 5: Cell Height Correctness
        // Feature: city-terrain-generation, Property 5: Cell Height Correctness
        // ===================================================================

        /// <summary>
        /// Property 5: Cell Height Correctness — Streets at Y=0, Sidewalks at Y=0.05.
        /// The WorldPosition method returns the correct Y value for each cell type.
        /// **Validates: Requirements 2.5, 2.6**
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property5_CellHeightCorrectness_StreetsAtY0_SidewalksAtY005()
        {
            PropertyTestUtility.ForAll(rng =>
            {
                var (width, depth, interval) = RandomGridParams(rng);
                var grid = CreateAndAssignGrid(width, depth, interval);
                float cellSize = grid.CellSize;

                for (int row = 0; row < depth; row++)
                {
                    for (int col = 0; col < width; col++)
                    {
                        var cell = grid.Cells[row, col];
                        Vector3 worldPos = cell.WorldPosition(cellSize);

                        if (cell.Type == CellType.Street)
                        {
                            Assert.IsTrue(Mathf.Approximately(worldPos.y, 0f),
                                $"Street cell ({row},{col}) has Y={worldPos.y}, expected 0. " +
                                $"Grid: {width}x{depth}, interval: {interval}");
                        }
                        else if (cell.Type == CellType.Sidewalk)
                        {
                            Assert.IsTrue(Mathf.Approximately(worldPos.y, 0.05f),
                                $"Sidewalk cell ({row},{col}) has Y={worldPos.y}, expected 0.05. " +
                                $"Grid: {width}x{depth}, interval: {interval}");
                        }
                    }
                }
            });
        }
    }
}
