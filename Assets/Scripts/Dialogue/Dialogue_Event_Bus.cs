using System;
using System.Collections.Generic;

namespace Flipit.Dialogue
{
    /// <summary>
    /// A static event tracking service that maintains a set of raised game event IDs.
    /// Uses a HashSet for O(1) lookup and enforces set semantics (no duplicates).
    /// Fires notifications only on actual state changes.
    /// </summary>
    public static class Dialogue_Event_Bus
    {
        private static readonly HashSet<string> _activeEvents = new HashSet<string>();

        /// <summary>
        /// Fired when an event ID is newly activated (added to the set).
        /// Only fires on actual state change (not if already active).
        /// </summary>
        public static event Action<string> OnEventActivated;

        /// <summary>
        /// Fired when an event ID is deactivated (removed from the set).
        /// Only fires on actual state change (not if already inactive).
        /// </summary>
        public static event Action<string> OnEventDeactivated;

        /// <summary>
        /// Activates an event ID by adding it to the raised events collection.
        /// Fires OnEventActivated only if the event was not already active.
        /// Null or empty strings are ignored.
        /// </summary>
        /// <param name="eventId">The event ID to activate.</param>
        public static void ActivateEvent(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
                return;

            if (_activeEvents.Add(eventId))
            {
                OnEventActivated?.Invoke(eventId);
            }
        }

        /// <summary>
        /// Deactivates an event ID by removing it from the raised events collection.
        /// Fires OnEventDeactivated only if the event was previously active.
        /// Null or empty strings are ignored.
        /// </summary>
        /// <param name="eventId">The event ID to deactivate.</param>
        public static void DeactivateEvent(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
                return;

            if (_activeEvents.Remove(eventId))
            {
                OnEventDeactivated?.Invoke(eventId);
            }
        }

        /// <summary>
        /// Queries whether an event ID is currently active in the raised events collection.
        /// Returns false for null or empty strings.
        /// </summary>
        /// <param name="eventId">The event ID to check.</param>
        /// <returns>True if the event ID is in the active set, false otherwise.</returns>
        public static bool IsEventActive(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
                return false;

            return _activeEvents.Contains(eventId);
        }

        /// <summary>
        /// Clears all raised events from the collection.
        /// Does not fire individual OnEventDeactivated notifications.
        /// </summary>
        public static void ClearAll()
        {
            _activeEvents.Clear();
        }
    }
}
