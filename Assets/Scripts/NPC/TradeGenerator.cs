using System;
using System.Collections.Generic;
using Flipit.Core;

namespace Flipit.NPC
{
    /// <summary>
    /// Generates random trade offers for the Collector NPC.
    /// Ensures at least one offer is a chip the player doesn't own.
    /// Requirements are based on chips the player actually has.
    /// </summary>
    public class TradeGenerator
    {
        private readonly Random random;

        public TradeGenerator(Random random = null)
        {
            this.random = random ?? new Random();
        }

        /// <summary>
        /// Generates trade offers based on config, catalog, and player inventory.
        /// Guarantees at least one offer for a chip the player doesn't own.
        /// </summary>
        public TradeOffer[] GenerateOffers(CollectorNPCConfig config, IChipCatalog catalog, IChipInventory inventory)
        {
            if (config == null || catalog == null || inventory == null)
                return new TradeOffer[0];

            int offerCount = config.TradeOfferCount;
            var allChipIds = catalog.GetAllChipIds();

            if (allChipIds.Count == 0)
                return new TradeOffer[0];

            // Determine which chips the player doesn't own
            var unownedChips = GetUnownedChips(catalog, inventory);
            var ownedChips = inventory.GetOwnedChipIds();

            // Can't generate trades if player has no chips to trade with
            if (ownedChips.Count == 0)
                return new TradeOffer[0];

            var offers = new List<TradeOffer>();
            var usedChipIds = new HashSet<string>();

            // First: guarantee at least one unowned chip offer
            if (unownedChips.Count > 0)
            {
                string unownedChipId = unownedChips[random.Next(unownedChips.Count)];
                var offer = CreateOffer(unownedChipId, catalog, inventory, config, usedChipIds);
                if (offer != null)
                {
                    offers.Add(offer);
                    usedChipIds.Add(unownedChipId);
                }
            }

            // Fill remaining slots with random chips from the full catalog
            int attempts = 0;
            int maxAttempts = offerCount * 10;

            while (offers.Count < offerCount && attempts < maxAttempts)
            {
                attempts++;
                string chipId = allChipIds[random.Next(allChipIds.Count)];

                if (usedChipIds.Contains(chipId))
                    continue;

                var offer = CreateOffer(chipId, catalog, inventory, config, usedChipIds);
                if (offer != null)
                {
                    offers.Add(offer);
                    usedChipIds.Add(chipId);
                }
            }

            return offers.ToArray();
        }

        private TradeOffer CreateOffer(string offeredChipId, IChipCatalog catalog,
            IChipInventory inventory, CollectorNPCConfig config, HashSet<string> excludeFromRequirements)
        {
            ChipRarity rarity = catalog.GetChipRarity(offeredChipId);

            // Generate requirements from chips the player owns
            var requirements = GenerateRequirements(offeredChipId, inventory, config, excludeFromRequirements);

            if (requirements == null || requirements.Length == 0)
                return null;

            return new TradeOffer(offeredChipId, rarity, requirements);
        }

        private TradeRequirement[] GenerateRequirements(string offeredChipId, IChipInventory inventory,
            CollectorNPCConfig config, HashSet<string> excludeChipIds)
        {
            var ownedChips = inventory.GetOwnedChipIds();
            if (ownedChips.Count == 0)
                return null;

            // Filter out chips that shouldn't be used as requirements
            var availableForRequirement = new List<string>();
            for (int i = 0; i < ownedChips.Count; i++)
            {
                string chipId = ownedChips[i];
                // Don't require the chip being offered
                if (chipId == offeredChipId)
                    continue;

                if (inventory.GetChipCount(chipId) > 0)
                    availableForRequirement.Add(chipId);
            }

            if (availableForRequirement.Count == 0)
                return null;

            // Determine number of different chip types to require
            int minTypes = Math.Max(1, config.MinTradeRequirementTypes);
            int maxTypes = Math.Min(config.MaxTradeRequirementTypes, availableForRequirement.Count);

            if (minTypes > maxTypes)
                minTypes = maxTypes;

            int typeCount = random.Next(minTypes, maxTypes + 1);

            // Shuffle available chips and pick typeCount of them
            var selectedChips = PickRandom(availableForRequirement, typeCount);

            var requirements = new TradeRequirement[selectedChips.Count];
            for (int i = 0; i < selectedChips.Count; i++)
            {
                string chipId = selectedChips[i];
                int playerOwns = inventory.GetChipCount(chipId);
                // Random amount between 1 and what the player owns
                int amount = random.Next(1, playerOwns + 1);
                requirements[i] = new TradeRequirement(chipId, amount);
            }

            return requirements;
        }

        private List<string> PickRandom(List<string> source, int count)
        {
            var result = new List<string>();
            var available = new List<string>(source);

            count = Math.Min(count, available.Count);

            for (int i = 0; i < count; i++)
            {
                int index = random.Next(available.Count);
                result.Add(available[index]);
                available.RemoveAt(index);
            }

            return result;
        }

        private List<string> GetUnownedChips(IChipCatalog catalog, IChipInventory inventory)
        {
            var allChips = catalog.GetAllChipIds();
            var unowned = new List<string>();

            for (int i = 0; i < allChips.Count; i++)
            {
                if (!inventory.HasChip(allChips[i]))
                    unowned.Add(allChips[i]);
            }

            return unowned;
        }
    }
}
