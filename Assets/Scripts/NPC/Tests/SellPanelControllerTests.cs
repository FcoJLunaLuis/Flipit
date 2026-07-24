using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Flipit.Core;
using Random = System.Random;

namespace Flipit.NPC.Tests
{
    [TestFixture]
    public class SellPanelControllerTests
    {
        private GameObject rootObject;
        private CollectorNPCUIController uiController;
        private CollectorNPCManager npcManager;
        private SellPanelController sellPanel;

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

            // UI Controller setup
            var uiObj = new GameObject("UIController");
            uiObj.transform.SetParent(rootObject.transform);
            uiController = uiObj.AddComponent<CollectorNPCUIController>();

            // NPC Manager setup
            var managerObj = new GameObject("Manager");
            managerObj.transform.SetParent(rootObject.transform);
            npcManager = managerObj.AddComponent<CollectorNPCManager>();

            // Sell Panel
            var sellPanelObj = new GameObject("SellPanelController");
            sellPanelObj.transform.SetParent(rootObject.transform);
            sellPanel = sellPanelObj.AddComponent<SellPanelController>();

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

            // Wire up UI Controller panels
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
                { "rare_dragon", 2 }
            });

            var calculator = new SellPriceCalculator(new Random(42));
            var tradeGen = new TradeGenerator(new Random(42));
            npcManager.InjectDependencies(config, walletData, inventory, catalog, calculator, tradeGen);
            npcManager.RefreshCyclePrices();
            npcManager.RefreshOffers();

            sellPanel.InjectDependencies(uiController, npcManager);
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
        public void RefreshChipList_PopulatesListFromInventory()
        {
            sellPanel.RefreshChipList();

            Assert.AreEqual(3, sellPanel.ChipListCount);
        }

        [Test]
        public void RefreshChipList_StartsAtIndex0()
        {
            sellPanel.RefreshChipList();

            Assert.AreEqual(0, sellPanel.SelectedChipIndex);
        }

        [Test]
        public void RefreshChipList_SetsStateToChipList()
        {
            sellPanel.RefreshChipList();

            Assert.AreEqual(SellPanelState.ChipList, sellPanel.PanelState);
        }

        [Test]
        public void GetChipInfo_ValidIndex_ReturnsInfo()
        {
            sellPanel.RefreshChipList();

            var info = sellPanel.GetChipInfo(0);

            Assert.IsNotNull(info);
            Assert.IsNotNull(info.ChipId);
            Assert.Greater(info.OwnedCount, 0);
            Assert.Greater(info.UnitPrice, 0);
        }

        [Test]
        public void GetChipInfo_InvalidIndex_ReturnsNull()
        {
            sellPanel.RefreshChipList();

            Assert.IsNull(sellPanel.GetChipInfo(-1));
            Assert.IsNull(sellPanel.GetChipInfo(100));
        }

        [Test]
        public void SelectedChipOwnedCount_ReturnsCorrectCount()
        {
            sellPanel.RefreshChipList();

            int count = sellPanel.SelectedChipOwnedCount;

            Assert.Greater(count, 0);
        }

        [Test]
        public void SelectedChipSellPrice_ReturnsPositive()
        {
            sellPanel.RefreshChipList();

            int price = sellPanel.SelectedChipSellPrice;

            Assert.Greater(price, 0);
        }

        [Test]
        public void TotalSellPrice_IsQuantityTimesUnitPrice()
        {
            sellPanel.RefreshChipList();

            int unitPrice = sellPanel.SelectedChipSellPrice;
            int quantity = sellPanel.SelectedQuantity; // starts at 1

            Assert.AreEqual(unitPrice * quantity, sellPanel.TotalSellPrice);
        }

        [Test]
        public void ConfirmSell_WithValidData_ExecutesSell()
        {
            sellPanel.RefreshChipList();

            // Simulate reaching confirmation state
            string chipId = sellPanel.SelectedChipId;
            int beforeCount = inventory.GetChipCount(chipId);
            long beforeMoney = walletData.GetTotalInSheintavos();

            // Open the menu flow so uiController is in right state
            uiController.OpenMenu();
            uiController.OpenSellPanel();
            uiController.ShowConfirmation();

            SellResult capturedResult = null;
            sellPanel.OnSellExecuted += (r) => capturedResult = r;

            sellPanel.ConfirmSell();

            Assert.IsNotNull(capturedResult);
            Assert.IsTrue(capturedResult.Success);
            Assert.AreEqual(beforeCount - 1, inventory.GetChipCount(chipId));
            Assert.Greater(walletData.GetTotalInSheintavos(), beforeMoney);
        }

        [Test]
        public void ConfirmSell_FiresOnSellExecutedEvent()
        {
            sellPanel.RefreshChipList();
            uiController.OpenMenu();
            uiController.OpenSellPanel();
            uiController.ShowConfirmation();

            bool fired = false;
            sellPanel.OnSellExecuted += (r) => fired = true;

            sellPanel.ConfirmSell();

            Assert.IsTrue(fired);
        }

        [Test]
        public void CancelSell_ReturnsToQuantitySelect()
        {
            sellPanel.RefreshChipList();
            uiController.OpenMenu();
            uiController.OpenSellPanel();
            uiController.ShowConfirmation();

            sellPanel.CancelSell();

            Assert.AreEqual(SellPanelState.QuantitySelect, sellPanel.PanelState);
        }

        [Test]
        public void InitialQuantity_IsOne()
        {
            sellPanel.RefreshChipList();

            Assert.AreEqual(1, sellPanel.SelectedQuantity);
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
