namespace Flipit.DailyCycle
{
    /// <summary>
    /// Categorizes daily events by their impact on the player.
    /// Used for weighted random selection during end-of-day transitions.
    /// </summary>
    public enum Daily_Event_Category
    {
        /// <summary>Beneficial events that reward the player.</summary>
        Positive,

        /// <summary>Flavor events with no mechanical impact.</summary>
        Neutral,

        /// <summary>Minor setback events with bounded losses.</summary>
        Negative
    }
}