using NUnit.Framework;
using UnityEngine;
using Random = System.Random;

namespace Flipit.Core.Tests
{
    [TestFixture]
    public class BagGeneratorTests
    {
        private BagGenerator generator;
        private PityTracker tracker;
        private PityConfig pityConfig;
        private BagConfig greenBag;
        private BagConfig redBag;
        private BagConfig whiteBag;

        [SetUp]
        public void SetUp()
        {
            // Use fixed seed for deterministic tests
            generator = new BagGenerator(new Random(42));
            tracker = new PityTracker();
            pityConfig = ScriptableObject.CreateInstance<PityConfig>();

            greenBag = ScriptableObject.CreateInstance<BagConfig>();
            redBag = ScriptableObject.CreateInstance<BagConfig>();
            whiteBag = ScriptableObject.CreateInstance<BagConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(pityConfig);
            UnityEngine.Object.DestroyImmediate(greenBag);
            UnityEngine.Object.DestroyImmediate(redBag);
            UnityEngine.Object.DestroyImmediate(whiteBag);
        }

        [Test]
        public void GenerateBagContents_ReturnsNonNullArray()
        {
            SetupBagConfig(greenBag, BagTier.Green, 1, 2);

            var results = generator.GenerateBagContents(greenBag, tracker, pityConfig);

            Assert.IsNotNull(results);
            Assert.Greater(results.Length, 0);
        }

        [Test]
        public void GenerateBagContents_GreenBag_ChipCountWithinRange()
        {
            SetupBagConfig(greenBag, BagTier.Green, 1, 2);

            // Generate many bags to test range
            for (int i = 0; i < 50; i++)
            {
                var gen = new BagGenerator(new Random(i));
                var results = gen.GenerateBagContents(greenBag, new PityTracker(), pityConfig);

                Assert.GreaterOrEqual(results.Length, 1);
                Assert.LessOrEqual(results.Length, 2);
            }
        }

        [Test]
        public void GenerateBagContents_RedBag_ChipCountWithinRange()
        {
            SetupBagConfig(redBag, BagTier.Red, 2, 3);

            for (int i = 0; i < 50; i++)
            {
                var gen = new BagGenerator(new Random(i));
                var results = gen.GenerateBagContents(redBag, new PityTracker(), pityConfig);

                Assert.GreaterOrEqual(results.Length, 2);
                Assert.LessOrEqual(results.Length, 3);
            }
        }

        [Test]
        public void GenerateBagContents_WhiteBag_ChipCountWithinRange()
        {
            SetupBagConfig(whiteBag, BagTier.White, 3, 5);

            for (int i = 0; i < 50; i++)
            {
                var gen = new BagGenerator(new Random(i));
                var results = gen.GenerateBagContents(whiteBag, new PityTracker(), pityConfig);

                Assert.GreaterOrEqual(results.Length, 3);
                Assert.LessOrEqual(results.Length, 5);
            }
        }

        [Test]
        public void GenerateBagContents_WhiteBag_SometimesGivesMaxChips()
        {
            SetupBagConfig(whiteBag, BagTier.White, 3, 5);

            int maxCount = 0;
            for (int i = 0; i < 100; i++)
            {
                var gen = new BagGenerator(new Random(i));
                var results = gen.GenerateBagContents(whiteBag, new PityTracker(), pityConfig);
                if (results.Length == 5) maxCount++;
            }

            // With ~20% chance, should get max at least a few times in 100 tries
            Assert.Greater(maxCount, 5, "White bag should give max chips approximately 20% of the time");
        }

        [Test]
        public void GenerateBagContents_AllChipsHaveValidRarity()
        {
            SetupBagConfig(greenBag, BagTier.Green, 1, 2);

            var results = generator.GenerateBagContents(greenBag, tracker, pityConfig);

            foreach (var chip in results)
            {
                Assert.IsTrue(
                    chip.Rarity == ChipRarity.Common ||
                    chip.Rarity == ChipRarity.Rare ||
                    chip.Rarity == ChipRarity.UltraRare);
            }
        }

        [Test]
        public void GenerateBagContents_RegistersBagOpened()
        {
            SetupBagConfig(greenBag, BagTier.Green, 1, 2);

            generator.GenerateBagContents(greenBag, tracker, pityConfig);

            Assert.AreEqual(1, tracker.TotalBagsOpened);
        }

        [Test]
        public void GenerateBagContents_RegistersChipResults()
        {
            SetupBagConfig(greenBag, BagTier.Green, 1, 2);

            var results = generator.GenerateBagContents(greenBag, tracker, pityConfig);

            int totalRegistered = tracker.TotalCommons + tracker.TotalRares + tracker.TotalUltraRares;
            Assert.AreEqual(results.Length, totalRegistered);
        }

        [Test]
        public void GenerateBagContents_HighBaseProbability_MostlyCommon()
        {
            SetupBagConfig(greenBag, BagTier.Green, 1, 1);

            int commonCount = 0;
            int total = 200;

            for (int i = 0; i < total; i++)
            {
                var gen = new BagGenerator(new Random(i));
                var localTracker = new PityTracker();
                var results = gen.GenerateBagContents(greenBag, localTracker, pityConfig);

                if (results[0].Rarity == ChipRarity.Common)
                    commonCount++;
            }

            // With default green bag probabilities (90% common fallback after rare/ultra checks),
            // most should be common
            float commonRatio = (float)commonCount / total;
            Assert.Greater(commonRatio, 0.4f, "Expected majority of results to be Common");
        }

        [Test]
        public void GenerateBagContents_PitySystem_EventuallyGivesRare()
        {
            SetupBagConfig(greenBag, BagTier.Green, 1, 1);

            // Open many bags without getting rare to trigger pity
            var persistentTracker = new PityTracker();
            bool gotRare = false;

            for (int i = 0; i < 100; i++)
            {
                var gen = new BagGenerator(new Random(i + 1000));
                var results = gen.GenerateBagContents(greenBag, persistentTracker, pityConfig);

                if (results[0].Rarity == ChipRarity.Rare || results[0].Rarity == ChipRarity.UltraRare)
                {
                    gotRare = true;
                    break;
                }
            }

            Assert.IsTrue(gotRare, "Pity system should guarantee a rare within guarantee threshold");
        }

        private void SetupBagConfig(BagConfig config, BagTier tier, int min, int max)
        {
            // Use serialized fields via SerializedObject to set values
            var so = new UnityEditor.SerializedObject(config);
            so.FindProperty("bagName").stringValue = tier.ToString() + " Bag";
            so.FindProperty("tier").enumValueIndex = (int)tier;
            so.FindProperty("minChips").intValue = min;
            so.FindProperty("maxChips").intValue = max;

            var rarityArray = so.FindProperty("rarityWeights");
            rarityArray.arraySize = 3;

            // Common
            var common = rarityArray.GetArrayElementAtIndex(0);
            common.FindPropertyRelative("rarity").enumValueIndex = (int)ChipRarity.Common;
            common.FindPropertyRelative("baseProbability").floatValue = 0.9f;
            common.FindPropertyRelative("pityBonusPerMiss").floatValue = 0f;
            common.FindPropertyRelative("maxProbability").floatValue = 1f;

            // Rare
            var rare = rarityArray.GetArrayElementAtIndex(1);
            rare.FindPropertyRelative("rarity").enumValueIndex = (int)ChipRarity.Rare;
            rare.FindPropertyRelative("baseProbability").floatValue = 0.4f;
            rare.FindPropertyRelative("pityBonusPerMiss").floatValue = 0.05f;
            rare.FindPropertyRelative("maxProbability").floatValue = 0.8f;

            // UltraRare
            var ultraRare = rarityArray.GetArrayElementAtIndex(2);
            ultraRare.FindPropertyRelative("rarity").enumValueIndex = (int)ChipRarity.UltraRare;
            ultraRare.FindPropertyRelative("baseProbability").floatValue = 0.01f;
            ultraRare.FindPropertyRelative("pityBonusPerMiss").floatValue = 0.02f;
            ultraRare.FindPropertyRelative("maxProbability").floatValue = 0.5f;

            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}