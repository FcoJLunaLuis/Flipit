using System;
using Flipit.Core;

namespace Flipit.NPC
{
    /// <summary>
    /// Calculates sell prices for chips based on rarity and configuration.
    /// Prices are randomized within the configured range.
    /// Pure logic class with no Unity dependencies (except through config).
    /// </summary>
    public class SellPriceCalculator
    {
        private readonly Random random;

        public SellPriceCalculator(Random random = null)
        {
            this.random = random ?? new Random();
        }

        /// <summary>
        /// Calculates the sell price for a single chip of the given rarity.
        /// Returns a random value within the configured range.
        /// Returns 0 if the rarity is not configured.
        /// </summary>
        public int CalculateSellPrice(ChipRarity rarity, CollectorNPCConfig config)
        {
            if (config == null)
                return 0;

            SellPriceRange range = config.GetSellPriceRange(rarity);
            if (range == null)
                return 0;

            return random.Next(range.MinPrice, range.MaxPrice + 1);
        }

        /// <summary>
        /// Calculates the total sell price for multiple chips of the same rarity.
        /// Each individual chip gets its own randomized price within the range.
        /// Returns 0 if quantity is invalid or rarity not configured.
        /// </summary>
        public int CalculateTotalSellPrice(ChipRarity rarity, int quantity, CollectorNPCConfig config)
        {
            if (quantity <= 0 || config == null)
                return 0;

            SellPriceRange range = config.GetSellPriceRange(rarity);
            if (range == null)
                return 0;

            int total = 0;
            for (int i = 0; i < quantity; i++)
            {
                total += random.Next(range.MinPrice, range.MaxPrice + 1);
            }

            return total;
        }

        /// <summary>
        /// Calculates a fixed sell price for a batch (all chips at the same price).
        /// Useful for showing a preview price before confirming.
        /// Returns the unit price multiplied by quantity.
        /// </summary>
        public int CalculateBatchSellPrice(ChipRarity rarity, int quantity, int unitPrice)
        {
            if (quantity <= 0 || unitPrice <= 0)
                return 0;

            return unitPrice * quantity;
        }

        /// <summary>
        /// Generates a "cycle price" — a single randomized price for a rarity
        /// that stays fixed for the duration of a cycle.
        /// Call this once per cycle per rarity and store the result.
        /// </summary>
        public int GenerateCyclePrice(ChipRarity rarity, CollectorNPCConfig config)
        {
            return CalculateSellPrice(rarity, config);
        }
    }
}
