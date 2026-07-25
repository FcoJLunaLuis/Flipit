namespace Flipit.DailyCycle
{
    /// <summary>
    /// Serializable snapshot of daily cycle state for future Save/Load integration.
    /// </summary>
    [System.Serializable]
    public struct Daily_Cycle_Save_Data
    {
        /// <summary>The current day number (starts at 1).</summary>
        public int DayCounter;

        /// <summary>The title of the last executed daily event.</summary>
        public string LastEventName;

        /// <summary>
        /// Creates a new save data snapshot.
        /// </summary>
        /// <param name="dayCounter">Current day number.</param>
        /// <param name="lastEventName">Name of the last executed event.</param>
        public Daily_Cycle_Save_Data(int dayCounter, string lastEventName)
        {
            DayCounter = dayCounter;
            LastEventName = lastEventName;
        }
    }
}
