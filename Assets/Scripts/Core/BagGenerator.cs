using System;

namespace Flipit.Core
{
    public class BagGenerator
    {
        private readonly Random random;

        public BagGenerator(Random random = null)
        {
            this.random = random ?? new Random();
        }

        public ChipResult[] GenerateBagContents(BagConfig config, PityTracker tracker, PityConfig pityConfig)
        {
            int chipCount = DetermineChipCount(config);
            var results = new ChipResult[chipCount];

            tracker.RegisterBagOpened();

            for (int i = 0; i < chipCount; i++)
            {
                ChipRarity rarity = DetermineRarity(config, tracker, pityConfig);
                results[i] = new ChipResult(rarity);
                tracker.RegisterResult(rarity);
            }

            return results;
        }

        private int DetermineChipCount(BagConfig config)
        {
            if (config.MinChips == config.MaxChips)
                return config.MinChips;

            if (config.Tier == BagTier.White)
                return DetermineWhiteBagChipCount(config);

            // For Green and Red bags: simple random between min and max
            return random.Next(config.MinChips, config.MaxChips + 1);
        }

        private int DetermineWhiteBagChipCount(BagConfig config)
        {
            // White bag special logic: ~20% chance (roughly 1 in 5) to get max chips
            float roll = (float)random.NextDouble();

            if (roll < 0.2f)
                return config.MaxChips;

            // Otherwise random between min and max-1
            return random.Next(config.MinChips, config.MaxChips);
        }

        private ChipRarity DetermineRarity(BagConfig config, PityTracker tracker, PityConfig pityConfig)
        {
            // Roll from rarest to most common (UltraRare first, then Rare, then Common)
            // This ensures rarer results are checked first

            RarityProbability ultraRareProb = config.GetRarityProbability(ChipRarity.UltraRare);
            if (ultraRareProb != null)
            {
                float adjustedProb = tracker.GetAdjustedProbability(
                    ChipRarity.UltraRare,
                    ultraRareProb.BaseProbability,
                    pityConfig);

                float roll = (float)random.NextDouble();
                if (roll < adjustedProb)
                    return ChipRarity.UltraRare;
            }

            RarityProbability rareProb = config.GetRarityProbability(ChipRarity.Rare);
            if (rareProb != null)
            {
                float adjustedProb = tracker.GetAdjustedProbability(
                    ChipRarity.Rare,
                    rareProb.BaseProbability,
                    pityConfig);

                float roll = (float)random.NextDouble();
                if (roll < adjustedProb)
                    return ChipRarity.Rare;
            }

            return ChipRarity.Common;
        }
    }
}