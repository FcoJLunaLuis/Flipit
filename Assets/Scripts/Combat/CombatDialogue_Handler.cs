using UnityEngine;
using Flipit.Dialogue;

namespace Flipit.Combat
{
    /// <summary>
    /// Monitors the Dialogue_Manager for combat marker lines ([COMBAT_ACCEPT] / [COMBAT_REJECT]).
    /// When detected, intercepts before the typewriter displays the text and triggers
    /// the appropriate Combat_System method. Also publishes combat dialogue events
    /// through the Dialogue_Event_Bus.
    /// </summary>
    public class CombatDialogue_Handler : MonoBehaviour
    {
        public static CombatDialogue_Handler Instance { get; private set; }

        private const string CombatAcceptMarker = "[COMBAT_ACCEPT]";
        private const string CombatRejectMarker = "[COMBAT_REJECT]";

        /// <summary>
        /// The FlipCombat_NPC currently engaged in dialogue. Set by the NPC before
        /// starting dialogue so we can retrieve the CombatSceneName.
        /// </summary>
        public FlipCombat_NPC ActiveCombatNPC { get; private set; }

        private int _lastCheckedLineIndex = -1;
        private bool _monitoring;
        private string _forcedSceneName;
        private Flipit.Dialogue.DialogueData _forcedDialogueData;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Called by FlipCombat_NPC.Interact() before starting dialogue.
        /// Registers the NPC so the handler knows which scene to load on accept.
        /// </summary>
        /// <param name="npc">The FlipCombat_NPC initiating the combat dialogue.</param>
        public void RegisterCombatNPC(FlipCombat_NPC npc)
        {
            ActiveCombatNPC = npc;
            _forcedSceneName = null;
            _lastCheckedLineIndex = -1;
            _monitoring = true;

            // Notify Combat_System that a combat dialogue is opening
            if (Combat_System.Instance != null &&
                Combat_System.Instance.CurrentState == CombatState.Idle)
            {
                Combat_System.Instance.NotifyCombatDialogueOpened();
            }

            // Publish combat dialogue started event via Dialogue_Event_Bus
            Dialogue_Event_Bus.ActivateEvent("combat_dialogue_started");
        }

        /// <summary>
        /// Called by Combat_System or TriggerZone_Encounter for forced encounters that use the dialogue system.
        /// Registers a scene name without a FlipCombat_NPC reference.
        /// </summary>
        public void RegisterForcedEncounter(string combatSceneName, Flipit.Dialogue.DialogueData dialogueData)
        {
            ActiveCombatNPC = null;
            _forcedSceneName = combatSceneName;
            _forcedDialogueData = dialogueData;
            _lastCheckedLineIndex = -1;
            _monitoring = true;

            // Notify Combat_System that a combat dialogue is opening
            if (Combat_System.Instance != null &&
                Combat_System.Instance.CurrentState == CombatState.Idle)
            {
                Combat_System.Instance.NotifyCombatDialogueOpened();
            }

            Dialogue_Event_Bus.ActivateEvent("combat_dialogue_started");
        }

        /// <summary>
        /// Clears the active combat NPC registration and stops monitoring.
        /// </summary>
        public void UnregisterCombatNPC()
        {
            ActiveCombatNPC = null;
            _forcedSceneName = null;
            _forcedDialogueData = null;
            _monitoring = false;
            _lastCheckedLineIndex = -1;
        }

        private void Update()
        {
            if (!_monitoring)
                return;

            if (Dialogue_Manager.Instance == null)
                return;

            // Don't interfere if combat is already transitioning
            if (Combat_System.Instance != null &&
                Combat_System.Instance.CurrentState == CombatState.Transitioning)
            {
                _monitoring = false;
                ActiveCombatNPC = null;
                return;
            }

            // Only check when the Dialogue_Manager is actively in a typing or waiting state
            var state = Dialogue_Manager.Instance.CurrentState;
            if (state == DialogueState.Idle || state == DialogueState.Closing)
            {
                // Dialogue closed without hitting a marker — clean up
                if (ActiveCombatNPC != null && state == DialogueState.Idle)
                {
                    HandleDialogueEndedWithoutMarker();
                }
                return;
            }

            int currentIndex = Dialogue_Manager.Instance.CurrentLineIndex;

            // Only process each line index once
            if (currentIndex == _lastCheckedLineIndex)
                return;

            _lastCheckedLineIndex = currentIndex;

            // Check if the current line text starts with a combat marker
            CheckCurrentLineForMarker();
        }

        private void CheckCurrentLineForMarker()
        {
            if (Dialogue_Manager.Instance == null)
                return;

            // Determine which DialogueData to use
            DialogueData dialogueData = null;
            if (ActiveCombatNPC != null && ActiveCombatNPC.HasValidDialogue)
            {
                dialogueData = ActiveCombatNPC.DialogueData;
            }
            else if (_forcedDialogueData != null)
            {
                dialogueData = _forcedDialogueData;
            }

            if (dialogueData == null)
                return;

            int lineIndex = Dialogue_Manager.Instance.CurrentLineIndex;

            if (lineIndex < 0 || lineIndex >= dialogueData.Lines.Count)
                return;

            string lineText = dialogueData.Lines[lineIndex].Text;

            if (string.IsNullOrEmpty(lineText))
                return;

            if (lineText.StartsWith(CombatAcceptMarker))
            {
                InterceptCombatAccept();
            }
            else if (lineText.StartsWith(CombatRejectMarker))
            {
                InterceptCombatReject();
            }
        }

        private void InterceptCombatAccept()
        {
            _monitoring = false;

            // Stop the typewriter from displaying the marker text
            StopTypewriter();

            // Get scene name from either the NPC or the forced encounter
            string sceneName = "";
            if (ActiveCombatNPC != null)
            {
                sceneName = ActiveCombatNPC.CombatSceneName;
            }
            else if (!string.IsNullOrEmpty(_forcedSceneName))
            {
                sceneName = _forcedSceneName;
            }

            ActiveCombatNPC = null;
            _forcedSceneName = null;
            _forcedDialogueData = null;

            if (Combat_System.Instance != null && !string.IsNullOrEmpty(sceneName))
            {
                Combat_System.Instance.OnCombatAccepted(sceneName);
                CloseDialogueSilently();
            }
            else
            {
                Debug.LogError("[CombatDialogue_Handler] Cannot accept combat: Combat_System or scene name is missing.");
                CloseDialogueSilently();
            }

            Dialogue_Event_Bus.ActivateEvent("combat_dialogue_ended");
        }

        private void InterceptCombatReject()
        {
            _monitoring = false;

            // Stop the typewriter from displaying the marker text
            StopTypewriter();

            // Close dialogue normally
            CloseDialogueSilently();

            var npc = ActiveCombatNPC;
            ActiveCombatNPC = null;

            // Trigger combat rejection
            if (Combat_System.Instance != null)
            {
                Combat_System.Instance.OnCombatRejected();
            }

            // Publish combat dialogue ended event
            Dialogue_Event_Bus.ActivateEvent("combat_dialogue_ended");
        }

        private void HandleDialogueEndedWithoutMarker()
        {
            _monitoring = false;
            ActiveCombatNPC = null;
            _forcedSceneName = null;
            _forcedDialogueData = null;

            // Still notify Combat_System about rejection (dialogue closed = reject)
            if (Combat_System.Instance != null &&
                Combat_System.Instance.CurrentState == CombatState.DialogueOpen)
            {
                Combat_System.Instance.OnCombatRejected();
            }

            Dialogue_Event_Bus.ActivateEvent("combat_dialogue_ended");
        }

        /// <summary>
        /// Stops the Typewriter_Effect to prevent it from displaying marker text.
        /// </summary>
        private void StopTypewriter()
        {
            // Find the Typewriter_Effect on the Dialogue_Manager's GameObject or its children
            if (Dialogue_Manager.Instance != null)
            {
                var typewriter = Dialogue_Manager.Instance.GetComponentInChildren<Typewriter_Effect>();
                if (typewriter != null)
                {
                    typewriter.Stop();
                }
            }
        }

        /// <summary>
        /// Closes the dialogue through the Dialogue_Manager without the normal flow.
        /// </summary>
        private void CloseDialogueSilently()
        {
            if (Dialogue_Manager.Instance != null &&
                Dialogue_Manager.Instance.CurrentState != DialogueState.Idle &&
                Dialogue_Manager.Instance.CurrentState != DialogueState.Closing)
            {
                Dialogue_Manager.Instance.CloseDialogue();
            }
        }
    }
}
