using UnityEngine;

namespace Flipit.Dialogue
{
    /// <summary>
    /// A MonoBehaviour placed on NPC GameObjects that holds a reference to DialogueData
    /// and provides an interaction entry point for the Player_Interactor.
    /// </summary>
    public class NPC_Interactable : MonoBehaviour
    {
        [SerializeField] private DialogueData dialogueData;

        /// <summary>
        /// The dialogue data asset assigned to this NPC.
        /// </summary>
        public DialogueData DialogueData => dialogueData;

        /// <summary>
        /// Returns true if this NPC has valid dialogue data (non-null with at least one line).
        /// When false, the Dialogue_Manager shall not start a session (Req 2.5).
        /// </summary>
        public bool HasValidDialogue => dialogueData != null && dialogueData.Lines.Count > 0;

        /// <summary>
        /// Called by Player_Interactor when the player initiates interaction.
        /// Override in subclasses to add custom behavior before/after dialogue starts.
        /// Base implementation starts dialogue via Dialogue_Manager.
        /// </summary>
        public virtual void Interact()
        {
            if (Dialogue_Manager.Instance == null)
                return;

            if (Dialogue_Manager.Instance.CurrentState != DialogueState.Idle)
                return;

            Dialogue_Manager.Instance.StartDialogue(dialogueData);
        }
    }
}
