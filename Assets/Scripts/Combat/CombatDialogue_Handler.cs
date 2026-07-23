using UnityEngine;
using Flipit.Dialogue;

namespace Flipit.Combat
{
    public class CombatDialogue_Handler : MonoBehaviour
    {
        public static CombatDialogue_Handler Instance { get; private set; }

        private const string CombatAcceptMarker = "[COMBAT_ACCEPT]";
        private const string CombatRejectMarker = "[COMBAT_REJECT]";

        public FlipCombat_NPC ActiveCombatNPC { get; private set; }

        private int _lastCheckedLineIndex = -1;
        private bool _monitoring;
        private string _forcedSceneName;
        private DialogueData _forcedDialogueData;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void RegisterCombatNPC(FlipCombat_NPC npc)
        {
            ActiveCombatNPC = npc;
            _forcedSceneName = null;
            _forcedDialogueData = null;
            _lastCheckedLineIndex = -1;
            _monitoring = true;

            if (Combat_System.Instance != null && Combat_System.Instance.CurrentState == CombatState.Idle)
                Combat_System.Instance.NotifyCombatDialogueOpened();

            Dialogue_Event_Bus.ActivateEvent("combat_dialogue_started");
        }

        public void RegisterForcedEncounter(string sceneName, DialogueData dialogueData)
        {
            ActiveCombatNPC = null;
            _forcedSceneName = sceneName;
            _forcedDialogueData = dialogueData;
            _lastCheckedLineIndex = -1;
            _monitoring = true;

            if (Combat_System.Instance != null && Combat_System.Instance.CurrentState == CombatState.Idle)
                Combat_System.Instance.NotifyCombatDialogueOpened();

            Dialogue_Event_Bus.ActivateEvent("combat_dialogue_started");
        }

        private void Update()
        {
            if (!_monitoring) return;
            if (Dialogue_Manager.Instance == null) return;

            if (Combat_System.Instance != null && Combat_System.Instance.CurrentState == CombatState.Transitioning)
            {
                _monitoring = false;
                ActiveCombatNPC = null;
                _forcedSceneName = null;
                _forcedDialogueData = null;
                return;
            }

            var state = Dialogue_Manager.Instance.CurrentState;
            if (state == DialogueState.Idle || state == DialogueState.Closing)
            {
                if (state == DialogueState.Idle && (ActiveCombatNPC != null || _forcedSceneName != null))
                    HandleDialogueEndedWithoutMarker();
                return;
            }

            int currentIndex = Dialogue_Manager.Instance.CurrentLineIndex;
            if (currentIndex == _lastCheckedLineIndex) return;
            _lastCheckedLineIndex = currentIndex;
            CheckCurrentLineForMarker();
        }

        private void CheckCurrentLineForMarker()
        {
            DialogueData data = null;
            if (ActiveCombatNPC != null && ActiveCombatNPC.HasValidDialogue)
                data = ActiveCombatNPC.DialogueData;
            else if (_forcedDialogueData != null)
                data = _forcedDialogueData;
            if (data == null) return;

            int idx = Dialogue_Manager.Instance.CurrentLineIndex;
            if (idx < 0 || idx >= data.Lines.Count) return;

            string text = data.Lines[idx].Text;
            if (string.IsNullOrEmpty(text)) return;

            if (text.StartsWith(CombatAcceptMarker)) InterceptCombatAccept();
            else if (text.StartsWith(CombatRejectMarker)) InterceptCombatReject();
        }

        private void InterceptCombatAccept()
        {
            _monitoring = false;
            StopTypewriter();

            string sceneName = ActiveCombatNPC != null ? ActiveCombatNPC.CombatSceneName : _forcedSceneName ?? "";
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
                CloseDialogueSilently();
            }
            Dialogue_Event_Bus.ActivateEvent("combat_dialogue_ended");
        }

        private void InterceptCombatReject()
        {
            _monitoring = false;
            StopTypewriter();
            CloseDialogueSilently();
            ActiveCombatNPC = null;
            _forcedSceneName = null;
            _forcedDialogueData = null;
            if (Combat_System.Instance != null) Combat_System.Instance.OnCombatRejected();
            Dialogue_Event_Bus.ActivateEvent("combat_dialogue_ended");
        }

        private void HandleDialogueEndedWithoutMarker()
        {
            _monitoring = false;
            ActiveCombatNPC = null;
            _forcedSceneName = null;
            _forcedDialogueData = null;
            if (Combat_System.Instance != null && Combat_System.Instance.CurrentState == CombatState.DialogueOpen)
                Combat_System.Instance.OnCombatRejected();
            Dialogue_Event_Bus.ActivateEvent("combat_dialogue_ended");
        }

        private void StopTypewriter()
        {
            if (Dialogue_Manager.Instance != null)
            {
                var tw = Dialogue_Manager.Instance.GetComponentInChildren<Typewriter_Effect>();
                if (tw != null) tw.Stop();
            }
        }

        private void CloseDialogueSilently()
        {
            if (Dialogue_Manager.Instance != null &&
                Dialogue_Manager.Instance.CurrentState != DialogueState.Idle &&
                Dialogue_Manager.Instance.CurrentState != DialogueState.Closing)
                Dialogue_Manager.Instance.CloseDialogue();
        }
    }
}
