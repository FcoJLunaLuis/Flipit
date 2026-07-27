using NUnit.Framework;
using UnityEngine;

namespace Flipit.NPC.Tests
{
    [TestFixture]
    public class CollectorNPCUIControllerTests
    {
        private GameObject controllerObject;
        private CollectorNPCUIController controller;
        private GameObject mainMenuPanel;
        private GameObject sellPanel;
        private GameObject tradePanel;
        private GameObject confirmationPanel;
        private GameObject resultPanel;

        [SetUp]
        public void SetUp()
        {
            controllerObject = new GameObject("UIController");
            controller = controllerObject.AddComponent<CollectorNPCUIController>();

            // Create panel GameObjects for testing
            mainMenuPanel = new GameObject("MainMenu");
            sellPanel = new GameObject("SellPanel");
            tradePanel = new GameObject("TradePanel");
            confirmationPanel = new GameObject("ConfirmationPanel");
            resultPanel = new GameObject("ResultPanel");

            // All start inactive
            mainMenuPanel.SetActive(false);
            sellPanel.SetActive(false);
            tradePanel.SetActive(false);
            confirmationPanel.SetActive(false);
            resultPanel.SetActive(false);

            // Inject panels via serialized fields
            var so = new UnityEditor.SerializedObject(controller);
            so.FindProperty("mainMenuPanel").objectReferenceValue = mainMenuPanel;
            so.FindProperty("sellPanel").objectReferenceValue = sellPanel;
            so.FindProperty("tradePanel").objectReferenceValue = tradePanel;
            so.FindProperty("confirmationPanel").objectReferenceValue = confirmationPanel;
            so.FindProperty("resultPanel").objectReferenceValue = resultPanel;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(controllerObject);
            Object.DestroyImmediate(mainMenuPanel);
            Object.DestroyImmediate(sellPanel);
            Object.DestroyImmediate(tradePanel);
            Object.DestroyImmediate(confirmationPanel);
            Object.DestroyImmediate(resultPanel);
        }

        // --- Open/Close Tests ---

        [Test]
        public void InitialState_IsClosed()
        {
            Assert.AreEqual(NPCUIState.Closed, controller.CurrentState);
        }

        [Test]
        public void OpenMenu_SetsStateToMainMenu()
        {
            controller.OpenMenu();

            Assert.AreEqual(NPCUIState.MainMenu, controller.CurrentState);
        }

        [Test]
        public void OpenMenu_ActivatesMainMenuPanel()
        {
            controller.OpenMenu();

            Assert.IsTrue(mainMenuPanel.activeSelf);
        }

        [Test]
        public void OpenMenu_DeactivatesOtherPanels()
        {
            controller.OpenMenu();

            Assert.IsFalse(sellPanel.activeSelf);
            Assert.IsFalse(tradePanel.activeSelf);
            Assert.IsFalse(confirmationPanel.activeSelf);
            Assert.IsFalse(resultPanel.activeSelf);
        }

        [Test]
        public void OpenMenu_FiresOnMenuOpenedEvent()
        {
            bool fired = false;
            controller.OnMenuOpened += () => fired = true;

            controller.OpenMenu();

            Assert.IsTrue(fired);
        }

        [Test]
        public void OpenMenu_StartsAtIndex0()
        {
            controller.OpenMenu();

            Assert.AreEqual(0, controller.MainMenuIndex);
        }

        [Test]
        public void CloseMenu_SetsStateToClosed()
        {
            controller.OpenMenu();
            controller.CloseMenu();

            Assert.AreEqual(NPCUIState.Closed, controller.CurrentState);
        }

        [Test]
        public void CloseMenu_DeactivatesAllPanels()
        {
            controller.OpenMenu();
            controller.CloseMenu();

            Assert.IsFalse(mainMenuPanel.activeSelf);
            Assert.IsFalse(sellPanel.activeSelf);
            Assert.IsFalse(tradePanel.activeSelf);
            Assert.IsFalse(confirmationPanel.activeSelf);
            Assert.IsFalse(resultPanel.activeSelf);
        }

        [Test]
        public void CloseMenu_FiresOnMenuClosedEvent()
        {
            controller.OpenMenu();
            bool fired = false;
            controller.OnMenuClosed += () => fired = true;

            controller.CloseMenu();

            Assert.IsTrue(fired);
        }

        // --- Navigation Tests ---

        [Test]
        public void OpenMenu_WhenAlreadyOpen_DoesNothing()
        {
            controller.OpenMenu();
            controller.OpenSellPanel();

            // Try to open menu again — should not change state
            controller.OpenMenu();

            Assert.AreEqual(NPCUIState.SellPanel, controller.CurrentState);
        }

        // --- Panel Transition Tests ---

        [Test]
        public void OpenSellPanel_SetsStateToSellPanel()
        {
            controller.OpenMenu();
            controller.OpenSellPanel();

            Assert.AreEqual(NPCUIState.SellPanel, controller.CurrentState);
        }

        [Test]
        public void OpenSellPanel_ActivatesSellPanel()
        {
            controller.OpenMenu();
            controller.OpenSellPanel();

            Assert.IsTrue(sellPanel.activeSelf);
            Assert.IsFalse(mainMenuPanel.activeSelf);
        }

        [Test]
        public void OpenSellPanel_FiresEvent()
        {
            controller.OpenMenu();
            bool fired = false;
            controller.OnSellPanelOpened += () => fired = true;

            controller.OpenSellPanel();

            Assert.IsTrue(fired);
        }

        [Test]
        public void OpenTradePanel_SetsStateToTradePanel()
        {
            controller.OpenMenu();
            controller.OpenTradePanel();

            Assert.AreEqual(NPCUIState.TradePanel, controller.CurrentState);
        }

        [Test]
        public void OpenTradePanel_ActivatesTradePanel()
        {
            controller.OpenMenu();
            controller.OpenTradePanel();

            Assert.IsTrue(tradePanel.activeSelf);
            Assert.IsFalse(mainMenuPanel.activeSelf);
        }

        [Test]
        public void OpenTradePanel_FiresEvent()
        {
            controller.OpenMenu();
            bool fired = false;
            controller.OnTradePanelOpened += () => fired = true;

            controller.OpenTradePanel();

            Assert.IsTrue(fired);
        }

        [Test]
        public void ReturnToMainMenu_FromSellPanel_Works()
        {
            controller.OpenMenu();
            controller.OpenSellPanel();
            controller.ReturnToMainMenu();

            Assert.AreEqual(NPCUIState.MainMenu, controller.CurrentState);
            Assert.IsTrue(mainMenuPanel.activeSelf);
            Assert.IsFalse(sellPanel.activeSelf);
        }

        [Test]
        public void ReturnToMainMenu_FromTradePanel_Works()
        {
            controller.OpenMenu();
            controller.OpenTradePanel();
            controller.ReturnToMainMenu();

            Assert.AreEqual(NPCUIState.MainMenu, controller.CurrentState);
            Assert.IsTrue(mainMenuPanel.activeSelf);
            Assert.IsFalse(tradePanel.activeSelf);
        }

        // --- Confirmation Tests ---

        [Test]
        public void ShowConfirmation_SetsStateToConfirmation()
        {
            controller.OpenMenu();
            controller.OpenSellPanel();
            controller.ShowConfirmation();

            Assert.AreEqual(NPCUIState.Confirmation, controller.CurrentState);
            Assert.IsTrue(confirmationPanel.activeSelf);
        }

        [Test]
        public void HideConfirmation_ReturnsToSpecifiedState()
        {
            controller.OpenMenu();
            controller.OpenSellPanel();
            controller.ShowConfirmation();
            controller.HideConfirmation(NPCUIState.SellPanel);

            Assert.AreEqual(NPCUIState.SellPanel, controller.CurrentState);
            Assert.IsFalse(confirmationPanel.activeSelf);
        }

        // --- Result Tests ---

        [Test]
        public void ShowResult_SetsStateToResult()
        {
            controller.OpenMenu();
            controller.ShowResult();

            Assert.AreEqual(NPCUIState.Result, controller.CurrentState);
            Assert.IsTrue(resultPanel.activeSelf);
        }

        [Test]
        public void HideResult_ReturnsToSpecifiedState()
        {
            controller.OpenMenu();
            controller.OpenSellPanel();
            controller.ShowResult();
            controller.HideResult(NPCUIState.SellPanel);

            Assert.AreEqual(NPCUIState.SellPanel, controller.CurrentState);
            Assert.IsFalse(resultPanel.activeSelf);
        }
    }
}
