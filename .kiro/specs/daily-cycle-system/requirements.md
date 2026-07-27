# Requirements Document

## Introduction

The Daily Cycle System is the core gameplay progression loop for Flipit. It structures each play session as a single "day" that begins at the School spawn point and ends when the player interacts with the Mother NPC at home. The system provides routine, progression, and replayability through configurable daily events that introduce small gameplay variations. This is a gameplay loop system, not a real-time day/night lighting system.

The system must integrate with the existing Dialogue System, City Terrain, and Combat System without modifying validated implementations.

## Glossary

- **Daily_Cycle_Manager**: The singleton MonoBehaviour orchestrator responsible for managing the current day, end-of-day transitions, event selection, and player respawn
- **Daily_Event**: A ScriptableObject-based definition representing a small gameplay variation that executes once per day during the end-of-day transition
- **Day_Counter**: An integer value tracking the current gameplay day, starting at 1, stored in memory
- **School_Spawn_Point**: A configurable Transform reference representing the position where the player respawns at the start of each day
- **Mother_NPC**: An NPC located near the player's house that triggers the end-of-day sequence through the existing Dialogue System
- **End_Of_Day_Transition**: The full sequence from dialogue completion through fade, event execution, day increment, respawn, and fade-in
- **Event_Card**: A UI element displayed during the transition that shows the title and description of the daily event
- **Fade_System**: A UI overlay component that performs screen fade-to-black and fade-from-black transitions
- **Daily_Cycle_Event_Bus**: A static event service that broadcasts daily cycle state changes to other systems
- **Dialogue_Manager**: The existing singleton orchestrator for the dialogue system (Flipit.Dialogue namespace)
- **Player_Controller**: The existing MonoBehaviour handling player movement in the CityTerrain namespace
- **Persistent_World**: The design constraint that the city, NPCs, buildings, and props remain unchanged between days

## Requirements

### Requirement 1: Day Counter Management

**User Story:** As a player, I want the game to track which day I am on, so that I can see my progression through the game.

#### Acceptance Criteria

1. THE Daily_Cycle_Manager SHALL maintain a Day_Counter integer value initialized to 1 when the application starts
2. WHEN NextDay is called, THE Daily_Cycle_Manager SHALL increment the Day_Counter by exactly 1
3. THE Daily_Cycle_Manager SHALL expose a GetCurrentDay method that returns the current Day_Counter value as an integer greater than or equal to 1
4. THE Daily_Cycle_Manager SHALL store the Day_Counter in memory without persisting to disk, such that the value resets to 1 on each new application launch
5. THE Daily_Cycle_Manager SHALL implement a singleton pattern where Awake sets a static Instance property, calls DontDestroyOnLoad on its GameObject, and destroys any duplicate instance that already exists
6. IF a second Daily_Cycle_Manager instance is created, THEN THE Daily_Cycle_Manager SHALL destroy the duplicate GameObject and preserve the original Instance and its Day_Counter value

### Requirement 2: Player Respawn at School

**User Story:** As a player, I want to start each day at school, so that the daily routine feels consistent and grounded.

#### Acceptance Criteria

1. WHEN RespawnPlayer is called, THE Daily_Cycle_Manager SHALL move the player GameObject to the School_Spawn_Point position and match the player's rotation to the School_Spawn_Point rotation
2. THE Daily_Cycle_Manager SHALL reference both the School_Spawn_Point and the player GameObject through configurable SerializeField fields, not through hardcoded coordinates or runtime lookups
3. WHEN RespawnPlayer is called, THE Daily_Cycle_Manager SHALL preserve all existing GameObjects in the scene without destroying, duplicating, or regenerating any object
4. WHEN RespawnPlayer is called, THE Daily_Cycle_Manager SHALL only modify the player position and rotation, leaving all other scene state unchanged including NPC positions, gameplay variables, and active component states
5. IF the School_Spawn_Point reference or the player GameObject reference is unassigned when RespawnPlayer is called, THEN THE Daily_Cycle_Manager SHALL log a warning and skip the respawn operation without throwing an exception

### Requirement 3: End of Day Trigger via Mother NPC

**User Story:** As a player, I want to end the day by talking to my mother at home, so that the transition feels narrative and intentional.

#### Acceptance Criteria

1. WHEN the Mother_NPC dialogue reaches its final line and the Dialogue_Manager transitions to Idle (natural completion), THE Daily_Cycle_Manager SHALL begin the End_Of_Day_Transition sequence
2. IF the player cancels the Mother_NPC dialogue before reaching the final line, THEN THE Daily_Cycle_Manager SHALL NOT begin the End_Of_Day_Transition sequence and the player SHALL remain free to interact again
3. THE Mother_NPC SHALL use the existing Dialogue System (Flipit.Dialogue) for interaction by extending NPC_Interactable without reimplementing dialogue state management or UI handling
4. THE Daily_Cycle_Manager SHALL listen for dialogue completion through event-driven communication, not by polling or modifying the Dialogue_Manager
5. IF the Daily_Cycle_Manager receives an end-of-day request while an End_Of_Day_Transition is already in progress, THEN THE Daily_Cycle_Manager SHALL ignore the request and log a warning without interrupting the current transition

### Requirement 4: End of Day Transition Sequence

**User Story:** As a player, I want a smooth visual transition between days, so that the passage of time feels cinematic and deliberate.

#### Acceptance Criteria

1. WHEN EndCurrentDay is called, THE Daily_Cycle_Manager SHALL disable player movement and interaction input before beginning the visual transition
2. WHEN EndCurrentDay is called, THE Fade_System SHALL fade the screen to black over a configurable duration with a default of 1 second and a valid range of 0.5 to 3 seconds
3. WHEN the fade-to-black completes, THE Daily_Cycle_Manager SHALL display the text "End of Day X" centered on screen, where X is the current Day_Counter value
4. WHEN the end-of-day text has been displayed for a configurable duration with a default of 2 seconds and a valid range of 1 to 5 seconds, THE Daily_Cycle_Manager SHALL execute TriggerRandomEvent
5. IF TriggerRandomEvent finds no available events, THEN THE Daily_Cycle_Manager SHALL skip event execution and proceed directly to calling NextDay
6. WHEN the random event execution completes, THE Daily_Cycle_Manager SHALL call NextDay to increment the Day_Counter
7. WHEN NextDay completes, THE Daily_Cycle_Manager SHALL call RespawnPlayer to move the player to the School_Spawn_Point
8. WHEN the respawn completes, THE Fade_System SHALL fade the screen from black to clear over a configurable duration with a default of 1 second and a valid range of 0.5 to 3 seconds
9. WHEN the fade-in completes, THE Daily_Cycle_Manager SHALL re-enable player movement and interaction input
10. WHILE the End_Of_Day_Transition is in progress, THE Daily_Cycle_Manager SHALL prevent all player input including movement and interaction
11. IF EndCurrentDay is called while an End_Of_Day_Transition is already in progress, THEN THE Daily_Cycle_Manager SHALL ignore the duplicate call and leave the current transition unaffected

### Requirement 5: Daily Event System Architecture

**User Story:** As a developer, I want daily events defined as ScriptableObjects with a shared base class, so that new events can be added without modifying existing code.

#### Acceptance Criteria

1. THE Daily_Event base class SHALL define the following fields: Title (string, max 50 characters), Description (string, max 200 characters), Category (enum: Positive, Neutral, Negative), Probability (float, range 0.0 to 1.0, default 1.0), and an abstract Execute method that returns a boolean indicating whether the event was applied successfully
2. THE Daily_Event SHALL be a ScriptableObject decorated with a CreateAssetMenu attribute so that event definitions are created as asset files in the Unity Editor
3. WHEN the Daily_Cycle_Manager triggers event selection, THE Daily_Cycle_Manager SHALL select one Daily_Event from its configured collection using weighted random selection based on each event's Category, where category weights (Positive, Neutral, Negative) are configurable floats that default to 0.30, 0.50, and 0.20 respectively
4. WHEN a new Daily_Event type is created, THE Daily_Cycle_Manager SHALL support the new event without requiring modification to the manager or existing event classes (Open/Closed Principle)
5. THE Daily_Event Execute method SHALL receive an interface-typed context parameter that exposes the current day number, player coin count, and player Flipit count, without creating direct dependencies on other manager classes
6. IF the Daily_Cycle_Manager's configured event collection contains zero events, THEN THE Daily_Cycle_Manager SHALL log a warning and skip event execution without interrupting the day transition

### Requirement 6: Event Selection and Probability

**User Story:** As a designer, I want to configure the probability distribution of daily events by category, so that I can balance the player experience.

#### Acceptance Criteria

1. THE Daily_Cycle_Manager SHALL select exactly one event per TriggerRandomEvent call using a weighted random system based on category probabilities
2. THE Daily_Cycle_Manager SHALL expose category probabilities as configurable SerializeField float values in the range 0 to 100 with defaults: Positive 30, Neutral 50, Negative 20, and SHALL normalize these values at runtime so they sum to 100% before selection
3. WHEN TriggerRandomEvent is called, THE Daily_Cycle_Manager SHALL first select a category by generating a random value against the normalized category weights, then select an event from that category using uniform random selection among all events in that category
4. IF no events exist in the selected category, THEN THE Daily_Cycle_Manager SHALL attempt to select a random event from the Neutral category; IF the Neutral category is also empty, THEN THE Daily_Cycle_Manager SHALL skip event execution and continue the transition
5. IF no events are configured at all, THEN THE Daily_Cycle_Manager SHALL skip event execution, log a warning with the prefix [Daily_Cycle_Manager], and continue the day transition without throwing an exception

### Requirement 7: Event Safety Constraints

**User Story:** As a player, I want daily events to add variety without ruining my progress, so that each day feels fresh instead of frustrating.

#### Acceptance Criteria

1. THE Daily_Event Execute method SHALL preserve all existing NPCs in the scene without removing, disabling, or duplicating any NPC GameObject
2. THE Daily_Event Execute method SHALL preserve the city layout without regenerating terrain, destroying buildings, or repositioning landmarks
3. THE Daily_Event Execute method SHALL preserve access to all vendors and challenger NPCs without disabling their interactable components
4. THE Daily_Event Execute method SHALL preserve the player's ability to traverse the path from School Spawn Point to the Mother NPC and to interact with the Mother NPC, without blocking that route or disabling any interaction required to end the day
5. WHEN a Negative category event executes, THE Daily_Event SHALL limit resource losses to a configurable maximum threshold per resource type (coins, Flipits, consumables) exposed as a serialized field, and SHALL limit any temporary debuff (such as movement speed reduction) to a configurable maximum duration exposed as a serialized field defaulting to no more than 30 seconds
6. WHEN a new day begins, THE DailyCycleManager SHALL reset all temporary effects applied by the previous day's event so that no debuff persists across days

### Requirement 8: Persistent World Integrity

**User Story:** As a player, I want the city to remain the same between days, so that my mental map and NPC locations stay consistent.

#### Acceptance Criteria

1. WHEN NextDay is called, THE Daily_Cycle_Manager SHALL not call City_Generator.PrepareGeneration, City_Generator.GenerateCity, City_Generator.ClearGenerated, or any ExecuteStage method
2. WHEN NextDay is called, THE Daily_Cycle_Manager SHALL not instantiate or destroy any GameObject in the scene, except repositioning the Player GameObject to the School Spawn Point
3. WHEN NextDay is called, THE Daily_Cycle_Manager SHALL not modify NPC Transform positions, NPC GameObject active states, or NPC MonoBehaviour component configurations
4. THE Daily_Cycle_Manager SHALL only modify the following gameplay variables during the day transition: Day_Counter, Player GameObject position (respawn to School Spawn Point), and Daily Event effect values (coins, Flipits, movement speed, combat reward multipliers, vendor discount flags)
5. WHEN NextDay is called, THE Daily_Cycle_Manager SHALL preserve the scene hierarchy unchanged, meaning no GameObject parent-child relationships, no GameObject names, and no sibling order shall be altered

### Requirement 9: Transition UI Presentation

**User Story:** As a player, I want to see what day ended and what event happened, so that I stay informed and engaged during transitions.

#### Acceptance Criteria

1. WHEN the fade-to-black completes, THE Transition UI SHALL display the "End of Day X" text centered on screen for a configurable duration between 1.5 and 4.0 seconds (default: 2.5 seconds) before showing the Event_Card
2. WHEN a Daily_Event is selected, THE Transition UI SHALL display an Event_Card showing the event Title (maximum 60 characters) and Description (maximum 200 characters)
3. WHEN the Event_Card is displayed, THE Transition UI SHALL hold the Event_Card visible for a configurable duration between 2.0 and 5.0 seconds (default: 3.0 seconds) before proceeding to fade-in
4. WHILE the End_Of_Day_Transition is in progress, THE Transition UI SHALL render above all other UI elements using an overlay Canvas with the highest sorting order
5. THE Fade_System SHALL use a full-screen CanvasGroup overlay for fade transitions with a configurable fade duration between 0.3 and 2.0 seconds (default: 0.8 seconds)

### Requirement 10: Fade System

**User Story:** As a player, I want smooth screen fades between days, so that transitions feel polished rather than abrupt.

#### Acceptance Criteria

1. THE Fade_System SHALL provide a FadeOut method that linearly interpolates screen opacity from 0 (clear) to 1 (black) over a configurable duration between 0.1 and 5.0 seconds, defaulting to 1.0 second
2. THE Fade_System SHALL provide a FadeIn method that linearly interpolates screen opacity from 1 (black) to 0 (clear) over a configurable duration between 0.1 and 5.0 seconds, defaulting to 1.0 second
3. WHEN a fade operation completes its full duration, THE Fade_System SHALL invoke the completion callback provided to that operation
4. THE Fade_System SHALL use a Canvas set to Screen Space Overlay with a sorting order of 999 to render above all other UI
5. IF FadeOut or FadeIn is called while a fade is already in progress, THEN THE Fade_System SHALL snap the current fade to its target opacity, invoke its completion callback, and then start the new fade operation
6. WHEN the Fade_System initializes, THE Fade_System SHALL set the overlay image opacity to 0 (clear) so that the screen is fully visible by default

### Requirement 11: Integration with Existing Systems

**User Story:** As a developer, I want the Daily Cycle System to work alongside existing systems without breaking validated implementations, so that previously approved features remain stable.

#### Acceptance Criteria

1. THE Daily_Cycle_Manager SHALL communicate with the Dialogue System exclusively through events (Dialogue_Event_Bus) or public API calls (Dialogue_Manager.Instance.StartDialogue, Dialogue_Manager.Instance.CloseDialogue, Dialogue_Manager.Instance.CurrentState) without modifying any source file in the Flipit.Dialogue assembly
2. THE Daily_Cycle_Manager SHALL disable player input by calling PlayerInput.SwitchCurrentActionMap("UI") and restore it by calling PlayerInput.SwitchCurrentActionMap("Player"), matching the mechanism used by Combat_System
3. IF Combat_System.CurrentState is not Idle when the Daily Cycle transition begins, THEN THE Daily_Cycle_Manager SHALL wait until Combat_System.CurrentState returns to Idle before disabling player input and starting the end-of-day sequence
4. THE Daily_Cycle_Manager SHALL reside in a new assembly (Flipit.DailyCycle) that declares dependencies on Flipit.Dialogue and Flipit.CityTerrain, and no existing assembly (Flipit.Dialogue, Flipit.Combat, Flipit.CityTerrain) SHALL add a reference to Flipit.DailyCycle
5. THE Daily_Cycle_Manager SHALL implement the singleton pattern by exposing a static Instance property, checking for duplicate instances in Awake, destroying duplicates via Destroy(gameObject), and calling DontDestroyOnLoad to persist across scene transitions
6. IF the Daily Cycle System requires a modification to any source file belonging to Flipit.Dialogue, Flipit.Combat, or Flipit.CityTerrain assemblies, THEN THE developer SHALL document the change with a rationale, list all affected files, and obtain written approval before implementation

### Requirement 12: DailyCycleManager Public API

**User Story:** As a developer, I want a clear public API on the DailyCycleManager, so that other systems can interact with the daily cycle predictably.

#### Acceptance Criteria

1. THE Daily_Cycle_Manager SHALL expose a public EndCurrentDay method that disables player movement and initiates the End_Of_Day_Transition sequence (fade out, display day summary, execute random event, increment day, respawn player, fade in, enable player movement)
2. THE Daily_Cycle_Manager SHALL expose a public NextDay method that increments the Day_Counter by 1 and broadcasts a "day_started" event via the Daily_Cycle_Event_Bus
3. THE Daily_Cycle_Manager SHALL expose a public TriggerRandomEvent method that selects one Daily_Event using weighted probability from the configured event pool and executes it
4. IF TriggerRandomEvent is called when no Daily_Events are configured, THEN THE Daily_Cycle_Manager SHALL log a warning and skip event execution without interrupting the End_Of_Day_Transition sequence
5. THE Daily_Cycle_Manager SHALL expose a public RespawnPlayer method that repositions the player GameObject at the configured School_Spawn_Point Transform and broadcasts a "player_respawned" event
6. IF RespawnPlayer is called when the School_Spawn_Point is not assigned, THEN THE Daily_Cycle_Manager SHALL log an error and leave the player position unchanged
7. THE Daily_Cycle_Manager SHALL expose a public GetCurrentDay method that returns the current Day_Counter as an integer starting at 1 on the first day and incrementing by 1 each time NextDay is called
8. THE Daily_Cycle_Event_Bus SHALL broadcast "day_ending" when EndCurrentDay begins, "event_triggered" when a Daily_Event executes, "day_ended" when the transition sequence completes, and "day_started" when the new day begins after player respawn

### Requirement 13: Editor Debug Tools

**User Story:** As a developer, I want editor menu items to test daily cycle functions in isolation, so that I can verify each component without playing through the full loop.

#### Acceptance Criteria

1. THE Editor Debug Tools SHALL provide a menu item at "Flipit → Daily Cycle → Force Next Day" that calls NextDay and RespawnPlayer on the active scene's Daily_Cycle_Manager instance
2. THE Editor Debug Tools SHALL provide a menu item at "Flipit → Daily Cycle → Trigger Random Event" that calls TriggerRandomEvent on the active scene's Daily_Cycle_Manager instance
3. THE Editor Debug Tools SHALL provide a menu item at "Flipit → Daily Cycle → Reset Day Counter" that sets the Day_Counter to 1
4. THE Editor Debug Tools SHALL provide a menu item at "Flipit → Daily Cycle → Go To Day..." that displays an editor input dialog accepting integer values from 1 to 9999 and sets the Day_Counter to the entered value
5. THE Editor Debug Tools SHALL be placed in an Editor-only assembly (includePlatforms limited to Editor) so that they are excluded from player builds
6. WHILE in the Unity Editor, THE Daily_Cycle_Manager Inspector SHALL display the current Day_Counter, the last executed event name (or "None" if no event has been executed), and the current transition state
7. IF a menu item is invoked and no Daily_Cycle_Manager instance exists in the active scene, THEN THE Editor Debug Tools SHALL log a warning message to the Console and take no further action

### Requirement 14: Architecture Extensibility

**User Story:** As a developer, I want the Daily Cycle architecture to support future features like weather, weekdays, and seasonal events, so that expansion does not require redesigning the core system.

#### Acceptance Criteria

1. THE Daily_Cycle_Manager SHALL use a static Daily_Cycle_Event_Bus class that publishes lifecycle events (day_started, day_ended, event_executed) via Action delegates, allowing external systems to subscribe and unsubscribe without holding a reference to Daily_Cycle_Manager internal state
2. THE Daily_Event system SHALL support adding new event categories beyond Positive, Neutral, and Negative through ScriptableObject configuration, such that a new category can be introduced without modifying existing DailyEvent subclasses or the Daily_Cycle_Manager source code
3. THE Daily_Cycle_Manager SHALL expose the Day_Counter through a public read-only property (get-only, no public setter) that external systems can query at any time without modifying the counter value
4. THE Daily_Cycle_Manager SHALL expose its persistence-related data (Day_Counter and event history of up to 365 entries) through a single public read-only data property, returning a serializable struct or class that a future Save/Load system can extract in one call without modifying Daily_Cycle_Manager source code
5. WHEN a new external system subscribes to the Daily_Cycle_Event_Bus, THE Daily_Cycle_Manager SHALL continue to function without recompilation or modification, verifiable by adding a test subscriber script that receives day_started and day_ended events without changes to existing assembly code

### Requirement 15: Development Staging

**User Story:** As a developer, I want the Daily Cycle System developed in validated stages, so that each component is verified before building dependent features.

#### Acceptance Criteria

1. THE development process SHALL implement Stage 1 (Daily_Cycle_Manager, Day_Counter, School Respawn) before any other stage
2. WHEN a stage is complete, THE developer SHALL provide a deliverable report listing: files created, files modified, Unity components added, inspector configuration required, scene changes made, and testing instructions
3. IF Stage N has not received explicit validation approval from the developer, THEN THE development process SHALL NOT begin Stage N+1
4. THE development process SHALL follow this stage order: Stage 1 (Manager, Counter, Respawn), Stage 2 (Mother NPC, Dialogue Integration, Fade), Stage 3 (Event System, Selection, Execution), Stage 4 (Transition UI, Event Card), Stage 5 (Editor Tools, Testing Utilities)
5. WHEN a stage requires modification to an existing validated system, THE deliverable report SHALL list each affected file, the reason for modification, and confirmation that no existing functionality is broken
6. A stage SHALL be considered complete when all components listed in its scope are implemented, the deliverable report is provided, and the testing instructions can be executed without errors in the Unity Editor
