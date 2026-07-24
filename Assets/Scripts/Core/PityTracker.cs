using System;
using UnityEngine;

namespace Flipit.Core
{
    [Serializable]
    public class PityTracker
    {
        [SerializeField] private int bagsSinceLastRare;
        [SerializeField] private int bagsSinceLastUltraRare;
        [SerializeField] private int totalBagsOpened;
        [SerializeField] private int totalCommons;
        [SerializeField] private int totalRares;
        [SerializeField] private int totalUltraRares;

        public int BagsSinceLastRare => bagsSinceLastRare;
        public int BagsSinceLastUltraRare => bagsSinceLastUltraRare;
        public int TotalBagsOpened => totalBagsOpened;
        public int TotalCommons => totalCommons;
        public int TotalRares => totalRares;
        public int TotalUltraRares => totalUltraRares;

        public float GetAdjustedProbability(ChipRarity rarity, float baseProbability, PityConfig config)
        {
            if (config == null)
                return baseProbability;

            float bonus = 0f;

            switch (rarity)
            {
                case ChipRarity.Rare:
                    bonus = GetRareBonus(baseProbability, config);
                    break;
                case ChipRarity.UltraRare:
                    bonus = GetUltraRareBonus(baseProbability, config);
                    break;
                case ChipRarity.Common:
                    return baseProbability;
            }

            float frustrationBonus = GetFrustrationBonus(config);
            float totalProbability = baseProbability + bonus + frustrationBonus;

            return Mathf.Clamp01(totalProbability);
        }

        public void RegisterBagOpened()
        {
            totalBagsOpened++;
            bagsSinceLastRare++;
            bagsSinceLastUltraRare++;
        }

        public void RegisterResult(ChipRarity rarity)
        {
            switch (rarity)
            {
                case ChipRarity.Common:
                    totalCommons++;
                    break;
                case ChipRarity.Rare:
                    totalRares++;
                    bagsSinceLastRare = 0;
                    break;
                case ChipRarity.UltraRare:
                    totalUltraRares++;
                    bagsSinceLastUltraRare = 0;
                    bagsSinceLastRare = 0;
                    break;
            }
        }

        public void Reset()
        {
            bagsSinceLastRare = 0;
            bagsSinceLastUltraRare = 0;
            totalBagsOpened = 0;
            totalCommons = 0;
            totalRares = 0;
            totalUltraRares = 0;
        }

        private float GetRareBonus(float baseProbability, PityConfig config)
        {
            if (config.BagsUntilGuaranteedRare <= 0)
                return 0f;

            if (bagsSinceLastRare >= config.BagsUntilGuaranteedRare)
                return 1f - baseProbability;

            float progress = (float)bagsSinceLastRare / config.BagsUntilGuaranteedRare;
            return config.PityCurve.Evaluate(progress) * (1f - baseProbability);
        }

        private float GetUltraRareBonus(float baseProbability, PityConfig config)
        {
            if (config.BagsUntilGuaranteedUltraRare <= 0)
                return 0f;

            if (bagsSinceLastUltraRare >= config.BagsUntilGuaranteedUltraRare)
                return 1f - baseProbability;

            float progress = (float)bagsSinceLastUltraRare / config.BagsUntilGuaranteedUltraRare;
            return config.PityCurve.Evaluate(progress) * (1f - baseProbability);
        }

        private float GetFrustrationBonus(PityConfig config)
        {
            if (config.FrustrationThreshold <= 0f || totalBagsOpened == 0)
                return 0f;

            int totalChips = totalCommons + totalRares + totalUltraRares;
            if (totalChips == 0)
                return 0f;

            float commonRatio = (float)totalCommons / totalChips;

            if (commonRatio > config.FrustrationThreshold)
            {
                float frustrationIntensity = (commonRatio - config.FrustrationThreshold) / (1f - config.FrustrationThreshold);
                return frustrationIntensity * config.FrustrationBonusMax;
            }

            return 0f;
        }
    }
}