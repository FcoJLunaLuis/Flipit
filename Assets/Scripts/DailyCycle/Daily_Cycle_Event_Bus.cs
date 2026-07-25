using System;

namespace Flipit.DailyCycle
{
    /// <summary>
    /// Static event service for daily cycle lifecycle events.
    /// External systems subscribe without referencing Daily_Cycle_Manager internals.
    /// </summary>
    public static class Daily_Cycle_Event_Bus
    {
        /// <summary>
        /// Fired when a new day begins. Parameter is the new day number.
        /// </summary>
        public static event Action<int> OnDayStarted;

        /// <summary>
        /// Fired when the current day is ending (before transition begins).
        /// Parameter is the current day number.
        /// </summary>
        public static event Action<int> OnDayEnding;

        /// <summary>
        /// Fired when a daily event is triggered. Parameter is the event title.
        /// </summary>
        public static event Action<string> OnEventTriggered;

        /// <summary>
        /// Fired when the day transition is fully complete.
        /// Parameter is the completed day number.
        /// </summary>
        public static event Action<int> OnDayEnded;

        /// <summary>
        /// Fired when the player has been respawned at the School spawn point.
        /// </summary>
        public static event Action OnPlayerRespawned;

        /// <summary>
        /// Broadcasts that a new day has started.
        /// </summary>
        /// <param name="day">The new day number.</param>
        public static void BroadcastDayStarted(int day) => OnDayStarted?.Invoke(day);

        /// <summary>
        /// Broadcasts that the current day is ending.
        /// </summary>
        /// <param name="day">The current day number.</param>
        public static void BroadcastDayEnding(int day) => OnDayEnding?.Invoke(day);

        /// <summary>
        /// Broadcasts that a daily event has been triggered.
        /// </summary>
        /// <param name="title">The title of the triggered event.</param>
        public static void BroadcastEventTriggered(string title) => OnEventTriggered?.Invoke(title);

        /// <summary>
        /// Broadcasts that the day transition has fully completed.
        /// </summary>
        /// <param name="day">The completed day number.</param>
        public static void BroadcastDayEnded(int day) => OnDayEnded?.Invoke(day);

        /// <summary>
        /// Broadcasts that the player has been respawned.
        /// </summary>
        public static void BroadcastPlayerRespawned() => OnPlayerRespawned?.Invoke();
    }
}
