namespace Flipit.DailyCycle
{
    /// <summary>
    /// Read-only context passed to Daily_Event.Execute().
    /// Decouples events from manager internals.
    /// </summary>
    public interface IDailyEventContext
    {
        /// <summary>The current in-game day number (1-based).</summary>
        int CurrentDay { get; }

        /// <summary>The player's current coin count.</summary>
        int PlayerCoins { get; }

        /// <summary>The player's current Flipit count.</summary>
        int PlayerFlipits { get; }
    }
}