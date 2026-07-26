using UnityEngine;

namespace Flipit.DailyCycle
{
    /// <summary>
    /// Neutral daily event that provides flavor text with no mechanical impact.
    /// Does NOT destroy/create GameObjects, remove NPCs, or regenerate the city.
    /// </summary>
    [CreateAssetMenu(fileName = "New_Neutral_Flavor_Event", menuName = "Flipit/Daily Event/Neutral Flavor")]
    public class Neutral_Flavor_Event : Daily_Event
    {
        /// <summary>
        /// Executes the neutral flavor effect. No mechanical changes — purely immersive.
        /// </summary>
        /// <param name="context">Read-only game state context.</param>
        /// <returns>True always; neutral events cannot fail.</returns>
        public override bool Execute(IDailyEventContext context)
        {
            // Flavor event — no mechanical impact. Just logs for immersion placeholder.
            // No GameObjects destroyed/created. Respects safety constraints (Req 7.1, 7.2, 7.3).
            Debug.Log($"[Neutral_Flavor_Event] A calm day in the city. Day {context.CurrentDay}.");
            return true;
        }
    }
}
