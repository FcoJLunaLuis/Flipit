using System;

namespace Flipit.NPC
{
    /// <summary>
    /// Represents a single chip requirement for a trade.
    /// The player must provide this many chips of the specified type.
    /// </summary>
    [Serializable]
    public class TradeRequirement
    {
        public string RequiredChipId;
        public int RequiredAmount;

        public TradeRequirement(string requiredChipId, int requiredAmount)
        {
            RequiredChipId = requiredChipId;
            RequiredAmount = requiredAmount;
        }
    }
}
