using System;
using UnityEngine;
using Flipit.Core;

namespace Flipit.NPC
{
    /// <summary>
    /// Orchestrates the Collector NPC business logic: selling chips and trading.
    /// Manages cycle prices and trade offers.
    /// </summary>
    public class CollectorNPCManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private CollectorNPCConfig config;
        [SerializeField] private WalletData wallet;

        // Dependencies (injected or set via inspector adapter)
        private IChipInventory inventory;
        private IChipCatalog catalog;

        // Internal systems
        private SellPriceCalculator priceCalculator;
        private TradeGenerator tradeGenerator;

        // Cycle state
        private int[] cyclePrices; // indexed by ChipRarity enum
        private TradeOffer[] currentOffers;

        // Public accessors
        public TradeOffer[] CurrentOffers => currentOffers;
        public CollectorNPCConfig Config => config;

        public event Action OnOffersRefreshed;
        public event Action<SellResult> OnSellCompleted;
        public event Action<TradeResult> OnTradeCompleted;

        private void Awake()
        {
            priceCalculator = new SellPriceCalculator();
            tradeGenerator = new TradeGenerator();
        }

        /// <summary>
        /// Initializes the NPC with its dependencies. Call after Awake.
        /// </summary>
        public void Initialize(IChipInventory chipInventory, IChipCatalog chipCatalog)
        {
            inventory = chipInventory;
            catalog = chipCatalog;
            RefreshCyclePrices();
            RefreshOffers();
        }

        /// <summary>
        /// Refreshes sell prices for the current cycle.
        /// Call when a new game cycle begins.
        /// </summary>
        public void RefreshCyclePrices()
        {
            int rarityCount = Enum.GetValues(typeof(ChipRarity)).Length;
            cyclePrices = new int[rarityCount];

            foreach (ChipRarity rarity in Enum.GetValues(typeof(ChipRarity)))
            {
                cyclePrices[(int)rarity] = priceCalculator.GenerateCyclePrice(rarity, config);
            }
        }

        /// <summary>
        /// Refreshes trade offers. Call when a new cycle begins.
        /// </summary>
        public void RefreshOffers()
        {
            if (catalog == null || inventory == null || config == null)
            {
                currentOffers = new TradeOffer[0];
                return;
            }

            currentOffers = tradeGenerator.GenerateOffers(config, catalog, inventory);
            OnOffersRefreshed?.Invoke();
        }

        /// <summary>
        /// Gets the current cycle sell price for a chip rarity (in Sheintavos).
        /// </summary>
        public int GetCycleSellPrice(ChipRarity rarity)
        {
            if (cyclePrices == null || (int)rarity >= cyclePrices.Length)
                return 0;

            return cyclePrices[(int)rarity];
        }

        /// <summary>
        /// Attempts to sell chips to the NPC.
        /// </summary>
        public SellResult TrySell(string chipId, int quantity)
        {
            if (inventory == null)
                return SellResult.Failed("Sistema de inventario no disponible");

            if (wallet == null)
                return SellResult.Failed("Wallet no configurado");

            if (config == null)
                return SellResult.Failed("Configuracion del NPC no disponible");

            if (string.IsNullOrEmpty(chipId))
                return SellResult.Failed("Ficha invalida");

            if (quantity <= 0)
                return SellResult.Failed("Cantidad invalida");

            int owned = inventory.GetChipCount(chipId);
            if (owned < quantity)
                return SellResult.Failed("No tienes suficientes fichas de este tipo");

            // Calculate price using cycle price
            ChipRarity rarity = catalog != null ? catalog.GetChipRarity(chipId) : ChipRarity.Common;
            int unitPrice = GetCycleSellPrice(rarity);
            int totalPrice = priceCalculator.CalculateBatchSellPrice(rarity, quantity, unitPrice);

            if (totalPrice <= 0)
                return SellResult.Failed("No se pudo calcular el precio de venta");

            // Execute: remove chips and add money
            if (!inventory.RemoveChips(chipId, quantity))
                return SellResult.Failed("Error al remover fichas del inventario");

            wallet.Add(totalPrice, 0, 0);

            var result = SellResult.Succeeded(chipId, quantity, totalPrice);
            OnSellCompleted?.Invoke(result);
            return result;
        }

        /// <summary>
        /// Attempts to execute a trade with the NPC.
        /// </summary>
        public TradeResult TryTrade(TradeOffer offer)
        {
            if (inventory == null)
                return TradeResult.Failed("Sistema de inventario no disponible");

            if (offer == null)
                return TradeResult.Failed("Oferta invalida");

            if (offer.Requirements == null || offer.Requirements.Length == 0)
                return TradeResult.Failed("La oferta no tiene requisitos");

            // Verify the player can afford the trade
            if (!offer.CanPlayerAfford(inventory))
                return TradeResult.Failed("No tienes las fichas necesarias para este intercambio");

            // Execute: remove required chips
            for (int i = 0; i < offer.Requirements.Length; i++)
            {
                var req = offer.Requirements[i];
                if (!inventory.RemoveChips(req.RequiredChipId, req.RequiredAmount))
                {
                    // This shouldn't happen since we checked CanPlayerAfford,
                    // but handle gracefully
                    return TradeResult.Failed("Error al procesar el intercambio");
                }
            }

            // Add the offered chip
            inventory.AddChips(offer.OfferedChipId, 1);

            var result = TradeResult.Succeeded(offer.OfferedChipId);
            OnTradeCompleted?.Invoke(result);
            return result;
        }

        /// <summary>
        /// Checks if the player can afford a specific trade offer.
        /// </summary>
        public bool CanAffordTrade(TradeOffer offer)
        {
            if (offer == null || inventory == null)
                return false;

            return offer.CanPlayerAfford(inventory);
        }

        /// <summary>
        /// Gets the inventory reference (for UI to query chip counts).
        /// </summary>
        public IChipInventory GetInventory()
        {
            return inventory;
        }

        /// <summary>
        /// Gets the catalog reference (for UI to query chip info).
        /// </summary>
        public IChipCatalog GetCatalog()
        {
            return catalog;
        }

        // For testing: inject all dependencies
        public void InjectDependencies(CollectorNPCConfig npcConfig, WalletData walletData,
            IChipInventory chipInventory, IChipCatalog chipCatalog,
            SellPriceCalculator calculator = null, TradeGenerator generator = null)
        {
            config = npcConfig;
            wallet = walletData;
            inventory = chipInventory;
            catalog = chipCatalog;
            priceCalculator = calculator ?? new SellPriceCalculator();
            tradeGenerator = generator ?? new TradeGenerator();
        }
    }
}
