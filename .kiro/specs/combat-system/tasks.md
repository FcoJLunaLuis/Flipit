# Implementation Plan: Combat System

## Overview

Implementación incremental del sistema de combate para Flipit. Se construyen primero las interfaces y enums base, luego los componentes de combate (FlipCombat_NPC, Combat_System, Combat_Transition_UI), seguido del sistema de encuentros aleatorios, y finalmente la escena de combate y la integración completa. Cada paso se valida con tests antes de avanzar.

## Tasks

- [x] 1. Set up Combat namespace, core interfaces and enums
  - [x] 1.1 Create CombatState enum and Combat_Event_Bus static class
    - Create `Assets/Scripts/Combat/CombatState.cs` with enum values: Idle, DialogueOpen, ForcedEncounter, Transitioning
    - Create `Assets/Scripts/Combat/Combat_Event_Bus.cs` following the same pattern as Dialogue_Event_Bus (static HashSet, RaiseEvent, ClearAll, OnCombatEventRaised event)
    - Create `Assets/Scripts/Combat/Flipit.Combat.asmdef` assembly definition referencing Flipit.Dialogue
    - _Requirements: 6.5_

  - [x] 1.2 Create EncounterConfig ScriptableObject
    - Create `Assets/Scripts/Combat/EncounterConfig.cs` as a ScriptableObject with clamped serialized fields: CheckInterval [1,60], MinTimeBetweenEncounters [5,300], EncounterProbability [0,1], SpawnRadius [1,50], SpawnDistanceFromPlayer [2,4]
    - Use `[Range]` attributes and property accessors that enforce clamping in code
    - _Requirements: 5.1, 5.4_

  - [x]* 1.3 Write property test for configuration parameter clamping
    - **Property 6: Configuration parameters clamped to valid ranges**
    - **Validates: Requirements 5.1, 5.4, 3.2**
    - Generate random float values and verify EncounterConfig fields are always clamped to their specified ranges

- [x] 2. Implement FlipCombat_NPC and voluntary combat flow
  - [x] 2.1 Create FlipCombat_NPC class inheriting from NPC_Interactable
    - Create `Assets/Scripts/Combat/FlipCombat_NPC.cs` in namespace Flipit.Combat
    - Inherit from `Flipit.Dialogue.NPC_Interactable`
    - Add serialized fields: `_combatSceneName` (string), `_npcDisplayName` (string)
    - The DialogueData assigned to this NPC will contain accept/reject options
    - _Requirements: 1.1, 1.2, 6.1_

  - [x]* 2.2 Write property test for interaction prompt visibility
    - **Property 1: Interaction prompt visibility matches distance threshold**
    - **Validates: Requirements 1.1, 1.2**
    - Generate random player/NPC positions and verify prompt visibility is true iff distance <= 2.0

  - [x] 2.3 Create Combat_System singleton MonoBehaviour
    - Create `Assets/Scripts/Combat/Combat_System.cs` in namespace Flipit.Combat
    - Implement singleton pattern (same as Dialogue_Manager)
    - Serialized fields: Combat_Transition_UI reference, PlayerInput reference
    - Public properties: CurrentState (CombatState), Instance
    - Public methods: OnCombatAccepted(string combatSceneName), OnCombatRejected(), ForceCombatEncounter(RandomEncounter_NPC), DisablePlayerInput(), RestorePlayerInput()
    - State machine: Idle → DialogueOpen → Transitioning → Idle (on scene load) or Idle → ForcedEncounter → Transitioning → Idle
    - On accept: publish "combat_accepted" via Combat_Event_Bus, call Combat_Transition_UI.ShowTransition
    - On reject: restore PlayerInput to "Player" action map, set state to Idle
    - _Requirements: 1.3, 1.4, 1.5, 1.6, 1.7, 6.3, 6.4_

  - [x]* 2.4 Write property test for combat dialogue preconditions
    - **Property 2: Combat dialogue preconditions gate opening**
    - **Validates: Requirements 1.3, 1.7**
    - Generate all combinations of (playerInRange, dialogueManagerState) and verify dialogue only opens when playerInRange=true AND state=Idle

  - [x]* 2.5 Write property test for reject restores pre-dialogue state
    - **Property 3: Reject restores pre-dialogue state**
    - **Validates: Requirements 1.5, 6.3**
    - Generate random valid pre-dialogue states, simulate reject, verify action map is "Player" and Dialogue_Manager is Idle

- [x] 3. Implement Combat_Transition_UI
  - [x] 3.1 Create Combat_Transition_UI MonoBehaviour
    - Create `Assets/Scripts/Combat/Combat_Transition_UI.cs` in namespace Flipit.Combat
    - Serialized fields: Canvas _overlayCanvas, TMP_Text _challengeText, float _displayDuration [1.5, 3.0] default 2.0
    - Method ShowTransition(string sceneName, Action onComplete, Action onError): enables overlay Canvas, sets text to "RETO ACEPTADO", starts coroutine that waits _displayDuration then calls SceneManager.LoadSceneAsync
    - Method HideOverlay(): disables overlay Canvas
    - Error handling: try-catch around LoadSceneAsync, on failure call onError, log Debug.LogError, hide overlay
    - Validate scene name exists in Build Settings via SceneUtility.GetBuildIndexByScenePath before loading
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_

  - [x]* 3.2 Write unit tests for Combat_Transition_UI
    - Test overlay text is "RETO ACEPTADO"
    - Test displayDuration is clamped between 1.5 and 3.0
    - Test scene load failure invokes onError callback and hides overlay
    - _Requirements: 3.1, 3.2, 3.5_

- [x] 4. Checkpoint - Core voluntary combat flow
  - Ensure all tests pass, ask the user if questions arise.

- [x] 5. Implement Random Encounter system
  - [x] 5.1 Create RandomEncounter_NPC MonoBehaviour
    - Create `Assets/Scripts/Combat/RandomEncounter_NPC.cs` in namespace Flipit.Combat
    - Serialized fields: _combatSceneName (string), _autoAcceptTimeout (float, default 30)
    - Public properties: CombatSceneName, AutoAcceptTimeout, IsActive
    - Methods: Activate() sets IsActive=true, Deactivate() sets IsActive=false
    - On Activate: calls Combat_System.Instance.ForceCombatEncounter(this)
    - _Requirements: 2.1, 2.2, 2.3, 2.5_

  - [x] 5.2 Create Random_Encounter_Manager MonoBehaviour
    - Create `Assets/Scripts/Combat/Random_Encounter_Manager.cs` in namespace Flipit.Combat
    - Serialized fields: EncounterConfig reference or inline clamped fields (checkInterval, minTimeBetweenEncounters, encounterProbability, spawnRadius), RandomEncounter_NPC prefab, Transform[] spawnPoints
    - Public property: IsEncounterActive
    - Coroutine-based timer that calls EvaluateEncounter() every checkInterval seconds
    - EvaluateEncounter(): checks Dialogue_Manager.CurrentState == Idle AND !IsEncounterActive, rolls probability, finds valid spawn point, instantiates prefab, calls Activate()
    - FindValidSpawnPoint(): filters spawn points within spawnRadius of player, validates no collider obstruction via Physics2D.OverlapCircle(point, 0.5f), returns random valid point or null
    - If no valid spawn point found: log Debug.LogWarning, cancel encounter
    - Enforce minTimeBetweenEncounters cooldown between spawns
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 2.6_

  - [x] 5.3 Implement forced encounter flow in Combat_System
    - Add ForceCombatEncounter(RandomEncounter_NPC) implementation: set state to ForcedEncounter, DisablePlayerInput(), show forced dialogue with single "Aceptar combate" option
    - Start auto-accept coroutine (30s timeout) that auto-invokes OnCombatAccepted if player doesn't respond
    - Cancel timeout coroutine if player accepts manually
    - _Requirements: 2.2, 2.3, 2.4, 2.7_

  - [x]* 5.4 Write property test for random encounter spawn position validity
    - **Property 4: Random encounter spawn position is valid**
    - **Validates: Requirements 2.1**
    - Generate random player positions and spawn point arrays, verify selected spawn point distance is between 2.0 and 4.0 units and not obstructed

  - [x]* 5.5 Write property test for encounter suppression guard
    - **Property 5: Encounter suppression guard**
    - **Validates: Requirements 2.6, 5.3**
    - Generate random Dialogue_Manager states and IsEncounterActive flags, verify no spawn occurs unless state=Idle AND IsEncounterActive=false

  - [x]* 5.6 Write property test for spawn point selection within radius
    - **Property 8: Spawn point selection within configured radius**
    - **Validates: Requirements 5.2, 5.5**
    - Generate random player positions, spawn radius values, and spawn point arrays; verify selected point is within radius or no spawn occurs

- [x] 6. Checkpoint - Random encounter system
  - Ensure all tests pass, ask the user if questions arise.

- [x] 7. Create Combat Scene and layout
  - [x] 7.1 Create CombatScene with city layout
    - Create new Unity scene `Assets/Scenes/CombatScene.unity`
    - Set up city layout with collision boundaries (BoxCollider2D or edge colliders) preventing player from exiting playable area
    - Place minimum 4 FlipCombat_NPC instances outside buildings, each with a Collider2D
    - Place minimum 3 spawn point Transforms for RandomEncounter_NPC, distributed with at least 5 units separation
    - Create a designated player entry point (empty GameObject "PlayerSpawnPoint")
    - Add scene to Build Settings
    - _Requirements: 4.1, 4.2, 4.3, 4.4_

  - [x] 7.2 Create player spawning logic for Combat Scene
    - Add spawn logic in Combat_System or a CombatScene_Setup MonoBehaviour: on scene load, place Player at PlayerSpawnPoint position
    - Ensure Player has TopDownPlayerMovement and Player_Interactor components active
    - Configure PlayerInput with "Player" action map and SendMessages mode
    - _Requirements: 4.4, 4.5_

  - [x]* 7.3 Write property test for spawn point minimum separation
    - **Property 7: Spawn points maintain minimum separation**
    - **Validates: Requirements 4.3**
    - Generate sets of spawn point positions and verify every pair has distance >= 5.0 units

  - [x]* 7.4 Write unit tests for Combat Scene structure
    - Test CombatScene contains >= 4 FlipCombat_NPC instances
    - Test CombatScene contains >= 3 spawn points
    - Test player spawns at entry point with correct components
    - _Requirements: 4.2, 4.3, 4.4_

- [x] 8. Integration and wiring
  - [x] 8.1 Wire FlipCombat_NPC dialogue callbacks to Combat_System
    - Create DialogueData assets for FlipCombat_NPCs with accept/reject structure (Line 0: challenge text with two options, Line 1: [COMBAT_ACCEPT] marker, Line 2: [COMBAT_REJECT] marker)
    - Implement interception logic in Combat_System: subscribe to Dialogue_Manager typewriter/line events, detect [COMBAT_ACCEPT] marker text, trigger OnCombatAccepted
    - Implement [COMBAT_REJECT] marker detection to trigger OnCombatRejected
    - Publish "combat_dialogue_started" and "combat_dialogue_ended" events via Dialogue_Event_Bus
    - _Requirements: 1.3, 1.4, 1.5, 6.2, 6.5_

  - [x] 8.2 Wire Combat_Transition_UI into Combat_System accept flow
    - In Combat_System.OnCombatAccepted: set state to Transitioning, call Combat_Transition_UI.ShowTransition with target scene name
    - On ShowTransition completion: scene loads automatically
    - On ShowTransition error: call RestorePlayerInput(), set state to Idle, log error
    - _Requirements: 3.1, 3.3, 3.5, 6.4_

  - [x] 8.3 Wire Random_Encounter_Manager into main scene
    - Add Random_Encounter_Manager component to a persistent GameObject in DialogueDemoScene
    - Configure spawn points array with Transform references
    - Set EncounterConfig values (or use ScriptableObject reference)
    - Ensure it suppresses encounters during active dialogues
    - _Requirements: 5.1, 5.2, 5.3_

  - [x]* 8.4 Write property test for combat events order
    - **Property 9: Combat events published in correct order**
    - **Validates: Requirements 6.5**
    - Simulate complete combat dialogue sessions and verify "combat_dialogue_started" is always published before "combat_dialogue_ended" with exactly one of each per session

  - [x]* 8.5 Write integration tests for full combat flows
    - Test full accept flow: interact → dialogue → accept → overlay → scene load callback
    - Test full reject flow: interact → dialogue → reject → normal state restored
    - Test random encounter flow: spawn → forced dialogue → accept → transition
    - _Requirements: 1.3, 1.4, 1.5, 2.1, 2.3, 2.4, 3.1, 3.3_

- [x] 9. Final checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation
- Property tests validate universal correctness properties from the design document
- Unit tests validate specific examples and edge cases
- All scripts use namespace `Flipit.Combat` and follow existing conventions (PascalCase methods, _camelCase private fields)
- The project uses Unity 6000.5.4f with URP, C#, and the new Input System
- Assembly definition (asmdef) references Flipit.Dialogue to access existing types

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1", "1.2"] },
    { "id": 1, "tasks": ["1.3", "2.1"] },
    { "id": 2, "tasks": ["2.2", "2.3", "3.1"] },
    { "id": 3, "tasks": ["2.4", "2.5", "3.2"] },
    { "id": 4, "tasks": ["5.1", "5.2"] },
    { "id": 5, "tasks": ["5.3", "5.4", "5.5", "5.6"] },
    { "id": 6, "tasks": ["7.1"] },
    { "id": 7, "tasks": ["7.2", "7.3", "7.4"] },
    { "id": 8, "tasks": ["8.1", "8.2", "8.3"] },
    { "id": 9, "tasks": ["8.4", "8.5"] }
  ]
}
```
