using System.Collections.Generic;
using Flipit.Core;

namespace Flipit.NPC
{
    /// <summary>
    /// Mock implementation of IChipCatalog for testing.
    /// Contains a hardcoded set of chips across all rarities.
    /// Replace with real catalog after merge with album branch.
    /// </summary>
    public class MockChipCatalog : IChipCatalog
    {
        private readonly Dictionary<string, ChipRarity> chips = new Dictionary<string, ChipRarity>();
        private readonly Dictionary<ChipRarity, List<string>> chipsByRarity = new Dictionary<ChipRarity, List<string>>();
        private readonly List<string> allChipIds = new List<string>();

        public MockChipCatalog()
        {
            // Initialize rarity lists
            chipsByRarity[ChipRarity.Common] = new List<string>();
            chipsByRarity[ChipRarity.Rare] = new List<string>();
            chipsByRarity[ChipRarity.UltraRare] = new List<string>();

            // Populate with test data
            AddChip("common_aguila", ChipRarity.Common);
            AddChip("common_sol", ChipRarity.Common);
            AddChip("common_luna", ChipRarity.Common);
            AddChip("common_estrella", ChipRarity.Common);
            AddChip("common_nube", ChipRarity.Common);
            AddChip("common_flor", ChipRarity.Common);
            AddChip("common_rio", ChipRarity.Common);
            AddChip("common_monte", ChipRarity.Common);

            AddChip("rare_dragon", ChipRarity.Rare);
            AddChip("rare_fenix", ChipRarity.Rare);
            AddChip("rare_kraken", ChipRarity.Rare);
            AddChip("rare_quetzal", ChipRarity.Rare);
            AddChip("rare_jaguar", ChipRarity.Rare);

            AddChip("ultra_cosmico", ChipRarity.UltraRare);
            AddChip("ultra_dimensional", ChipRarity.UltraRare);
            AddChip("ultra_celestial", ChipRarity.UltraRare);
        }

        /// <summary>
        /// Constructor that allows custom chip data for unit testing.
        /// </summary>
        public MockChipCatalog(Dictionary<string, ChipRarity> customChips)
        {
            chipsByRarity[ChipRarity.Common] = new List<string>();
            chipsByRarity[ChipRarity.Rare] = new List<string>();
            chipsByRarity[ChipRarity.UltraRare] = new List<string>();

            foreach (var kvp in customChips)
            {
                AddChip(kvp.Key, kvp.Value);
            }
        }

        private void AddChip(string chipId, ChipRarity rarity)
        {
            chips[chipId] = rarity;
            chipsByRarity[rarity].Add(chipId);
            allChipIds.Add(chipId);
        }

        public IReadOnlyList<string> GetAllChipIds()
        {
            return allChipIds;
        }

        public ChipRarity GetChipRarity(string chipId)
        {
            if (chips.TryGetValue(chipId, out ChipRarity rarity))
                return rarity;

            return ChipRarity.Common; // Default fallback
        }

        public IReadOnlyList<string> GetChipsByRarity(ChipRarity rarity)
        {
            if (chipsByRarity.TryGetValue(rarity, out List<string> list))
                return list;

            return new List<string>();
        }

        public bool ChipExists(string chipId)
        {
            return chips.ContainsKey(chipId);
        }
    }
}
