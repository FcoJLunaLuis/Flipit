using System;
using System.Collections.Generic;

namespace Flipit.Combat
{
    public static class Combat_Event_Bus
    {
        private static readonly HashSet<string> _activeEvents = new HashSet<string>();

        public static event Action<string> OnCombatEventRaised;
        public static event Action<string> OnCombatEventDeactivated;

        public static void RaiseEvent(string eventId)
        {
            if (string.IsNullOrEmpty(eventId)) return;
            if (_activeEvents.Add(eventId))
                OnCombatEventRaised?.Invoke(eventId);
        }

        public static void DeactivateEvent(string eventId)
        {
            if (string.IsNullOrEmpty(eventId)) return;
            if (_activeEvents.Remove(eventId))
                OnCombatEventDeactivated?.Invoke(eventId);
        }

        public static bool IsEventActive(string eventId)
        {
            if (string.IsNullOrEmpty(eventId)) return false;
            return _activeEvents.Contains(eventId);
        }

        public static void ClearAll() => _activeEvents.Clear();
    }
}
