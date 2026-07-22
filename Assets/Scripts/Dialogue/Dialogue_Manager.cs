using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Flipit.Dialogue
{
    /// <summary>
    /// Central orchestrator for the dialogue system. Manages dialogue state transitions,
    /// coordinates between UI, input, and data subsystems.
    /// Implements a singleton pattern for global access.
    /// </summary>
    public class Dialogue_Manager : MonoBehaviour
    {
        public static Dialogue_Manager Instance { get; private set; }

        [SerializeField] private MonoBehaviour dialogueUIComponent; // Must implement IDialogueUI
        [SerializeField] private Typewriter_Effect typewriterEffect;
        [SerializeField] private PlayerInput playerInput;

        /// <summary>
        /// The current state of the dialogue state machine. Initialized to Idle.
        /// </summary>
        public DialogueState CurrentState { get; private set; } = DialogueState.Idle;

        /// <summary>
        /// The index of the current dialogue line being displayed.
        /// </summary>
        public int CurrentLineIndex { get; private set; }

        private const int MaxDisplayedOptions = 4;

        private IDialogueUI _dialogueUI;
        private DialogueData _currentDialogueData;
        private InputAction _submitAction;
        private InputAction _cancelAction;
        private InputAction _navigateAction;

        // Valid state transitions defined as a lookup table.
        private static readonly Dictionary<DialogueState, HashSet<DialogueState>> _validTransitions =
            new Dictionary<DialogueState, HashSet<DialogueState>>
            {
                { DialogueState.Idle, new HashSet<DialogueState> { DialogueState.Typing } },
                { DialogueState.Typing, new HashSet<DialogueState> { DialogueState.WaitingForInput, DialogueState.ShowingChoices, DialogueState.Closing } },
                { DialogueState.WaitingForInput, new HashSet<DialogueState> { DialogueState.Typing, DialogueState.Closing } },
                { DialogueState.ShowingChoices, new HashSet<DialogueState> { DialogueState.Typing, DialogueState.Closing } },
                { DialogueState.Closing, new HashSet<DialogueState> { DialogueState.Idle } },
                { DialogueState.Transitioning, new HashSet<DialogueState>() } // Reserved for future use
            };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        /// <summary>
        /// Attempts to transition to the target state. Returns true if the transition is valid
        /// and was performed, false otherwise. Logs a warning on invalid transitions.
        /// </summary>
        /// <param name="targetState">The desired target state.</param>
        /// <returns>True if the transition succeeded, false if it was invalid.</returns>
        public bool TryTransition(DialogueState targetState)
        {
            if (_validTransitions.TryGetValue(CurrentState, out var validTargets) && validTargets.Contains(targetState))
            {
                CurrentState = targetState;
                return true;
            }

            Debug.LogWarning($"[Dialogue_Manager] Invalid state transition attempted: {CurrentState} -> {targetState}");
            return false;
        }

        /// <summary>
        /// Starts a dialogue session using the provided dialogue data.
        /// Validates preconditions before initiating.
        /// </summary>
        /// <param name="data">The dialogue data to use for the session.</param>
        public void StartDialogue(DialogueData data)
        {
            if (data == null || data.Lines.Count == 0)
            {
                Debug.LogWarning("[Dialogue_Manager] Cannot start dialogue: Dialogue data is null or empty.");
                return;
            }

            if (CurrentState != DialogueState.Idle)
            {
                Debug.LogWarning($"[Dialogue_Manager] Cannot start dialogue: Current state is {CurrentState}, expected Idle.");
                return;
            }

            // Resolve IDialogueUI from the serialized MonoBehaviour field.
            _dialogueUI = dialogueUIComponent as IDialogueUI;
            if (_dialogueUI == null)
            {
                Debug.LogWarning("[Dialogue_Manager] Cannot start dialogue: IDialogueUI is not assigned.");
                return;
            }

            // Store the current dialogue data for the session.
            _currentDialogueData = data;
            CurrentLineIndex = 0;

            // Transition to Typing state to begin the dialogue session.
            if (!TryTransition(DialogueState.Typing))
                return;

            // Switch input map to UI.
            if (playerInput != null)
            {
                playerInput.SwitchCurrentActionMap("UI");
                SubscribeToUIActions();
            }

            // Set up UI.
            _dialogueUI.Show();
            _dialogueUI.SetSpeakerName(data.SpeakerName);

            // Subscribe to event bus for dynamic re-filtering.
            Dialogue_Event_Bus.OnEventActivated += HandleEventBusChanged;
            Dialogue_Event_Bus.OnEventDeactivated += HandleEventBusChanged;

            // Subscribe to typewriter completion.
            if (typewriterEffect != null)
            {
                typewriterEffect.OnTypingComplete += OnTypewriterComplete;
            }

            // Start typing the first line.
            StartTypingCurrentLine();
        }

        /// <summary>
        /// Called when the Typewriter_Effect finishes revealing text.
        /// Transitions to ShowingChoices if filtered options exist, else WaitingForInput.
        /// </summary>
        public void OnTypewriterComplete()
        {
            var filteredOptions = GetFilteredOptions();

            if (filteredOptions.Count > 0)
            {
                if (!TryTransition(DialogueState.ShowingChoices))
                    return;

                _dialogueUI.ShowAdvanceIndicator(false);
                _dialogueUI.ShowOptions(filteredOptions, OnOptionSelectedFromUI);
            }
            else
            {
                if (!TryTransition(DialogueState.WaitingForInput))
                    return;

                _dialogueUI.ShowAdvanceIndicator(true);
            }
        }

        /// <summary>
        /// Advances to the next dialogue line. Called when the player presses Submit
        /// while in WaitingForInput state.
        /// </summary>
        public void AdvanceLine()
        {
            CurrentLineIndex++;

            if (CurrentLineIndex < _currentDialogueData.Lines.Count)
            {
                if (!TryTransition(DialogueState.Typing))
                    return;

                _dialogueUI.ShowAdvanceIndicator(false);
                StartTypingCurrentLine();
            }
            else
            {
                CloseDialogue();
            }
        }

        /// <summary>
        /// Selects the dialogue option at the given index from the currently filtered options.
        /// Navigates to the target line index, or closes if the target is invalid.
        /// </summary>
        /// <param name="optionIndex">The index within the filtered options list.</param>
        public void SelectOption(int optionIndex)
        {
            var filteredOptions = GetFilteredOptions();

            if (optionIndex < 0 || optionIndex >= filteredOptions.Count)
            {
                CloseDialogue();
                return;
            }

            var selectedOption = filteredOptions[optionIndex];
            int targetLineIndex = selectedOption.TargetLineIndex;

            _dialogueUI.HideOptions();

            if (targetLineIndex >= 0 && targetLineIndex < _currentDialogueData.Lines.Count)
            {
                CurrentLineIndex = targetLineIndex;

                if (!TryTransition(DialogueState.Typing))
                    return;

                StartTypingCurrentLine();
            }
            else
            {
                CloseDialogue();
            }
        }

        /// <summary>
        /// Closes the current dialogue session. Hides UI, switches input map back to Player,
        /// unsubscribes from events, and transitions to Idle.
        /// </summary>
        public void CloseDialogue()
        {
            if (!TryTransition(DialogueState.Closing))
                return;

            // Stop any active typing.
            if (typewriterEffect != null)
            {
                typewriterEffect.Stop();
            }

            // Unsubscribe from events.
            Dialogue_Event_Bus.OnEventActivated -= HandleEventBusChanged;
            Dialogue_Event_Bus.OnEventDeactivated -= HandleEventBusChanged;

            if (typewriterEffect != null)
            {
                typewriterEffect.OnTypingComplete -= OnTypewriterComplete;
            }

            // Hide UI (immediate callback for now).
            _dialogueUI.HideOptions();
            _dialogueUI.ShowAdvanceIndicator(false);
            _dialogueUI.Hide(() =>
            {
                // Unsubscribe from UI actions before switching map.
                UnsubscribeFromUIActions();

                // Switch input map back to Player.
                if (playerInput != null)
                {
                    playerInput.SwitchCurrentActionMap("Player");
                }

                // Transition to Idle.
                TryTransition(DialogueState.Idle);

                _currentDialogueData = null;
            });
        }

        /// <summary>
        /// Returns the list of dialogue options for the current line, filtered by event bus state.
        /// Options with no required event ID are always included.
        /// Options whose required event ID is active on the bus are included.
        /// Results are capped at MaxDisplayedOptions (4).
        /// </summary>
        /// <returns>A filtered list of available dialogue options, max 4.</returns>
        public List<DialogueOption> GetFilteredOptions()
        {
            if (_currentDialogueData == null || CurrentLineIndex >= _currentDialogueData.Lines.Count)
                return new List<DialogueOption>();

            var currentLine = _currentDialogueData.Lines[CurrentLineIndex];

            if (!currentLine.HasOptions)
                return new List<DialogueOption>();

            var filtered = new List<DialogueOption>();

            foreach (var option in currentLine.Options)
            {
                if (!option.HasRequiredEvent || Dialogue_Event_Bus.IsEventActive(option.RequiredEventId))
                {
                    filtered.Add(option);

                    if (filtered.Count >= MaxDisplayedOptions)
                        break;
                }
            }

            return filtered;
        }

        /// <summary>
        /// Handles event bus state changes during an active dialogue session.
        /// Re-filters options when in ShowingChoices state.
        /// Auto-selects if re-filtering reduces to exactly one option.
        /// </summary>
        private void HandleEventBusChanged(string eventId)
        {
            if (CurrentState != DialogueState.ShowingChoices)
                return;

            var filteredOptions = GetFilteredOptions();

            if (filteredOptions.Count == 0)
            {
                // All options filtered out — treat as no options, transition to WaitingForInput.
                _dialogueUI.HideOptions();
                if (TryTransition(DialogueState.WaitingForInput))
                {
                    _dialogueUI.ShowAdvanceIndicator(true);
                }
            }
            else if (filteredOptions.Count == 1)
            {
                // Auto-select the single remaining option.
                _dialogueUI.HideOptions();
                var selectedOption = filteredOptions[0];
                int targetLineIndex = selectedOption.TargetLineIndex;

                if (targetLineIndex >= 0 && targetLineIndex < _currentDialogueData.Lines.Count)
                {
                    CurrentLineIndex = targetLineIndex;

                    if (TryTransition(DialogueState.Typing))
                    {
                        StartTypingCurrentLine();
                    }
                }
                else
                {
                    CloseDialogue();
                }
            }
            else
            {
                // Update the UI with the new filtered options.
                _dialogueUI.ShowOptions(filteredOptions, OnOptionSelectedFromUI);
            }
        }

        /// <summary>
        /// Callback passed to IDialogueUI.ShowOptions. Invoked by the UI when the player selects an option.
        /// </summary>
        private void OnOptionSelectedFromUI(int optionIndex)
        {
            SelectOption(optionIndex);
        }

        // ─── Input Action Handlers ───────────────────────────────────────────────

        private void SubscribeToUIActions()
        {
            if (playerInput == null) return;

            var uiMap = playerInput.actions.FindActionMap("UI");
            if (uiMap == null) return;

            _submitAction = uiMap.FindAction("Submit");
            _cancelAction = uiMap.FindAction("Cancel");
            _navigateAction = uiMap.FindAction("Navigate");

            if (_submitAction != null) _submitAction.performed += HandleSubmit;
            if (_cancelAction != null) _cancelAction.performed += HandleCancel;
            if (_navigateAction != null) _navigateAction.performed += HandleNavigate;
        }

        private void UnsubscribeFromUIActions()
        {
            if (_submitAction != null) _submitAction.performed -= HandleSubmit;
            if (_cancelAction != null) _cancelAction.performed -= HandleCancel;
            if (_navigateAction != null) _navigateAction.performed -= HandleNavigate;

            _submitAction = null;
            _cancelAction = null;
            _navigateAction = null;
        }

        private void HandleSubmit(InputAction.CallbackContext context)
        {
            switch (CurrentState)
            {
                case DialogueState.Typing:
                    if (typewriterEffect != null)
                        typewriterEffect.SkipToEnd();
                    break;

                case DialogueState.WaitingForInput:
                    AdvanceLine();
                    break;

                case DialogueState.ShowingChoices:
                    var ui = dialogueUIComponent as Dialogue_UI;
                    if (ui != null)
                        ui.ConfirmSelection();
                    break;
            }
        }

        private void HandleCancel(InputAction.CallbackContext context)
        {
            switch (CurrentState)
            {
                case DialogueState.Typing:
                case DialogueState.WaitingForInput:
                case DialogueState.ShowingChoices:
                    CloseDialogue();
                    break;

                case DialogueState.Closing:
                    // Ignore input during Closing state (Req 7.3).
                    break;
            }
        }

        private void HandleNavigate(InputAction.CallbackContext context)
        {
            if (CurrentState != DialogueState.ShowingChoices)
                return;

            Vector2 navigation = context.ReadValue<Vector2>();

            var ui = dialogueUIComponent as Dialogue_UI;
            if (ui == null) return;

            if (navigation.y > 0f)
                ui.NavigateUp();
            else if (navigation.y < 0f)
                ui.NavigateDown();
        }

        /// <summary>
        /// Legacy SendMessages handler kept for compatibility.
        /// </summary>
        public void OnSubmit(InputValue value) { HandleSubmitInternal(); }
        public void OnCancel(InputValue value) { HandleCancelInternal(); }
        public void OnNavigate(InputValue value)
        {
            if (CurrentState != DialogueState.ShowingChoices) return;
            var nav = value.Get<Vector2>();
            var ui = dialogueUIComponent as Dialogue_UI;
            if (ui == null) return;
            if (nav.y > 0f) ui.NavigateUp();
            else if (nav.y < 0f) ui.NavigateDown();
        }

        private void HandleSubmitInternal()
        {
            switch (CurrentState)
            {
                case DialogueState.Typing:
                    if (typewriterEffect != null) typewriterEffect.SkipToEnd();
                    break;
                case DialogueState.WaitingForInput:
                    AdvanceLine();
                    break;
                case DialogueState.ShowingChoices:
                    var ui = dialogueUIComponent as Dialogue_UI;
                    if (ui != null) ui.ConfirmSelection();
                    break;
            }
        }

        private void HandleCancelInternal()
        {
            switch (CurrentState)
            {
                case DialogueState.Typing:
                case DialogueState.WaitingForInput:
                case DialogueState.ShowingChoices:
                    CloseDialogue();
                    break;
                case DialogueState.Closing:
                    break;
            }
        }

        // ─── Private Helpers ─────────────────────────────────────────────────────

        /// <summary>
        /// Starts the typewriter effect for the current dialogue line.
        /// </summary>
        private void StartTypingCurrentLine()
        {
            if (_currentDialogueData == null || CurrentLineIndex >= _currentDialogueData.Lines.Count)
                return;

            var currentLine = _currentDialogueData.Lines[CurrentLineIndex];

            if (typewriterEffect != null)
            {
                typewriterEffect.StartTyping(currentLine.Text, _dialogueUI);
            }
        }
    }
}
