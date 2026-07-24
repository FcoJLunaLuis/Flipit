using NUnit.Framework;
using UnityEngine;

namespace Flipit.Core.Tests
{
    [TestFixture]
    public class PityTrackerTests
    {
        private PityTracker tracker;
        private PityConfig config;

        [SetUp]
        public void SetUp()
        {
            tracker = new PityTracker();
            config = ScriptableObject.CreateInstance<PityConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(config);
        }

        [Test]
        public void RegisterBagOpened_IncrementsBagCounters()
        {
            tracker.RegisterBagOpened();

            Assert.AreEqual(1, tracker.TotalBagsOpened);
            Assert.AreEqual(1, tracker.BagsSinceLastRare);
            Assert.AreEqual(1, tracker.BagsSinceLastUltraRare);
        }

        [Test]
        public void RegisterResult_Common_IncrementsCommonCount()
        {
            tracker.RegisterResult(ChipRarity.Common);

            Assert.AreEqual(1, tracker.TotalCommons);
            Assert.AreEqual(0, tracker.TotalRares);
            Assert.AreEqual(0, tracker.TotalUltraRares);
        }

        [Test]
        public void RegisterResult_Rare_ResetsRareCounter()
        {
            tracker.RegisterBagOpened();
            tracker.RegisterBagOpened();
            tracker.RegisterBagOpened();

            tracker.RegisterResult(ChipRarity.Rare);

            Assert.AreEqual(0, tracker.BagsSinceLastRare);
            Assert.AreEqual(1, tracker.TotalRares);
        }

        [Test]
        public void RegisterResult_UltraRare_ResetsBothCounters()
        {
            tracker.RegisterBagOpened();
            tracker.RegisterBagOpened();

            tracker.RegisterResult(ChipRarity.UltraRare);

            Assert.AreEqual(0, tracker.BagsSinceLastRare);
            Assert.AreEqual(0, tracker.BagsSinceLastUltraRare);
            Assert.AreEqual(1, tracker.TotalUltraRares);
        }

        [Test]
        public void GetAdjustedProbability_Common_ReturnsBase()
        {
            float result = tracker.GetAdjustedProbability(ChipRarity.Common, 0.9f, config);

            Assert.AreEqual(0.9f, result, 0.001f);
        }

        [Test]
        public void GetAdjustedProbability_NoPity_ReturnsBaseProbability()
        {
            float result = tracker.GetAdjustedProbability(ChipRarity.Rare, 0.4f, config);

            // With 0 bags since last rare, pity curve at 0 progress should give 0 bonus
            Assert.AreEqual(0.4f, result, 0.01f);
        }

        [Test]
        public void GetAdjustedProbability_AtGuaranteeThreshold_ReturnsOne()
        {
            // Default bagsUntilGuaranteedRare is 10
            for (int i = 0; i < 10; i++)
                tracker.RegisterBagOpened();

            float result = tracker.GetAdjustedProbability(ChipRarity.Rare, 0.4f, config);

            Assert.AreEqual(1f, result, 0.001f);
        }

        [Test]
        public void GetAdjustedProbability_HalfwayToPity_IncreasesProbability()
        {
            // Default bagsUntilGuaranteedRare is 10, so 5 bags = 50% progress
            for (int i = 0; i < 5; i++)
                tracker.RegisterBagOpened();

            float result = tracker.GetAdjustedProbability(ChipRarity.Rare, 0.4f, config);

            // Should be higher than base but less than 1
            Assert.Greater(result, 0.4f);
            Assert.Less(result, 1f);
        }

        [Test]
        public void GetAdjustedProbability_UltraRare_AtGuarantee_ReturnsOne()
        {
            // Default bagsUntilGuaranteedUltraRare is 50
            for (int i = 0; i < 50; i++)
                tracker.RegisterBagOpened();

            float result = tracker.GetAdjustedProbability(ChipRarity.UltraRare, 0.01f, config);

            Assert.AreEqual(1f, result, 0.001f);
        }

        [Test]
        public void GetAdjustedProbability_NeverExceedsOne()
        {
            for (int i = 0; i < 100; i++)
                tracker.RegisterBagOpened();

            float result = tracker.GetAdjustedProbability(ChipRarity.Rare, 0.9f, config);

            Assert.LessOrEqual(result, 1f);
        }

        [Test]
        public void GetAdjustedProbability_NullConfig_ReturnsBase()
        {
            float result = tracker.GetAdjustedProbability(ChipRarity.Rare, 0.5f, null);

            Assert.AreEqual(0.5f, result, 0.001f);
        }

        [Test]
        public void FrustrationBonus_AppliesWhenTooManyCommons()
        {
            // Register many commons to trigger frustration (threshold default 0.85)
            for (int i = 0; i < 20; i++)
            {
                tracker.RegisterBagOpened();
                tracker.RegisterResult(ChipRarity.Common);
            }

            // Common ratio is 100% > 85% threshold, so frustration bonus should apply
            float result = tracker.GetAdjustedProbability(ChipRarity.Rare, 0.4f, config);

            Assert.Greater(result, 0.4f);
        }

        [Test]
        public void FrustrationBonus_DoesNotApplyWhenBalanced()
        {
            // Mix of results keeping common ratio below threshold
            for (int i = 0; i < 5; i++)
            {
                tracker.RegisterResult(ChipRarity.Common);
                tracker.RegisterResult(ChipRarity.Rare);
            }

            // Common ratio is 50%, well below 85% threshold
            // With 0 bagsSinceLastRare (just got one), pity should be minimal
            float result = tracker.GetAdjustedProbability(ChipRarity.Rare, 0.4f, config);

            // Should be very close to base (only minimal pity curve bonus)
            Assert.AreEqual(0.4f, result, 0.05f);
        }

        [Test]
        public void Reset_ClearsAllCounters()
        {
            tracker.RegisterBagOpened();
            tracker.RegisterResult(ChipRarity.Common);
            tracker.RegisterResult(ChipRarity.Rare);
            tracker.RegisterResult(ChipRarity.UltraRare);

            tracker.Reset();

            Assert.AreEqual(0, tracker.BagsSinceLastRare);
            Assert.AreEqual(0, tracker.BagsSinceLastUltraRare);
            Assert.AreEqual(0, tracker.TotalBagsOpened);
            Assert.AreEqual(0, tracker.TotalCommons);
            Assert.AreEqual(0, tracker.TotalRares);
            Assert.AreEqual(0, tracker.TotalUltraRares);
        }
    }
}