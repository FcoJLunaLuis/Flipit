using UnityEngine;

namespace Flipit.Dialogue
{
    /// <summary>
    /// A serializable struct representing a selectable dialogue response option.
    /// Contains display text, an optional required event ID for filtering, and a target line index for branching.
    /// </summary>
    [System.Serializable]
    public struct DialogueOption
    {
        [SerializeField] private string displayText;
        [SerializeField] private string requiredEventId;
        [SerializeField, Min(0)] private int targetLineIndex;

        public string DisplayText => displayText;
        public string RequiredEventId => requiredEventId;
        public int TargetLineIndex => targetLineIndex;
        public bool HasRequiredEvent => !string.IsNullOrEmpty(requiredEventId);

        public DialogueOption(string displayText, string requiredEventId, int targetLineIndex)
        {
            this.displayText = displayText;
            this.requiredEventId = requiredEventId;
            this.targetLineIndex = targetLineIndex;
        }
    }
}
