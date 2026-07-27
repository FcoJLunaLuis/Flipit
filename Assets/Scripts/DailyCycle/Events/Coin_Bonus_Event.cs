using UnityEngine;

namespace Flipit.DailyCycle
{
    /// <summary>
    /// Positive daily event that grants bonus coins to the player.
    /// Does NOT destroy/create GameObjects, remove NPCs, or regenerate the city.
    /// </summary>
    [CreateAssetMenu(fileName = "New_Coin_Bonus_Event", menuName = "Flipit/Daily Event/Coin Bonus")]
    public class Coin_Bonus_Event : Daily_Event
    {
        [SerializeField] private int _bonusAmount = 10;

        /// <summary>The number of bonus coins awarded to the player.</summary>
        public int BonusAmount => _bonusAmount;

        /// <summary>
        /// Executes the coin bonus effect. In a full implementation this would
        /// add coins to the player's wallet. Currently logs the effect.
        /// </summary>
        /// <param name="context">Read-only game state context.</param>
        /// <returns>True if the event was applied successfully.</returns>
        public override bool Execute(IDailyEventContext context)
        {
            // In a full implementation, this would add coins to the player's wallet.
            // No GameObjects destroyed/created. Respects safety constraints (Req 7.1, 7.2, 7.3).
            Debug.Log($"[Coin_Bonus_Event] Player receives {_bonusAmount} bonus coins on day {context.CurrentDay}.");
            return true;
        }
    }
}
