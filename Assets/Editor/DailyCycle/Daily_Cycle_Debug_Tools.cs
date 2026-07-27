using UnityEditor;
using UnityEngine;
using Flipit.DailyCycle;

namespace Flipit.DailyCycle.Editor
{
    /// <summary>
    /// Editor menu items for testing daily cycle functions in isolation.
    /// Menu path: Flipit → Daily Cycle
    /// </summary>
    public static class Daily_Cycle_Debug_Tools
    {
        [MenuItem("Flipit/Daily Cycle/Force Next Day")]
        public static void ForceNextDay()
        {
            if (!TryGetManager(out var manager)) return;
            manager.NextDay();
            manager.RespawnPlayer();
            Debug.Log($"[Daily_Cycle_Debug] Forced next day. Now day {manager.GetCurrentDay()}.");
        }

        [MenuItem("Flipit/Daily Cycle/Trigger Random Event")]
        public static void TriggerRandomEvent()
        {
            if (!TryGetManager(out var manager)) return;
            manager.TriggerRandomEvent();
            Debug.Log($"[Daily_Cycle_Debug] Triggered random event: {manager.LastEventName}.");
        }

        [MenuItem("Flipit/Daily Cycle/Reset Day Counter")]
        public static void ResetDayCounter()
        {
            if (!TryGetManager(out var manager)) return;
            manager.SetDayCounterForDebug(1);
            Debug.Log("[Daily_Cycle_Debug] Day counter reset to 1.");
        }

        [MenuItem("Flipit/Daily Cycle/Go To Day...")]
        public static void GoToDay()
        {
            if (!TryGetManager(out _)) return;
            GoToDayWindow.ShowWindow();
        }

        private static bool TryGetManager(out Daily_Cycle_Manager manager)
        {
            manager = Daily_Cycle_Manager.Instance;
            if (manager == null)
            {
                manager = Object.FindObjectOfType<Daily_Cycle_Manager>();
            }
            if (manager == null)
            {
                Debug.LogWarning("[Daily_Cycle_Debug] No Daily_Cycle_Manager instance found in the active scene.");
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Small editor window for the "Go To Day..." dialog.
    /// Accepts an integer between 1 and 9999.
    /// </summary>
    public class GoToDayWindow : EditorWindow
    {
        private int _targetDay = 1;

        public static void ShowWindow()
        {
            var window = GetWindow<GoToDayWindow>(true, "Go To Day");
            window.minSize = new Vector2(250, 80);
            window.maxSize = new Vector2(250, 80);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            _targetDay = EditorGUILayout.IntField("Target Day:", _targetDay);
            _targetDay = Mathf.Clamp(_targetDay, 1, 9999);

            EditorGUILayout.Space(4);
            if (GUILayout.Button("Go"))
            {
                var manager = Daily_Cycle_Manager.Instance;
                if (manager == null)
                    manager = FindObjectOfType<Daily_Cycle_Manager>();

                if (manager != null)
                {
                    manager.SetDayCounterForDebug(_targetDay);
                    Debug.Log($"[Daily_Cycle_Debug] Day counter set to {_targetDay}.");
                }
                else
                {
                    Debug.LogWarning("[Daily_Cycle_Debug] No Daily_Cycle_Manager instance found.");
                }
                Close();
            }
        }
    }
}
