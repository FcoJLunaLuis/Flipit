using UnityEngine;

namespace Flipit.Core
{
    [CreateAssetMenu(fileName = "PityConfig", menuName = "Flipit/Shop/Pity Config")]
    public class PityConfig : ScriptableObject
    {
        [Header("Guaranteed Rarity Thresholds")]
        [Tooltip("Number of bags without a Rare chip before guaranteeing one")]
        [SerializeField, Min(0)] private int bagsUntilGuaranteedRare = 10;

        [Tooltip("Number of bags without an Ultra Rare chip before guaranteeing one")]
        [SerializeField, Min(0)] private int bagsUntilGuaranteedUltraRare = 50;

        [Header("Frustration Protection")]
        [Tooltip("If the ratio of Common chips exceeds this threshold, bonus probability is applied")]
        [SerializeField, Range(0f, 1f)] private float frustrationThreshold = 0.85f;

        [Tooltip("Maximum bonus probability applied when player is frustrated")]
        [SerializeField, Range(0f, 0.5f)] private float frustrationBonusMax = 0.15f;

        [Header("Pity Curve")]
        [Tooltip("Curve that defines how probability increases as bags without target rarity grow. X: progress (0=just got one, 1=at guarantee threshold). Y: bonus multiplier (0=no bonus, 1=full bonus)")]
        [SerializeField] private AnimationCurve pityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public int BagsUntilGuaranteedRare => bagsUntilGuaranteedRare;
        public int BagsUntilGuaranteedUltraRare => bagsUntilGuaranteedUltraRare;
        public float FrustrationThreshold => frustrationThreshold;
        public float FrustrationBonusMax => frustrationBonusMax;
        public AnimationCurve PityCurve => pityCurve;
    }
}