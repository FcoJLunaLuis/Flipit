using System;
using UnityEngine;
using Flipit.Core;

namespace Flipit.NPC
{
    /// <summary>
    /// Defines the sell price range for a specific chip rarity.
    /// The actual sell price will be randomized within this range each cycle.
    /// </summary>
    [Serializable]
    public class SellPriceRange
    {
        [SerializeField] private ChipRarity rarity;
        [SerializeField, Min(1)] private int minPrice = 1;
        [SerializeField, Min(1)] private int maxPrice = 10;

        public ChipRarity Rarity => rarity;
        public int MinPrice => minPrice;
        public int MaxPrice => maxPrice;

        public SellPriceRange() { }

        public SellPriceRange(ChipRarity rarity, int minPrice, int maxPrice)
        {
            this.rarity = rarity;
            this.minPrice = minPrice;
            this.maxPrice = maxPrice;
        }
    }
}
