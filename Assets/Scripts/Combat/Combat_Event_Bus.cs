using System;
using System.Collections.Generic;

namespace Flipit.Combat
{
    /// <summary>
    /// A static event bus for the combat system.
    /// Maintains a set of raised combat event IDs using the same pattern as Dialogue_Event_Bus.
    /// Uses a HashSet for O(1) lookup and enforces set semantics (no duplicates).
    /// Fires notifications only on actual state changes.
    /// </summary>
    public static class Combat_Event_Bus
    {
        private static readonly HashSet<string> _activeEvents = new HashSet<string>();

        /// <summary>
        /// Fired when a combat event ID is newly raised (added to the set).
        /// Only fires on actual state change (not if already active).
        /// </summary>
        public static event Action<string> OnCombatEventRaised;

        /// <summary>
        /// Fired when a combat event ID is deactivated (removed from the set).
        /// Only fires on actual state change (not if already inactive).
        /// </summary>
        public static event Action<string> OnCombatEventDeactivated;

        /// <summary>
        /// Raises a combat event by adding it to the active events collection.
        /// Fires OnCombatEventRaised only if the event was not already active.
        /// Null or empty strings are ignored.
        /// </summary>
        /// <param name="eventId">The combat event ID to raise.</param>
        public static void RaiseEvent(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
                return;

            if (_activeEvents.Add(eventId))
            {
                OnCombatEventRaised?.Invoke(eventId);
            }
        }

        /// <summary>
        /// Deactivates a combat event by removing it from the active events collection.
        /// Fires OnCombatEventDeactivated only if the event was previously active.
        /// Null or empty strings are ignored.
        /// </summary>
        /// <param name="eventId">The combat event ID to deactivate.</param>
        public static void DeactivateEvent(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
                return;

            if (_activeEvents.Remove(eventId))
            {
                OnCombatEventDeactivated?.Invoke(eventId);
            }
        }

        /// <summary>
        /// Queries whether a combat event ID is currently active.
        /// Returns false for null or empty strings.
        /// </summary>
        /// <param name="eventId">The combat event ID to check.</param>
        /// <returns>True if the event ID is in the active set, false otherwise.</returns>
        public static bool IsEventActive(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
                return false;

            return _activeEvents.Contains(eventId);
        }

        /// <summary>
        /// Clears all active combat events from the collection.
        /// Does not fire individual OnCombatEventDeactivated notifications.
        /// </summary>
        public static void ClearAll()
        {
            _activeEvents.Clear();
        }
    }
}
