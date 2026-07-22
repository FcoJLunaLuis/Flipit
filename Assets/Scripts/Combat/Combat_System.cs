using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Flipit.Combat
{
    /// <summary>
    /// Singleton central del sistema de combate. Coordina entre NPCs de combate,
    /// el Dialogue_Manager, transiciones de escena y el event bus.
    /// Follows the same singleton pattern as Dialogue_Manager.
    /// </summary>
    public class Combat_System : MonoBehaviour
    {
        public static Combat_System Instance { get; private set; }

        [SerializeField] private Combat_Transition_UI _transitionUI;
        [SerializeField] private PlayerInput _playerInput;

        [Header("Forced Encounter UI")]
        [SerializeField] private GameObject _forcedEncounterPanel;
        [SerializeField] private Button _forcedAcceptButton;
        [SerializeField] private Flipit.Dialogue.DialogueData _forcedEncounterDialogueData;

        private RandomEncounter_NPC _activeEncounter;
        private Coroutine _autoAcceptCoroutine;

        /// <summary>
        /// The current state of the combat state machine. Initialized to Idle.
        /// </summary>
        public CombatState CurrentState { get; private set; } = CombatState.Idle;

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
        /// Invoked by dialogue option callback when the player accepts combat.
        /// Publishes "combat_accepted" event, starts transition overlay, and sets state to Transitioning.
        /// </summary>
        /// <param name="combatSceneName">The name of the combat scene to load.</param>
        public void OnCombatAccepted(string combatSceneName)
        {
            if (CurrentState == CombatState.Transitioning)
            {
                // Already transitioning (duplicate call), ignore silently
                return;
            }

            if (CurrentState != CombatState.DialogueOpen && CurrentState != CombatState.ForcedEncounter)
            {
                Debug.LogWarning($"[Combat_System] OnCombatAccepted called in invalid state: {CurrentState}");
                return;
            }

            CurrentState = CombatState.Transitioning;

            Combat_Event_Bus.RaiseEvent("combat_accepted");

            DisablePlayerInput();

            Combat_Event_Bus.RaiseEvent("combat_transition_started");

            if (_transitionUI != null)
            {
                _transitionUI.ShowTransition(
                    combatSceneName,
                    onComplete: () =>
                    {
                        // Scene loaded successfully — reset state
                        CurrentState = CombatState.Idle;
                    },
                    onError: () =>
                    {
                        // Scene load failed — restore player state
                        Debug.LogError($"[Combat_System] Failed to load combat scene: {combatSceneName}");
                        RestorePlayerInput();
                        CurrentState = CombatState.Idle;
                    }
                );
            }
            else
            {
                Debug.LogError("[Combat_System] _transitionUI is not assigned. Cannot show transition.");
                RestorePlayerInput();
                CurrentState = CombatState.Idle;
            }
        }

        /// <summary>
        /// Invoked by dialogue option callback when the player rejects combat.
        /// Restores PlayerInput to "Player" action map and sets state to Idle.
        /// Publishes "combat_dialogue_ended" event.
        /// </summary>
        public void OnCombatRejected()
        {
            if (CurrentState != CombatState.DialogueOpen)
            {
                Debug.LogWarning($"[Combat_System] OnCombatRejected called in invalid state: {CurrentState}");
                return;
            }

            RestorePlayerInput();
            CurrentState = CombatState.Idle;

            Combat_Event_Bus.RaiseEvent("combat_dialogue_ended");
        }

        /// <summary>
        /// Invoked by Random_Encounter_Manager for forced encounters.
        /// Disables player input and starts a forced dialogue with only the accept option.
        /// Uses the same Dialogue_Manager flow as voluntary combat for consistency.
        /// </summary>
        /// <param name="encounter">The RandomEncounter_NPC that triggered the forced encounter.</param>
        public void ForceCombatEncounter(RandomEncounter_NPC encounter)
        {
            if (CurrentState != CombatState.Idle)
            {
                Debug.LogWarning($"[Combat_System] ForceCombatEncounter called in invalid state: {CurrentState}");
                return;
            }

            CurrentState = CombatState.ForcedEncounter;
            _activeEncounter = encounter;

            DisablePlayerInput();

            Combat_Event_Bus.RaiseEvent("random_encounter_spawned");

            // Use the dialogue system to show the forced encounter message
            if (Flipit.Dialogue.Dialogue_Manager.Instance != null && _forcedEncounterDialogueData != null)
            {
                // Register with the handler so it can intercept the accept marker
                if (CombatDialogue_Handler.Instance != null)
                {
                    CombatDialogue_Handler.Instance.RegisterForcedEncounter(
                        encounter.CombatSceneName, _forcedEncounterDialogueData);
                }

                Flipit.Dialogue.Dialogue_Manager.Instance.StartDialogue(_forcedEncounterDialogueData);
            }
            else
            {
                // Fallback: show the UI panel approach
                ShowForcedEncounterPanel();
            }

            _autoAcceptCoroutine = StartCoroutine(AutoAcceptCoroutine(encounter.AutoAcceptTimeout));
        }

        /// <summary>
        /// Public method wired to the forced accept button. Cancels the timeout coroutine
        /// and invokes OnCombatAccepted with the active encounter's scene name.
        /// </summary>
        public void AcceptForcedEncounter()
        {
            if (CurrentState != CombatState.ForcedEncounter || _activeEncounter == null)
            {
                Debug.LogWarning("[Combat_System] AcceptForcedEncounter called in invalid state or without active encounter.");
                return;
            }

            CancelAutoAcceptCoroutine();
            HideForcedEncounterPanel();

            string sceneName = _activeEncounter.CombatSceneName;
            CleanUpActiveEncounter();

            OnCombatAccepted(sceneName);
        }

        private void ShowForcedEncounterPanel()
        {
            if (_forcedEncounterPanel != null)
            {
                _forcedEncounterPanel.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[Combat_System] _forcedEncounterPanel is not assigned.");
            }

            if (_forcedAcceptButton != null)
            {
                _forcedAcceptButton.onClick.AddListener(AcceptForcedEncounter);
            }
            else
            {
                Debug.LogWarning("[Combat_System] _forcedAcceptButton is not assigned.");
            }
        }

        private void HideForcedEncounterPanel()
        {
            if (_forcedAcceptButton != null)
            {
                _forcedAcceptButton.onClick.RemoveListener(AcceptForcedEncounter);
            }

            if (_forcedEncounterPanel != null)
            {
                _forcedEncounterPanel.SetActive(false);
            }
        }

        private IEnumerator AutoAcceptCoroutine(float timeout)
        {
            yield return new WaitForSeconds(timeout);

            // Auto-accept after timeout
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
            if (_autoAcceptCoroutine != null)
            {
                StopCoroutine(_autoAcceptCoroutine);
                _autoAcceptCoroutine = null;
            }
        }

        private void CleanUpActiveEncounter()
        {
            if (_activeEncounter != null)
            {
                _activeEncounter.Deactivate();
                _activeEncounter = null;
            }
        }

        /// <summary>
        /// Transitions state to DialogueOpen when a combat dialogue is opened.
        /// Should be called when a FlipCombat_NPC dialogue starts.
        /// </summary>
        public void NotifyCombatDialogueOpened()
        {
            if (CurrentState != CombatState.Idle)
            {
                Debug.LogWarning($"[Combat_System] NotifyCombatDialogueOpened called in invalid state: {CurrentState}");
                return;
            }

            CurrentState = CombatState.DialogueOpen;
            DisablePlayerInput();

            Combat_Event_Bus.RaiseEvent("combat_dialogue_started");
        }

        /// <summary>
        /// Disables player movement by switching PlayerInput to "UI" action map.
        /// </summary>
        public void DisablePlayerInput()
        {
            if (_playerInput == null)
            {
                Debug.LogWarning("[Combat_System] _playerInput is not assigned. Cannot disable player input.");
                return;
            }

            _playerInput.SwitchCurrentActionMap("UI");
        }

        /// <summary>
        /// Restores player movement by switching PlayerInput to "Player" action map.
        /// </summary>
        public void RestorePlayerInput()
        {
            if (_playerInput == null)
            {
                Debug.LogWarning("[Combat_System] _playerInput is not assigned. Cannot restore player input.");
                return;
            }

            _playerInput.SwitchCurrentActionMap("Player");
        }
    }
}
