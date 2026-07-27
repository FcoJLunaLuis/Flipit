# Implementation Plan: Exploration Validation

## Overview

This plan implements the exploration validation system for Flipit's procedurally generated city. It adds smooth movement, camera zoom, clipping prevention, validation gizmos, a runtime orchestrator, and an Editor Window — all composing with existing `Player_Controller`, `Isometric_Camera`, and `Building_Transparency_System` components. Implementation uses C# targeting the Flipit.CityTerrain runtime assembly and NUnit + PropertyTestUtility for testing.

## Tasks

- [ ] 1. Implement Smooth_Movement MonoBehaviour
  - [ ] 1.1 Create `Assets/Scripts/CityTerrain/Runtime/Smooth_Movement.cs`
    - Implement the `Smooth_Movement` MonoBehaviour in the `Flipit.CityTerrain` namespace
    - Add `[RequireComponent(typeof(Player_Controller))]` and `[RequireComponent(typeof(CharacterController))]`
    - Implement serialized fields: `_maxSpeed` (5f), `_acceleration` (20f), `_deceleration` (15f), `_deadZoneThreshold` (0.01f)
    - Implement `OnMove(InputValue value)` to capture input from New Input System
    - Implement `Update()` with the smooth movement algorithm: linear acceleration/deceleration, dead zone snap-to-zero, gravity, and `CharacterController.Move`
    - Expose `CurrentVelocity` and `MaxSpeed` as public read-only properties for testing
    - Disable `Player_Controller.enabled` in `Awake()` to prevent double-move
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5_

  - [ ]* 1.2 Write property tests for Smooth_Movement (Properties 2-5)
    - Create `Assets/Tests/EditMode/SmoothMovementTests.cs`
    - **Property 2: Velocity accelerates linearly with input** — For random acceleration rates and max speeds, verify velocity increases by (acceleration × deltaTime) per simulated frame
    - **Validates: Requirements 2.1**
    - **Property 3: Velocity decelerates linearly without input** — For random deceleration rates and starting velocities, verify velocity decreases by (deceleration × deltaTime) per frame
    - **Validates: Requirements 2.2**
    - **Property 4: Velocity magnitude never exceeds max speed** — For random input sequences, verify horizontal velocity magnitude ≤ maxSpeed
    - **Validates: Requirements 2.4**
    - **Property 5: Micro-velocity snaps to zero** — For any velocity below 0.01, verify it snaps to exactly zero
    - **Validates: Requirements 2.5**

- [ ] 2. Implement Camera_Zoom MonoBehaviour
  - [ ] 2.1 Create `Assets/Scripts/CityTerrain/Runtime/Camera_Zoom.cs`
    - Implement the `Camera_Zoom` MonoBehaviour in the `Flipit.CityTerrain` namespace
    - Add `[RequireComponent(typeof(Camera))]`
    - Implement serialized fields: `_zoomStep` (0.5f), `_minOrthoSize` (3f), `_maxOrthoSize` (20f), `_smoothDuration` (0.15f)
    - Implement `OnScroll(InputValue value)` to read mouse scroll wheel delta
    - Implement `Update()` with smooth zoom: adjust `_targetOrthoSize` on input, lerp `camera.orthographicSize` toward target each frame
    - Expose `TargetOrthoSize`, `MinOrthoSize`, `MaxOrthoSize` as public read-only properties
    - _Requirements: 3.3, 3.4, 3.5_

  - [ ]* 2.2 Write property test for Camera_Zoom (Property 8)
    - Create `Assets/Tests/EditMode/CameraZoomTests.cs`
    - **Property 8: Zoom orthographic size clamped within bounds** — For random scroll input sequences (positive/negative, any magnitude), verify orthographicSize always remains within [minOrthoSize, maxOrthoSize]
    - **Validates: Requirements 3.3, 3.4**

- [ ] 3. Implement Camera_Clipping_Prevention MonoBehaviour
  - [ ] 3.1 Create `Assets/Scripts/CityTerrain/Runtime/Camera_Clipping_Prevention.cs`
    - Implement the `Camera_Clipping_Prevention` MonoBehaviour in the `Flipit.CityTerrain` namespace
    - Implement serialized fields: `_player` (Transform), `_sphereCastRadius` (0.3f), `_offsetFromHit` (0.5f), `_smoothSpeed` (5f), `_cityGeometryLayer` (LayerMask)
    - Implement `LateUpdate()` with clipping prevention algorithm: SphereCast from player toward desired camera position, adjust position if hit, smooth return when clear
    - Use `[DefaultExecutionOrder]` to run after Isometric_Camera
    - _Requirements: 4.1, 4.2, 4.3, 4.4_

  - [ ]* 3.2 Write property test for Camera_Clipping_Prevention (Property 9)
    - Create `Assets/Tests/EditMode/CameraClippingTests.cs`
    - **Property 9: Camera positioned before obstacles on clipping** — For any building collider between player and desired camera position, verify camera is placed at (hitDistance − 0.5) from player, remaining outside geometry
    - **Validates: Requirements 4.2**

- [ ] 4. Checkpoint - Ensure all movement and camera tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 5. Implement Validation_Gizmo_Renderer MonoBehaviour
  - [ ] 5.1 Create `Assets/Scripts/CityTerrain/Runtime/Validation_Gizmo_Renderer.cs`
    - Implement the `Validation_Gizmo_Renderer` MonoBehaviour in the `Flipit.CityTerrain` namespace
    - Implement serialized fields: `_spawnPoint` (Transform), `_navigationBounds` (Vector2), `_cameraTarget` (Transform), `_cityGeometryLayer` (LayerMask)
    - Implement `OnDrawGizmos()`: green sphere (radius 1.0) at spawn point, yellow wireframe rectangle for navigation bounds
    - Implement `OnDrawGizmosSelected()`: cyan wireframe cubes for building BoxColliders, magenta sphere (radius 0.5) at camera target
    - Expose `NavigationBounds` as public get/set property for Editor configuration
    - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5_

  - [ ]* 5.2 Write property test for Validation_Gizmo_Renderer (Property 11)
    - Create `Assets/Tests/EditMode/ValidationGizmoTests.cs`
    - **Property 11: Navigation bounds gizmo matches grid dimensions** — For any grid configuration (random width, depth, cellSize), verify the gizmo rectangle dimensions equal (width × cellSize, depth × cellSize)
    - **Validates: Requirements 7.2**

- [ ] 6. Implement Exploration_Validator MonoBehaviour (Runtime Orchestrator)
  - [ ] 6.1 Create `Assets/Scripts/CityTerrain/Runtime/Exploration_Validator.cs`
    - Implement the `Exploration_Validator` MonoBehaviour in the `Flipit.CityTerrain` namespace
    - Implement serialized fields: `_moveSpeed` (5f), `_acceleration` (20f), `_deceleration` (15f), `_cameraSmoothSpeed` (5f), `_cameraFollowOffset` (10f), `_cameraOrthoSize` (8f)
    - Implement `Awake()`: locate SpawnPoint (child of School_Landmark), position player, validate all required components are present
    - Implement `LocateAndSpawnPlayer()`: find PlayerSpawn, set player position/rotation, ensure CharacterController params (radius 0.5, height 2.0, center (0,1,0))
    - Implement `ValidateComponents()`: verify Player_Controller, Smooth_Movement, Isometric_Camera, Camera_Zoom, Camera_Clipping_Prevention, Building_Transparency_System exist
    - Expose `IsInitialized` as public read-only property
    - Handle error cases: missing SpawnPoint (log error, abort), missing CharacterController (add with defaults, log warning)
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 9.1, 9.2, 9.3_

  - [ ]* 6.2 Write property test for Exploration_Validator spawn (Property 1)
    - Create `Assets/Tests/EditMode/ExplorationValidatorTests.cs`
    - **Property 1: Player spawns at PlayerSpawn position** — For any scene with SpawnPoint at random positions, verify Player_Controller world position equals SpawnPoint world position after initialization
    - **Validates: Requirements 1.1**

  - [ ]* 6.3 Write property test for camera rotation invariance (Property 6)
    - Add to `Assets/Tests/EditMode/ExplorationValidatorTests.cs`
    - **Property 6: Camera rotation remains fixed** — For random player positions, zoom levels, and frame counts, verify Isometric_Camera rotation remains exactly (30°, 45°, 0°)
    - **Validates: Requirements 3.1**

  - [ ]* 6.4 Write property test for validation mode exclusions (Property 12)
    - Add to `Assets/Tests/EditMode/ExplorationValidatorTests.cs`
    - **Property 12: Validation mode excludes NPC, combat, and dialogue** — For any city generated in validation mode, verify zero GameObjects with NPC_Interactable, FlipCombat_NPC, TriggerZone_Encounter, Dialogue_Manager, or Combat_System components
    - **Validates: Requirements 9.1, 9.2, 9.3**

- [ ] 7. Checkpoint - Ensure orchestrator and gizmo tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 8. Implement Exploration_Validator_Window EditorWindow
  - [ ] 8.1 Create `Assets/Editor/ExplorationValidatorWindow.cs`
    - Implement the `ExplorationValidatorWindow` EditorWindow (no namespace, matching CityGeneratorBuilder pattern)
    - Add `[MenuItem("Flipit/Validate Exploration")]` on `ShowWindow()` static method
    - Implement `OnGUI()` with status labels (`_cityGenerated`, `_playerSpawned`, `_cameraPositioned`) and buttons for each step
    - Implement `GenerateCity()`: open/create CityExplorationScene, find/create City_Generator, call PrepareGeneration + ExecuteStage1 + ExecuteStage2, apply decorative variation (heights/colors), attach Building_Transparency_System
    - Implement `SpawnPlayer()`: locate SpawnPoint, create/reposition Player with CharacterController, PlayerInput, Player_Controller, Smooth_Movement
    - Implement `PositionCamera()`: create/reposition Isometric_Camera with Camera_Zoom and Camera_Clipping_Prevention
    - Implement `StartValidation()`: attach Exploration_Validator + Validation_Gizmo_Renderer, enter Play Mode via `EditorApplication.isPlaying = true`
    - Implement `ValidateSceneExists()`: check CityExplorationScene exists, show error dialog if not
    - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5, 8.6, 8.7, 9.4, 9.5_

- [ ] 9. Write remaining property and unit tests
  - [ ]* 9.1 Write property test for camera follow convergence (Property 7)
    - Add to `Assets/Tests/EditMode/ExplorationValidatorTests.cs`
    - **Property 7: Camera follows player via Lerp** — For random player positions and smooth speeds, verify camera position converges toward (playerPosition + isometricOffset) each frame
    - **Validates: Requirements 3.2**

  - [ ]* 9.2 Write property test for transparency round trip (Property 10)
    - Create `Assets/Tests/EditMode/TransparencyRoundTripTests.cs`
    - **Property 10: Occlusion transparency round trip** — For any building that becomes/stops occluding, verify alpha converges to target (0.3) when occluding and back to 1.0 when clear, independently for multiple buildings
    - **Validates: Requirements 5.2, 5.3, 5.4**

  - [ ]* 9.3 Write unit tests for edge cases and integration
    - Add unit tests across test files covering: SpawnPoint missing → error logged (Req 1.2), CharacterController params verified (Req 1.3), player Y-rotation = 0 (Req 1.4), zoom smooth transition not instant (Req 3.5), clipping recovery when obstruction clears (Req 4.3), gizmo draws at spawn position (Req 7.1), Editor Window menu item exists (Req 8.1), missing scene → error (Req 8.7), only Stages 1-2 executed (Req 9.4)
    - _Requirements: 1.2, 1.3, 1.4, 3.5, 4.3, 7.1, 8.1, 8.7, 9.4_

- [ ] 10. Final checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation
- Property tests validate universal correctness properties using the existing `PropertyTestUtility.ForAll` infrastructure (100 iterations per property, seeded Random)
- Unit tests validate specific scenarios and edge cases
- All runtime scripts go in `Assets/Scripts/CityTerrain/Runtime/` under the `Flipit.CityTerrain` namespace (existing assembly definition)
- The Editor Window goes in `Assets/Editor/` without a namespace (matching CityGeneratorBuilder pattern)
- Test files go in `Assets/Tests/EditMode/` using NUnit and `[Category("Property")]` for PBT tests
- The existing `Building_Transparency_System` is reused as-is; only test coverage is new

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1", "2.1", "3.1", "5.1"] },
    { "id": 1, "tasks": ["1.2", "2.2", "3.2", "5.2"] },
    { "id": 2, "tasks": ["6.1"] },
    { "id": 3, "tasks": ["6.2", "6.3", "6.4"] },
    { "id": 4, "tasks": ["8.1"] },
    { "id": 5, "tasks": ["9.1", "9.2", "9.3"] }
  ]
}
```
