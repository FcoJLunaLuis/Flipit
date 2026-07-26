using UnityEditor;
using UnityEngine;
using Flipit.DailyCycle;

namespace Flipit.DailyCycle.Editor
{
    /// <summary>
    /// Custom Inspector for Daily_Cycle_Manager that displays runtime state.
    /// Shows Current Day, Last Event Name, and Transition State as read-only fields.
    /// </summary>
    [CustomEditor(typeof(Daily_Cycle_Manager))]
    public class Daily_Cycle_Manager_Editor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw default inspector fields
            DrawDefaultInspector();

            // Separator
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime State", EditorStyles.boldLabel);

            var manager = (Daily_Cycle_Manager)target;

            // Display read-only runtime fields
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField("Current Day", manager.CurrentDay);
            EditorGUILayout.TextField("Last Event", manager.LastEventName);
            EditorGUILayout.Toggle("Is Transitioning", manager.IsTransitioning);
            EditorGUI.EndDisabledGroup();

            // Auto-repaint during play mode to show live values
            if (Application.isPlaying)
            {
                Repaint();
            }
        }
    }
}
