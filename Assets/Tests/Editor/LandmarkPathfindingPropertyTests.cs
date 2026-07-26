using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Flipit.CityTerrain;
using Random = System.Random;

namespace Flipit.CityTerrain.Tests
{
    /// <summary>
    /// Property-based tests for LandmarkPlacer and PathFinder algorithms.
    /// Feature: city-terrain-generation, Properties 7–8
    /// Validates: Requirements 3.1, 3.2, 3.5
    /// Uses PropertyTestUtility with random valid grids (width 8–30, depth 8–30, interval 3–8).
    /// </summary>
    [TestFixture]
    public class LandmarkPathfindingPropertyTests
    {
        /// <summary>
        /// Helper: Generates random grid parameters within valid ranges for landmark tests.
        /// Uses larger minimum (8) to ensure enough cells for landmarks on opposite edges.
        /// </summary>
        private static (int width, int depth, int interval, float cellSize) RandomGridParams(Random rng)
        {
            int width = rng.Next(8, 31);
            int depth = rng.Next(8, 31);
            int interval = rng.Next(3, 9);
            float cellSize = rng.Next(1, 11);
            return (width, depth, interval, cellSize);
        }

        /// <summary>
        /// Helper: Creates a valid grid with assigned cell types.
        /// </summary>
        private static Grid_Layout CreateValidGrid(int width, int depth, float cellSize, int interval)
        {
            var grid = new Grid_Layout(width, depth, cellSize);
            GridGenerator.AssignCellTypes(grid, interval);
            return grid;
        }

        // ===================================================================
        // Property 7: Landmark Placement Constraints
        // Feature: city-terrain-generation, Property 7: Landmark Placement Constraints
        // ===================================================================

        /// <summary>
        /// Property 7: Landmark Placement Constraints
        /// For any valid grid where landmark placement succeeds:
        /// - School is on a grid-edge Building_Cell adjacent to a Sidewalk_Cell
        /// - House is on the opposite edge adjacent to a Sidewalk_Cell
        /// - Euclidean distance between them is >= 60% of grid diagonal
        /// **Validates: Requirements 3.1, 3.2**
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property7_LandmarkPlacementConstraints()
        {
            int successCount = 0;

            PropertyTestUtility.ForAll(rng =>
            {
                var (width, depth, interval, cellSize) = RandomGridParams(rng);
                var grid = CreateValidGrid(width, depth, cellSize, interval);
                var result = LandmarkPlacer.PlaceLandmarks(grid);

                // Only validate when placement succeeds (small grids may fail)
                if (!result.Success)
                    return;

                successCount++;

                // School must be on edge
                Assert.IsTrue(grid.IsOnEdge(result.SchoolCell.Row, result.SchoolCell.Col),
                    $"School at ({result.SchoolCell.Row},{result.SchoolCell.Col}) is not on grid edge. " +
                    $"Grid: {width}x{depth}, interval: {interval}");

                // School must be a Building cell
                Assert.AreEqual(CellType.Building, result.SchoolCell.Type,
                    $"School cell type is {result.SchoolCell.Type}, expected Building. " +
                    $"Grid: {width}x{depth}, interval: {interval}");

                // School must be adjacent to a Sidewalk cell
                Assert.IsTrue(HasAdjacentCellOfType(grid, result.SchoolCell.Row, result.SchoolCell.Col, CellType.Sidewalk),
                    $"School at ({result.SchoolCell.Row},{result.SchoolCell.Col}) is not adjacent to any Sidewalk. " +
                    $"Grid: {width}x{depth}, interval: {interval}");

                // House must be on edge
                Assert.IsTrue(grid.IsOnEdge(result.HouseCell.Row, result.HouseCell.Col),
                    $"House at ({result.HouseCell.Row},{result.HouseCell.Col}) is not on grid edge. " +
                    $"Grid: {width}x{depth}, interval: {interval}");

                // House must be a Building cell
                Assert.AreEqual(CellType.Building, result.HouseCell.Type,
                    $"House cell type is {result.HouseCell.Type}, expected Building. " +
                    $"Grid: {width}x{depth}, interval: {interval}");

                // House must be adjacent to a Sidewalk cell
                Assert.IsTrue(HasAdjacentCellOfType(grid, result.HouseCell.Row, result.HouseCell.Col, CellType.Sidewalk),
                    $"House at ({result.HouseCell.Row},{result.HouseCell.Col}) is not adjacent to any Sidewalk. " +
                    $"Grid: {width}x{depth}, interval: {interval}");

                // School and House must be on opposite edges
                Assert.IsTrue(AreOnOppositeEdges(grid, result.SchoolCell, result.HouseCell),
                    $"School ({result.SchoolCell.Row},{result.SchoolCell.Col}) and " +
                    $"House ({result.HouseCell.Row},{result.HouseCell.Col}) are not on opposite edges. " +
                    $"Grid: {width}x{depth}, interval: {interval}");

                // Distance must be >= 60% of grid diagonal
                float distance = Vector3.Distance(
                    result.SchoolCell.WorldPosition(grid.CellSize),
                    result.HouseCell.WorldPosition(grid.CellSize));

                float gridWorldWidth = grid.Width * grid.CellSize;
                float gridWorldDepth = grid.Depth * grid.CellSize;
                float diagonal = Mathf.Sqrt(
                    gridWorldWidth * gridWorldWidth + gridWorldDepth * gridWorldDepth);
                float minDistance = 0.6f * diagonal;

                Assert.GreaterOrEqual(distance, minDistance - 0.001f,
                    $"Distance {distance:F2} < 60% diagonal {minDistance:F2}. " +
                    $"School ({result.SchoolCell.Row},{result.SchoolCell.Col}), " +
                    $"House ({result.HouseCell.Row},{result.HouseCell.Col}). " +
                    $"Grid: {width}x{depth}, cellSize: {cellSize}, interval: {interval}");
            });

            // Ensure at least some placements succeeded to avoid vacuous truth
            Assert.Greater(successCount, 0,
                "No landmark placements succeeded across all iterations — test may be vacuously true.");
            TestContext.WriteLine($"[PBT] Property 7: {successCount} successful placements validated.");
        }

        // ===================================================================
        // Property 8: School Spawn Point Position
        // Feature: city-terrain-generation, Property 8: School Spawn Point Position
        // ===================================================================

        /// <summary>
        /// Property 8: School Spawn Point Position
        /// For any valid grid where landmark placement succeeds:
        /// - The SchoolSpawnCell is a Sidewalk cell adjacent to the SchoolCell
        /// - Its world position Y is 0.05 (sidewalk height)
        /// - Its world position is at the center of that Sidewalk_Cell
        /// **Validates: Requirements 3.5**
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property8_SchoolSpawnPointPosition()
        {
            int successCount = 0;

            PropertyTestUtility.ForAll(rng =>
            {
                var (width, depth, interval, cellSize) = RandomGridParams(rng);
                var grid = CreateValidGrid(width, depth, cellSize, interval);
                var result = LandmarkPlacer.PlaceLandmarks(grid);

                // Only validate when placement succeeds
                if (!result.Success)
                    return;

                successCount++;

                // SchoolSpawnCell must be a Sidewalk cell
                Assert.AreEqual(CellType.Sidewalk, result.SchoolSpawnCell.Type,
                    $"SchoolSpawnCell type is {result.SchoolSpawnCell.Type}, expected Sidewalk. " +
                    $"Grid: {width}x{depth}, interval: {interval}");

                // SchoolSpawnCell must be adjacent to SchoolCell
                Assert.IsTrue(IsOrthogonallyAdjacent(result.SchoolSpawnCell, result.SchoolCell),
                    $"SchoolSpawnCell ({result.SchoolSpawnCell.Row},{result.SchoolSpawnCell.Col}) " +
                    $"is not adjacent to SchoolCell ({result.SchoolCell.Row},{result.SchoolCell.Col}). " +
                    $"Grid: {width}x{depth}, interval: {interval}");

                // The spawn position Y must be 0.05 (sidewalk height)
                Vector3 spawnPos = result.SchoolSpawnCell.WorldPosition(grid.CellSize);
                Assert.IsTrue(Mathf.Approximately(spawnPos.y, 0.05f),
                    $"SchoolSpawnCell Y={spawnPos.y}, expected 0.05. " +
                    $"Grid: {width}x{depth}, interval: {interval}");

                // The spawn position should be at the cell center
                // Cell center: x = col * cellSize, z = row * cellSize
                float expectedX = result.SchoolSpawnCell.Col * grid.CellSize;
                float expectedZ = result.SchoolSpawnCell.Row * grid.CellSize;
                Assert.IsTrue(Mathf.Approximately(spawnPos.x, expectedX),
                    $"SchoolSpawnCell X={spawnPos.x}, expected {expectedX}. " +
                    $"Grid: {width}x{depth}, interval: {interval}");
                Assert.IsTrue(Mathf.Approximately(spawnPos.z, expectedZ),
                    $"SchoolSpawnCell Z={spawnPos.z}, expected {expectedZ}. " +
                    $"Grid: {width}x{depth}, interval: {interval}");
            });

            // Ensure at least some placements succeeded
            Assert.Greater(successCount, 0,
                "No landmark placements succeeded across all iterations — test may be vacuously true.");
            TestContext.WriteLine($"[PBT] Property 8: {successCount} successful placements validated.");
        }

        // ===================================================================
        // Helper methods
        // ===================================================================

        private static bool HasAdjacentCellOfType(Grid_Layout grid, int row, int col, CellType type)
        {
            var neighbors = grid.GetAdjacentCells(row, col);
            foreach (var neighbor in neighbors)
            {
                if (neighbor.Type == type)
                    return true;
            }
            return false;
        }

        private static bool AreOnOppositeEdges(Grid_Layout grid, CellData a, CellData b)
        {
            bool aOnTop = a.Row == 0;
            bool aOnBottom = a.Row == grid.Depth - 1;
            bool aOnLeft = a.Col == 0;
            bool aOnRight = a.Col == grid.Width - 1;

            bool bOnTop = b.Row == 0;
            bool bOnBottom = b.Row == grid.Depth - 1;
            bool bOnLeft = b.Col == 0;
            bool bOnRight = b.Col == grid.Width - 1;

            // Check top-bottom pair
            if ((aOnTop && bOnBottom) || (aOnBottom && bOnTop))
                return true;

            // Check left-right pair
            if ((aOnLeft && bOnRight) || (aOnRight && bOnLeft))
                return true;

            return false;
        }

        private static bool IsOrthogonallyAdjacent(CellData a, CellData b)
        {
            int rowDiff = Math.Abs(a.Row - b.Row);
            int colDiff = Math.Abs(a.Col - b.Col);

            // Orthogonally adjacent means exactly one step in one direction
            return (rowDiff == 1 && colDiff == 0) || (rowDiff == 0 && colDiff == 1);
        }
    }
}
