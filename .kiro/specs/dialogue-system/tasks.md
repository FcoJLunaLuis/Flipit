# Implementation Plan: Dialogue System

## Overview

Implement a data-driven dialogue system for Flipit using Unity 6, Canvas-based uGUI with TextMeshPro, the new Input System, and ScriptableObjects. The implementation follows an incremental approach: data models and core interfaces first, then the state machine and event bus, followed by proximity detection, typewriter effect, UI implementation, and finally integration wiring.

## Tasks

- [x] 1. Set up project structure and core interfaces
  - [x] 1.1 Create directory structure and define enums/interfaces
    - Create `Assets/Scripts/Dialogue/` directory structure
    - Create `DialogueState` enum with states: Idle, Typing, WaitingForInput, ShowingChoices, Transitioning, Closing
    - Create `IDialogueUI` interface with all methods: Show(), Hide(Action), SetSpeakerName(string), SetDialogueText(string), SetVisibleCharacterCount(int), ShowAdvanceIndicator(bool), ShowTypingIndicator(bool), ShowOptions(List<DialogueOption>, Action<int>), HideOptions(), ClearText()
    - _Requirements: 11.1, 11.2, 10.1_

  - [x] 1.2 Create ScriptableObject data models
    - Create `Dialogue_Option` serializable struct with displayText, requiredEventId, targetLineIndex fields and read-only properties
    - Create `Dialogue_Line` serializable struct with text, options list, HasOptions property, and null-safe Options accessor
    - Create `Dialogue_Data` ScriptableObject with speakerName, lines list, and CreateAssetMenu attribute
    - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5, 8.6, 8.7, 8.8, 8.9_

  - [ ]* 1.3 Write property test for HasOptions correctness
    - **Property 14: HasOptions reflects list content**
    - Generate random Dialogue_Line instances with varying option list states (null, empty, populated)
    - Assert HasOptions returns true iff options list contains one or more entries
    - **Validates: Requirements 8.6**

- [x] 2. Implement Dialogue_Event_Bus
  - [x] 2.1 Implement the static Dialogue_Event_Bus class
    - Create static class with HashSet<string> storage for O(1) lookup
    - Implement ActivateEvent(string), DeactivateEvent(string), IsEventActive(string), ClearAll() methods
    - Implement OnEventActivated and OnEventDeactivated events (Action<string>)
    - Ensure set semantics: no duplicate entries, fire notifications on actual state changes
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.6_

  - [ ]* 2.2 Write property test for event bus set semantics
    - **Property 12: Event bus set semantics**
    - Generate random sequences of ActivateEvent/DeactivateEvent calls with arbitrary strings
    - Assert collection behaves as a mathematical set: each ID appears at most once, IsEventActive correctness, ClearAll empties the set
    - **Validates: Requirements 9.1, 9.2, 9.3, 9.4, 9.6**

- [x] 3. Implement Dialogue_Manager state machine
  - [x] 3.1 Create Dialogue_Manager MonoBehaviour with state machine
    - Create Dialogue_Manager as MonoBehaviour singleton
    - Implement DialogueState property with initial state Idle
    - Implement TryTransition(DialogueState target) with transition table validation
    - Log warnings on invalid transitions with current/target state info
    - Return false for invalid transitions, true for valid ones
    - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5, 10.6, 10.7, 10.8_

  - [ ]* 3.2 Write property test for state machine transition validity
    - **Property 3: State machine transition validity**
    - Generate all (currentState, targetState) pairs exhaustively
    - Assert TryTransition returns true and updates state only for valid pairs in the transition set
    - Assert TryTransition returns false and state remains unchanged for invalid pairs
    - **Validates: Requirements 10.2, 10.3, 10.4, 10.5, 10.6, 10.7, 10.8**

  - [ ]* 3.3 Write property test for cancel transitions
    - **Property 13: Cancel from active states transitions to Closing**
    - Generate random active states (Typing, WaitingForInput, ShowingChoices)
    - Assert performing Cancel transitions state to Closing
    - **Validates: Requirements 7.2**

- [x] 4. Checkpoint - Core logic verification
  - Ensure all tests pass, ask the user if questions arise.

- [x] 5. Implement Player_Interactor proximity detection
  - [x] 5.1 Create Player_Interactor MonoBehaviour
    - Implement proximity detection using Physics2D.OverlapCircleAll in FixedUpdate
    - Filter results for NPC_Interactable components
    - Select nearest NPC by transform distance as CurrentTarget
    - Clear target when no NPC_Interactable is within radius
    - Expose interactionRadius with default 2.0, Min(0.1f) attribute
    - Implement OnInteract callback for Input System
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 2.1_

  - [x] 5.2 Create NPC_Interactable MonoBehaviour
    - Implement with serialized Dialogue_Data reference
    - Expose DialogueData property and HasValidDialogue check (non-null, lines count > 0)
    - _Requirements: 2.5_

  - [ ]* 5.3 Write property test for proximity target selection
    - **Property 1: Proximity target is nearest NPC within radius or null**
    - Generate random player positions, NPC positions, and valid radii
    - Assert CurrentTarget is the nearest NPC_Interactable within radius, or null if none are in range
    - **Validates: Requirements 1.1, 1.2, 1.3**

  - [ ]* 5.4 Write property test for interaction radius clamping
    - **Property 2: Interaction radius clamping**
    - Generate random float values (including negatives and very small values)
    - Assert effective radius is always max(value, 0.1)
    - **Validates: Requirements 1.4, 1.5**

- [x] 6. Implement Typewriter_Effect
  - [x] 6.1 Create Typewriter_Effect MonoBehaviour
    - Implement coroutine-based character reveal using WaitForSeconds and IDialogueUI.SetVisibleCharacterCount
    - Expose characterDelay field with default 0.03f and Min(0.01f)
    - Implement StartTyping(string text, IDialogueUI ui) method
    - Implement SkipToEnd() that stops coroutine, sets visible count to total, fires OnTypingComplete once
    - Implement Stop() to halt without completion notification
    - Fire OnTypingComplete event when natural typing completes
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_

  - [ ]* 6.2 Write property test for typewriter skip
    - **Property 4: Typewriter skip reveals all characters and fires completion**
    - Generate random non-empty strings and random skip points during reveal
    - Assert SkipToEnd sets visible count equal to total character count and fires OnTypingComplete exactly once
    - **Validates: Requirements 3.3, 3.4, 3.5**

- [x] 7. Implement option filtering and dialogue advancement logic
  - [x] 7.1 Implement option filtering in Dialogue_Manager
    - Query Dialogue_Event_Bus to filter options by requiredEventId
    - Include options with empty/null requiredEventId
    - Include options whose requiredEventId is active on the bus
    - Exclude options whose requiredEventId is not active
    - Cap displayed options at 4
    - Handle all-filtered case: treat as no options, transition to WaitingForInput
    - Subscribe to OnEventActivated/OnEventDeactivated during active sessions for dynamic re-filtering
    - Auto-select when re-filtering reduces to exactly one option
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 6.6, 6.7, 6.8, 9.5_

  - [x] 7.2 Implement dialogue advancement and option selection logic
    - OnTypewriterComplete: transition to ShowingChoices if filtered options exist, else WaitingForInput
    - AdvanceLine: increment CurrentLineIndex, transition to Typing if next line exists, else Closing
    - SelectOption: navigate to target line index if valid, else transition to Closing
    - Guard: reject StartDialogue if Dialogue_Data is null/empty or IDialogueUI is not assigned
    - _Requirements: 4.1, 4.3, 4.4, 5.1, 5.4, 5.5, 2.5, 11.4_

  - [ ]* 7.3 Write property test for option filtering
    - **Property 7: Option filtering by event bus**
    - Generate random sets of options with varying requiredEventIds and random active event sets
    - Assert option is included iff requiredEventId is empty/null OR is active on the bus
    - **Validates: Requirements 6.1, 6.2, 6.3, 6.4**

  - [ ]* 7.4 Write property test for all-filtered fallback
    - **Property 8: All options filtered yields WaitingForInput**
    - Generate Dialogue_Lines where all options have non-active requiredEventIds
    - Assert Dialogue_Manager transitions to WaitingForInput
    - **Validates: Requirements 6.5**

  - [ ]* 7.5 Write property test for max options cap
    - **Property 9: Maximum four displayed options**
    - Generate Dialogue_Lines with N > 4 available options after filtering
    - Assert exactly 4 options are displayed
    - **Validates: Requirements 6.8, 5.2**

  - [ ]* 7.6 Write property test for post-typing state routing
    - **Property 5: Post-typing state depends on options presence**
    - Generate Dialogue_Lines with and without available options
    - Assert transition to ShowingChoices when options exist, WaitingForInput otherwise
    - **Validates: Requirements 4.1, 5.1**

  - [ ]* 7.7 Write property test for dialogue advancement
    - **Property 6: Dialogue advancement or closing on submit**
    - Generate dialogue sessions at various CurrentLineIndex positions
    - Assert Submit increments index and transitions to Typing if next line exists, or Closing if not
    - **Validates: Requirements 4.3, 4.4**

  - [ ]* 7.8 Write property test for option selection navigation
    - **Property 11: Option selection navigates to target line or closes**
    - Generate options with various targetLineIndex values (valid, -1, out of bounds)
    - Assert navigation to target line or Closing as appropriate
    - **Validates: Requirements 5.4, 5.5**

- [x] 8. Checkpoint - Core systems verification
  - Ensure all tests pass, ask the user if questions arise.

- [x] 9. Implement Dialogue_UI and choice navigation
  - [x] 9.1 Create Dialogue_UI MonoBehaviour implementing IDialogueUI
    - Create Canvas-based uGUI implementation with TextMeshPro
    - Wire serialized fields: dialogueCanvas, speakerNameText, dialogueText, advanceIndicator, typingIndicator, optionButtons array (max 4)
    - Implement all IDialogueUI methods: Show/Hide canvas, set speaker name, set/clear text, manage indicators, show/hide options with callback
    - _Requirements: 11.3, 2.3, 4.2_

  - [x] 9.2 Create DialogueOptionButton component and choice navigation
    - Create DialogueOptionButton MonoBehaviour with labelText (TMP_Text) and highlightImage (Image)
    - Implement Setup(string text, bool highlighted) and SetHighlighted(bool) methods
    - Implement choice navigation in Dialogue_UI: Navigate Up/Down wraps highlight index using modulo arithmetic
    - First option highlighted as default selection
    - _Requirements: 5.2, 5.3_

  - [ ]* 9.3 Write property test for choice navigation wrapping
    - **Property 10: Choice navigation wrapping**
    - Generate random option counts (1-4) and random sequences of Navigate Up/Down
    - Assert highlighted index = (current + direction) mod option_count
    - **Validates: Requirements 5.3**

- [x] 10. Wire Input System integration and dialogue session lifecycle
  - [x] 10.1 Implement Input System action map switching
    - Wire PlayerInput reference in Dialogue_Manager
    - Switch to UI action map on dialogue start (same frame)
    - Switch to Player action map on dialogue close (same frame)
    - Bind Submit, Cancel, and Navigate actions from UI map to Dialogue_Manager methods
    - _Requirements: 2.2, 7.1, 7.3_

  - [x] 10.2 Implement full dialogue session lifecycle
    - Wire StartDialogue: validate data/UI → transition Idle→Typing → switch input map → show UI → set speaker name → start typewriter on first line
    - Wire CloseDialogue: transition to Closing → hide UI → switch input map → transition Closing→Idle
    - Wire Cancel action to CloseDialogue from active states
    - Ignore Cancel during Closing state
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 7.1, 7.2, 7.3_

  - [ ]* 10.3 Write unit tests for Input System integration
    - Test input map switches to UI on session start
    - Test input map switches to Player on session close
    - Test Cancel ignored during Closing state
    - Test session start rejected with null Dialogue_Data
    - Test session start rejected with no IDialogueUI assigned
    - _Requirements: 2.2, 2.5, 7.1, 7.3, 11.4_

- [x] 11. Create test infrastructure and Edit Mode test assembly
  - [x] 11.1 Set up test assemblies and property test utilities
    - Create `Assets/Tests/EditMode/` directory with assembly definition referencing dialogue scripts
    - Create `Assets/Tests/PlayMode/` directory with assembly definition
    - Implement lightweight random generator utility class with seed control for PBT reproducibility
    - Configure minimum 100 iterations per property test
    - _Requirements: All property tests depend on this infrastructure_

- [x] 12. Final checkpoint - Full system verification
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation
- Property tests validate universal correctness properties from the design document
- Unit tests validate specific examples and edge cases
- The test infrastructure (task 11) should be created before running any property tests but can be implemented in parallel with core logic
- All code is C# targeting Unity 6 with URP

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1", "11.1"] },
    { "id": 1, "tasks": ["1.2", "2.1"] },
    { "id": 2, "tasks": ["1.3", "2.2", "3.1"] },
    { "id": 3, "tasks": ["3.2", "3.3", "5.1", "5.2"] },
    { "id": 4, "tasks": ["5.3", "5.4", "6.1"] },
    { "id": 5, "tasks": ["6.2", "7.1"] },
    { "id": 6, "tasks": ["7.2", "7.3", "7.4", "7.5"] },
    { "id": 7, "tasks": ["7.6", "7.7", "7.8", "9.1"] },
    { "id": 8, "tasks": ["9.2", "9.3"] },
    { "id": 9, "tasks": ["10.1"] },
    { "id": 10, "tasks": ["10.2"] },
    { "id": 11, "tasks": ["10.3"] }
  ]
}
```
