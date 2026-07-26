using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Flipit.CityTerrain.Tests
{
    /// <summary>
    /// Lightweight property-based testing utility with seed-controlled random generation.
    /// Provides reproducible pseudo-random inputs for property tests using System.Random.
    /// On failure, logs the seed so tests can be reproduced deterministically.
    /// </summary>
    public static class PropertyTestUtility
    {
        /// <summary>
        /// Default minimum number of iterations per property test.
        /// </summary>
        public const int DefaultIterations = 100;

        /// <summary>
        /// Runs a property test with the specified number of iterations.
        /// Each iteration uses a unique seed derived from the base seed + iteration index.
        /// On failure, the seed is reported for reproducibility.
        /// </summary>
        public static void ForAll(Action<Random> property, int iterations = DefaultIterations, int? baseSeed = null)
        {
            int seed = baseSeed ?? Environment.TickCount;
            TestContext.WriteLine($"[PBT] Base seed: {seed}, Iterations: {iterations}");

            for (int i = 0; i < iterations; i++)
            {
                int iterationSeed = seed + i;
                var random = new Random(iterationSeed);

                try
                {
                    property(random);
                }
                catch (Exception ex)
                {
                    throw new AssertionException(
                        $"Property failed on iteration {i} with seed {iterationSeed}.\n" +
                        $"Base seed: {seed}\n" +
                        $"To reproduce, run with baseSeed: {iterationSeed}\n" +
                        $"Original exception: {ex.Message}",
                        ex);
                }
            }
        }
    }

    /// <summary>
    /// Extension methods for System.Random to generate common test data types.
    /// </summary>
    public static class RandomExtensions
    {
        public static float NextFloat(this Random random, float min, float max)
        {
            return (float)(random.NextDouble() * (max - min) + min);
        }

        public static float NextFloat(this Random random)
        {
            return (float)random.NextDouble();
        }

        public static bool NextBool(this Random random)
        {
            return random.Next(2) == 1;
        }

        public static T PickFrom<T>(this Random random, T[] items)
        {
            return items[random.Next(items.Length)];
        }

        public static T PickFrom<T>(this Random random, IList<T> items)
        {
            return items[random.Next(items.Count)];
        }
    }
}
