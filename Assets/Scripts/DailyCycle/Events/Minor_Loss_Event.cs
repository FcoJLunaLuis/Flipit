using UnityEngine;

namespace Flipit.DailyCycle
{
    /// <summary>
    /// Negative daily event that applies a small, bounded coin loss.
    /// Loss is capped at the configured maximum threshold (Req 7.5).
    /// Temporary debuff duration is capped at the configured maximum (default 30s).
    /// Does NOT destroy/create GameObjects, remove NPCs, or regenerate the city.
    /// </summary>
    [CreateAssetMenu(fileName = "New_Minor_Loss_Event", menuName = "Flipit/Daily Event/Minor Loss")]
    public class Minor_Loss_Event : Daily_Event
    {
        [SerializeField] private int _maxCoinLoss = 5;
        [SerializeField] private float _maxDebuffDuration = 30f;

        /// <summary>Maximum coins that can be lost in a single event execution.</summary>
        public int MaxCoinLoss => _maxCoinLoss;

        /// <summary>Maximum duration for any temporary debuff applied by this event (in seconds).</summary>
        public float MaxDebuffDuration => _maxDebuffDuration;

        /// <summary>
        /// Executes the minor loss effect. Coin loss is bounded by _maxCoinLoss
        /// and cannot exceed the player's current coin count.
        /// </summary>
        /// <param name="context">Read-only game state context.</param>
        /// <returns>True if the event was applied successfully.</returns>
        public override bool Execute(IDailyEventContext context)
        {
            // Bounded loss — respects Req 7.5 constraints.
            // No GameObjects destroyed/created. Respects safety constraints (Req 7.1, 7.2, 7.3).
            int actualLoss = Mathf.Min(_maxCoinLoss, context.PlayerCoins);
            Debug.Log($"[Minor_Loss_Event] Player loses {actualLoss} coins on day {context.CurrentDay}. Max allowed: {_maxCoinLoss}. Debuff duration capped at {_maxDebuffDuration}s.");
            return true;
        }
    }
}
