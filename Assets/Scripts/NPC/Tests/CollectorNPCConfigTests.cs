using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Flipit.Core;

namespace Flipit.NPC.Tests
{
    [TestFixture]
    public class CollectorNPCConfigTests
    {
        private CollectorNPCConfig config;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<CollectorNPCConfig>();
            SetupDefaultConfig();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(config);
        }

        [Test]
        public void GetSellPriceRange_Common_ReturnsCorrectRange()
        {
            var range = config.GetSellPriceRange(ChipRarity.Common);

            Assert.IsNotNull(range);
            Assert.AreEqual(ChipRarity.Common, range.Rarity);
            Assert.AreEqual(5, range.MinPrice);
            Assert.AreEqual(15, range.MaxPrice);
        }

        [Test]
        public void GetSellPriceRange_Rare_ReturnsCorrectRange()
        {
            var range = config.GetSellPriceRange(ChipRarity.Rare);

            Assert.IsNotNull(range);
            Assert.AreEqual(ChipRarity.Rare, range.Rarity);
            Assert.AreEqual(25, range.MinPrice);
            Assert.AreEqual(75, range.MaxPrice);
        }

        [Test]
        public void GetSellPriceRange_UltraRare_ReturnsCorrectRange()
        {
            var range = config.GetSellPriceRange(ChipRarity.UltraRare);

            Assert.IsNotNull(range);
            Assert.AreEqual(ChipRarity.UltraRare, range.Rarity);
            Assert.AreEqual(100, range.MinPrice);
            Assert.AreEqual(300, range.MaxPrice);
        }

        [Test]
        public void TradeOfferCount_DefaultValue_IsFive()
        {
            Assert.AreEqual(5, config.TradeOfferCount);
        }

        [Test]
        public void MaxTradeRequirementTypes_DefaultValue_IsThree()
        {
            Assert.AreEqual(3, config.MaxTradeRequirementTypes);
        }

        [Test]
        public void MinTradeRequirementTypes_DefaultValue_IsOne()
        {
            Assert.AreEqual(1, config.MinTradeRequirementTypes);
        }

        [Test]
        public void GetSellPriceRange_NullArray_ReturnsNull()
        {
            var emptyConfig = ScriptableObject.CreateInstance<CollectorNPCConfig>();

            var range = emptyConfig.GetSellPriceRange(ChipRarity.Common);

            Assert.IsNull(range);

            Object.DestroyImmediate(emptyConfig);
        }

        [Test]
        public void SellPriceRange_MinPriceAlwaysLessOrEqualMaxPrice()
        {
            var ranges = config.SellPriceRanges;

            foreach (var range in ranges)
            {
                Assert.LessOrEqual(range.MinPrice, range.MaxPrice,
                    $"MinPrice should be <= MaxPrice for {range.Rarity}");
            }
        }

        private void SetupDefaultConfig()
        {
            var so = new SerializedObject(config);

            var sellPrices = so.FindProperty("sellPriceRanges");
            sellPrices.arraySize = 3;

            // Common: 5-15
            var common = sellPrices.GetArrayElementAtIndex(0);
            common.FindPropertyRelative("rarity").enumValueIndex = (int)ChipRarity.Common;
            common.FindPropertyRelative("minPrice").intValue = 5;
            common.FindPropertyRelative("maxPrice").intValue = 15;

            // Rare: 25-75
            var rare = sellPrices.GetArrayElementAtIndex(1);
            rare.FindPropertyRelative("rarity").enumValueIndex = (int)ChipRarity.Rare;
            rare.FindPropertyRelative("minPrice").intValue = 25;
            rare.FindPropertyRelative("maxPrice").intValue = 75;

            // UltraRare: 100-300
            var ultraRare = sellPrices.GetArrayElementAtIndex(2);
            ultraRare.FindPropertyRelative("rarity").enumValueIndex = (int)ChipRarity.UltraRare;
            ultraRare.FindPropertyRelative("minPrice").intValue = 100;
            ultraRare.FindPropertyRelative("maxPrice").intValue = 300;

            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
