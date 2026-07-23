using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Flipit.Dialogue;

namespace Flipit.Combat
{
    public class Combat_System : MonoBehaviour
    {
        public static Combat_System Instance { get; private set; }

        [SerializeField] private Combat_Transition_UI _transitionUI;
        [SerializeField] private PlayerInput _playerInput;

        [Header("Forced Encounter UI")]
        [SerializeField] private GameObject _forcedEncounterPanel;
        [SerializeField] private Button _forcedAcceptButton;
        [SerializeField] private DialogueData _forcedEncounterDialogueData;

        private RandomEncounter_NPC _activeEncounter;
        private Coroutine _autoAcceptCoroutine;

        public CombatState CurrentState { get; private set; } = CombatState.Idle;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void OnCombatAccepted(string combatSceneName)
        {
            if (CurrentState == CombatState.Transitioning) return;

            if (CurrentState != CombatState.DialogueOpen && CurrentState != CombatState.ForcedEncounter)
            {
                Debug.LogWarning($"[Combat_System] OnCombatAccepted invalid state: {CurrentState}");
                return;
            }

            CurrentState = CombatState.Transitioning;
            Combat_Event_Bus.RaiseEvent("combat_accepted");
            DisablePlayerInput();
            Combat_Event_Bus.RaiseEvent("combat_transition_started");

            if (_transitionUI != null)
            {
                _transitionUI.ShowTransition(combatSceneName,
                    onComplete: () => { CurrentState = CombatState.Idle; },
                    onError: () =>
                    {
                        RestorePlayerInput();
                        CurrentState = CombatState.Idle;
                    });
            }
            else
            {
                RestorePlayerInput();
                CurrentState = CombatState.Idle;
            }
        }

        public void OnCombatRejected()
        {
            if (CurrentState != CombatState.DialogueOpen) return;
            RestorePlayerInput();
            CurrentState = CombatState.Idle;
            Combat_Event_Bus.RaiseEvent("combat_dialogue_ended");
        }

        public void ForceCombatEncounter(RandomEncounter_NPC encounter)
        {
            if (CurrentState != CombatState.Idle) return;
            CurrentState = CombatState.ForcedEncounter;
            _activeEncounter = encounter;
            DisablePlayerInput();
            Combat_Event_Bus.RaiseEvent("random_encounter_spawned");

            if (Dialogue_Manager.Instance != null && _forcedEncounterDialogueData != null)
            {
                if (CombatDialogue_Handler.Instance != null)
                    CombatDialogue_Handler.Instance.RegisterForcedEncounter(
                        encounter.CombatSceneName, _forcedEncounterDialogueData);
                Dialogue_Manager.Instance.StartDialogue(_forcedEncounterDialogueData);
            }
            else
            {
                ShowForcedEncounterPanel();
            }
            _autoAcceptCoroutine = StartCoroutine(AutoAcceptCoroutine(encounter.AutoAcceptTimeout));
        }

        public void AcceptForcedEncounter()
        {
            if (CurrentState != CombatState.ForcedEncounter || _activeEncounter == null) return;
            CancelAutoAcceptCoroutine();
            HideForcedEncounterPanel();
            string sceneName = _activeEncounter.CombatSceneName;
            CleanUpActiveEncounter();
            OnCombatAccepted(sceneName);
        }

        public void NotifyCombatDialogueOpened()
        {
            if (CurrentState != CombatState.Idle) return;
            CurrentState = CombatState.DialogueOpen;
            DisablePlayerInput();
            Combat_Event_Bus.RaiseEvent("combat_dialogue_started");
        }

        public void DisablePlayerInput()
        {
            if (_playerInput != null) _playerInput.SwitchCurrentActionMap("UI");
        }

        public void RestorePlayerInput()
        {
            if (_playerInput != null) _playerInput.SwitchCurrentActionMap("Player");
        }

        private void ShowForcedEncounterPanel()
        {
            if (_forcedEncounterPanel != null) _forcedEncounterPanel.SetActive(true);
            if (_forcedAcceptButton != null) _forcedAcceptButton.onClick.AddListener(AcceptForcedEncounter);
        }

        private void HideForcedEncounterPanel()
        {
            if (_forcedAcceptButton != null) _forcedAcceptButton.onClick.RemoveListener(AcceptForcedEncounter);
            if (_forcedEncounterPanel != null) _forcedEncounterPanel.SetActive(false);
        }

        private IEnumerator AutoAcceptCoroutine(float timeout)
        {
            yield return new WaitForSeconds(timeout);
            if (CurrentState == CombatState.ForcedEncounter && _activeEncounter != null)
            {
                HideForcedEncounterPanel();
                string sceneName = _activeEncounter.CombatSceneName;
                CleanUpActiveEncounter();
                OnCombatAccepted(sceneName);
            }
            _autoAcceptCoroutine = null;
        }

        private void CancelAutoAcceptCoroutine()
        {
            if (_autoAcceptCoroutine != null) { StopCoroutine(_autoAcceptCoroutine); _autoAcceptCoroutine = null; }
        }

        private void CleanUpActiveEncounter()
        {
            if (_activeEncounter != null) { _activeEncounter.Deactivate(); _activeEncounter = null; }
        }
    }
}
