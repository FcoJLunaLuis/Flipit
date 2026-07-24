using UnityEngine;
using UnityEngine.InputSystem;
using Flipit.Core;

namespace Flipit.Shop
{
    public class ShopUIController : MonoBehaviour
    {
        private const int Columns = 5;

        [Header("References")]
        [SerializeField] private ShopManager shopManager;
        [SerializeField] private InputActionAsset inputActions;

        [Header("Input Configuration (Configurable for Playtest)")]
        [Tooltip("Name of the action used to select/confirm. Change for playtest configurations.")]
        [SerializeField] private string selectActionName = "Select";
        [Tooltip("Name of the action used to cancel/exit. Change for playtest configurations.")]
        [SerializeField] private string cancelActionName = "Cancel";
        [Tooltip("Name of the action used to navigate. Change for playtest configurations.")]
        [SerializeField] private string navigateActionName = "Navigate";

        [Header("UI Panels")]
        [SerializeField] private GameObject confirmationPanel;
        [SerializeField] private GameObject resultsPanel;

        // Navigation state
        private int currentRow;
        private int currentColumn;
        private bool isConfirmationOpen;
        private bool isResultsOpen;

        // Input
        private InputActionMap shopActionMap;
        private InputAction navigateAction;
        private InputAction selectAction;
        private InputAction cancelAction;
        private bool navigationConsumed;

        // Results display
        private PurchaseResult lastResult;

        public int CurrentRow => currentRow;
        public int CurrentColumn => currentColumn;
        public bool IsConfirmationOpen => isConfirmationOpen;
        public bool IsResultsOpen => isResultsOpen;

        public event System.Action OnExitShop;
        public event System.Action<int, int> OnCursorMoved;
        public event System.Action<BagConfig> OnConfirmationShown;
        public event System.Action OnConfirmationClosed;
        public event System.Action<PurchaseResult> OnResultsShown;

        private void OnEnable()
        {
            SetupInput();
            EnableInput();
        }

        private void OnDisable()
        {
            DisableInput();
        }

        private void SetupInput()
        {
            if (inputActions == null) return;

            shopActionMap = inputActions.FindActionMap("Shop");
            if (shopActionMap == null) return;

            navigateAction = shopActionMap.FindAction(navigateActionName);
            selectAction = shopActionMap.FindAction(selectActionName);
            cancelAction = shopActionMap.FindAction(cancelActionName);
        }

        private void EnableInput()
        {
            shopActionMap?.Enable();
        }

        private void DisableInput()
        {
            shopActionMap?.Disable();
        }

        private void Update()
        {
            if (shopActionMap == null) return;

            HandleNavigation();
            HandleSelect();
            HandleCancel();
        }

        private void HandleNavigation()
        {
            if (navigateAction == null || isConfirmationOpen || isResultsOpen) return;

            Vector2 input = navigateAction.ReadValue<Vector2>();

            if (input.sqrMagnitude < 0.5f)
            {
                navigationConsumed = false;
                return;
            }

            if (navigationConsumed) return;
            navigationConsumed = true;

            int rows = shopManager != null ? shopManager.TierCount : 3;

            if (input.x > 0.5f)
                currentColumn = (currentColumn + 1) % Columns;
            else if (input.x < -0.5f)
                currentColumn = (currentColumn - 1 + Columns) % Columns;

            if (input.y > 0.5f)
                currentRow = (currentRow - 1 + rows) % rows;
            else if (input.y < -0.5f)
                currentRow = (currentRow + 1) % rows;

            OnCursorMoved?.Invoke(currentRow, currentColumn);
        }

        private void HandleSelect()
        {
            if (selectAction == null) return;
            if (!selectAction.WasPerformedThisFrame()) return;

            if (isResultsOpen)
            {
                CloseResults();
                return;
            }

            if (isConfirmationOpen)
            {
                ConfirmPurchase();
                return;
            }

            ShowConfirmation();
        }

        private void HandleCancel()
        {
            if (cancelAction == null) return;
            if (!cancelAction.WasPerformedThisFrame()) return;

            if (isResultsOpen)
            {
                CloseResults();
                return;
            }

            if (isConfirmationOpen)
            {
                CloseConfirmation();
                return;
            }

            RequestExit();
        }

        private void ShowConfirmation()
        {
            if (shopManager == null) return;

            var config = shopManager.GetBagConfig(currentRow);
            if (config == null) return;

            isConfirmationOpen = true;
            if (confirmationPanel != null)
                confirmationPanel.SetActive(true);

            OnConfirmationShown?.Invoke(config);
        }

        private void CloseConfirmation()
        {
            isConfirmationOpen = false;
            if (confirmationPanel != null)
                confirmationPanel.SetActive(false);

            OnConfirmationClosed?.Invoke();
        }

        private void ConfirmPurchase()
        {
            if (shopManager == null) return;

            lastResult = shopManager.TryPurchase(currentRow, currentColumn);
            CloseConfirmation();
            ShowResults(lastResult);
        }

        private void ShowResults(PurchaseResult result)
        {
            isResultsOpen = true;
            if (resultsPanel != null)
                resultsPanel.SetActive(true);

            OnResultsShown?.Invoke(result);
        }

        private void CloseResults()
        {
            isResultsOpen = false;
            lastResult = null;
            if (resultsPanel != null)
                resultsPanel.SetActive(false);
        }

        public void RequestExit()
        {
            DisableInput();
            OnExitShop?.Invoke();
        }

        public void ResetCursor()
        {
            currentRow = 0;
            currentColumn = 0;
        }
    }
}