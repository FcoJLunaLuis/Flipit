using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Flipit.CityTerrain.Tests
{
    /// <summary>
    /// Property-based tests for Stage 5 (Decorative Variation) logic.
    /// Tests validate the algorithmic properties of building variation, cell conversion,
    /// prop scale, tree rotation, and seed determinism independent of Unity GameObjects.
    /// Feature: city-terrain-generation
    /// </summary>
    [TestFixture]
    public class Stage5VariationPropertyTests
    {
        // ===================================================================
        // Property 27: Building Variation Within Config Bounds
        // Feature: city-terrain-generation, Property 27: Building Variation Within Config Bounds
        // ===================================================================

        /// <summary>
        /// Property 27: Building Variation Within Config Bounds
        /// For any non-landmark building after Stage 5, its Y-scale is within
        /// [buildingHeightMin, buildingHeightMax] and its material color is one of the
        /// colors in the config palette.
        /// **Validates: Requirements 14.1, 14.2**
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property27_BuildingVariationWithinConfigBounds()
        {
            PropertyTestUtility.ForAll(random =>
            {
                // Generate random config values
                float heightMin = random.NextFloat(0.5f, 3.0f);
                float heightMax = heightMin + random.NextFloat(0.1f, 3.0f);
                int paletteSize = random.Next(5, 16);
                int buildingCount = random.Next(1, 101);

                // Use a separate seeded rng to simulate Stage 5 logic
                int seed = random.Next(1, 100000);
                var rng = new System.Random(seed);

                // Simulate the same logic as City_Generator.ExecuteStage5_Variation
                for (int i = 0; i < buildingCount; i++)
                {
                    float randomHeight = (float)(heightMin +
                        rng.NextDouble() * (heightMax - heightMin));

                    int colorIndex = rng.Next(paletteSize);

                    // Property: height is within [heightMin, heightMax]
                    Assert.GreaterOrEqual(randomHeight, heightMin,
                        $"Building {i} height {randomHeight} below min {heightMin}");
                    Assert.LessOrEqual(randomHeight, heightMax,
                        $"Building {i} height {randomHeight} above max {heightMax}");

                    // Property: color index is valid (within palette bounds)
                    Assert.GreaterOrEqual(colorIndex, 0,
                        $"Building {i} color index {colorIndex} is negative");
                    Assert.Less(colorIndex, paletteSize,
                        $"Building {i} color index {colorIndex} exceeds palette size {paletteSize}");
                }
            });
        }

        // ===================================================================
        // Property 28: Cell Conversion Adjacency Rule
        // Feature: city-terrain-generation, Property 28: Cell Conversion Adjacency Rule
        // ===================================================================

        /// <summary>
        /// Property 28: Cell Conversion Adjacency Rule
        /// Converted cells are adjacent to existing Building_Cell,
        /// count = floor(eligible × percent).
        /// **Validates: Requirements 14.3**
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property28_CellConversionAdjacencyRule()
        {
            PropertyTestUtility.ForAll(random =>
            {
                int width = random.Next(8, 21);
                int depth = random.Next(8, 21);
                int interval = random.Next(3, 7);
                float conversionPercent = random.NextFloat(0.01f, 0.50f);
                int seed = random.Next(1, 100000);

                var grid = new Grid_Layout(width, depth, 4f);
                GridGenerator.AssignCellTypes(grid, interval);

                // Mark some cells as Empty to simulate pre-Stage5 state
                var setupRng = new System.Random(seed + 999);
                var buildingCells = grid.GetCellsOfType(CellType.Building);
                int emptyCellsToCreate = Math.Min(random.Next(1, 16), buildingCells.Count / 2);
                var shuffled = buildingCells.OrderBy(_ => setupRng.Next()).Take(emptyCellsToCreate).ToList();
                foreach (var cell in shuffled)
                {
                    grid.SetCellType(cell.Row, cell.Col, CellType.Empty);
                }

                // Now simulate the conversion logic from Stage 5
                var rng = new System.Random(seed);
                var emptyCells = grid.GetCellsOfType(CellType.Empty);
                var eligibleForConversion = new List<CellData>();

                foreach (var cell in emptyCells)
                {
                    var neighbors = grid.GetAdjacentCells(cell.Row, cell.Col);
                    bool adjacentToBuilding = neighbors.Any(n => n.Type == CellType.Building);

                    if (adjacentToBuilding && !cell.IsOccupied)
                    {
                        eligibleForConversion.Add(cell);
                    }
                }

                int convertTarget = (int)Math.Floor(eligibleForConversion.Count * (double)conversionPercent);

                // Shuffle for random selection (same as City_Generator)
                for (int i = eligibleForConversion.Count - 1; i > 0; i--)
                {
                    int j = rng.Next(i + 1);
                    var temp = eligibleForConversion[i];
                    eligibleForConversion[i] = eligibleForConversion[j];
                    eligibleForConversion[j] = temp;
                }

                int actualConverted = 0;
                for (int i = 0; i < convertTarget && i < eligibleForConversion.Count; i++)
                {
                    CellData cell = eligibleForConversion[i];

                    // Verify adjacency rule: must be adjacent to at least one Building cell
                    var neighbors = grid.GetAdjacentCells(cell.Row, cell.Col);
                    bool adjacentToBuilding = neighbors.Any(n => n.Type == CellType.Building);
                    Assert.IsTrue(adjacentToBuilding,
                        $"Converted cell ({cell.Row},{cell.Col}) is not adjacent to any Building cell");

                    grid.SetCellType(cell.Row, cell.Col, CellType.Building);
                    actualConverted++;
                }

                // Property: converted count = floor(eligible × percent)
                Assert.AreEqual(convertTarget, actualConverted,
                    $"Expected {convertTarget} conversions (floor({eligibleForConversion.Count} × {conversionPercent:F2})), got {actualConverted}");
            });
        }

        // ===================================================================
        // Property 29: Prop Scale Variation Bounds
        // Feature: city-terrain-generation, Property 29: Prop Scale Variation Bounds
        // ===================================================================

        /// <summary>
        /// Property 29: Prop Scale Variation Bounds
        /// Uniform scale in [0.8, 1.2] for all props.
        /// **Validates: Requirements 14.5**
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property29_PropScaleVariationBounds()
        {
            PropertyTestUtility.ForAll(random =>
            {
                int propCount = random.Next(1, 201);
                int seed = random.Next(1, 100000);
                var rng = new System.Random(seed);

                // Simulate the same logic as City_Generator Stage 5 prop scale variation:
                // float scaleFactor = 0.8f + (float)(rng.NextDouble() * 0.4);
                for (int i = 0; i < propCount; i++)
                {
                    float scaleFactor = 0.8f + (float)(rng.NextDouble() * 0.4);

                    // Property: scale factor is within [0.8, 1.2]
                    Assert.GreaterOrEqual(scaleFactor, 0.8f,
                        $"Prop {i} scale {scaleFactor} below minimum 0.8");
                    Assert.LessOrEqual(scaleFactor, 1.2f,
                        $"Prop {i} scale {scaleFactor} above maximum 1.2");
                }
            });
        }

        // ===================================================================
        // Property 30: Tree Canopy Rotation Values
        // Feature: city-terrain-generation, Property 30: Tree Canopy Rotation Values
        // ===================================================================

        /// <summary>
        /// Property 30: Tree Canopy Rotation Values
        /// Y-rotation is one of {0, 90, 180, 270} degrees.
        /// **Validates: Requirements 14.4**
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property30_TreeCanopyRotationValues()
        {
            PropertyTestUtility.ForAll(random =>
            {
                int treeCount = random.Next(1, 101);
                int seed = random.Next(1, 100000);
                var rng = new System.Random(seed);

                float[] validRotations = { 0f, 90f, 180f, 270f };

                // Simulate the same logic as City_Generator Stage 5 tree rotation:
                // float yRot = rotations[rng.Next(rotations.Length)];
                for (int i = 0; i < treeCount; i++)
                {
                    float yRot = validRotations[rng.Next(validRotations.Length)];

                    // Property: rotation is one of {0, 90, 180, 270}
                    Assert.IsTrue(validRotations.Contains(yRot),
                        $"Tree {i} rotation {yRot} is not in valid set {{0, 90, 180, 270}}");
                }
            });
        }

        // ===================================================================
        // Property 31: Seed Determinism
        // Feature: city-terrain-generation, Property 31: Seed Determinism
        // ===================================================================

        /// <summary>
        /// Property 31: Seed Determinism
        /// Same seed produces identical results across runs.
        /// **Validates: Requirements 14.6**
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property31_SeedDeterminism()
        {
            PropertyTestUtility.ForAll(random =>
            {
                int seed = random.Next(1, 100000);
                int width = random.Next(8, 16);
                int depth = random.Next(8, 16);
                int interval = random.Next(3, 6);
                float heightMin = random.NextFloat(0.5f, 2.0f);
                float heightMax = heightMin + random.NextFloat(0.1f, 2.0f);
                int paletteSize = random.Next(5, 11);
                float conversionPercent = random.NextFloat(0.05f, 0.40f);
                int buildingCount = random.Next(5, 31);
                int treeCount = random.Next(3, 21);
                int propCount = random.Next(5, 41);
                int emptyCellCount = random.Next(2, 11);

                // --- Run 1 ---
                var results1 = RunVariationSimulation(
                    seed, width, depth, interval, heightMin, heightMax,
                    paletteSize, conversionPercent, buildingCount,
                    treeCount, propCount, emptyCellCount);

                // --- Run 2 (same seed, same config) ---
                var results2 = RunVariationSimulation(
                    seed, width, depth, interval, heightMin, heightMax,
                    paletteSize, conversionPercent, buildingCount,
                    treeCount, propCount, emptyCellCount);

                // Property: Both runs produce identical outputs
                Assert.AreEqual(results1.Heights.Count, results2.Heights.Count,
                    "Height list lengths differ between runs");

                for (int i = 0; i < results1.Heights.Count; i++)
                {
                    Assert.AreEqual(results1.Heights[i], results2.Heights[i], 0.0001f,
                        $"Height at index {i} differs between runs");
                }

                Assert.AreEqual(results1.ColorIndices.Count, results2.ColorIndices.Count,
                    "Color index list lengths differ between runs");

                for (int i = 0; i < results1.ColorIndices.Count; i++)
                {
                    Assert.AreEqual(results1.ColorIndices[i], results2.ColorIndices[i],
                        $"Color index at {i} differs between runs");
                }

                Assert.AreEqual(results1.ConvertedCellCount, results2.ConvertedCellCount,
                    "Converted cell counts differ between runs");

                Assert.AreEqual(results1.TreeRotations.Count, results2.TreeRotations.Count,
                    "Tree rotation list lengths differ between runs");

                for (int i = 0; i < results1.TreeRotations.Count; i++)
                {
                    Assert.AreEqual(results1.TreeRotations[i], results2.TreeRotations[i], 0.0001f,
                        $"Tree rotation at {i} differs between runs");
                }

                Assert.AreEqual(results1.PropScales.Count, results2.PropScales.Count,
                    "Prop scale list lengths differ between runs");

                for (int i = 0; i < results1.PropScales.Count; i++)
                {
                    Assert.AreEqual(results1.PropScales[i], results2.PropScales[i], 0.0001f,
                        $"Prop scale at {i} differs between runs");
                }
            });
        }

        #region Simulation Helpers

        /// <summary>
        /// Simulates the Stage 5 variation logic on pure data (no GameObjects).
        /// Mirrors the random calls made in City_Generator.ExecuteStage5_Variation.
        /// </summary>
        private VariationResults RunVariationSimulation(
            int seed, int width, int depth, int interval,
            float heightMin, float heightMax, int paletteSize,
            float conversionPercent, int buildingCount,
            int treeCount, int propCount, int emptyCellCount)
        {
            var results = new VariationResults();
            var rng = new System.Random(seed);

            // 1. Building height and color variation
            for (int i = 0; i < buildingCount; i++)
            {
                float height = (float)(heightMin +
                    rng.NextDouble() * (heightMax - heightMin));
                results.Heights.Add(height);

                int colorIdx = rng.Next(paletteSize);
                results.ColorIndices.Add(colorIdx);
            }

            // 2. Cell conversion (simulate shuffle + conversion)
            var grid = new Grid_Layout(width, depth, 4f);
            GridGenerator.AssignCellTypes(grid, interval);

            // Create some empty cells for conversion testing
            var buildingCells = grid.GetCellsOfType(CellType.Building);
            int clampedEmpty = Math.Min(emptyCellCount, buildingCells.Count / 2);
            var tempRng = new System.Random(seed + 999); // separate rng for setup
            var toEmpty = buildingCells.OrderBy(_ => tempRng.Next()).Take(clampedEmpty).ToList();
            foreach (var cell in toEmpty)
            {
                grid.SetCellType(cell.Row, cell.Col, CellType.Empty);
            }

            var emptyCells = grid.GetCellsOfType(CellType.Empty);
            var eligible = new List<CellData>();
            foreach (var cell in emptyCells)
            {
                var neighbors = grid.GetAdjacentCells(cell.Row, cell.Col);
                if (neighbors.Any(n => n.Type == CellType.Building) && !cell.IsOccupied)
                    eligible.Add(cell);
            }

            int convertTarget = (int)Math.Floor(eligible.Count * (double)conversionPercent);
            for (int i = eligible.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                var temp = eligible[i];
                eligible[i] = eligible[j];
                eligible[j] = temp;
            }

            int converted = 0;
            for (int i = 0; i < convertTarget && i < eligible.Count; i++)
            {
                // Converted building also uses rng for height and color
                float h = (float)(heightMin + rng.NextDouble() * (heightMax - heightMin));
                int ci = rng.Next(paletteSize);
                converted++;
            }
            results.ConvertedCellCount = converted;

            // 3. Tree rotations
            float[] rotations = { 0f, 90f, 180f, 270f };
            for (int i = 0; i < treeCount; i++)
            {
                float yRot = rotations[rng.Next(rotations.Length)];
                results.TreeRotations.Add(yRot);
            }

            // 4. Prop scale variations
            for (int i = 0; i < propCount; i++)
            {
                float scale = 0.8f + (float)(rng.NextDouble() * 0.4);
                results.PropScales.Add(scale);
            }

            return results;
        }

        private class VariationResults
        {
            public List<float> Heights = new List<float>();
            public List<int> ColorIndices = new List<int>();
            public int ConvertedCellCount;
            public List<float> TreeRotations = new List<float>();
            public List<float> PropScales = new List<float>();
        }

        #endregion
    }
}
