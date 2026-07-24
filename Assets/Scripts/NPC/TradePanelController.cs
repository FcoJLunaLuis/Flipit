using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Flipit.Core;

namespace Flipit.NPC
{
    /// <summary>
    /// Controls the Trade Panel UI within the Collector NPC interface.
    /// Displays the NPC's trade offers and handles trade confirmation.
    /// </summary>
    public class TradePanelController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CollectorNPCUIController uiController;
        [SerializeField] private CollectorNPCManager npcManager;

        // Internal state
        private TradePanelState panelState = TradePanelState.OfferList;
        private int selectedOfferIndex;
        private bool navigationConsumed;

        // Cached input actions
        private InputAction navigateAction;
        private InputAction selectAction;
        private InputAction cancelAction;

        // Public accessors
        public TradePanelState PanelState => panelState;
        public int SelectedOfferIndex => selectedOfferIndex;

        public int OfferCount
        {
            get
            {
                var offers = npcManager?.CurrentOffers;
                return offers != null ? offers.Length : 0;
            }
        }

        public TradeOffer SelectedOffer
        {
            get
            {
                var offers = npcManager?.CurrentOffers;
                if (offers == null || selectedOfferIndex < 0 || selectedOfferIndex >= offers.Length)
                    return null;
                return offers[selectedOfferIndex];
            }
        }

        public bool CanAffordSelectedOffer
        {
            get
            {
                if (npcManager == null || SelectedOffer == null)
                    return false;
                return npcManager.CanAffordTrade(SelectedOffer);
            }
        }

        // Events
        public event Action<int> OnOfferCursorMoved;
        public event Action<TradeOffer> OnOfferSelected; // when player selects an offer to view details
        public event Action<TradeOffer> OnTradeConfirmationRequested;
        public event Action<TradeResult> OnTradeExecuted;
        public event Action OnPanelClosed;

        private void OnEnable()
        {
            if (uiController != null)
            {
                uiController.OnTradePanelOpened += HandlePanelOpened;
            }
        }

        private void OnDisable()
        {
            if (uiController != null)
            {
                uiController.OnTradePanelOpened -= HandlePanelOpened;
            }
        }

        private void Update()
        {
            if (uiController == null || uiController.CurrentState != NPCUIState.TradePanel)
            {
                if (uiController != null && uiController.CurrentState == NPCUIState.Confirmation
                    && panelState == TradePanelState.Confirmation)
                {
                    HandleConfirmationInput();
                }
                return;
            }

            switch (panelState)
            {
                case TradePanelState.OfferList:
                    HandleOfferListInput();
                    break;
            }
        }

        /// <summary>
        /// Gets trade offer info for display at a specific index.
        /// </summary>
        public TradeOfferDisplayInfo GetOfferDisplayInfo(int index)
        {
            var offers = npcManager?.CurrentOffers;
            if (offers == null || index < 0 || index >= offers.Length)
                return null;

            var offer = offers[index];
            bool canAfford = npcManager.CanAffordTrade(offer);

            return new TradeOfferDisplayInfo(offer, canAfford);
        }

        /// <summary>
        /// Confirms the trade. Called when player confirms in the confirmation dialog.
        /// </summary>
        public void ConfirmTrade()
        {
            if (npcManager == null || SelectedOffer == null)
                return;

            var result = npcManager.TryTrade(SelectedOffer);
            OnTradeExecuted?.Invoke(result);

            if (result.Success)
            {
                uiController.HideConfirmation(NPCUIState.TradePanel);
                uiController.ShowResult();
                panelState = TradePanelState.Result;
            }
            else
            {
                uiController.HideConfirmation(NPCUIState.TradePanel);
                panelState = TradePanelState.OfferList;
            }
        }

        /// <summary>
        /// Cancels the trade and returns to offer list.
        /// </summary>
        public void CancelTrade()
        {
            uiController.HideConfirmation(NPCUIState.TradePanel);
            panelState = TradePanelState.OfferList;
        }

        /// <summary>
        /// Returns from result screen back to offer list.
        /// </summary>
        public void DismissResult()
        {
            uiController.HideResult(NPCUIState.TradePanel);
            panelState = TradePanelState.OfferList;
        }

        private void HandlePanelOpened()
        {
            CacheInputActions();
            selectedOfferIndex = 0;
            panelState = TradePanelState.OfferList;
            OnOfferCursorMoved?.Invoke(selectedOfferIndex);
        }

        private void HandleOfferListInput()
        {
            HandleOfferListNavigation();
            HandleOfferListSelect();
            HandleOfferListCancel();
        }

        private void HandleOfferListNavigation()
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

            int count = OfferCount;
            if (count == 0) return;

            if (input.y > 0.5f)
                selectedOfferIndex = (selectedOfferIndex - 1 + count) % count;
            else if (input.y < -0.5f)
                selectedOfferIndex = (selectedOfferIndex + 1) % count;

            OnOfferCursorMoved?.Invoke(selectedOfferIndex);
        }

        private void HandleOfferListSelect()
        {
            if (selectAction == null) return;
            if (!selectAction.WasPerformedThisFrame()) return;

            if (OfferCount == 0 || SelectedOffer == null) return;

            if (!CanAffordSelectedOffer)
            {
                // Can't afford — notify but don't proceed
                OnOfferSelected?.Invoke(SelectedOffer);
                return;
            }

            // Show confirmation
            panelState = TradePanelState.Confirmation;
            uiController.ShowConfirmation();
            OnTradeConfirmationRequested?.Invoke(SelectedOffer);
        }

        private void HandleOfferListCancel()
        {
            if (cancelAction == null) return;
            if (!cancelAction.WasPerformedThisFrame()) return;

            panelState = TradePanelState.OfferList;
            uiController.ReturnToMainMenu();
            OnPanelClosed?.Invoke();
        }

        private void HandleConfirmationInput()
        {
            if (selectAction == null || cancelAction == null) return;

            if (selectAction.WasPerformedThisFrame())
            {
                ConfirmTrade();
            }
            else if (cancelAction.WasPerformedThisFrame())
            {
                CancelTrade();
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
    /// States within the Trade Panel.
    /// </summary>
    public enum TradePanelState
    {
        OfferList,
        Confirmation,
        Result
    }

    /// <summary>
    /// Display info for a trade offer.
    /// </summary>
    public class TradeOfferDisplayInfo
    {
        public TradeOffer Offer { get; private set; }
        public bool CanAfford { get; private set; }

        public string OfferedChipId => Offer.OfferedChipId;
        public ChipRarity OfferedRarity => Offer.OfferedRarity;
        public TradeRequirement[] Requirements => Offer.Requirements;

        public TradeOfferDisplayInfo(TradeOffer offer, bool canAfford)
        {
            Offer = offer;
            CanAfford = canAfford;
        }
    }
}
