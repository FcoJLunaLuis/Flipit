using System;

namespace Flipit.Core
{
    [Serializable]
    public class ChipResult
    {
        public ChipRarity Rarity;
        public string ChipId;

        public ChipResult(ChipRarity rarity, string chipId = "")
        {
            Rarity = rarity;
            ChipId = chipId;
        }
    }
}