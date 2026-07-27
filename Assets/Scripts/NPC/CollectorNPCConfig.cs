using UnityEngine;
using Flipit.Core;

namespace Flipit.NPC
{
    /// <summary>
    /// Configuration asset for the Collector NPC.
    /// Defines sell prices, trade offer generation rules, and limits.
    /// </summary>
    [CreateAssetMenu(fileName = "CollectorNPCConfig", menuName = "Flipit/NPC/Collector Config")]
    public class CollectorNPCConfig : ScriptableObject
    {
        [Header("Sell Prices (in Sheintavos)")]
        [Tooltip("Price range per rarity. The actual price is randomized each cycle.")]
        [SerializeField] private SellPriceRange[] sellPriceRanges;

        [Header("Trade Configuration")]
        [Tooltip("Number of trade offers the NPC shows at once.")]
        [SerializeField, Min(1)] private int tradeOfferCount = 5;

        [Tooltip("Maximum number of different chip types required per trade offer.")]
        [SerializeField, Min(1)] private int maxTradeRequirementTypes = 3;

        [Tooltip("Minimum number of different chip types required per trade offer.")]
        [SerializeField, Min(1)] private int minTradeRequirementTypes = 1;

        // Public accessors
        public SellPriceRange[] SellPriceRanges => sellPriceRanges;
        public int TradeOfferCount => tradeOfferCount;
        public int MaxTradeRequirementTypes => maxTradeRequirementTypes;
        public int MinTradeRequirementTypes => minTradeRequirementTypes;

        /// <summary>
        /// Returns the sell price range for a specific rarity, or null if not configured.
        /// </summary>
        public SellPriceRange GetSellPriceRange(ChipRarity rarity)
        {
            if (sellPriceRanges == null) return null;

            for (int i = 0; i < sellPriceRanges.Length; i++)
            {
                if (sellPriceRanges[i].Rarity == rarity)
                    return sellPriceRanges[i];
            }

            return null;
        }
    }
}
