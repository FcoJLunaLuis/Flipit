# Requirements Document

## Introduction

A data-driven dialogue system for "Flipit", a top-down Unity 6 game. The system enables player interaction with NPCs through proximity-based triggers, displays dialogue with a typewriter text effect, and supports branching conversations with selectable response options filtered by game events. The system uses Canvas-based uGUI with TextMeshPro, the new Input System for action map switching, and ScriptableObjects for dialogue authoring.

## Glossary

- **Dialogue_System**: The complete set of components responsible for initiating, managing, displaying, and concluding NPC dialogue interactions.
- **Dialogue_Manager**: The singleton MonoBehaviour that orchestrates dialogue flow, maintains state, and coordinates between data, UI, and input subsystems.
- **Dialogue_UI**: The Canvas-based uGUI component responsible for rendering dialogue text, speaker names, and response option buttons to the screen.
- **Typewriter_Effect**: The component that reveals dialogue text character-by-character at a configurable speed using TMP maxVisibleCharacters.
- **Dialogue_Data**: A ScriptableObject asset containing the speaker name and an ordered list of Dialogue_Lines for a single conversation.
- **Dialogue_Line**: A data structure within Dialogue_Data containing the text to display and an optional list of Dialogue_Options.
- **Dialogue_Option**: A data structure representing a selectable response, containing display text, an optional required event ID for filtering, and a target line index for branching.
- **Dialogue_Event_Bus**: A static service that tracks raised game event IDs and provides filtering logic for option availability.
- **NPC_Interactable**: A MonoBehaviour placed on NPC GameObjects that holds a reference to Dialogue_Data and provides an interaction entry point.
- **Player_Interactor**: A MonoBehaviour on the player GameObject that detects nearby NPC_Interactable components via proximity and dispatches interaction requests.
- **Dialogue_State**: An enumeration of the Dialogue_Manager states: Idle, Typing, WaitingForInput, ShowingChoices, Transitioning, Closing.
- **Interaction_Radius**: The configurable distance threshold within which the Player_Interactor can detect and interact with an NPC_Interactable.
- **Input_Action_Map**: A named group of input bindings in the Unity Input System (Player map for gameplay, UI map for dialogue navigation).

## Requirements

### Requirement 1: NPC Proximity Detection

**User Story:** As a player, I want to detect nearby NPCs so that I know when I can start a conversation.

#### Acceptance Criteria

1. WHILE the Player_Interactor is within the Interaction_Radius of an NPC_Interactable, THE Player_Interactor SHALL identify that NPC_Interactable as the current interaction target.
2. WHEN the Player_Interactor exits the Interaction_Radius of the current interaction target, THE Player_Interactor SHALL clear the current interaction target.
3. WHEN multiple NPC_Interactable components are within the Interaction_Radius, THE Player_Interactor SHALL select the NPC_Interactable with the shortest transform-position distance to the Player_Interactor as the current interaction target, re-evaluating each detection cycle.
4. THE Player_Interactor SHALL expose the Interaction_Radius as a configurable float field with a default value of 2.0 units and a minimum permitted value of 0.1 units.
5. IF the Interaction_Radius is set to a value below 0.1 units, THEN THE Player_Interactor SHALL clamp the value to 0.1 units.

### Requirement 2: Dialogue Initiation

**User Story:** As a player, I want to start a conversation with a nearby NPC by pressing the interact button so that I can engage with the story.

#### Acceptance Criteria

1. WHEN the player performs the Interact action and a current interaction target exists and the Dialogue_State is Idle, THE Dialogue_Manager SHALL start a dialogue session using the Dialogue_Data from the current interaction target.
2. WHEN a dialogue session starts, THE Dialogue_Manager SHALL switch the active Input_Action_Map from Player to UI within the same frame.
3. WHEN a dialogue session starts, THE Dialogue_UI SHALL become visible and display the speaker name from the Dialogue_Data within the same frame.
4. WHEN a dialogue session starts, THE Dialogue_Manager SHALL transition the Dialogue_State from Idle to Typing and begin displaying the first Dialogue_Line.
5. IF the current interaction target has null or empty Dialogue_Data, THEN THE Dialogue_Manager SHALL not start a dialogue session and SHALL remain in the Idle state.

### Requirement 3: Typewriter Text Display

**User Story:** As a player, I want dialogue text to appear character-by-character so that the reading experience feels natural and engaging.

#### Acceptance Criteria

1. WHILE the Dialogue_State is Typing, THE Typewriter_Effect SHALL reveal one character of the current Dialogue_Line text per configured delay interval by incrementing TMP maxVisibleCharacters.
2. THE Typewriter_Effect SHALL expose the character delay as a configurable float field with a minimum value of 0.01 seconds and a default value of 0.03 seconds.
3. WHEN all characters of the current Dialogue_Line text have been revealed, THE Typewriter_Effect SHALL notify the Dialogue_Manager that typing is complete.
4. WHILE the Dialogue_State is Typing, WHEN the player performs the Submit action, THE Typewriter_Effect SHALL stop the revealing coroutine and set TMP maxVisibleCharacters equal to the total character count of the current Dialogue_Line text.
5. WHEN the Typewriter_Effect reveals all remaining characters via the Submit action skip, THE Typewriter_Effect SHALL notify the Dialogue_Manager that typing is complete.

### Requirement 4: Dialogue Advancement

**User Story:** As a player, I want to advance through dialogue lines so that I can progress the conversation at my own pace.

#### Acceptance Criteria

1. WHEN the Typewriter_Effect notifies completion and the current Dialogue_Line has no options, THE Dialogue_Manager SHALL transition the Dialogue_State to WaitingForInput.
2. WHILE the Dialogue_State is WaitingForInput, THE Dialogue_UI SHALL display an advance indicator to signal the player that input is expected.
3. WHEN the player performs the Submit action while the Dialogue_State is WaitingForInput, THE Dialogue_Manager SHALL increment CurrentLineIndex, pass the new Dialogue_Line text to the Typewriter_Effect, and transition the Dialogue_State to Typing.
4. WHEN the player performs the Submit action while the Dialogue_State is WaitingForInput and no subsequent Dialogue_Line exists, THE Dialogue_Manager SHALL transition the Dialogue_State to Closing.

### Requirement 5: Branching Dialogue Choices

**User Story:** As a player, I want to select from response options so that my choices influence the conversation path.

#### Acceptance Criteria

1. WHEN the Typewriter_Effect notifies completion and the current Dialogue_Line has options, THE Dialogue_Manager SHALL transition the Dialogue_State to ShowingChoices.
2. WHILE the Dialogue_State is ShowingChoices, THE Dialogue_UI SHALL display up to 4 available Dialogue_Option buttons for the current Dialogue_Line, with the first option visually highlighted as the default selection.
3. WHILE the Dialogue_State is ShowingChoices, THE Dialogue_UI SHALL move the visual highlight to the next or previous Dialogue_Option button when the player performs the Navigate Up or Navigate Down action, wrapping from the last option to the first and vice versa.
4. WHEN the player performs the Submit action while a Dialogue_Option is highlighted, THE Dialogue_Manager SHALL navigate to the Dialogue_Line at the target line index specified by the selected option and transition the Dialogue_State to Typing.
5. IF a Dialogue_Option target line index is -1 or references an index beyond the last Dialogue_Line, THEN THE Dialogue_Manager SHALL transition the Dialogue_State to Closing.

### Requirement 6: Dynamic Option Filtering

**User Story:** As a game designer, I want dialogue options to appear or hide based on game events so that conversations adapt to player progress.

#### Acceptance Criteria

1. WHEN the Dialogue_State transitions to ShowingChoices, THE Dialogue_Manager SHALL query the Dialogue_Event_Bus to filter Dialogue_Options based on their required event IDs.
2. WHEN a Dialogue_Option has no required event ID (empty or null string), THE Dialogue_Manager SHALL include that option in the available options list.
3. WHEN a Dialogue_Option has a required event ID that has been raised on the Dialogue_Event_Bus, THE Dialogue_Manager SHALL include that option in the available options list.
4. WHEN a Dialogue_Option has a required event ID that has not been raised on the Dialogue_Event_Bus, THE Dialogue_Manager SHALL exclude that option from the available options list.
5. IF all Dialogue_Options for a Dialogue_Line are excluded after filtering, THEN THE Dialogue_Manager SHALL treat the line as having no options and transition the Dialogue_State to WaitingForInput.
6. WHILE the Dialogue_State is ShowingChoices, WHEN the Dialogue_Event_Bus fires OnEventActivated or OnEventDeactivated, THE Dialogue_Manager SHALL re-filter the available options and update the Dialogue_UI immediately.
7. IF dynamic re-filtering reduces the available options to exactly one, THEN THE Dialogue_Manager SHALL auto-select that option and transition the Dialogue_State to Typing with the target line.
8. THE Dialogue_Manager SHALL display a maximum of 4 filtered Dialogue_Options at any time.

### Requirement 7: Dialogue Session Closing

**User Story:** As a player, I want the dialogue to close cleanly so that I can return to gameplay.

#### Acceptance Criteria

1. WHEN the Dialogue_State transitions to Closing, THE Dialogue_Manager SHALL hide the Dialogue_UI, switch the active Input_Action_Map from UI to Player, and transition the Dialogue_State to Idle, completing all three operations within the same frame in that order.
2. WHILE the Dialogue_State is Typing, WaitingForInput, or ShowingChoices, WHEN the player performs the Cancel action, THE Dialogue_Manager SHALL transition the Dialogue_State to Closing.
3. IF the player performs the Cancel action while the Dialogue_State is Closing, THEN THE Dialogue_Manager SHALL ignore the input and remain in the Closing state.

### Requirement 8: Dialogue Data Authoring

**User Story:** As a game designer, I want to author dialogue in ScriptableObject assets so that conversations can be created and modified without code changes.

#### Acceptance Criteria

1. THE Dialogue_Data SHALL be a ScriptableObject that can be created via Unity's Create Asset menu.
2. THE Dialogue_Data SHALL store a speaker name as a string field.
3. THE Dialogue_Data SHALL store an ordered list of Dialogue_Line entries containing at least one entry.
4. THE Dialogue_Line SHALL store the display text as a string field.
5. THE Dialogue_Line SHALL store an optional list of Dialogue_Option entries, where an empty list indicates no options are present.
6. THE Dialogue_Line SHALL expose a HasOptions boolean property that returns true when the options list contains one or more entries.
7. THE Dialogue_Option SHALL store the display text as a string field.
8. THE Dialogue_Option SHALL store an optional required event ID as a string field, where an empty or null string indicates no event is required.
9. THE Dialogue_Option SHALL store the target line index as a non-negative integer field representing a zero-based index into the Dialogue_Data line list.

### Requirement 9: Dialogue Event Bus

**User Story:** As a game designer, I want a centralized event tracking service so that game events can influence dialogue option availability.

#### Acceptance Criteria

1. THE Dialogue_Event_Bus SHALL maintain a collection of raised event ID strings where each event ID appears at most once.
2. WHEN a game system activates an event ID on the Dialogue_Event_Bus, THE Dialogue_Event_Bus SHALL add that event ID to the raised events collection if it is not already present and fire an OnEventActivated notification with the event ID.
3. WHEN queried with an event ID, THE Dialogue_Event_Bus SHALL return true if the event ID exists in the raised events collection and false otherwise.
4. WHEN a game system deactivates an event ID on the Dialogue_Event_Bus, THE Dialogue_Event_Bus SHALL remove that event ID from the raised events collection if present and fire an OnEventDeactivated notification with the event ID.
5. WHILE the Dialogue_State is not Idle, THE Dialogue_Manager SHALL subscribe to OnEventActivated and OnEventDeactivated notifications and re-evaluate available Dialogue_Options for the current Dialogue_Line upon receiving either notification.
6. THE Dialogue_Event_Bus SHALL provide a method to clear all raised events from the collection.

### Requirement 10: Dialogue State Machine Integrity

**User Story:** As a developer, I want the dialogue state machine to enforce valid transitions so that the system remains in a consistent state.

#### Acceptance Criteria

1. WHEN the Dialogue_Manager is initialized, THE Dialogue_Manager SHALL set the Dialogue_State to Idle.
2. THE Dialogue_Manager SHALL only permit transitions from Idle to Typing.
3. THE Dialogue_Manager SHALL only permit transitions from Typing to WaitingForInput, ShowingChoices, or Closing.
4. THE Dialogue_Manager SHALL only permit transitions from WaitingForInput to Typing or Closing.
5. THE Dialogue_Manager SHALL only permit transitions from ShowingChoices to Typing or Closing.
6. THE Dialogue_Manager SHALL only permit transitions from Closing to Idle.
7. IF an invalid state transition is attempted, THEN THE Dialogue_Manager SHALL reject the transition, log a warning message indicating the current state and the attempted target state, and remain in the current state.
8. IF an invalid state transition is attempted, THEN THE Dialogue_Manager SHALL return a false result to the caller to indicate the transition was not performed.

### Requirement 11: Dialogue UI Interface Abstraction

**User Story:** As a developer, I want the dialogue UI to be accessed through an interface so that different UI implementations can be swapped without modifying the Dialogue_Manager.

#### Acceptance Criteria

1. THE Dialogue_System SHALL define an IDialogueUI interface with the following methods: Show(), Hide(Action onComplete), SetSpeakerName(string), SetDialogueText(string), SetVisibleCharacterCount(int), ShowAdvanceIndicator(bool), ShowTypingIndicator(bool), ShowOptions(List<DialogueOption>, Action<int>), HideOptions(), and ClearText().
2. THE Dialogue_Manager SHALL reference the UI exclusively through the IDialogueUI interface, with the implementation assigned via a serialized field on the Dialogue_Manager MonoBehaviour.
3. THE Dialogue_UI SHALL implement the IDialogueUI interface using Canvas-based uGUI and TextMeshPro components.
4. IF the Dialogue_Manager has no IDialogueUI implementation assigned when a dialogue session starts, THEN THE Dialogue_Manager SHALL log a warning and remain in the Idle state without starting the session.
