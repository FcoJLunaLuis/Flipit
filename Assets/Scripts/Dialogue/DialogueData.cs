using System.Collections.Generic;
using UnityEngine;

namespace Flipit.Dialogue
{
    /// <summary>
    /// A ScriptableObject asset containing dialogue content for a single conversation.
    /// Stores a speaker name and an ordered list of dialogue lines.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "Flipit/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        [SerializeField] private string speakerName;
        [SerializeField] private List<DialogueLine> lines = new();

        public string SpeakerName => speakerName;
        public IReadOnlyList<DialogueLine> Lines => lines;

        /// <summary>
        /// Creates a DialogueData instance populated with the given data. Useful for testing.
        /// </summary>
        public static DialogueData Create(string speakerName, List<DialogueLine> lines)
        {
            var data = ScriptableObject.CreateInstance<DialogueData>();
            data.speakerName = speakerName;
            data.lines = lines ?? new List<DialogueLine>();
            return data;
        }
    }
}
