using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Flipit.Core;
using Random = System.Random;

namespace Flipit.NPC.Tests
{
    [TestFixture]
    public class CollectorNPCManagerTests
    {
        private CollectorNPCManager manager;
        private GameObject managerObject;
        private CollectorNPCConfig config;
        private WalletData walletData;
        private MockChipInventory inventory;
        private MockChipCatalog catalog;

        [SetUp]
        public void SetUp()
        {
            managerObject = new GameObject("TestNPCManager");
            manager = managerObject.AddComponent<CollectorNPCManager>();

            config = ScriptableObject.CreateInstance<CollectorNPCConfig>();
            SetupConfig();

            walletData = ScriptableObject.CreateInstance<WalletData>();
            walletData.InitializeNewGame(); // Starts with 25 pejecoins

            catalog = new MockChipCatalog();
            inventory = new MockChipInventory(new Dictionary<string, int>
            {
                { "common_aguila", 5 },
                { "common_sol", 3 },
                { "common_luna", 4 },
                { "rare_dragon", 3 },
                { "rare_fenix", 2 }
            });

            var calculator = new SellPriceCalculator(new Random(42));
            var tradeGen = new TradeGenerator(new Random(42));

            manager.InjectDependencies(config, walletData, inventory, catalog, calculator, tradeGen);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(managerObject);
            Object.DestroyImmediate(config);
            Object.DestroyImmediate(walletData);
        }

        // --- Sell Tests ---

        [Test]
        public void TrySell_ValidSell_ReturnsSuccess()
        {
            manager.RefreshCyclePrices();

            var result = manager.TrySell("common_aguila", 2);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("common_aguila", result.ChipId);
            Assert.AreEqual(2, result.QuantitySold);
            Assert.Greater(result.MoneyEarned, 0);
        }

        [Test]
        public void TrySell_ValidSell_RemovesChipsFromInventory()
        {
            manager.RefreshCyclePrices();

            manager.TrySell("common_aguila", 2);

            Assert.AreEqual(3, inventory.GetChipCount("common_aguila"));
        }

        [Test]
        public void TrySell_ValidSell_AddsMoneyToWallet()
        {
            manager.RefreshCyclePrices();
            long beforeTotal = walletData.GetTotalInSheintavos();

            var result = manager.TrySell("common_aguila", 2);

            long afterTotal = walletData.GetTotalInSheintavos();
            Assert.AreEqual(result.MoneyEarned, afterTotal - beforeTotal);
        }

        [Test]
        public void TrySell_NotEnoughChips_ReturnsFailed()
        {
            manager.RefreshCyclePrices();

            var result = manager.TrySell("common_aguila", 100);

            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.FailReason);
            // Inventory unchanged
            Assert.AreEqual(5, inventory.GetChipCount("common_aguila"));
        }

        [Test]
        public void TrySell_InvalidChipId_ReturnsFailed()
        {
            manager.RefreshCyclePrices();

            var result = manager.TrySell("", 1);

            Assert.IsFalse(result.Success);
        }

        [Test]
        public void TrySell_ZeroQuantity_ReturnsFailed()
        {
            manager.RefreshCyclePrices();

            var result = manager.TrySell("common_aguila", 0);

            Assert.IsFalse(result.Success);
        }

        [Test]
        public void TrySell_NegativeQuantity_ReturnsFailed()
        {
            manager.RefreshCyclePrices();

            var result = manager.TrySell("common_aguila", -1);

            Assert.IsFalse(result.Success);
        }

        [Test]
        public void TrySell_FiresOnSellCompletedEvent()
        {
            manager.RefreshCyclePrices();
            SellResult eventResult = null;
            manager.OnSellCompleted += (r) => eventResult = r;

            manager.TrySell("common_aguila", 1);

            Assert.IsNotNull(eventResult);
            Assert.IsTrue(eventResult.Success);
        }

        [Test]
        public void TrySell_AllChips_RemovesFromInventory()
        {
            manager.RefreshCyclePrices();

            var result = manager.TrySell("common_aguila", 5);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, inventory.GetChipCount("common_aguila"));
            Assert.IsFalse(inventory.HasChip("common_aguila"));
        }

        // --- Trade Tests ---

        [Test]
        public void TryTrade_ValidTrade_ReturnsSuccess()
        {
            var offer = new TradeOffer("ultra_cosmico", ChipRarity.UltraRare, new TradeRequirement[]
            {
                new TradeRequirement("common_aguila", 2),
                new TradeRequirement("rare_dragon", 1)
            });

            var result = manager.TryTrade(offer);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("ultra_cosmico", result.ChipObtained);
        }

        [Test]
        public void TryTrade_ValidTrade_RemovesRequiredChips()
        {
            var offer = new TradeOffer("ultra_cosmico", ChipRarity.UltraRare, new TradeRequirement[]
            {
                new TradeRequirement("common_aguila", 2),
                new TradeRequirement("rare_dragon", 1)
            });

            manager.TryTrade(offer);

            Assert.AreEqual(3, inventory.GetChipCount("common_aguila"));
            Assert.AreEqual(2, inventory.GetChipCount("rare_dragon"));
        }

        [Test]
        public void TryTrade_ValidTrade_AddsOfferedChip()
        {
            var offer = new TradeOffer("ultra_cosmico", ChipRarity.UltraRare, new TradeRequirement[]
            {
                new TradeRequirement("common_aguila", 2)
            });

            manager.TryTrade(offer);

            Assert.AreEqual(1, inventory.GetChipCount("ultra_cosmico"));
        }

        [Test]
        public void TryTrade_CannotAfford_ReturnsFailed()
        {
            var offer = new TradeOffer("ultra_cosmico", ChipRarity.UltraRare, new TradeRequirement[]
            {
                new TradeRequirement("common_aguila", 100) // Player only has 5
            });

            var result = manager.TryTrade(offer);

            Assert.IsFalse(result.Success);
            // Inventory unchanged
            Assert.AreEqual(5, inventory.GetChipCount("common_aguila"));
        }

        [Test]
        public void TryTrade_NullOffer_ReturnsFailed()
        {
            var result = manager.TryTrade(null);

            Assert.IsFalse(result.Success);
        }

        [Test]
        public void TryTrade_FiresOnTradeCompletedEvent()
        {
            TradeResult eventResult = null;
            manager.OnTradeCompleted += (r) => eventResult = r;

            var offer = new TradeOffer("ultra_cosmico", ChipRarity.UltraRare, new TradeRequirement[]
            {
                new TradeRequirement("common_aguila", 1)
            });

            manager.TryTrade(offer);

            Assert.IsNotNull(eventResult);
            Assert.IsTrue(eventResult.Success);
        }

        // --- Offers Tests ---

        [Test]
        public void RefreshOffers_GeneratesOffers()
        {
            manager.RefreshOffers();

            Assert.IsNotNull(manager.CurrentOffers);
            Assert.Greater(manager.CurrentOffers.Length, 0);
        }

        [Test]
        public void RefreshOffers_FiresOnOffersRefreshedEvent()
        {
            bool eventFired = false;
            manager.OnOffersRefreshed += () => eventFired = true;

            manager.RefreshOffers();

            Assert.IsTrue(eventFired);
        }

        [Test]
        public void Initialize_SetsUpOffersAndPrices()
        {
            // Re-initialize to test the flow
            manager.Initialize(inventory, catalog);

            Assert.IsNotNull(manager.CurrentOffers);
            Assert.Greater(manager.CurrentOffers.Length, 0);
            Assert.Greater(manager.GetCycleSellPrice(ChipRarity.Common), 0);
        }

        // --- Cycle Price Tests ---

        [Test]
        public void GetCycleSellPrice_AfterRefresh_ReturnsPositiveValue()
        {
            manager.RefreshCyclePrices();

            int commonPrice = manager.GetCycleSellPrice(ChipRarity.Common);
            int rarePrice = manager.GetCycleSellPrice(ChipRarity.Rare);
            int ultraPrice = manager.GetCycleSellPrice(ChipRarity.UltraRare);

            Assert.Greater(commonPrice, 0);
            Assert.Greater(rarePrice, 0);
            Assert.Greater(ultraPrice, 0);
        }

        [Test]
        public void GetCycleSellPrice_BeforeRefresh_ReturnsZero()
        {
            // Fresh manager without RefreshCyclePrices called
            var freshObj = new GameObject("FreshManager");
            var freshManager = freshObj.AddComponent<CollectorNPCManager>();
            freshManager.InjectDependencies(config, walletData, inventory, catalog);

            int price = freshManager.GetCycleSellPrice(ChipRarity.Common);

            Assert.AreEqual(0, price);

            Object.DestroyImmediate(freshObj);
        }

        // --- CanAffordTrade Tests ---

        [Test]
        public void CanAffordTrade_WhenCanAfford_ReturnsTrue()
        {
            var offer = new TradeOffer("ultra_cosmico", ChipRarity.UltraRare, new TradeRequirement[]
            {
                new TradeRequirement("common_aguila", 2)
            });

            Assert.IsTrue(manager.CanAffordTrade(offer));
        }

        [Test]
        public void CanAffordTrade_WhenCannotAfford_ReturnsFalse()
        {
            var offer = new TradeOffer("ultra_cosmico", ChipRarity.UltraRare, new TradeRequirement[]
            {
                new TradeRequirement("common_aguila", 100)
            });

            Assert.IsFalse(manager.CanAffordTrade(offer));
        }

        [Test]
        public void CanAffordTrade_NullOffer_ReturnsFalse()
        {
            Assert.IsFalse(manager.CanAffordTrade(null));
        }

        private void SetupConfig()
        {
            var so = new SerializedObject(config);

            so.FindProperty("tradeOfferCount").intValue = 5;
            so.FindProperty("minTradeRequirementTypes").intValue = 1;
            so.FindProperty("maxTradeRequirementTypes").intValue = 3;

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
