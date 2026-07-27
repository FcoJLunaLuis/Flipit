using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Flipit.NPC
{
    /// <summary>
    /// Controls the Collector NPC UI navigation and state transitions.
    /// Handles input for navigating between panels: MainMenu, SellPanel, TradePanel.
    /// Follows the same Input System pattern as ShopUIController.
    /// </summary>
    public class CollectorNPCUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CollectorNPCManager npcManager;
        [SerializeField] private InputActionAsset inputActions;

        [Header("Input Configuration (Configurable)")]
        [Tooltip("Name of the action used to select/confirm.")]
        [SerializeField] private string selectActionName = "Select";
        [Tooltip("Name of the action used to cancel/go back.")]
        [SerializeField] private string cancelActionName = "Cancel";
        [Tooltip("Name of the action used to navigate.")]
        [SerializeField] private string navigateActionName = "Navigate";

        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject sellPanel;
        [SerializeField] private GameObject tradePanel;
        [SerializeField] private GameObject confirmationPanel;
        [SerializeField] private GameObject resultPanel;

        // State
        private NPCUIState currentState = NPCUIState.Closed;
        private int mainMenuIndex; // 0 = Sell, 1 = Trade
        private bool navigationConsumed;
        private int inputCooldownFrames;

        // Input
        private InputActionMap npcActionMap;
        private InputAction navigateAction;
        private InputAction selectAction;
        private InputAction cancelAction;

        // Public accessors
        public NPCUIState CurrentState => currentState;
        public int MainMenuIndex => mainMenuIndex;
        public CollectorNPCManager NPCManager => npcManager;

        // Events
        public event Action OnMenuOpened;
        public event Action OnMenuClosed;
        public event Action<int> OnMainMenuCursorMoved; // index 0=Sell, 1=Trade
        public event Action OnSellPanelOpened;
        public event Action OnTradePanelOpened;
        public event Action OnConfirmationOpened;
        public event Action OnConfirmationClosed;
        public event Action OnResultShown;
        public event Action OnResultClosed;

        private void OnEnable()
        {
            SetupInput();
        }

        private void OnDisable()
        {
            DisableInput();
        }

        /// <summary>
        /// Opens the NPC UI. Call this from external trigger (cube click, action button).
        /// </summary>
        public void OpenMenu()
        {
            if (currentState != NPCUIState.Closed)
                return;

            currentState = NPCUIState.MainMenu;
            mainMenuIndex = 0;

            SetPanelActive(mainMenuPanel, true);
            SetPanelActive(sellPanel, false);
            SetPanelActive(tradePanel, false);
            SetPanelActive(confirmationPanel, false);
            SetPanelActive(resultPanel, false);

            EnableInput();
            OnMenuOpened?.Invoke();
            OnMainMenuCursorMoved?.Invoke(mainMenuIndex);
        }

        /// <summary>
        /// Closes the NPC UI completely.
        /// </summary>
        public void CloseMenu()
        {
            currentState = NPCUIState.Closed;

            SetPanelActive(mainMenuPanel, false);
            SetPanelActive(sellPanel, false);
            SetPanelActive(tradePanel, false);
            SetPanelActive(confirmationPanel, false);
            SetPanelActive(resultPanel, false);

            DisableInput();
            OnMenuClosed?.Invoke();
        }

        /// <summary>
        /// Transitions to the sell panel.
        /// </summary>
        public void OpenSellPanel()
        {
            currentState = NPCUIState.SellPanel;

            SetPanelActive(mainMenuPanel, false);
            SetPanelActive(sellPanel, true);

            OnSellPanelOpened?.Invoke();
        }

        /// <summary>
        /// Transitions to the trade panel.
        /// </summary>
        public void OpenTradePanel()
        {
            currentState = NPCUIState.TradePanel;

            SetPanelActive(mainMenuPanel, false);
            SetPanelActive(tradePanel, true);

            OnTradePanelOpened?.Invoke();
        }

        /// <summary>
        /// Returns to the main menu from any sub-panel.
        /// </summary>
        public void ReturnToMainMenu()
        {
            currentState = NPCUIState.MainMenu;
            inputCooldownFrames = 2; // Prevent same-frame cancel from closing menu

            SetPanelActive(sellPanel, false);
            SetPanelActive(tradePanel, false);
            SetPanelActive(confirmationPanel, false);
            SetPanelActive(resultPanel, false);
            SetPanelActive(mainMenuPanel, true);

            OnMainMenuCursorMoved?.Invoke(mainMenuIndex);
        }

        /// <summary>
        /// Shows the confirmation panel.
        /// </summary>
        public void ShowConfirmation()
        {
            SetPanelActive(confirmationPanel, true);
            currentState = NPCUIState.Confirmation;
            OnConfirmationOpened?.Invoke();
        }

        /// <summary>
        /// Hides the confirmation panel and returns to previous panel.
        /// </summary>
        public void HideConfirmation(NPCUIState returnTo)
        {
            SetPanelActive(confirmationPanel, false);
            currentState = returnTo;
            OnConfirmationClosed?.Invoke();
        }

        /// <summary>
        /// Shows a result message panel.
        /// </summary>
        public void ShowResult()
        {
            SetPanelActive(resultPanel, true);
            currentState = NPCUIState.Result;
            OnResultShown?.Invoke();
        }

        /// <summary>
        /// Hides the result panel and returns to specified state.
        /// </summary>
        public void HideResult(NPCUIState returnTo)
        {
            SetPanelActive(resultPanel, false);
            currentState = returnTo;
            OnResultClosed?.Invoke();
        }

        private void Update()
        {
            if (npcActionMap == null || currentState == NPCUIState.Closed)
                return;

            if (inputCooldownFrames > 0)
            {
                inputCooldownFrames--;
                return;
            }

            HandleMainMenuInput();
        }

        private void HandleMainMenuInput()
        {
            if (currentState != NPCUIState.MainMenu)
                return;

            HandleMainMenuNavigation();
            HandleMainMenuSelect();
            HandleMainMenuCancel();
        }

        private void HandleMainMenuNavigation()
        {
            if (navigateAction == null) return;

            Vector2 input = navigateAction.ReadValue<Vector2>();

            if (input.sqrMagnitude < 0.5f)
            {
                navigationConsumed = false;
                return;
            }

            if (navigationConsumed) return;
            navigationConsumed = true;

            // Vertical navigation between Sell (0) and Trade (1)
            if (input.y > 0.5f)
                mainMenuIndex = 0;
            else if (input.y < -0.5f)
                mainMenuIndex = 1;

            OnMainMenuCursorMoved?.Invoke(mainMenuIndex);
        }

        private void HandleMainMenuSelect()
        {
            if (selectAction == null) return;
            if (!selectAction.WasPerformedThisFrame()) return;

            if (mainMenuIndex == 0)
                OpenSellPanel();
            else
                OpenTradePanel();
        }

        private void HandleMainMenuCancel()
        {
            if (cancelAction == null) return;
            if (!cancelAction.WasPerformedThisFrame()) return;

            CloseMenu();
        }

        private void SetupInput()
        {
            if (inputActions == null) return;

            npcActionMap = inputActions.FindActionMap("NPC");
            if (npcActionMap == null)
            {
                // Fallback: try UI action map
                npcActionMap = inputActions.FindActionMap("UI");
            }

            if (npcActionMap == null) return;

            navigateAction = npcActionMap.FindAction(navigateActionName);
            selectAction = npcActionMap.FindAction(selectActionName);
            cancelAction = npcActionMap.FindAction(cancelActionName);
        }

        private void EnableInput()
        {
            npcActionMap?.Enable();
        }

        private void DisableInput()
        {
            npcActionMap?.Disable();
        }

        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
                panel.SetActive(active);
        }

        // For testing
        public void InjectDependencies(CollectorNPCManager manager, InputActionAsset actions)
        {
            npcManager = manager;
            inputActions = actions;
        }

        /// <summary>
        /// Gets the navigate action for sub-panels to reuse.
        /// </summary>
        public InputAction GetNavigateAction() => navigateAction;

        /// <summary>
        /// Gets the select action for sub-panels to reuse.
        /// </summary>
        public InputAction GetSelectAction() => selectAction;

        /// <summary>
        /// Gets the cancel action for sub-panels to reuse.
        /// </summary>
        public InputAction GetCancelAction() => cancelAction;
    }
}
