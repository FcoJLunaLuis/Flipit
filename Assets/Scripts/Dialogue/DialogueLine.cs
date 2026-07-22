using System.Collections.Generic;
using UnityEngine;

namespace Flipit.Dialogue
{
    /// <summary>
    /// A serializable struct representing a single line of dialogue.
    /// Contains the display text and an optional list of dialogue options for branching.
    /// </summary>
    [System.Serializable]
    public struct DialogueLine
    {
        [SerializeField, TextArea(2, 5)] private string text;
        [SerializeField] private List<DialogueOption> options;

        public string Text => text;
        public IReadOnlyList<DialogueOption> Options => options ?? new List<DialogueOption>();
        public bool HasOptions => options != null && options.Count > 0;

        public DialogueLine(string text, List<DialogueOption> options = null)
        {
            this.text = text;
            this.options = options;
        }
    }
}
