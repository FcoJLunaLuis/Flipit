using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Flipit.Core;
using Random = System.Random;

namespace Flipit.NPC.Tests
{
    [TestFixture]
    public class SellPriceCalculatorTests
    {
        private SellPriceCalculator calculator;
        private CollectorNPCConfig config;

        [SetUp]
        public void SetUp()
        {
            calculator = new SellPriceCalculator(new Random(42));
            config = ScriptableObject.CreateInstance<CollectorNPCConfig>();
            SetupConfig();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(config);
        }

        [Test]
        public void CalculateSellPrice_Common_WithinRange()
        {
            for (int seed = 0; seed < 100; seed++)
            {
                var calc = new SellPriceCalculator(new Random(seed));
                int price = calc.CalculateSellPrice(ChipRarity.Common, config);

                Assert.GreaterOrEqual(price, 5, $"Seed {seed}: price below min");
                Assert.LessOrEqual(price, 15, $"Seed {seed}: price above max");
            }
        }

        [Test]
        public void CalculateSellPrice_Rare_WithinRange()
        {
            for (int seed = 0; seed < 100; seed++)
            {
                var calc = new SellPriceCalculator(new Random(seed));
                int price = calc.CalculateSellPrice(ChipRarity.Rare, config);

                Assert.GreaterOrEqual(price, 25);
                Assert.LessOrEqual(price, 75);
            }
        }

        [Test]
        public void CalculateSellPrice_UltraRare_WithinRange()
        {
            for (int seed = 0; seed < 100; seed++)
            {
                var calc = new SellPriceCalculator(new Random(seed));
                int price = calc.CalculateSellPrice(ChipRarity.UltraRare, config);

                Assert.GreaterOrEqual(price, 100);
                Assert.LessOrEqual(price, 300);
            }
        }

        [Test]
        public void CalculateSellPrice_NullConfig_ReturnsZero()
        {
            int price = calculator.CalculateSellPrice(ChipRarity.Common, null);

            Assert.AreEqual(0, price);
        }

        [Test]
        public void CalculateSellPrice_UnconfiguredRarity_ReturnsZero()
        {
            // Create config with only Common
            var sparseConfig = ScriptableObject.CreateInstance<CollectorNPCConfig>();
            var so = new SerializedObject(sparseConfig);
            var sellPrices = so.FindProperty("sellPriceRanges");
            sellPrices.arraySize = 1;
            var common = sellPrices.GetArrayElementAtIndex(0);
            common.FindPropertyRelative("rarity").enumValueIndex = (int)ChipRarity.Common;
            common.FindPropertyRelative("minPrice").intValue = 5;
            common.FindPropertyRelative("maxPrice").intValue = 10;
            so.ApplyModifiedPropertiesWithoutUndo();

            int price = calculator.CalculateSellPrice(ChipRarity.UltraRare, sparseConfig);

            Assert.AreEqual(0, price);

            UnityEngine.Object.DestroyImmediate(sparseConfig);
        }

        [Test]
        public void CalculateSellPrice_DeterministicWithSameSeed()
        {
            var calc1 = new SellPriceCalculator(new Random(99));
            var calc2 = new SellPriceCalculator(new Random(99));

            int price1 = calc1.CalculateSellPrice(ChipRarity.Rare, config);
            int price2 = calc2.CalculateSellPrice(ChipRarity.Rare, config);

            Assert.AreEqual(price1, price2);
        }

        [Test]
        public void CalculateTotalSellPrice_MultipleChips_SumWithinExpectedRange()
        {
            int quantity = 5;

            for (int seed = 0; seed < 50; seed++)
            {
                var calc = new SellPriceCalculator(new Random(seed));
                int total = calc.CalculateTotalSellPrice(ChipRarity.Common, quantity, config);

                // min possible: 5 * 5 = 25, max possible: 15 * 5 = 75
                Assert.GreaterOrEqual(total, 25);
                Assert.LessOrEqual(total, 75);
            }
        }

        [Test]
        public void CalculateTotalSellPrice_SingleChip_EqualsIndividualPrice()
        {
            var calc1 = new SellPriceCalculator(new Random(42));
            var calc2 = new SellPriceCalculator(new Random(42));

            int individual = calc1.CalculateSellPrice(ChipRarity.Common, config);
            int total = calc2.CalculateTotalSellPrice(ChipRarity.Common, 1, config);

            Assert.AreEqual(individual, total);
        }

        [Test]
        public void CalculateTotalSellPrice_ZeroQuantity_ReturnsZero()
        {
            int total = calculator.CalculateTotalSellPrice(ChipRarity.Common, 0, config);

            Assert.AreEqual(0, total);
        }

        [Test]
        public void CalculateTotalSellPrice_NegativeQuantity_ReturnsZero()
        {
            int total = calculator.CalculateTotalSellPrice(ChipRarity.Common, -5, config);

            Assert.AreEqual(0, total);
        }

        [Test]
        public void CalculateTotalSellPrice_NullConfig_ReturnsZero()
        {
            int total = calculator.CalculateTotalSellPrice(ChipRarity.Common, 3, null);

            Assert.AreEqual(0, total);
        }

        [Test]
        public void CalculateBatchSellPrice_CorrectMultiplication()
        {
            int result = calculator.CalculateBatchSellPrice(ChipRarity.Common, 5, 10);

            Assert.AreEqual(50, result);
        }

        [Test]
        public void CalculateBatchSellPrice_ZeroQuantity_ReturnsZero()
        {
            int result = calculator.CalculateBatchSellPrice(ChipRarity.Common, 0, 10);

            Assert.AreEqual(0, result);
        }

        [Test]
        public void CalculateBatchSellPrice_ZeroUnitPrice_ReturnsZero()
        {
            int result = calculator.CalculateBatchSellPrice(ChipRarity.Common, 5, 0);

            Assert.AreEqual(0, result);
        }

        [Test]
        public void GenerateCyclePrice_WithinRange()
        {
            for (int seed = 0; seed < 100; seed++)
            {
                var calc = new SellPriceCalculator(new Random(seed));
                int price = calc.GenerateCyclePrice(ChipRarity.Rare, config);

                Assert.GreaterOrEqual(price, 25);
                Assert.LessOrEqual(price, 75);
            }
        }

        [Test]
        public void CalculateSellPrice_ProducesVariety()
        {
            // Verify prices aren't always the same value
            bool hasVariety = false;
            int firstPrice = new SellPriceCalculator(new Random(0)).CalculateSellPrice(ChipRarity.Rare, config);

            for (int seed = 1; seed < 20; seed++)
            {
                var calc = new SellPriceCalculator(new Random(seed));
                int price = calc.CalculateSellPrice(ChipRarity.Rare, config);
                if (price != firstPrice)
                {
                    hasVariety = true;
                    break;
                }
            }

            Assert.IsTrue(hasVariety, "Prices should vary across different seeds");
        }

        private void SetupConfig()
        {
            var so = new SerializedObject(config);
            var sellPrices = so.FindProperty("sellPriceRanges");
            sellPrices.arraySize = 3;

            var common = sellPrices.GetArrayElementAtIndex(0);
            common.FindPropertyRelative("rarity").enumValueIndex = (int)ChipRarity.Common;
            common.FindPropertyRelative("minPrice").intValue = 5;
            common.FindPropertyRelative("maxPrice").intValue = 15;

            var rare = sellPrices.GetArrayElementAtIndex(1);
            rare.FindPropertyRelative("rarity").enumValueIndex = (int)ChipRarity.Rare;
            rare.FindPropertyRelative("minPrice").intValue = 25;
            rare.FindPropertyRelative("maxPrice").intValue = 75;

            var ultraRare = sellPrices.GetArrayElementAtIndex(2);
            ultraRare.FindPropertyRelative("rarity").enumValueIndex = (int)ChipRarity.UltraRare;
            ultraRare.FindPropertyRelative("minPrice").intValue = 100;
            ultraRare.FindPropertyRelative("maxPrice").intValue = 300;

            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
