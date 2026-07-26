using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Flipit.Dialogue.Tests
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
        /// <param name="property">The property assertion to test. Receives a seeded Random instance.</param>
        /// <param name="iterations">Number of iterations to run. Defaults to 100.</param>
        /// <param name="baseSeed">Base seed for reproducibility. If null, uses a time-based seed and logs it.</param>
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

        /// <summary>
        /// Runs a property test that also receives the iteration index.
        /// Useful for exhaustive enumeration patterns.
        /// </summary>
        /// <param name="property">The property assertion. Receives iteration index and a seeded Random.</param>
        /// <param name="iterations">Number of iterations to run.</param>
        /// <param name="baseSeed">Base seed for reproducibility.</param>
        public static void ForAll(Action<int, Random> property, int iterations = DefaultIterations, int? baseSeed = null)
        {
            int seed = baseSeed ?? Environment.TickCount;
            TestContext.WriteLine($"[PBT] Base seed: {seed}, Iterations: {iterations}");

            for (int i = 0; i < iterations; i++)
            {
                int iterationSeed = seed + i;
                var random = new Random(iterationSeed);

                try
                {
                    property(i, random);
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
    /// Used by property tests to create random inputs within specified domains.
    /// </summary>
    public static class RandomExtensions
    {
        /// <summary>
        /// Generates a random float within the specified range [min, max).
        /// </summary>
        public static float NextFloat(this Random random, float min, float max)
        {
            return (float)(random.NextDouble() * (max - min) + min);
        }

        /// <summary>
        /// Generates a random float within [0, max).
        /// </summary>
        public static float NextFloat(this Random random, float max)
        {
            return random.NextFloat(0f, max);
        }

        /// <summary>
        /// Generates a random float within [0, 1).
        /// </summary>
        public static float NextFloat(this Random random)
        {
            return (float)random.NextDouble();
        }

        /// <summary>
        /// Generates a random boolean.
        /// </summary>
        public static bool NextBool(this Random random)
        {
            return random.Next(2) == 1;
        }

        /// <summary>
        /// Generates a random string of the specified length using alphanumeric characters.
        /// </summary>
        public static string NextString(this Random random, int minLength = 1, int maxLength = 50)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";
            int length = random.Next(minLength, maxLength + 1);
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }
            return new string(result);
        }

        /// <summary>
        /// Picks a random element from an array.
        /// </summary>
        public static T PickFrom<T>(this Random random, T[] items)
        {
            return items[random.Next(items.Length)];
        }

        /// <summary>
        /// Picks a random element from a list.
        /// </summary>
        public static T PickFrom<T>(this Random random, IList<T> items)
        {
            return items[random.Next(items.Count)];
        }

        /// <summary>
        /// Generates a random 2D position within the specified bounds.
        /// </summary>
        public static UnityEngine.Vector2 NextVector2(this Random random, float min = -100f, float max = 100f)
        {
            return new UnityEngine.Vector2(
                random.NextFloat(min, max),
                random.NextFloat(min, max)
            );
        }

        /// <summary>
        /// Generates a list of random items using the provided generator function.
        /// </summary>
        public static List<T> NextList<T>(this Random random, Func<Random, T> generator, int minCount = 0, int maxCount = 10)
        {
            int count = random.Next(minCount, maxCount + 1);
            var list = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(generator(random));
            }
            return list;
        }

        /// <summary>
        /// Generates a random enum value from the specified enum type.
        /// </summary>
        public static T NextEnum<T>(this Random random) where T : Enum
        {
            var values = (T[])Enum.GetValues(typeof(T));
            return values[random.Next(values.Length)];
        }
    }
}
