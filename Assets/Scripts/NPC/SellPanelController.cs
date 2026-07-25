using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Flipit.Core;

namespace Flipit.NPC
{
    /// <summary>
    /// Controls the Sell Panel UI within the Collector NPC interface.
    /// Handles chip list navigation, quantity selection, and sell confirmation.
    /// </summary>
    public class SellPanelController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CollectorNPCUIController uiController;
        [SerializeField] private CollectorNPCManager npcManager;

        // Internal state
        private SellPanelState panelState = SellPanelState.ChipList;
        private List<string> ownedChipIds = new List<string>();
        private int selectedChipIndex;
        private int selectedQuantity = 1;
        private bool navigationConsumed;
        private int inputCooldownFrames;

        // Cached input actions
        private InputAction navigateAction;
        private InputAction selectAction;
        private InputAction cancelAction;

        // Public accessors
        public SellPanelState PanelState => panelState;
        public int SelectedChipIndex => selectedChipIndex;
        public int SelectedQuantity => selectedQuantity;
        public int ChipListCount => ownedChipIds.Count;

        public string SelectedChipId =>
            selectedChipIndex >= 0 && selectedChipIndex < ownedChipIds.Count
                ? ownedChipIds[selectedChipIndex]
                : null;

        public int SelectedChipOwnedCount
        {
            get
            {
                var inventory = npcManager?.GetInventory();
                if (inventory == null || SelectedChipId == null)
                    return 0;
                return inventory.GetChipCount(SelectedChipId);
            }
        }

        public int SelectedChipSellPrice
        {
            get
            {
                if (npcManager == null || SelectedChipId == null) return 0;
                var catalog = npcManager.GetCatalog();
                if (catalog == null) return 0;
                ChipRarity rarity = catalog.GetChipRarity(SelectedChipId);
                return npcManager.GetCycleSellPrice(rarity);
            }
        }

        public int TotalSellPrice => selectedQuantity * SelectedChipSellPrice;

        // Events
        public event Action<int> OnChipListCursorMoved;
        public event Action<int> OnQuantityChanged;
        public event Action<string, int, int> OnSellConfirmationRequested; // chipId, quantity, totalPrice
        public event Action<SellResult> OnSellExecuted;
        public event Action OnPanelClosed;

        private void OnEnable()
        {
            if (uiController != null)
            {
                uiController.OnSellPanelOpened += HandlePanelOpened;
            }
        }

        private void OnDisable()
        {
            if (uiController != null)
            {
                uiController.OnSellPanelOpened -= HandlePanelOpened;
            }
        }

        private void Update()
        {
            if (inputCooldownFrames > 0)
            {
                inputCooldownFrames--;
                return;
            }

            if (uiController == null || uiController.CurrentState != NPCUIState.SellPanel)
            {
                if (uiController != null && uiController.CurrentState == NPCUIState.Confirmation
                    && panelState == SellPanelState.Confirmation)
                {
                    HandleConfirmationInput();
                }
                return;
            }

            switch (panelState)
            {
                case SellPanelState.ChipList:
                    HandleChipListInput();
                    break;
                case SellPanelState.QuantitySelect:
                    HandleQuantityInput();
                    break;
            }
        }

        /// <summary>
        /// Refreshes the chip list from the player's inventory.
        /// </summary>
        public void RefreshChipList()
        {
            ownedChipIds.Clear();

            var inventory = npcManager?.GetInventory();
            if (inventory == null) return;

            var owned = inventory.GetOwnedChipIds();
            for (int i = 0; i < owned.Count; i++)
            {
                ownedChipIds.Add(owned[i]);
            }

            selectedChipIndex = 0;
            panelState = SellPanelState.ChipList;
        }

        /// <summary>
        /// Gets chip info for display at a specific index.
        /// </summary>
        public SellChipInfo GetChipInfo(int index)
        {
            if (index < 0 || index >= ownedChipIds.Count)
                return null;

            var inventory = npcManager?.GetInventory();
            var catalog = npcManager?.GetCatalog();
            if (inventory == null || catalog == null)
                return null;

            string chipId = ownedChipIds[index];
            ChipRarity rarity = catalog.GetChipRarity(chipId);
            int count = inventory.GetChipCount(chipId);
            int unitPrice = npcManager.GetCycleSellPrice(rarity);

            return new SellChipInfo(chipId, rarity, count, unitPrice);
        }

        /// <summary>
        /// Confirms the sell action. Called when player confirms in the confirmation dialog.
        /// </summary>
        public void ConfirmSell()
        {
            if (npcManager == null || SelectedChipId == null)
                return;

            var result = npcManager.TrySell(SelectedChipId, selectedQuantity);
            OnSellExecuted?.Invoke(result);

            if (result.Success)
            {
                uiController.HideConfirmation(NPCUIState.SellPanel);
                uiController.ShowResult();
                panelState = SellPanelState.Result;

                // Refresh list since inventory changed
                RefreshChipList();
            }
            else
            {
                // Stay in confirmation, show error
                uiController.HideConfirmation(NPCUIState.SellPanel);
                panelState = SellPanelState.ChipList;
            }
        }

        /// <summary>
        /// Cancels the sell and returns to previous state.
        /// </summary>
        public void CancelSell()
        {
            uiController.HideConfirmation(NPCUIState.SellPanel);
            panelState = SellPanelState.QuantitySelect;
        }

        private void HandlePanelOpened()
        {
            CacheInputActions();
            RefreshChipList();
            inputCooldownFrames = 2; // Skip input for 2 frames after opening
            OnChipListCursorMoved?.Invoke(selectedChipIndex);
        }

        private void HandleChipListInput()
        {
            HandleChipListNavigation();
            HandleChipListSelect();
            HandleChipListCancel();
        }

        private void HandleChipListNavigation()
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

            if (ownedChipIds.Count == 0) return;

            if (input.y > 0.5f)
                selectedChipIndex = (selectedChipIndex - 1 + ownedChipIds.Count) % ownedChipIds.Count;
            else if (input.y < -0.5f)
                selectedChipIndex = (selectedChipIndex + 1) % ownedChipIds.Count;

            OnChipListCursorMoved?.Invoke(selectedChipIndex);
        }

        private void HandleChipListSelect()
        {
            if (selectAction == null) return;
            if (!selectAction.WasPerformedThisFrame()) return;

            if (ownedChipIds.Count == 0) return;

            // Transition to quantity selection
            selectedQuantity = 1;
            panelState = SellPanelState.QuantitySelect;
            OnQuantityChanged?.Invoke(selectedQuantity);
        }

        private void HandleChipListCancel()
        {
            if (cancelAction == null) return;
            if (!cancelAction.WasPerformedThisFrame()) return;

            // Return to main menu
            panelState = SellPanelState.ChipList;
            uiController.ReturnToMainMenu();
            OnPanelClosed?.Invoke();
        }

        private void HandleQuantityInput()
        {
            HandleQuantityNavigation();
            HandleQuantitySelect();
            HandleQuantityCancel();
        }

        private void HandleQuantityNavigation()
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

            int maxQuantity = SelectedChipOwnedCount;
            if (maxQuantity <= 0) return;

            if (input.x > 0.5f || input.y > 0.5f)
                selectedQuantity = Mathf.Min(selectedQuantity + 1, maxQuantity);
            else if (input.x < -0.5f || input.y < -0.5f)
                selectedQuantity = Mathf.Max(selectedQuantity - 1, 1);

            OnQuantityChanged?.Invoke(selectedQuantity);
        }

        private void HandleQuantitySelect()
        {
            if (selectAction == null) return;
            if (!selectAction.WasPerformedThisFrame()) return;
            if (SelectedChipId == null || SelectedChipOwnedCount <= 0) return;

            // Show confirmation
            panelState = SellPanelState.Confirmation;
            uiController.ShowConfirmation();
            OnSellConfirmationRequested?.Invoke(SelectedChipId, selectedQuantity, TotalSellPrice);
        }

        private void HandleQuantityCancel()
        {
            if (cancelAction == null) return;
            if (!cancelAction.WasPerformedThisFrame()) return;

            // Go back to chip list
            panelState = SellPanelState.ChipList;
            OnChipListCursorMoved?.Invoke(selectedChipIndex);
        }

        private void HandleConfirmationInput()
        {
            if (selectAction == null || cancelAction == null) return;

            if (selectAction.WasPerformedThisFrame())
            {
                ConfirmSell();
            }
            else if (cancelAction.WasPerformedThisFrame())
            {
                CancelSell();
            }
        }

        private void CacheInputActions()
        {
            if (uiController == null) return;
            navigateAction = uiController.GetNavigateAction();
            selectAction = uiController.GetSelectAction();
            cancelAction = uiController.GetCancelAction();
        }

        // For testing
        public void InjectDependencies(CollectorNPCUIController controller, CollectorNPCManager manager)
        {
            uiController = controller;
            npcManager = manager;
        }
    }

    /// <summary>
    /// States within the Sell Panel.
    /// </summary>
    public enum SellPanelState
    {
        ChipList,
        QuantitySelect,
        Confirmation,
        Result
    }

    /// <summary>
    /// Display info for a chip in the sell list.
    /// </summary>
    public class SellChipInfo
    {
        public string ChipId { get; private set; }
        public ChipRarity Rarity { get; private set; }
        public int OwnedCount { get; private set; }
        public int UnitPrice { get; private set; }

        public SellChipInfo(string chipId, ChipRarity rarity, int ownedCount, int unitPrice)
        {
            ChipId = chipId;
            Rarity = rarity;
            OwnedCount = ownedCount;
            UnitPrice = unitPrice;
        }
    }
}
