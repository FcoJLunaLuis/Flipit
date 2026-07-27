using UnityEngine;
using Flipit.Dialogue;

namespace Flipit.DailyCycle
{
    /// <summary>
    /// Extends NPC_Interactable. After dialogue completes naturally (all lines shown),
    /// triggers the end-of-day transition via Daily_Cycle_Manager.
    /// Cancelled dialogue (player presses Cancel before all lines are shown) does NOT trigger end-of-day.
    /// 
    /// Supports two interaction paths:
    /// 1. Direct call to Interact() (if invoked by custom code)
    /// 2. Player_Interactor detection — detects when dialogue starts while this NPC is the target
    /// </summary>
    public class Mother_NPC : NPC_Interactable
    {
        private bool _waitingForDialogueEnd = false;
        private DialogueState _previousState = DialogueState.Idle;

        /// <summary>
        /// Initiates dialogue with the Mother NPC and begins listening for completion.
        /// Called if interaction goes through NPC_Interactable.Interact() path.
        /// </summary>
        public override void Interact()
        {
            base.Interact();
            _waitingForDialogueEnd = true;
            _previousState = DialogueState.Typing;
        }

        private void Update()
        {
            if (Dialogue_Manager.Instance == null)
                return;

            var currentState = Dialogue_Manager.Instance.CurrentState;

            // Detect when dialogue starts while we are the Player_Interactor's current target.
            // This handles the case where Player_Interactor calls StartDialogue directly
            // without going through our Interact() override.
            if (!_waitingForDialogueEnd && _previousState == DialogueState.Idle && currentState != DialogueState.Idle)
            {
                var interactor = FindObjectOfType<Player_Interactor>();
                if (interactor != null && interactor.CurrentTarget == this)
                {
                    _waitingForDialogueEnd = true;
                }
            }

            // Detect transition TO Idle from any non-Idle state (dialogue ended)
            if (_waitingForDialogueEnd && currentState == DialogueState.Idle && _previousState != DialogueState.Idle)
            {
                OnDialogueFinished();
            }

            _previousState = currentState;
        }

        /// <summary>
        /// Called when the dialogue session ends (transitions to Idle).
        /// Determines if it was a natural completion or cancellation, and triggers
        /// EndCurrentDay only on natural completion.
        /// </summary>
        private void OnDialogueFinished()
        {
            _waitingForDialogueEnd = false;

            // Distinguish natural completion from cancellation:
            // Natural completion: AdvanceLine() incremented CurrentLineIndex past the last line
            // (CurrentLineIndex >= Lines.Count), then called CloseDialogue().
            // Cancellation: HandleCancel called CloseDialogue() directly, so CurrentLineIndex
            // is still less than Lines.Count.
            int currentLineIndex = Dialogue_Manager.Instance.CurrentLineIndex;
            int totalLines = DialogueData != null ? DialogueData.Lines.Count : 0;

            if (totalLines > 0 && currentLineIndex >= totalLines)
            {
                // Natural completion — all lines were shown
                if (Daily_Cycle_Manager.Instance == null)
                {
                    Debug.LogWarning("[Mother_NPC] Daily_Cycle_Manager.Instance is null. Cannot end current day.");
                    return;
                }

                Daily_Cycle_Manager.Instance.EndCurrentDay();
            }
        }
    }
}
