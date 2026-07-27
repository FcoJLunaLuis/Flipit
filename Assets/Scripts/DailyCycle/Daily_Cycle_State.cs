namespace Flipit.DailyCycle
{
    /// <summary>
    /// Represents the current state of the Daily Cycle transition sequence.
    /// </summary>
    public enum Daily_Cycle_State
    {
        /// <summary>No transition in progress. Normal gameplay.</summary>
        Idle,

        /// <summary>Combat is active; waiting for it to finish before transitioning.</summary>
        WaitingForCombat,

        /// <summary>End-of-day coroutine is running.</summary>
        Transitioning
    }
}
