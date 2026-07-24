using System;
using UnityEngine;

namespace Flipit.Core
{
    [Serializable]
    public class RarityProbability
    {
        [SerializeField] private ChipRarity rarity;
        [SerializeField, Range(0f, 1f)] private float baseProbability;
        [SerializeField, Range(0f, 0.1f)] private float pityBonusPerMiss;
        [SerializeField, Range(0f, 1f)] private float maxProbability;

        public ChipRarity Rarity => rarity;
        public float BaseProbability => baseProbability;
        public float PityBonusPerMiss => pityBonusPerMiss;
        public float MaxProbability => maxProbability;

        public RarityProbability() { }

        public RarityProbability(ChipRarity rarity, float baseProbability, float pityBonusPerMiss, float maxProbability)
        {
            this.rarity = rarity;
            this.baseProbability = baseProbability;
            this.pityBonusPerMiss = pityBonusPerMiss;
            this.maxProbability = maxProbability;
        }
    }
}