using UnityEngine;
using Flipit.Core;

namespace Flipit.Shop
{
    public class ShopManager : MonoBehaviour
    {
        private const int BagsPerTier = 5;

        [Header("Configuration")]
        [SerializeField] private BagConfig[] bagConfigs;
        [SerializeField] private WalletData wallet;
        [SerializeField] private PityConfig pityConfig;

        [Header("State")]
        [SerializeField] private PityTracker pityTracker = new PityTracker();

        private BagGenerator bagGenerator;
        private ChipResult[][] preGeneratedBags;

        public int TierCount => bagConfigs != null ? bagConfigs.Length : 0;
        public PityTracker PityTracker => pityTracker;

        private void Awake()
        {
            bagGenerator = new BagGenerator();
        }

        public void InitializeShop()
        {
            bagGenerator = new BagGenerator();
            PreGenerateBags();
        }

        public void PreGenerateBags()
        {
            int totalBags = TierCount * BagsPerTier;
            preGeneratedBags = new ChipResult[totalBags][];

            for (int tier = 0; tier < TierCount; tier++)
            {
                for (int bag = 0; bag < BagsPerTier; bag++)
                {
                    int index = GetBagIndex(tier, bag);
                    preGeneratedBags[index] = bagGenerator.GenerateBagContents(
                        bagConfigs[tier], pityTracker, pityConfig);
                }
            }
        }

        public PurchaseResult TryPurchase(int tierIndex, int bagIndex)
        {
            if (bagConfigs == null || tierIndex < 0 || tierIndex >= TierCount)
                return PurchaseResult.Failed("Tipo de bolsa invalido");

            if (bagIndex < 0 || bagIndex >= BagsPerTier)
                return PurchaseResult.Failed("Indice de bolsa invalido");

            var config = bagConfigs[tierIndex];

            if (!wallet.CanAfford(config.CostSheintavos, config.CostPejecoins, config.CostAjolopesos))
                return PurchaseResult.Failed("No tienes suficiente dinero");

            if (!wallet.TrySpend(config.CostSheintavos, config.CostPejecoins, config.CostAjolopesos))
                return PurchaseResult.Failed("Error al procesar el pago");

            int index = GetBagIndex(tierIndex, bagIndex);
            ChipResult[] chips = preGeneratedBags[index];

            // Regenerate this bag slot
            preGeneratedBags[index] = bagGenerator.GenerateBagContents(
                bagConfigs[tierIndex], pityTracker, pityConfig);

            return PurchaseResult.Succeeded(chips);
        }

        public BagConfig GetBagConfig(int tierIndex)
        {
            if (bagConfigs == null || tierIndex < 0 || tierIndex >= TierCount)
                return null;
            return bagConfigs[tierIndex];
        }

        public bool CanAffordBag(int tierIndex)
        {
            var config = GetBagConfig(tierIndex);
            if (config == null || wallet == null) return false;
            return wallet.CanAfford(config.CostSheintavos, config.CostPejecoins, config.CostAjolopesos);
        }

        public int GetBagsPerTier()
        {
            return BagsPerTier;
        }

        private int GetBagIndex(int tierIndex, int bagIndex)
        {
            return tierIndex * BagsPerTier + bagIndex;
        }

        // For testing: inject dependencies
        public void InjectDependencies(BagConfig[] configs, WalletData walletData,
            PityConfig pityConfiguration, BagGenerator generator = null)
        {
            bagConfigs = configs;
            wallet = walletData;
            pityConfig = pityConfiguration;
            bagGenerator = generator ?? new BagGenerator();
            pityTracker = new PityTracker();
        }
    }
}