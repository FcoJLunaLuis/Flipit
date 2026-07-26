# Implementation Plan: Daily Cycle System

## Overview

The Daily Cycle System is implemented across 5 validated stages, each building on the previous. The implementation creates a new `Flipit.DailyCycle` assembly that integrates with existing Dialogue and CityTerrain systems without modifying them. All code uses C# 9.0 targeting .NET Standard 2.1 within Unity 6.

## Tasks

- [x] 1. Stage 1 — Manager, Counter, and Respawn
  - [x] 1.1 Create assembly definitions and project structure
    - Create `Assets/Scripts/DailyCycle/Flipit.DailyCycle.asmdef` with references to Flipit.Dialogue, Flipit.CityTerrain, Unity.InputSystem, Unity.TextMeshPro
    - Create `Assets/Editor/DailyCycle/Flipit.DailyCycle.Editor.asmdef` with Editor-only platform, referencing Flipit.DailyCycle
    - Create `Assets/Scripts/DailyCycle/Daily_Cycle_State.cs` enum (Idle, WaitingForCombat, Transitioning)
    - Create `Assets/Scripts/DailyCycle/Daily_Cycle_Save_Data.cs` serializable struct
    - _Requirements: 11.4, 13.5, 14.4_

  - [x] 1.2 Implement Daily_Cycle_Event_Bus
    - Create `Assets/Scripts/DailyCycle/Daily_Cycle_Event_Bus.cs`
    - Implement static event Action delegates: OnDayStarted(int), OnDayEnding(int), OnEventTriggered(string), OnDayEnded(int), OnPlayerRespawned
    - Implement Broadcast methods for each event
    - _Requirements: 12.8, 14.1, 14.5_

  - [x] 1.3 Implement Daily_Cycle_Manager singleton with day counter and respawn
    - Create `Assets/Scripts/DailyCycle/Daily_Cycle_Manager.cs`
    - Implement singleton pattern (Instance property, Awake with DontDestroyOnLoad, duplicate destruction)
    - Implement Day_Counter initialized to 1, GetCurrentDay(), NextDay() with broadcast
    - Implement RespawnPlayer() with null checks, position/rotation assignment, broadcast
    - Implement SerializeField references for _schoolSpawnPoint, _playerGameObject, _playerInput
    - Implement _isTransitioning guard, SaveData property
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 2.1, 2.2, 2.3, 2.4, 2.5, 12.1, 12.2, 12.5, 12.6, 12.7, 14.3_

  - [ ]* 1.4 Write property test: Day Counter Monotonic Increment
    - **Property 1: Day Counter Monotonic Increment**
    - Create `Assets/Scripts/DailyCycle/Tests/Daily_Cycle_Property_Tests.cs` with EditMode test assembly
    - Create `Assets/Scripts/DailyCycle/Tests/Flipit.DailyCycle.Tests.asmdef` (Editor platform, references Flipit.DailyCycle + UnityEngine.TestRunner + NUnit)
    - Test: For random starting day (1–9999), NextDay results in GetCurrentDay() == N + 1
    - Use [Test, Repeat(100)] with randomized setup per iteration
    - **Validates: Requirements 1.2, 1.3, 12.7**

  - [ ]* 1.5 Write property test: Respawn Positions Player and Broadcasts
    - **Property 2: Respawn Positions Player and Broadcasts**
    - Test: For random Vector3 position + Quaternion rotation on spawn point, RespawnPlayer sets player transform correctly AND broadcasts player_respawned event
    - Use [Test, Repeat(100)] with random floats for position/rotation
    - **Validates: Requirements 2.1, 12.5**

  - [ ]* 1.6 Write property test: Transition Idempotency
    - **Property 4: Transition Idempotency**
    - Test: When _isTransitioning is true, calling EndCurrentDay has no effect (no new coroutine, no state change)
    - Use [Test, Repeat(100)] with random timing scenarios
    - **Validates: Requirements 3.5, 4.11**

- [x] 2. Checkpoint — Stage 1 Validation
  - Ensure all tests pass, ask the user if questions arise.

- [x] 3. Stage 2 — Mother NPC, Dialogue Integration, and Fade System
  - [x] 3.1 Implement Fade_System
    - Create `Assets/Scripts/DailyCycle/Fade_System.cs`
    - Implement FadeOut(duration, onComplete) with coroutine lerp alpha 0→1
    - Implement FadeIn(duration, onComplete) with coroutine lerp alpha 1→0
    - Handle interruption: snap current fade to target, invoke callback, start new fade
    - Initialize alpha to 0 on Awake
    - Use Canvas with Screen Space Overlay, sorting order 999, CanvasGroup reference
    - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5, 10.6_

  - [x] 3.2 Implement Mother_NPC
    - Create `Assets/Scripts/DailyCycle/Mother_NPC.cs`
    - Extend NPC_Interactable (from Flipit.Dialogue)
    - Override Interact() to call base.Interact() and subscribe to dialogue completion
    - Listen for Dialogue_Manager state → Idle (natural completion only, not cancellation)
    - On completion: call Daily_Cycle_Manager.Instance.EndCurrentDay()
    - Guard against null Instance with warning log
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 11.1_

  - [x] 3.3 Implement EndCurrentDay coroutine in Daily_Cycle_Manager
    - Add EndCurrentDay() method with _isTransitioning guard
    - Implement EndOfDayCoroutine: wait for combat idle, disable input (SwitchActionMap → UI), fade out, show day summary, trigger event, show event card, NextDay, RespawnPlayer, fade in, restore input (SwitchActionMap → Player)
    - Add combat wait guard (Combat_System.Instance.CurrentState check)
    - Add SerializeField references for _fadeSystem, _transitionUI
    - Add timing configuration fields (_fadeOutDuration, _fadeInDuration, _daySummaryDuration, _eventCardDuration)
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 4.7, 4.8, 4.9, 4.10, 4.11, 11.2, 11.3_

  - [ ]* 3.4 Write property test: Fade System Reaches Target and Invokes Callback
    - **Property 9: Fade System Reaches Target and Invokes Callback**
    - Test: For random duration in [0.1, 5.0], FadeOut reaches alpha 1 and invokes callback exactly once; FadeIn reaches alpha 0 and invokes callback exactly once
    - Use [Test, Repeat(100)] with random float durations
    - **Validates: Requirements 10.1, 10.2, 10.3**

  - [ ]* 3.5 Write property test: Input Disabled Throughout Transition
    - **Property 12: Input Disabled Throughout Transition**
    - Test: From EndCurrentDay start to fade-in completion, PlayerInput action map remains "UI"; only switches back to "Player" after final fade-in callback
    - Use [Test, Repeat(100)] with random transition timing
    - **Validates: Requirements 4.1, 4.9, 4.10**

- [x] 4. Checkpoint — Stage 2 Validation
  - Ensure all tests pass, ask the user if questions arise.

- [x] 5. Stage 3 — Event System, Selection, and Execution
  - [x] 5.1 Implement Daily_Event base class and supporting types
    - Create `Assets/Scripts/DailyCycle/Daily_Event_Category.cs` enum (Positive, Neutral, Negative)
    - Create `Assets/Scripts/DailyCycle/Daily_Event_Context.cs` IDailyEventContext interface
    - Create `Assets/Scripts/DailyCycle/Daily_Event.cs` abstract ScriptableObject with CreateAssetMenu, Title, Description, Category, Probability, abstract Execute(IDailyEventContext)
    - _Requirements: 5.1, 5.2, 5.4, 5.5_

  - [x] 5.2 Implement weighted random event selection in Daily_Cycle_Manager
    - Add _eventPool (Daily_Event[]) SerializeField
    - Add _positiveWeight, _neutralWeight, _negativeWeight SerializeFields (defaults 30, 50, 20)
    - Implement TriggerRandomEvent(): normalize weights, roll category, filter candidates, fallback to Neutral if empty, uniform random within category, execute, broadcast
    - Implement BuildEventContext() returning IDailyEventContext with current day/coins/flipits
    - Handle empty pool with warning log
    - _Requirements: 5.3, 5.6, 6.1, 6.2, 6.3, 6.4, 6.5, 12.3, 12.4_

  - [x] 5.3 Create sample Daily_Event implementations for testing
    - Create `Assets/Scripts/DailyCycle/Events/Coin_Bonus_Event.cs` (Positive — adds coins)
    - Create `Assets/Scripts/DailyCycle/Events/Neutral_Flavor_Event.cs` (Neutral — log-only placeholder)
    - Create `Assets/Scripts/DailyCycle/Events/Minor_Loss_Event.cs` (Negative — small coin loss with configurable max threshold)
    - Each event respects safety constraints (no NPC removal, no city regen)
    - Add temporary effect reset hook in Daily_Cycle_Manager.NextDay()
    - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5, 7.6, 8.1, 8.2, 8.3, 8.4, 8.5_

  - [ ]* 5.4 Write property test: Weighted Category Event Selection
    - **Property 5: Weighted Category Event Selection**
    - Test: For random weights (0.01–100 each) and random pool sizes, TriggerRandomEvent selects exactly one event, category matches weighted roll, uniform within category
    - Use [Test, Repeat(100)] with random weight/pool configurations
    - **Validates: Requirements 5.3, 6.1, 6.2, 6.3, 12.3**

  - [ ]* 5.5 Write property test: Event Execution Preserves World State
    - **Property 6: Event Execution Preserves World State**
    - Test: For any Daily_Event execution, NPCs remain unchanged, no City_Generator calls, vendors/challengers remain enabled
    - Use [Test, Repeat(100)] with mock events
    - **Validates: Requirements 7.1, 7.2, 7.3**

  - [ ]* 5.6 Write property test: Negative Event Resource Loss Bounded
    - **Property 7: Negative Event Resource Loss Bounded**
    - Test: For any Negative event, resource losses ≤ configured max threshold, debuff duration ≤ configured max (30s default)
    - Use [Test, Repeat(100)] with random loss values
    - **Validates: Requirements 7.5**

  - [ ]* 5.7 Write property test: Temporary Effects Reset on New Day
    - **Property 8: Temporary Effects Reset on New Day**
    - Test: For any temporary effect (movement speed, combat multiplier, vendor discount), NextDay resets to default — no debuff persists across days
    - Use [Test, Repeat(100)] with random effect configurations
    - **Validates: Requirements 7.6**

  - [ ]* 5.8 Write property test: World Preservation During Day Transition
    - **Property 3: World Preservation During Day Transition**
    - Test: For any day transition, only Day_Counter, player position/rotation, and event effect values change; all other GameObjects/NPCs/components unchanged
    - Use [Test, Repeat(100)] with scene state snapshots
    - **Validates: Requirements 2.3, 2.4, 8.1, 8.2, 8.3, 8.4, 8.5**

- [x] 6. Checkpoint — Stage 3 Validation
  - Ensure all tests pass, ask the user if questions arise.

- [x] 7. Stage 4 — Transition UI and Event Card
  - [x] 7.1 Implement Transition_UI
    - Create `Assets/Scripts/DailyCycle/Transition_UI.cs`
    - Add SerializeField references: _dayText (TMP_Text), _eventTitleText (TMP_Text), _eventDescriptionText (TMP_Text), _dayPanel (GameObject), _eventCardPanel (GameObject)
    - Implement ShowDaySummary(int dayNumber): activate panel, set "End of Day {dayNumber}"
    - Implement HideDaySummary(): deactivate panel
    - Implement ShowEventCard(string title, string description): activate card, set texts
    - Implement HideEventCard(): deactivate card
    - Implement HideAll(): deactivate both panels
    - Ensure overlay Canvas with highest sorting order
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5_

  - [x] 7.2 Wire Transition_UI into EndOfDayCoroutine
    - Connect _transitionUI calls in Daily_Cycle_Manager coroutine
    - ShowDaySummary after fade out, wait _daySummaryDuration, HideDaySummary
    - ShowEventCard after event execution, wait _eventCardDuration, HideEventCard
    - Handle null _transitionUI reference with warning log and skip
    - _Requirements: 4.3, 4.4, 9.1, 9.2, 9.3_

  - [ ]* 7.3 Write property test: Event Card Displays Correct Data
    - **Property 10: Event Card Displays Correct Data**
    - Test: For any Daily_Event with random title (≤60 chars) and description (≤200 chars), ShowEventCard receives and displays exact title and description without truncation
    - Use [Test, Repeat(100)] with random string generation
    - **Validates: Requirements 9.2**

  - [ ]* 7.4 Write property test: Event Bus Lifecycle Event Ordering
    - **Property 11: Event Bus Lifecycle Event Ordering**
    - Test: For any complete transition, events broadcast in order: day_ending → event_triggered → day_started → player_respawned → day_ended
    - Use [Test, Repeat(100)] with random day numbers and event configs
    - **Validates: Requirements 12.8**

- [x] 8. Checkpoint — Stage 4 Validation
  - Ensure all tests pass, ask the user if questions arise.

- [x] 9. Stage 5 — Editor Tools and Testing Utilities
  - [x] 9.1 Implement Daily_Cycle_Debug_Tools
    - Create `Assets/Editor/DailyCycle/Daily_Cycle_Debug_Tools.cs`
    - Implement menu item "Flipit → Daily Cycle → Force Next Day" (calls NextDay + RespawnPlayer)
    - Implement menu item "Flipit → Daily Cycle → Trigger Random Event" (calls TriggerRandomEvent)
    - Implement menu item "Flipit → Daily Cycle → Reset Day Counter" (sets Day_Counter to 1)
    - Implement menu item "Flipit → Daily Cycle → Go To Day..." (EditorInputDialog, int 1–9999)
    - Guard all items: if no Daily_Cycle_Manager instance, log warning and return
    - _Requirements: 13.1, 13.2, 13.3, 13.4, 13.5, 13.7_

  - [x] 9.2 Implement Custom Inspector for Daily_Cycle_Manager
    - Create `Assets/Editor/DailyCycle/Daily_Cycle_Manager_Editor.cs`
    - Display read-only: Current Day Counter, Last Event Name, Current Transition State
    - Use EditorGUILayout with disabled group for runtime fields
    - _Requirements: 13.6_

  - [ ]* 9.3 Write integration test: Full End-of-Day Sequence
    - Create PlayMode test that validates the complete transition sequence from EndCurrentDay through fade, event, respawn, and fade-in
    - Verify all Event Bus broadcasts fire in correct order
    - Verify player position matches spawn point after completion
    - _Requirements: 4.1–4.11, 12.1, 12.8_

  - [ ]* 9.4 Write unit tests for Editor Debug Tools
    - Test menu items call correct Daily_Cycle_Manager methods
    - Test null instance guard logs warning
    - Test Go To Day with boundary values (1, 9999)
    - _Requirements: 13.1–13.7_

- [x] 10. Final Checkpoint — Full System Validation
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints align with the 5 development stages defined in requirements
- Property tests use NUnit [Test, Repeat(100)] with randomized setup per iteration (no external PBT library)
- All runtime code lives in `Assets/Scripts/DailyCycle/` under the `Flipit.DailyCycle` namespace
- All editor code lives in `Assets/Editor/DailyCycle/` under the `Flipit.DailyCycle.Editor` namespace
- No existing assembly (Flipit.Dialogue, Flipit.Combat, Flipit.CityTerrain) is modified
- Random seed is logged on property test failure for reproduction

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1"] },
    { "id": 1, "tasks": ["1.2"] },
    { "id": 2, "tasks": ["1.3"] },
    { "id": 3, "tasks": ["1.4", "1.5", "1.6"] },
    { "id": 4, "tasks": ["3.1", "3.2"] },
    { "id": 5, "tasks": ["3.3"] },
    { "id": 6, "tasks": ["3.4", "3.5"] },
    { "id": 7, "tasks": ["5.1"] },
    { "id": 8, "tasks": ["5.2"] },
    { "id": 9, "tasks": ["5.3"] },
    { "id": 10, "tasks": ["5.4", "5.5", "5.6", "5.7", "5.8"] },
    { "id": 11, "tasks": ["7.1"] },
    { "id": 12, "tasks": ["7.2"] },
    { "id": 13, "tasks": ["7.3", "7.4"] },
    { "id": 14, "tasks": ["9.1", "9.2"] },
    { "id": 15, "tasks": ["9.3", "9.4"] }
  ]
}
```
