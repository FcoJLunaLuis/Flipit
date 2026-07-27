using UnityEngine;

namespace Flipit.Core
{
    [CreateAssetMenu(fileName = "BagConfig", menuName = "Flipit/Shop/Bag Config")]
    public class BagConfig : ScriptableObject
    {
        [Header("Bag Info")]
        [SerializeField] private string bagName;
        [SerializeField] private BagTier tier;

        [Header("Chip Count")]
        [SerializeField, Min(1)] private int minChips = 1;
        [SerializeField, Min(1)] private int maxChips = 2;

        [Header("Cost")]
        [SerializeField, Min(0)] private int costSheintavos;
        [SerializeField, Min(0)] private int costPejecoins;
        [SerializeField, Min(0)] private int costAjolopesos;

        [Header("Rarity Probabilities")]
        [SerializeField] private RarityProbability[] rarityWeights;

        public string BagName => bagName;
        public BagTier Tier => tier;
        public int MinChips => minChips;
        public int MaxChips => maxChips;
        public int CostSheintavos => costSheintavos;
        public int CostPejecoins => costPejecoins;
        public int CostAjolopesos => costAjolopesos;
        public RarityProbability[] RarityWeights => rarityWeights;





        public RarityProbability GetRarityProbability(ChipRarity rarity)
        {
            if (rarityWeights == null) return null;

            for (int i = 0; i < rarityWeights.Length; i++)
            {
                if (rarityWeights[i].Rarity == rarity)
                    return rarityWeights[i];
            }
            return null;
        }
    }
}