# Implementation Plan: City Terrain Generation

## Overview

Implement a modular, seed-based procedural city terrain generation system for Flipit in Unity 6 using C#. The system generates a navigable city scene using placeholder geometry through 5 incremental stages (core layout, urban props, NPC placement, building transparency, decorative variation), all executed in Edit Mode via an Editor menu item. The architecture separates pure algorithmic logic (grid, placement, pathfinding) from Unity-dependent code (GameObjects, MonoBehaviours) to enable property-based testing.

## Tasks

- [x] 1. Set up project structure, assembly definitions, and core data types
  - [x] 1.1 Create folder structure and assembly definitions
    - Create `Assets/Scripts/CityTerrain/` folder with `Flipit.CityTerrain.asmdef` (rootNamespace: `Flipit.CityTerrain`, references: `Flipit.Dialogue`, `Flipit.Combat`, `Unity.InputSystem`)
    - Create `Assets/Scripts/CityTerrain/Data/`, `Assets/Scripts/CityTerrain/Generators/`, `Assets/Scripts/CityTerrain/Runtime/`, `Assets/Scripts/CityTerrain/Utilities/` subfolders
    - Create `Assets/Editor/Flipit.CityTerrain.Editor.asmdef` (references: `Flipit.CityTerrain`, platform: Editor only)
    - Create `Assets/Tests/Editor/Flipit.CityTerrain.Tests.Editor.asmdef` with references to `Flipit.CityTerrain`, `Flipit.Dialogue`, `Flipit.Combat`, precompiled references `nunit.framework.dll`, `FsCheck.dll`, defineConstraints `UNITY_INCLUDE_TESTS`
    - _Requirements: 17.1, 17.2, 17.5_

  - [x] 1.2 Implement CellType enum and CellData struct
    - Create `Assets/Scripts/CityTerrain/Data/CellType.cs` with enum values: Street, Sidewalk, Building, Park, Empty
    - Create `Assets/Scripts/CityTerrain/Data/CellData.cs` with struct containing Row, Col, Type, IsOccupied, IsIntersection, OccupantTag properties; implement `WorldPosition(float cellSize)` and `ManhattanDistance(CellData other)` methods
    - _Requirements: 2.1, 2.3, 2.4_

  - [x] 1.3 Implement Grid_Layout data model
    - Create `Assets/Scripts/CityTerrain/Data/Grid_Layout.cs` with Width, Depth, CellSize properties and `CellData[,] Cells` array
    - Implement `GetCell`, `SetCellType`, `MarkOccupied`, `IsCellAvailable`, `GetCellsOfType`, `GetUnoccupiedCellsOfType`, `GetAdjacentCells`, `GetEdgeBuildingCells`, `IsOnEdge` methods
    - _Requirements: 2.1, 2.3, 2.4_

  - [x] 1.4 Implement City_Config ScriptableObject
    - Create `Assets/Scripts/CityTerrain/City_Config.cs` as a ScriptableObject with `[CreateAssetMenu(fileName = "NewCityConfig", menuName = "Flipit/City Config")]`
    - Add all serialized fields with proper attributes: grid settings (width, depth, cellSize, streetInterval), seed, player/camera params, building variation, prop densities, NPC counts, NPC references, transparency, colors
    - Implement public properties with clamping logic matching the design (Min, Clamp, Max constraints)
    - _Requirements: 15.1, 15.2, 15.3, 15.4, 15.5, 15.6, 15.7, 15.8, 15.9, 15.10, 15.11, 15.12, 15.13_

- [x] 2. Implement grid generation and pathfinding algorithms
  - [x] 2.1 Implement GridGenerator static class
    - Create `Assets/Scripts/CityTerrain/Generators/GridGenerator.cs`
    - Implement `AssignCellTypes(Grid_Layout grid, int streetInterval)`: streets at interval rows/columns, sidewalks adjacent to streets, remaining cells as Building
    - Implement `IsIntersection(int row, int col, int streetInterval)`: returns true if row AND col are both street positions
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.7_

  - [ ]* 2.2 Write property tests for grid generation (Properties 1–5)
    - **Property 1: Grid Cell Partition** — Every cell is assigned exactly one type and total equals width × depth
    - **Property 2: Street Connectivity** — All Street_Cells form a connected graph via orthogonal adjacency
    - **Property 3: Sidewalk Adjacency Invariant** — Every cell adjacent to a Street_Cell that isn't a Street_Cell is Sidewalk
    - **Property 4: Street Placement at Intervals** — Cell at (row, col) is Street iff (row % I == 0) or (col % I == 0)
    - **Property 5: Cell Height Correctness** — Streets at Y=0, Sidewalks at Y=0.05
    - Use FsCheck generators with random width 5–30, depth 5–30, interval 3–8
    - **Validates: Requirements 2.1, 2.2, 2.3, 2.4, 2.5, 2.6**

  - [x] 2.3 Implement PathFinder (BFS)
    - Create `Assets/Scripts/CityTerrain/Utilities/PathFinder.cs`
    - Implement `FindPath(Grid_Layout grid, CellData start, CellData end)`: BFS traversal on Street and Sidewalk cells, returns ordered list from start to end, empty list if unreachable
    - _Requirements: 3.2, 11.2, 12.1_

  - [x] 2.4 Implement LandmarkPlacer algorithm
    - Create `Assets/Scripts/CityTerrain/Generators/LandmarkPlacer.cs`
    - Implement `PlaceLandmarks(Grid_Layout grid)` returning `LandmarkResult` struct with SchoolCell, HouseCell, SchoolSpawnCell, HouseTriggerCell
    - Find Building_Cells on grid edges adjacent to Sidewalk_Cells, select opposite-edge pair with ≥ 60% diagonal distance
    - _Requirements: 3.1, 3.2, 3.5, 3.7_

  - [ ]* 2.5 Write property tests for landmarks and pathfinding (Properties 7–8)
    - **Property 7: Landmark Placement Constraints** — School on edge adjacent to sidewalk, house on opposite edge, distance ≥ 60% diagonal
    - **Property 8: School Spawn Point Position** — Spawn at center of adjacent Sidewalk_Cell at Y=0.05
    - Use FsCheck with random valid grids
    - **Validates: Requirements 3.1, 3.2, 3.5**

- [x] 3. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 4. Implement prop placement algorithms
  - [x] 4.1 Implement PropPlacer static class
    - Create `Assets/Scripts/CityTerrain/Generators/PropPlacer.cs`
    - Implement `PlaceTrees(Grid_Layout, System.Random, float sidewalkDensity, float parkDensity)`: place on Sidewalk and Park cells using density-based algorithm
    - Implement `PlaceCars(Grid_Layout, System.Random, float density)`: place on non-intersection Street_Cells adjacent to Sidewalk
    - Implement `PlaceLampposts(Grid_Layout, int streetInterval)`: place on 4 corner Sidewalk_Cells at each intersection
    - Implement `PlaceBenches(Grid_Layout, System.Random, float density)`: place on Sidewalk_Cells adjacent to Park_Cells
    - Implement `PlaceTrashCans(Grid_Layout, System.Random, float density, int minSpacing)`: place with minimum 3-cell Manhattan distance
    - All methods return `PlacementResult` struct with PlacedCells, RequestedCount, ActualCount, PropType
    - _Requirements: 7.1, 7.2, 7.4, 8.1, 8.2, 8.5, 9.1, 9.2, 9.3, 9.7, 9.8_

  - [ ]* 4.2 Write property tests for prop placement (Properties 13–17)
    - **Property 13: Cell Occupancy Invariant** — No cell has more than one occupant
    - **Property 14: Prop Placement on Valid Cell Types** — Trees on Sidewalk/Park, cars on eligible Streets, lampposts on intersection corners, benches adjacent to Park, trash cans on Sidewalk
    - **Property 15: Prop Density Count** — Placed count equals floor(eligible × density) or max available
    - **Property 16: Car Orientation Matches Street Direction** — 0°/180° on horizontal streets, 90°/270° on vertical streets
    - **Property 17: Trash Can Minimum Spacing** — Manhattan distance ≥ 3 between any two trash cans
    - **Validates: Requirements 7.1, 7.2, 7.4, 8.1, 8.2, 8.4, 8.5, 9.1, 9.2, 9.3, 9.7**

- [x] 5. Implement NPC and encounter placement algorithms
  - [x] 5.1 Implement NPCPlacer static class
    - Create `Assets/Scripts/CityTerrain/Generators/NPCPlacer.cs`
    - Implement `PlaceVendors(Grid_Layout, System.Random, int count, int streetInterval)`: place on Sidewalk adjacent to Building, max 1 per block, ≥ 2-cell clearance from other NPCs
    - Implement `PlaceChallengers(Grid_Layout, System.Random, int count, List<CellData> path)`: place evenly along path, ≥ 3-cell clearance from other NPCs
    - Both return `NPCPlacementResult` struct
    - _Requirements: 10.1, 10.2, 10.4, 11.1, 11.2, 11.4_

  - [x] 5.2 Implement EncounterPlacer static class
    - Create `Assets/Scripts/CityTerrain/Generators/EncounterPlacer.cs`
    - Implement `PlaceEncounters(Grid_Layout, System.Random, int count, List<CellData> path, CellData schoolCell, CellData houseCell)`: place on path cells with ≥ 3-cell spacing between encounters, ≥ 5-cell distance from landmarks, not on occupied cells
    - Return `EncounterPlacementResult` struct
    - _Requirements: 12.1, 12.2, 12.5, 12.7_

  - [ ]* 5.3 Write property tests for NPC and encounter placement (Properties 18–22)
    - **Property 18: NPC Placement Validity and Clearance** — Vendors on Sidewalk adjacent to Building with ≥ 2 clearance, max 1 per block; Challengers on Sidewalk with ≥ 3 clearance
    - **Property 19: NPC Component Setup** — Vendor has CapsuleCollider + NPC_Interactable; Challenger has CapsuleCollider + FlipCombat_NPC
    - **Property 20: Challenger Even Distribution Along Path** — Gaps between consecutive challengers within ±30% of average
    - **Property 21: Encounter Placement Constraints** — On path Street/Sidewalk, ≥ 3 from other encounters, ≥ 5 from landmarks
    - **Property 22: Encounter Invisibility** — No MeshRenderer/SpriteRenderer, has trigger BoxCollider
    - **Validates: Requirements 10.1, 10.2, 10.3, 10.4, 11.1, 11.2, 11.3, 11.4, 12.1, 12.2, 12.3, 12.5, 12.6**

- [x] 6. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 7. Implement GeometryFactory and City_Generator orchestrator
  - [x] 7.1 Implement GeometryFactory static class
    - Create `Assets/Scripts/CityTerrain/Generators/GeometryFactory.cs`
    - Implement all static methods: `CreateGroundPlane`, `CreateStreetCell`, `CreateSidewalkCell`, `CreateBuilding`, `CreateTree`, `CreateCar`, `CreateLamppost`, `CreateBench`, `CreateTrashCan`, `CreateNPCCapsule`
    - All created objects assigned to "CityGeometry" layer
    - Use unlit flat-color materials; buildings get BoxCollider, trees/lampposts/trash cans get CapsuleCollider, cars/benches get BoxCollider
    - _Requirements: 1.1, 1.2, 1.3, 2.5, 2.6, 6.1, 6.2, 6.3, 6.4, 7.3, 7.5, 8.3, 9.4, 9.5, 9.6_

  - [x] 7.2 Implement City_Generator MonoBehaviour (Stage 1 – Core Layout)
    - Create `Assets/Scripts/CityTerrain/City_Generator.cs`
    - Implement `ExecuteGeneration()` and `ClearGenerated()` methods
    - Implement `ExecuteStage1_CoreLayout()`: create hierarchy parents, initialize grid with GridGenerator, create ground plane, instantiate streets/sidewalks/buildings, place landmarks via LandmarkPlacer, spawn player, setup camera
    - Initialize `System.Random` from City_Config seed (random if 0)
    - Handle landmark placement failure (log error, abort Stage 1)
    - Apply fallback ground color if missing
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 2.1, 2.5, 2.6, 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 4.1, 5.1, 14.6, 17.4, 17.6_

  - [x] 7.3 Implement City_Generator Stages 2–5
    - Implement `ExecuteStage2_UrbanProps()`: use PropPlacer to place trees, cars, lampposts, benches, trash cans via GeometryFactory
    - Implement `ExecuteStage3_NPCPlacement()`: find path via PathFinder, use NPCPlacer for vendors/challengers, EncounterPlacer for surprise encounters; attach NPC_Interactable/FlipCombat_NPC/TriggerZone_Encounter components with config references
    - Implement `ExecuteStage4_Transparency()`: attach Building_Transparency_System to camera
    - Implement `ExecuteStage5_Variation()`: apply random building heights/colors, convert empty cells, apply tree rotations, apply prop scale variation
    - Implement `LogStageCompletion(int stage, Dictionary<string, int> counts)` for per-stage logging
    - _Requirements: 7.1, 7.2, 8.1, 8.2, 8.4, 9.1, 9.2, 9.3, 10.1, 10.2, 10.3, 11.1, 11.2, 11.3, 12.1, 12.2, 12.3, 12.4, 12.5, 12.6, 12.7, 13.1, 14.1, 14.2, 14.3, 14.4, 14.5, 14.6, 16.7, 16.8_

  - [ ]* 7.4 Write property tests for Stage 5 variation (Properties 27–31)
    - **Property 27: Building Variation Within Config Bounds** — Y-scale in [heightMin, heightMax], color from palette
    - **Property 28: Cell Conversion Adjacency Rule** — Converted cells are adjacent to existing Building_Cell, count = floor(eligible × percent)
    - **Property 29: Prop Scale Variation Bounds** — Uniform scale in [0.8, 1.2]
    - **Property 30: Tree Canopy Rotation Values** — Y-rotation in {0, 90, 180, 270}
    - **Property 31: Seed Determinism** — Same seed produces identical results across runs
    - **Validates: Requirements 14.1, 14.2, 14.3, 14.4, 14.5, 14.6**

- [x] 8. Implement runtime components (Player, Camera, Transparency)
  - [x] 8.1 Implement Player_Controller MonoBehaviour
    - Create `Assets/Scripts/CityTerrain/Runtime/Player_Controller.cs`
    - Add `[RequireComponent(typeof(CharacterController))]`
    - Implement movement on XZ plane: read Move action from New Input System Player action map as Vector2, apply as (x, 0, y) × moveSpeed × deltaTime via CharacterController.Move
    - Apply gravity (-9.81 m/s²) each frame
    - Zero XZ translation when no input
    - Configure CharacterController: radius 0.5, height 2.0, center (0, 1, 0)
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 6.5_

  - [x] 8.2 Implement Isometric_Camera MonoBehaviour
    - Create `Assets/Scripts/CityTerrain/Runtime/Isometric_Camera.cs`
    - Set orthographic projection with configurable orthoSize
    - Fixed rotation (30°X, 45°Y, 0°Z) — never changes
    - Follow player: interpolate toward target position (player + followOffset along camera back-vector) using configurable smooth speed
    - Expose target, followOffset, smoothSpeed, orthoSize serialized fields
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 5.6_

  - [x] 8.3 Implement Building_Transparency_System MonoBehaviour
    - Create `Assets/Scripts/CityTerrain/Runtime/Building_Transparency_System.cs`
    - Raycast from camera to player each frame on CityGeometry layer
    - Track occluding buildings with per-renderer FadeState (CurrentAlpha, TargetAlpha, Material)
    - Fade occluding buildings to configurable alpha (default 0.3) over fadeDuration (default 0.2s) using Mathf.MoveTowards
    - Restore non-occluding buildings to alpha 1.0 and revert material to opaque
    - Handle multiple simultaneous occluding buildings independently
    - No modification when no buildings occlude
    - _Requirements: 13.1, 13.2, 13.3, 13.4, 13.5, 13.6, 13.7_

  - [ ]* 8.4 Write property tests for runtime logic (Properties 9–12, 23–26)
    - **Property 9: Player Movement Vector Transformation** — Input (x,y) → movement (x, 0, y) × speed × dt
    - **Property 10: Camera Rotation Invariant** — Rotation always (30, 45, 0)
    - **Property 11: Camera Follow Target Position** — Target = player + offset along back-vector
    - **Property 12: Collider Type Correctness** — BoxCollider for buildings/cars/benches, CapsuleCollider for trees/lampposts/trash cans, all on CityGeometry layer
    - **Property 23: Building Transparency Round-Trip** — Occluded → transparent → not occluded → opaque (alpha 1.0)
    - **Property 24: Transparency Applies to All Occluding Buildings** — All simultaneously occluding buildings have reduced alpha
    - **Property 25: No Transparency Modification Without Occlusion** — No ray intersection → no material changes
    - **Property 26: Alpha Transition Continuity** — Linear interpolation, no instantaneous jumps
    - **Validates: Requirements 4.3, 4.5, 5.2, 5.3, 5.5, 6.1, 6.2, 6.3, 6.4, 13.2, 13.3, 13.5, 13.6, 13.7**

- [x] 9. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 10. Implement Editor integration and stage confirmation dialogs
  - [x] 10.1 Implement CityGeneratorBuilder Editor script
    - Create `Assets/Editor/CityGeneratorBuilder.cs`
    - Add `[MenuItem("Flipit/Generate City")]` static method `GenerateCity()`
    - Open or create CityExplorationScene, find or create City_Generator GameObject
    - Execute generation with `EditorUtility.DisplayDialog` confirmation between stages
    - Implement `ShowStageConfirmation(int stage, string summary)` returning bool
    - Save scene after completion via `EditorSceneManager.SaveScene`
    - Handle cancellation (preserve current state) and errors (show error dialog)
    - _Requirements: 16.1, 16.2, 16.3, 16.4, 16.5, 16.6, 16.7, 16.8, 16.9, 17.3, 17.5_

  - [x] 10.2 Implement House trigger and level-completion event
    - Add trigger collider to House_Landmark covering adjacent Sidewalk_Cell (cellSize × 1 × cellSize)
    - On Player_Controller trigger enter, fire level-completion event via Combat_Event_Bus
    - _Requirements: 3.6_

  - [ ]* 10.3 Write property tests for config clamping and hierarchy (Properties 32–34)
    - **Property 32: Config Value Clamping** — Out-of-range values clamped to nearest bound with warning
    - **Property 33: Hierarchy Organization** — All objects under correct root parent (Ground, Streets, Sidewalks, Buildings, Props, NPCs, Encounters)
    - **Property 34: Clean Regeneration** — Same config + seed after clear produces identical hierarchy
    - **Validates: Requirements 15.11, 15.12, 17.4, 17.6**

  - [ ]* 10.4 Write property test for Ground Plane Dimensions (Property 6)
    - **Property 6: Ground Plane Dimensions** — Width = gridWidth × cellSize, Depth = gridDepth × cellSize
    - **Validates: Requirements 1.1**

- [x] 11. Final checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation
- Property tests validate universal correctness properties using FsCheck (C#/.NET)
- Unit tests validate specific examples and edge cases
- Pure algorithm classes (GridGenerator, LandmarkPlacer, PropPlacer, NPCPlacer, EncounterPlacer, PathFinder) are testable without Unity runtime
- The City_Generator orchestrator ties everything together with GeometryFactory for GameObject creation
- All generation runs in Edit Mode — no runtime generation overhead

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1"] },
    { "id": 1, "tasks": ["1.2", "1.3", "1.4"] },
    { "id": 2, "tasks": ["2.1", "2.3"] },
    { "id": 3, "tasks": ["2.2", "2.4"] },
    { "id": 4, "tasks": ["2.5", "4.1"] },
    { "id": 5, "tasks": ["4.2", "5.1", "5.2"] },
    { "id": 6, "tasks": ["5.3", "7.1"] },
    { "id": 7, "tasks": ["7.2"] },
    { "id": 8, "tasks": ["7.3", "8.1", "8.2", "8.3"] },
    { "id": 9, "tasks": ["7.4", "8.4", "10.1", "10.2"] },
    { "id": 10, "tasks": ["10.3", "10.4"] }
  ]
}
```
