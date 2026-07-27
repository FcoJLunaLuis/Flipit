using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Flipit.CityTerrain.Tests
{
    /// <summary>
    /// Property-based tests for NPCPlacer and EncounterPlacer algorithms.
    /// Feature: city-terrain-generation, Properties 18–22
    /// Validates: Requirements 10.1, 10.2, 10.3, 10.4, 11.1, 11.2, 11.3, 11.4, 12.1, 12.2, 12.3, 12.5, 12.6
    /// </summary>
    [TestFixture]
    public class NPCEncounterPropertyTests
    {
        // ===================================================================
        // Shared generators and helpers
        // ===================================================================

        /// <summary>
        /// Creates a test scenario with grid, landmarks, and path for NPC/encounter testing.
        /// Returns null if landmark placement or pathfinding fails.
        /// </summary>
        private static NPCTestScenario CreateScenario(System.Random rng)
        {
            int width = rng.Next(10, 26);
            int depth = rng.Next(10, 26);
            int streetInterval = rng.Next(3, 7);
            int seed = rng.Next(1, 100000);
            int vendorCount = rng.Next(1, 5);
            int challengerCount = rng.Next(1, 6);
            int encounterCount = rng.Next(1, 5);

            var grid = new Grid_Layout(width, depth, 4f);
            GridGenerator.AssignCellTypes(grid, streetInterval);

            var landmarkResult = LandmarkPlacer.PlaceLandmarks(grid);
            if (!landmarkResult.Success)
                return null;

            // Mark landmark cells as occupied
            grid.MarkOccupied(landmarkResult.SchoolCell.Row, landmarkResult.SchoolCell.Col, "school");
            grid.MarkOccupied(landmarkResult.HouseCell.Row, landmarkResult.HouseCell.Col, "house");

            // Find path between school spawn and house trigger
            var path = PathFinder.FindPath(grid, landmarkResult.SchoolSpawnCell, landmarkResult.HouseTriggerCell);
            if (path.Count == 0)
                return null;

            return new NPCTestScenario
            {
                Grid = grid,
                StreetInterval = streetInterval,
                Seed = seed,
                VendorCount = vendorCount,
                ChallengerCount = challengerCount,
                EncounterCount = encounterCount,
                Landmarks = landmarkResult,
                Path = path
            };
        }

        /// <summary>
        /// Runs a property test that requires a valid NPC scenario.
        /// Skips iterations where scenario creation fails (small grids may not support landmarks).
        /// </summary>
        private static void ForAllValidScenarios(Action<NPCTestScenario> property, int iterations = 100)
        {
            PropertyTestUtility.ForAll(rng =>
            {
                var scenario = CreateScenario(rng);
                if (scenario == null || scenario.Path.Count <= 5)
                    return; // Skip invalid scenarios
                property(scenario);
            }, iterations);
        }

        // ===================================================================
        // Property 18: NPC Placement Validity and Clearance
        // Feature: city-terrain-generation, Property 18
        // ===================================================================

        /// <summary>
        /// Property 18: NPC Placement Validity and Clearance
        /// For any valid grid configuration:
        /// - Vendors are placed on Sidewalk cells adjacent to at least one Building cell
        /// - Vendors have ≥ 2 Manhattan distance clearance from all other NPCs
        /// - Max 1 vendor per city block (defined by row/streetInterval, col/streetInterval)
        /// - Challengers are placed on Sidewalk cells
        /// - Challengers have ≥ 3 Manhattan distance clearance from all other NPCs
        /// **Validates: Requirements 10.1, 10.2, 10.4, 11.1, 11.2, 11.4**
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property18_NPCPlacementValidityAndClearance()
        {
            ForAllValidScenarios(scenario =>
            {
                var rng = new System.Random(scenario.Seed);

                // Place vendors first
                var vendorResult = NPCPlacer.PlaceVendors(
                    scenario.Grid, rng, scenario.VendorCount, scenario.StreetInterval);

                // Place challengers after vendors
                var challengerResult = NPCPlacer.PlaceChallengers(
                    scenario.Grid, rng, scenario.ChallengerCount, scenario.Path);

                // Validate vendors on Sidewalk adjacent to Building
                foreach (var vendorCell in vendorResult.PlacedCells)
                {
                    Assert.AreEqual(CellType.Sidewalk, vendorCell.Type,
                        $"Vendor at ({vendorCell.Row},{vendorCell.Col}) should be on Sidewalk but is {vendorCell.Type}");

                    Assert.IsTrue(HasAdjacentCellOfType(scenario.Grid, vendorCell.Row, vendorCell.Col, CellType.Building),
                        $"Vendor at ({vendorCell.Row},{vendorCell.Col}) should be adjacent to a Building cell");
                }

                // Validate vendor max 1 per block
                var vendorBlocks = new HashSet<(int, int)>();
                foreach (var vendorCell in vendorResult.PlacedCells)
                {
                    var blockKey = (vendorCell.Row / scenario.StreetInterval, vendorCell.Col / scenario.StreetInterval);
                    Assert.IsFalse(vendorBlocks.Contains(blockKey),
                        $"More than 1 vendor in block ({blockKey.Item1},{blockKey.Item2})");
                    vendorBlocks.Add(blockKey);
                }

                // Validate vendor clearance (≥ 2 from other vendors)
                for (int i = 0; i < vendorResult.PlacedCells.Count; i++)
                {
                    for (int j = i + 1; j < vendorResult.PlacedCells.Count; j++)
                    {
                        int dist = vendorResult.PlacedCells[i].ManhattanDistance(vendorResult.PlacedCells[j]);
                        Assert.GreaterOrEqual(dist, 2,
                            $"Vendor pair ({i},{j}) has distance {dist}, expected ≥ 2");
                    }
                }

                // Validate challengers on Sidewalk
                foreach (var challengerCell in challengerResult.PlacedCells)
                {
                    Assert.AreEqual(CellType.Sidewalk, challengerCell.Type,
                        $"Challenger at ({challengerCell.Row},{challengerCell.Col}) should be on Sidewalk");
                }

                // Validate challenger clearance (≥ 3 from all NPCs including vendors)
                var allNPCs = new List<CellData>();
                allNPCs.AddRange(vendorResult.PlacedCells);
                allNPCs.AddRange(challengerResult.PlacedCells);

                for (int i = 0; i < challengerResult.PlacedCells.Count; i++)
                {
                    var challenger = challengerResult.PlacedCells[i];
                    foreach (var other in allNPCs)
                    {
                        if (other.Row == challenger.Row && other.Col == challenger.Col)
                            continue; // Skip self
                        int dist = challenger.ManhattanDistance(other);
                        Assert.GreaterOrEqual(dist, 3,
                            $"Challenger at ({challenger.Row},{challenger.Col}) has distance {dist} from NPC at ({other.Row},{other.Col}), expected ≥ 3");
                    }
                }
            });
        }

        // ===================================================================
        // Property 19: NPC Component Setup (simplified for Edit Mode)
        // Feature: city-terrain-generation, Property 19
        // ===================================================================

        /// <summary>
        /// Property 19: NPC Component Setup
        /// For any valid NPC placement result:
        /// - Vendor NPCType is "vendor"
        /// - Challenger NPCType is "challenger"
        /// - Placed cells are marked with appropriate OccupantTag
        /// **Validates: Requirements 10.3, 11.3**
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property19_NPCComponentSetup()
        {
            ForAllValidScenarios(scenario =>
            {
                var rng = new System.Random(scenario.Seed);

                var vendorResult = NPCPlacer.PlaceVendors(
                    scenario.Grid, rng, scenario.VendorCount, scenario.StreetInterval);

                var challengerResult = NPCPlacer.PlaceChallengers(
                    scenario.Grid, rng, scenario.ChallengerCount, scenario.Path);

                // Vendor NPCType must be "vendor"
                Assert.AreEqual("vendor", vendorResult.NPCType);

                // Challenger NPCType must be "challenger"
                Assert.AreEqual("challenger", challengerResult.NPCType);

                // All vendor placed cells must be marked occupied with "vendor" tag
                foreach (var cell in vendorResult.PlacedCells)
                {
                    var gridCell = scenario.Grid.GetCell(cell.Row, cell.Col);
                    Assert.IsTrue(gridCell.IsOccupied,
                        $"Vendor cell ({cell.Row},{cell.Col}) should be marked occupied");
                    Assert.AreEqual("vendor", gridCell.OccupantTag,
                        $"Vendor cell ({cell.Row},{cell.Col}) OccupantTag should be 'vendor'");
                }

                // All challenger placed cells must be marked occupied with "challenger" tag
                foreach (var cell in challengerResult.PlacedCells)
                {
                    var gridCell = scenario.Grid.GetCell(cell.Row, cell.Col);
                    Assert.IsTrue(gridCell.IsOccupied,
                        $"Challenger cell ({cell.Row},{cell.Col}) should be marked occupied");
                    Assert.AreEqual("challenger", gridCell.OccupantTag,
                        $"Challenger cell ({cell.Row},{cell.Col}) OccupantTag should be 'challenger'");
                }
            });
        }

        // ===================================================================
        // Property 20: Challenger Even Distribution Along Path
        // Feature: city-terrain-generation, Property 20
        // ===================================================================

        /// <summary>
        /// Property 20: Challenger Even Distribution Along Path
        /// For any valid grid with challengers placed along the path:
        /// - The gaps between consecutive challengers (measured in path index distance)
        ///   are within ±30% of the average gap.
        /// **Validates: Requirements 11.2**
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property20_ChallengerEvenDistributionAlongPath()
        {
            ForAllValidScenarios(scenario =>
            {
                if (scenario.Path.Count <= 10)
                    return; // Need longer path for distribution test

                var rng = new System.Random(scenario.Seed);
                int challengerCount = Math.Max(2, scenario.ChallengerCount);

                var challengerResult = NPCPlacer.PlaceChallengers(
                    scenario.Grid, rng, challengerCount, scenario.Path);

                // Need at least 2 challengers to measure distribution
                if (challengerResult.ActualCount < 2)
                    return;

                // Find the path indices of each placed challenger
                var pathIndices = new List<int>();
                foreach (var challenger in challengerResult.PlacedCells)
                {
                    int nearestIdx = FindNearestPathIndex(challenger, scenario.Path);
                    pathIndices.Add(nearestIdx);
                }

                // Sort by path order
                pathIndices.Sort();

                // Calculate gaps between consecutive challengers
                var gaps = new List<int>();
                for (int i = 1; i < pathIndices.Count; i++)
                {
                    gaps.Add(pathIndices[i] - pathIndices[i - 1]);
                }

                if (gaps.Count == 0)
                    return;

                // Calculate average gap
                float averageGap = (float)gaps.Average();

                if (averageGap <= 0)
                    return; // Degenerate case

                // Check that all gaps are within ±30% of average (with tolerance of 3 cells)
                float minAllowed = averageGap * 0.7f;
                float maxAllowed = averageGap * 1.3f;

                foreach (var gap in gaps)
                {
                    Assert.GreaterOrEqual(gap, minAllowed - 3,
                        $"Gap {gap} is below min allowed ({minAllowed - 3}). Average gap: {averageGap}");
                    Assert.LessOrEqual(gap, maxAllowed + 3,
                        $"Gap {gap} is above max allowed ({maxAllowed + 3}). Average gap: {averageGap}");
                }
            });
        }

        // ===================================================================
        // Property 21: Encounter Placement Constraints
        // Feature: city-terrain-generation, Property 21
        // ===================================================================

        /// <summary>
        /// Property 21: Encounter Placement Constraints
        /// For any valid grid with encounters placed:
        /// - All encounters are on path cells (Street or Sidewalk type)
        /// - All encounters have ≥ 3 Manhattan distance from other encounters
        /// - All encounters have ≥ 5 Manhattan distance from landmarks
        /// **Validates: Requirements 12.1, 12.2, 12.5**
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property21_EncounterPlacementConstraints()
        {
            ForAllValidScenarios(scenario =>
            {
                if (scenario.Path.Count <= 10)
                    return;

                var rng = new System.Random(scenario.Seed);

                var encounterResult = EncounterPlacer.PlaceEncounters(
                    scenario.Grid, rng, scenario.EncounterCount, scenario.Path,
                    scenario.Landmarks.SchoolCell, scenario.Landmarks.HouseCell);

                // All encounters must be on Street or Sidewalk cells
                foreach (var cell in encounterResult.PlacedCells)
                {
                    Assert.IsTrue(cell.Type == CellType.Street || cell.Type == CellType.Sidewalk,
                        $"Encounter at ({cell.Row},{cell.Col}) is {cell.Type}, expected Street or Sidewalk");
                }

                // All encounters must have ≥ 3 Manhattan distance between each other
                for (int i = 0; i < encounterResult.PlacedCells.Count; i++)
                {
                    for (int j = i + 1; j < encounterResult.PlacedCells.Count; j++)
                    {
                        int dist = encounterResult.PlacedCells[i].ManhattanDistance(encounterResult.PlacedCells[j]);
                        Assert.GreaterOrEqual(dist, 3,
                            $"Encounters ({i},{j}) have distance {dist}, expected ≥ 3");
                    }
                }

                // All encounters must have ≥ 5 Manhattan distance from school and house
                foreach (var cell in encounterResult.PlacedCells)
                {
                    int schoolDist = cell.ManhattanDistance(scenario.Landmarks.SchoolCell);
                    int houseDist = cell.ManhattanDistance(scenario.Landmarks.HouseCell);
                    Assert.GreaterOrEqual(schoolDist, 5,
                        $"Encounter at ({cell.Row},{cell.Col}) is {schoolDist} from school, expected ≥ 5");
                    Assert.GreaterOrEqual(houseDist, 5,
                        $"Encounter at ({cell.Row},{cell.Col}) is {houseDist} from house, expected ≥ 5");
                }
            });
        }

        // ===================================================================
        // Property 22: Encounter Invisibility (Grid-level validation)
        // Feature: city-terrain-generation, Property 22
        // ===================================================================

        /// <summary>
        /// Property 22: Encounter Invisibility
        /// For any valid encounter placement:
        /// - Encounter cells are marked as occupied with "encounter" tag
        /// - Cells retain their Street/Sidewalk type (invisible placement)
        /// **Validates: Requirements 12.3, 12.6**
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property22_EncounterInvisibility()
        {
            ForAllValidScenarios(scenario =>
            {
                if (scenario.Path.Count <= 10)
                    return;

                var rng = new System.Random(scenario.Seed);

                var encounterResult = EncounterPlacer.PlaceEncounters(
                    scenario.Grid, rng, scenario.EncounterCount, scenario.Path,
                    scenario.Landmarks.SchoolCell, scenario.Landmarks.HouseCell);

                // All encounter cells must be marked as occupied with "encounter" tag
                foreach (var cell in encounterResult.PlacedCells)
                {
                    var gridCell = scenario.Grid.GetCell(cell.Row, cell.Col);
                    Assert.IsTrue(gridCell.IsOccupied,
                        $"Encounter cell ({cell.Row},{cell.Col}) should be marked occupied");
                    Assert.AreEqual("encounter", gridCell.OccupantTag,
                        $"Encounter cell ({cell.Row},{cell.Col}) OccupantTag should be 'encounter'");
                }

                // Cell types must remain unchanged (invisible placement)
                foreach (var cell in encounterResult.PlacedCells)
                {
                    var gridCell = scenario.Grid.GetCell(cell.Row, cell.Col);
                    Assert.IsTrue(gridCell.Type == CellType.Street || gridCell.Type == CellType.Sidewalk,
                        $"Encounter cell ({cell.Row},{cell.Col}) type is {gridCell.Type}, expected Street or Sidewalk");
                }

                // ActualCount matches placed cells count
                Assert.AreEqual(encounterResult.ActualCount, encounterResult.PlacedCells.Count);
            });
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

        /// <summary>
        /// Finds the index in the path list of the cell nearest to the given cell.
        /// </summary>
        private static int FindNearestPathIndex(CellData target, List<CellData> path)
        {
            int nearestIdx = 0;
            int minDist = int.MaxValue;

            for (int i = 0; i < path.Count; i++)
            {
                int dist = target.ManhattanDistance(path[i]);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestIdx = i;
                }
            }

            return nearestIdx;
        }
    }

    /// <summary>
    /// Test scenario containing all data needed for NPC and encounter property tests.
    /// </summary>
    public class NPCTestScenario
    {
        public Grid_Layout Grid;
        public int StreetInterval;
        public int Seed;
        public int VendorCount;
        public int ChallengerCount;
        public int EncounterCount;
        public LandmarkPlacer.LandmarkResult Landmarks;
        public List<CellData> Path;

        public override string ToString()
        {
            return $"NPCTestScenario(Width={Grid.Width}, Depth={Grid.Depth}, " +
                   $"Interval={StreetInterval}, Seed={Seed}, " +
                   $"Vendors={VendorCount}, Challengers={ChallengerCount}, " +
                   $"Encounters={EncounterCount}, PathLen={Path.Count})";
        }
    }
}
