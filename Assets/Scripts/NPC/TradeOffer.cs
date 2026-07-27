using System;
using Flipit.Core;

namespace Flipit.NPC
{
    /// <summary>
    /// Represents a trade offer from the NPC.
    /// The NPC offers one chip in exchange for the specified requirements.
    /// </summary>
    [Serializable]
    public class TradeOffer
    {
        public string OfferedChipId;
        public ChipRarity OfferedRarity;
        public TradeRequirement[] Requirements;

        public TradeOffer(string offeredChipId, ChipRarity offeredRarity, TradeRequirement[] requirements)
        {
            OfferedChipId = offeredChipId;
            OfferedRarity = offeredRarity;
            Requirements = requirements;
        }

        /// <summary>
        /// Checks if the player can afford this trade based on their inventory.
        /// </summary>
        public bool CanPlayerAfford(IChipInventory inventory)
        {
            if (Requirements == null || inventory == null)
                return false;

            for (int i = 0; i < Requirements.Length; i++)
            {
                int owned = inventory.GetChipCount(Requirements[i].RequiredChipId);
                if (owned < Requirements[i].RequiredAmount)
                    return false;
            }

            return true;
        }
    }
}
