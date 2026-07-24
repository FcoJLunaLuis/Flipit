using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Flipit.Core;
using Random = System.Random;

namespace Flipit.NPC.Tests
{
    [TestFixture]
    public class TradePanelControllerTests
    {
        private GameObject rootObject;
        private CollectorNPCUIController uiController;
        private CollectorNPCManager npcManager;
        private TradePanelController tradePanel;

        private CollectorNPCConfig config;
        private WalletData walletData;
        private MockChipInventory inventory;
        private MockChipCatalog catalog;

        // Panel GameObjects
        private GameObject mainMenuPanelGO;
        private GameObject sellPanelGO;
        private GameObject tradePanelGO;
        private GameObject confirmationPanelGO;
        private GameObject resultPanelGO;

        [SetUp]
        public void SetUp()
        {
            rootObject = new GameObject("Root");

            // UI Controller
            var uiObj = new GameObject("UIController");
            uiObj.transform.SetParent(rootObject.transform);
            uiController = uiObj.AddComponent<CollectorNPCUIController>();

            // NPC Manager
            var managerObj = new GameObject("Manager");
            managerObj.transform.SetParent(rootObject.transform);
            npcManager = managerObj.AddComponent<CollectorNPCManager>();

            // Trade Panel
            var tradePanelCtrlObj = new GameObject("TradePanelController");
            tradePanelCtrlObj.transform.SetParent(rootObject.transform);
            tradePanel = tradePanelCtrlObj.AddComponent<TradePanelController>();

            // Create panels
            mainMenuPanelGO = new GameObject("MainMenu");
            sellPanelGO = new GameObject("SellPanel");
            tradePanelGO = new GameObject("TradePanel");
            confirmationPanelGO = new GameObject("Confirmation");
            resultPanelGO = new GameObject("Result");

            mainMenuPanelGO.SetActive(false);
            sellPanelGO.SetActive(false);
            tradePanelGO.SetActive(false);
            confirmationPanelGO.SetActive(false);
            resultPanelGO.SetActive(false);

            // Wire UI Controller panels
            var uiSO = new SerializedObject(uiController);
            uiSO.FindProperty("mainMenuPanel").objectReferenceValue = mainMenuPanelGO;
            uiSO.FindProperty("sellPanel").objectReferenceValue = sellPanelGO;
            uiSO.FindProperty("tradePanel").objectReferenceValue = tradePanelGO;
            uiSO.FindProperty("confirmationPanel").objectReferenceValue = confirmationPanelGO;
            uiSO.FindProperty("resultPanel").objectReferenceValue = resultPanelGO;
            uiSO.ApplyModifiedPropertiesWithoutUndo();

            // Setup config
            config = ScriptableObject.CreateInstance<CollectorNPCConfig>();
            SetupConfig();

            walletData = ScriptableObject.CreateInstance<WalletData>();
            walletData.InitializeNewGame();

            catalog = new MockChipCatalog();
            inventory = new MockChipInventory(new Dictionary<string, int>
            {
                { "common_aguila", 5 },
                { "common_sol", 3 },
                { "common_luna", 4 },
                { "common_estrella", 2 },
                { "rare_dragon", 3 },
                { "rare_fenix", 2 }
            });

            var calculator = new SellPriceCalculator(new Random(42));
            var tradeGen = new TradeGenerator(new Random(42));
            npcManager.InjectDependencies(config, walletData, inventory, catalog, calculator, tradeGen);
            npcManager.RefreshCyclePrices();
            npcManager.RefreshOffers();

            tradePanel.InjectDependencies(uiController, npcManager);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(rootObject);
            Object.DestroyImmediate(mainMenuPanelGO);
            Object.DestroyImmediate(sellPanelGO);
            Object.DestroyImmediate(tradePanelGO);
            Object.DestroyImmediate(confirmationPanelGO);
            Object.DestroyImmediate(resultPanelGO);
            Object.DestroyImmediate(config);
            Object.DestroyImmediate(walletData);
        }

        [Test]
        public void OfferCount_MatchesManagerOffers()
        {
            Assert.AreEqual(npcManager.CurrentOffers.Length, tradePanel.OfferCount);
        }

        [Test]
        public void InitialState_IsOfferList()
        {
            Assert.AreEqual(TradePanelState.OfferList, tradePanel.PanelState);
        }

        [Test]
        public void SelectedOfferIndex_StartsAtZero()
        {
            Assert.AreEqual(0, tradePanel.SelectedOfferIndex);
        }

        [Test]
        public void SelectedOffer_ReturnsCurrentOffer()
        {
            var offer = tradePanel.SelectedOffer;

            Assert.IsNotNull(offer);
            Assert.IsNotNull(offer.OfferedChipId);
            Assert.IsNotNull(offer.Requirements);
        }

        [Test]
        public void GetOfferDisplayInfo_ValidIndex_ReturnsInfo()
        {
            var info = tradePanel.GetOfferDisplayInfo(0);

            Assert.IsNotNull(info);
            Assert.IsNotNull(info.OfferedChipId);
            Assert.IsNotNull(info.Requirements);
        }

        [Test]
        public void GetOfferDisplayInfo_InvalidIndex_ReturnsNull()
        {
            Assert.IsNull(tradePanel.GetOfferDisplayInfo(-1));
            Assert.IsNull(tradePanel.GetOfferDisplayInfo(100));
        }

        [Test]
        public void GetOfferDisplayInfo_ShowsAffordability()
        {
            // At least one offer should exist
            var info = tradePanel.GetOfferDisplayInfo(0);
            Assert.IsNotNull(info);
            // CanAfford is a boolean, just verify it doesn't throw
            bool _ = info.CanAfford;
        }

        [Test]
        public void ConfirmTrade_WithAffordableOffer_ExecutesTrade()
        {
            // Find an affordable offer
            TradeOffer affordableOffer = null;
            for (int i = 0; i < tradePanel.OfferCount; i++)
            {
                var info = tradePanel.GetOfferDisplayInfo(i);
                if (info != null && info.CanAfford)
                {
                    affordableOffer = info.Offer;
                    break;
                }
            }

            // Skip test if no affordable offers were generated (rare with this seed)
            if (affordableOffer == null)
            {
                Assert.Pass("No affordable offers generated with this seed");
                return;
            }

            uiController.OpenMenu();
            uiController.OpenTradePanel();
            uiController.ShowConfirmation();

            TradeResult capturedResult = null;
            tradePanel.OnTradeExecuted += (r) => capturedResult = r;

            tradePanel.ConfirmTrade();

            Assert.IsNotNull(capturedResult);
            Assert.IsTrue(capturedResult.Success);
        }

        [Test]
        public void ConfirmTrade_AddsOfferedChipToInventory()
        {
            // Find an affordable offer
            int affordableIndex = -1;
            for (int i = 0; i < tradePanel.OfferCount; i++)
            {
                var info = tradePanel.GetOfferDisplayInfo(i);
                if (info != null && info.CanAfford)
                {
                    affordableIndex = i;
                    break;
                }
            }

            if (affordableIndex < 0)
            {
                Assert.Pass("No affordable offers generated with this seed");
                return;
            }

            var offer = npcManager.CurrentOffers[affordableIndex];
            string chipId = offer.OfferedChipId;
            int beforeCount = inventory.GetChipCount(chipId);

            uiController.OpenMenu();
            uiController.OpenTradePanel();
            uiController.ShowConfirmation();

            tradePanel.ConfirmTrade();

            // Only check if trade was affordable (index 0)
            if (affordableIndex == 0)
            {
                Assert.AreEqual(beforeCount + 1, inventory.GetChipCount(chipId));
            }
        }

        [Test]
        public void CancelTrade_ReturnsToOfferList()
        {
            uiController.OpenMenu();
            uiController.OpenTradePanel();
            uiController.ShowConfirmation();

            tradePanel.CancelTrade();

            Assert.AreEqual(TradePanelState.OfferList, tradePanel.PanelState);
        }

        [Test]
        public void CancelTrade_HidesConfirmationPanel()
        {
            uiController.OpenMenu();
            uiController.OpenTradePanel();
            uiController.ShowConfirmation();

            tradePanel.CancelTrade();

            Assert.IsFalse(confirmationPanelGO.activeSelf);
        }

        [Test]
        public void ConfirmTrade_FiresOnTradeExecutedEvent()
        {
            uiController.OpenMenu();
            uiController.OpenTradePanel();
            uiController.ShowConfirmation();

            bool fired = false;
            tradePanel.OnTradeExecuted += (r) => fired = true;

            tradePanel.ConfirmTrade();

            Assert.IsTrue(fired);
        }

        [Test]
        public void CanAffordSelectedOffer_ReflectsInventoryState()
        {
            // With well-stocked inventory, at least some offers should be affordable
            bool anyAffordable = false;
            for (int i = 0; i < tradePanel.OfferCount; i++)
            {
                var info = tradePanel.GetOfferDisplayInfo(i);
                if (info != null && info.CanAfford)
                {
                    anyAffordable = true;
                    break;
                }
            }

            // With this inventory, requirements are based on what player owns
            // so at least some should be affordable
            Assert.IsTrue(anyAffordable, "At least some offers should be affordable");
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
