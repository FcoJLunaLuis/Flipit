# Requirements Document

## Introduction

A modular city terrain generation system for "Flipit", a third-person isometric RPG built in Unity 6. The system procedurally generates a believable city environment using placeholder geometry (cubes, cylinders, planes) through which a child protagonist navigates from school to home. The city supports commercial NPCs, challenger NPCs, and surprise encounter events placed naturally along streets and sidewalks. The generation is divided into five incremental stages: core layout, urban props, NPC placement, building transparency, and decorative variation.

## Glossary

- **City_Generator**: The top-level MonoBehaviour responsible for orchestrating city terrain generation across all stages.
- **Grid_Layout**: A data structure defining the city as a 2D grid of cells, where each cell is assigned a type (street, sidewalk, building, park, landmark).
- **Cell**: A single unit in the Grid_Layout representing a fixed-size square area of the city (default 4x4 world units).
- **Street_Cell**: A Cell type representing a road surface for vehicle traffic.
- **Sidewalk_Cell**: A Cell type representing a pedestrian walkway adjacent to streets.
- **Building_Cell**: A Cell type representing a building footprint.
- **Park_Cell**: A Cell type representing a green space area with trees and benches.
- **Ground_Plane**: A flat 3D quad mesh representing the base terrain surface of the city.
- **School_Landmark**: The building designated as the player spawn point and route start.
- **House_Landmark**: The building designated as the player destination and route end.
- **Isometric_Camera**: A camera positioned at a fixed isometric angle (45° rotation on Y-axis, 30° tilt on X-axis) that follows the player without rotation.
- **Player_Controller**: A 3D CharacterController-based MonoBehaviour that reads WASD input and moves the player on the XZ plane using the existing New Input System action maps.
- **Placeholder_Building**: A scaled cube primitive representing a building placeholder.
- **Placeholder_Tree**: A composite of a cylinder (trunk) and sphere (canopy) representing a tree placeholder.
- **Placeholder_Car**: A scaled cube primitive with a smaller cube on top representing a parked vehicle placeholder.
- **Placeholder_Lamppost**: A thin cylinder with a sphere on top representing a street light placeholder.
- **Placeholder_Bench**: A flattened cube primitive representing a bench placeholder.
- **Placeholder_TrashCan**: A small cylinder primitive representing a trash can placeholder.
- **Vendor_NPC**: An NPC placeholder that sells items to the player, placed on sidewalk cells near buildings.
- **Challenger_NPC**: An NPC placeholder that offers optional combat encounters, placed on sidewalk cells.
- **Surprise_Encounter**: A hidden trigger zone that forces the player into combat when entered, placed on street or sidewalk cells.
- **Building_Transparency_System**: A system that detects when buildings occlude the player from the Isometric_Camera and applies transparency to those buildings.
- **Prop_Distribution**: A system that distributes urban props (trees, cars, lampposts, benches, trash cans) across valid cells following density rules.
- **City_Config**: A ScriptableObject containing all configurable parameters for city generation (grid size, cell size, density values, building heights, colors).

## Requirements

### Requirement 1: Ground Plane Generation

**User Story:** As a developer, I want the system to generate a ground plane so that the city has a base surface for all other elements.

#### Acceptance Criteria

1. WHEN the City_Generator executes Stage 1, THE City_Generator SHALL create a Ground_Plane as a single-quad mesh oriented on the XZ plane (normal facing Y-up) at world position (0, 0, 0) with width equal to Grid_Layout columns multiplied by cell size and depth equal to Grid_Layout rows multiplied by cell size.
2. THE Ground_Plane SHALL use an unlit flat-color material with its color set to the ground plane color field defined in the City_Config, so that lighting does not affect the base surface appearance.
3. THE Ground_Plane SHALL have a MeshCollider component attached with its mesh matching the Ground_Plane geometry so that raycasts and physics interactions resolve against the surface.
4. IF the City_Config ground plane color field is not assigned (default black or alpha zero), THEN THE City_Generator SHALL apply a fallback color of middle gray (0.5, 0.5, 0.5, 1.0) and log a warning to the Unity Console indicating the missing configuration.

### Requirement 2: Grid-Based Street Layout

**User Story:** As a developer, I want the system to generate a grid of streets so that the city has a navigable road network.

#### Acceptance Criteria

1. WHEN the City_Generator executes Stage 1, THE City_Generator SHALL generate a Grid_Layout of configurable dimensions (default 10x10 cells, minimum 5x5 cells) with streets forming continuous rows and columns that span the full width and depth of the grid respectively, ensuring all Street_Cells are reachable from any other Street_Cell.
2. THE City_Generator SHALL place Street_Cells in full rows and full columns at configurable intervals (default every 4 cells, minimum every 3 cells) defined in City_Config, forming a grid of intersecting roads where each street row extends from column 0 to the last column and each street column extends from row 0 to the last row.
3. THE City_Generator SHALL place Sidewalk_Cells in every cell directly adjacent (perpendicular to the street direction) to a Street_Cell, provided that cell is not itself a Street_Cell.
4. THE City_Generator SHALL assign all remaining cells that are not Street_Cells or Sidewalk_Cells as Building_Cells.
5. WHEN a Street_Cell is instantiated, THE City_Generator SHALL create a flat quad of Cell size (default 4x4 world units) at the cell position at Y=0 with the street color defined in City_Config.
6. WHEN a Sidewalk_Cell is instantiated, THE City_Generator SHALL create a flat quad of Cell size (default 4x4 world units) at the cell position raised 0.05 units above Y=0 with the sidewalk color defined in City_Config.
7. IF the configured street interval is less than 3, THEN THE City_Generator SHALL clamp the interval to 3 and log a warning, ensuring at least one row of Building_Cells exists between parallel streets.

### Requirement 3: School and House Landmarks

**User Story:** As a player, I want to see a distinct school building and house so that I know where my journey starts and ends.

#### Acceptance Criteria

1. WHEN the City_Generator executes Stage 1, THE City_Generator SHALL place the School_Landmark on a Building_Cell at one edge of the Grid_Layout (first or last row, or first or last column) adjacent to a Sidewalk_Cell.
2. WHEN the City_Generator executes Stage 1, THE City_Generator SHALL place the House_Landmark on a Building_Cell at the opposite edge of the Grid_Layout (if School_Landmark is on the first row, House_Landmark is on the last row; if on the first column, on the last column) adjacent to a Sidewalk_Cell, ensuring a minimum Euclidean distance of 60% of the grid diagonal (sqrt((gridWidth * cellSize)² + (gridDepth * cellSize)²)) between the two landmarks' world positions.
3. THE School_Landmark SHALL be represented as a Placeholder_Building with a distinct color defined in City_Config and a horizontal scale of 1.5 × cellSize on both the X and Z axes, making it 1.5x larger than standard buildings (which occupy 1 × cellSize on each horizontal axis).
4. THE House_Landmark SHALL be represented as a Placeholder_Building with a distinct color defined in City_Config and a scale matching standard buildings (1 × cellSize on each horizontal axis).
5. THE School_Landmark SHALL have a designated spawn point (empty GameObject) positioned at the center of the adjacent Sidewalk_Cell (at sidewalk surface height) where the player appears at scene start.
6. THE House_Landmark SHALL have a trigger collider sized to cover the full adjacent Sidewalk_Cell area (cellSize × 1 × cellSize) that, WHEN the Player_Controller enters the trigger, fires a level-completion event via the Combat_Event_Bus so that listening systems can handle scene transition or end-of-level logic.
7. IF the City_Generator cannot find valid Building_Cells satisfying the edge-adjacency and minimum-distance constraints for both landmarks, THEN THE City_Generator SHALL log an error message indicating the placement failure and abort Stage 1 generation without placing either landmark.

### Requirement 4: Player Controller Setup

**User Story:** As a player, I want to move my character on the ground plane using WASD so that I can explore the city.

#### Acceptance Criteria

1. WHEN the City_Generator executes Stage 1, THE City_Generator SHALL instantiate a Player_Controller at the School_Landmark spawn point position.
2. THE Player_Controller SHALL use a CharacterController component (radius 0.5, height 2.0, center at (0, 1, 0)) to handle movement and collisions in 3D space; no separate CapsuleCollider is required since CharacterController provides collision detection.
3. WHILE the Player_Controller receives Move input from the Player action map, THE Player_Controller SHALL translate the character on the XZ plane at a configurable speed (default 5 units per second) defined in City_Config, reading the Move action as a Vector2 and applying it as (x, 0, y) movement direction.
4. THE Player_Controller SHALL apply gravity of -9.81 m/s² to the character each frame via CharacterController.Move so that the character remains grounded on the Ground_Plane.
5. WHILE the Player_Controller receives no Move input, THE Player_Controller SHALL not translate the character on the XZ plane.
6. THE Player_Controller SHALL use the existing New Input System Player action map for movement input.

### Requirement 5: Isometric Camera Setup

**User Story:** As a player, I want a fixed isometric camera that follows my character so that I can see the city from a consistent perspective.

#### Acceptance Criteria

1. WHEN the City_Generator executes Stage 1, THE City_Generator SHALL instantiate an Isometric_Camera configured with orthographic projection.
2. WHILE the CityExplorationScene is active, THE Isometric_Camera SHALL maintain a fixed rotation of 45 degrees on the Y-axis and 30 degrees on the X-axis.
3. THE Isometric_Camera SHALL follow the Player_Controller position each frame by interpolating toward a target position offset 10 units along the camera's back-vector from the player, using linear interpolation at a configurable speed (default: 5 units per second) defined in City_Config.
4. THE Isometric_Camera SHALL expose the orthographic size as a configurable float field in City_Config with a default value of 8 and a minimum value of 1.
5. THE Isometric_Camera SHALL not rotate in response to player input or movement direction.
6. THE Isometric_Camera SHALL expose the follow offset distance as a configurable float field in City_Config with a default value of 10 and a minimum value of 1.

### Requirement 6: Collision System

**User Story:** As a player, I want to collide with buildings and obstacles so that I cannot walk through solid objects.

#### Acceptance Criteria

1. WHEN the City_Generator instantiates a Placeholder_Building, THE City_Generator SHALL add a BoxCollider matching the building's scale so that the Player_Controller cannot pass through.
2. WHEN the City_Generator instantiates a composite prop (Placeholder_Tree, Placeholder_Car, Placeholder_Lamppost), THE City_Generator SHALL add a single BoxCollider to the prop's root GameObject sized to encompass all child geometry, using the following shape-to-collider mapping: BoxCollider for cube-based props (Placeholder_Car), CapsuleCollider for cylinder-based props (Placeholder_Lamppost, Placeholder_Tree).
3. WHEN the City_Generator instantiates a single-primitive prop (Placeholder_Bench, Placeholder_TrashCan), THE City_Generator SHALL add a BoxCollider for Placeholder_Bench and a CapsuleCollider for Placeholder_TrashCan, each sized to match the prop's defined dimensions.
4. THE City_Generator SHALL assign all generated buildings and props to a Physics Layer named "CityGeometry" so that collision filtering can be configured.
5. IF the Player_Controller contacts a collider on the CityGeometry layer, THEN THE Player_Controller SHALL stop movement in the direction of contact and slide along the surface.

### Requirement 7: Urban Prop Placement — Trees

**User Story:** As a developer, I want the system to place tree placeholders so that the city has vegetation for visual variety.

#### Acceptance Criteria

1. WHEN the City_Generator executes Stage 2, THE City_Generator SHALL instantiate Placeholder_Tree objects on Sidewalk_Cells and Park_Cells by randomly selecting cells using the City_Config seed value until the configured density is reached.
2. THE City_Generator SHALL place trees at a configurable density (default: 1 tree per 3 Sidewalk_Cells, 1 tree per 2 Park_Cells) defined in City_Config, rounding down fractional tree counts.
3. THE Placeholder_Tree SHALL consist of a cylinder (radius 0.15, height 1.5) as trunk and a sphere (radius 0.5) with its center positioned at Y-offset 2.0 from the cell surface as canopy, placed at the center of the cell.
4. THE City_Generator SHALL not place a Placeholder_Tree on a Sidewalk_Cell or Park_Cell that is already occupied by another prop or NPC.
5. THE Placeholder_Tree trunk SHALL use a flat-shaded material with a brown color and the canopy SHALL use a flat-shaded material with a green color, both colors defined in City_Config.

### Requirement 8: Urban Prop Placement — Cars

**User Story:** As a developer, I want the system to place parked car placeholders along streets so that the city feels inhabited.

#### Acceptance Criteria

1. WHEN the City_Generator executes Stage 2, THE City_Generator SHALL instantiate Placeholder_Car objects on Street_Cells adjacent to Sidewalk_Cells, positioned offset toward the adjacent sidewalk edge of the cell.
2. THE City_Generator SHALL place cars at a configurable density (default: 1 car per 5 eligible Street_Cells) defined in City_Config, where eligible Street_Cells are non-intersection cells adjacent to at least one Sidewalk_Cell.
3. THE Placeholder_Car SHALL consist of a lower cube (scale 2x0.5x1) as body and an upper cube (scale 1.2x0.4x0.8) centered on top as cabin.
4. THE City_Generator SHALL orient each Placeholder_Car with a Y-rotation of 0 or 180 degrees on horizontal streets and 90 or 270 degrees on vertical streets.
5. THE City_Generator SHALL not place a Placeholder_Car on a Street_Cell that is already occupied by another car or is an intersection cell (where a street row and column cross).

### Requirement 9: Urban Prop Placement — Lampposts, Benches, and Trash Cans

**User Story:** As a developer, I want the system to place miscellaneous urban props so that the city feels detailed and lived-in.

#### Acceptance Criteria

1. WHEN the City_Generator executes Stage 2, THE City_Generator SHALL instantiate Placeholder_Lamppost objects on Sidewalk_Cells at intersections (where two Street_Cell rows and columns cross) with one lamppost on each of the 4 corner Sidewalk_Cells of the intersection.
2. WHEN the City_Generator executes Stage 2, THE City_Generator SHALL instantiate Placeholder_Bench objects on Sidewalk_Cells that are directly adjacent to a Park_Cell at a configurable density (default: 1 bench per Sidewalk_Cell that shares an edge with a Park_Cell) defined in City_Config, oriented so the bench's long axis (1.2 scale axis) runs parallel to the shared Park_Cell edge.
3. WHEN the City_Generator executes Stage 2, THE City_Generator SHALL instantiate Placeholder_TrashCan objects on Sidewalk_Cells at a configurable density (default: 1 trash can per 8 Sidewalk_Cells) defined in City_Config, distributing trash cans evenly across the grid using a spacing of at least 3 cells between each Placeholder_TrashCan.
4. THE Placeholder_Lamppost SHALL consist of a thin cylinder (radius 0.05, height 3.0) with a sphere (radius 0.15) at the top.
5. THE Placeholder_Bench SHALL consist of a flattened cube (scale 1.2x0.4x0.5) raised 0.2 units above the sidewalk surface.
6. THE Placeholder_TrashCan SHALL consist of a cylinder (radius 0.2, height 0.6) placed on the sidewalk surface.
7. IF a cell position is already occupied by another prop, NPC, or landmark-adjacent cell (School_Landmark spawn point cell or House_Landmark trigger cell), THEN THE City_Generator SHALL skip placement on that cell and attempt the next valid cell.
8. IF the number of valid unoccupied Sidewalk_Cells is insufficient to satisfy the configured density for any prop type, THEN THE City_Generator SHALL place as many props as valid cells allow and log a warning to the Unity Console indicating the prop type and how many could not be placed.

### Requirement 10: Vendor NPC Placement

**User Story:** As a player, I want to encounter vendor NPCs in the city so that I can buy items during my journey home.

#### Acceptance Criteria

1. WHEN the City_Generator executes Stage 3, THE City_Generator SHALL instantiate Vendor_NPC placeholders on Sidewalk_Cells adjacent to Building_Cells.
2. THE City_Generator SHALL place a configurable number of Vendor_NPCs (default: 3) defined in City_Config, with a maximum of 1 Vendor_NPC per city block, selecting blocks using uniform random distribution from the available city blocks.
3. THE Vendor_NPC SHALL have a CapsuleCollider (radius 0.5, height 2.0) and an NPC_Interactable component referencing a vendor Dialogue_Data ScriptableObject.
4. THE City_Generator SHALL ensure each Vendor_NPC is placed on a Sidewalk_Cell not occupied by any other NPC, prop, or landmark entrance, with at least 2 cells of clearance (measured in Manhattan distance) from any other NPC.
5. THE Vendor_NPC SHALL be represented as a capsule primitive (radius 0.5, height 2.0) with a distinct color defined in City_Config.
6. IF the City_Generator cannot place all configured Vendor_NPCs due to insufficient valid Sidewalk_Cells or city blocks, THEN THE City_Generator SHALL place as many as possible and log a warning to the Unity Console indicating the number of Vendor_NPCs that could not be placed.

### Requirement 11: Challenger NPC Placement

**User Story:** As a player, I want to encounter challenger NPCs so that I can choose to accept or reject combat.

#### Acceptance Criteria

1. WHEN the City_Generator executes Stage 3, THE City_Generator SHALL instantiate Challenger_NPC placeholders on Sidewalk_Cells.
2. THE City_Generator SHALL place a configurable number of Challenger_NPCs (default: 4, minimum: 1, maximum: 10) defined in City_Config, evenly distributed along the path between School_Landmark and House_Landmark with approximately equal spacing between consecutive Challenger_NPCs.
3. THE Challenger_NPC SHALL have a CapsuleCollider (radius 0.5, height 2.0) and a FlipCombat_NPC component referencing a Dialogue_Data ScriptableObject and an EncounterConfig ScriptableObject.
4. THE City_Generator SHALL ensure each Challenger_NPC is placed on an unoccupied Sidewalk_Cell with at least 3 cells of clearance (measured in Manhattan distance) from any other NPC.
5. THE Challenger_NPC SHALL be represented as a capsule primitive (radius 0.5, height 2.0) with a distinct color different from Vendor_NPC, defined in City_Config.
6. IF the City_Generator cannot place all configured Challenger_NPCs due to insufficient valid Sidewalk_Cells, THEN THE City_Generator SHALL place as many as possible and log a warning to the Unity Console indicating the number that could not be placed.

### Requirement 12: Surprise Encounter Placement

**User Story:** As a player, I want random encounters to surprise me during exploration so that the journey has tension and unpredictability.

#### Acceptance Criteria

1. WHEN the City_Generator executes Stage 3, THE City_Generator SHALL instantiate Surprise_Encounter trigger zones on Street_Cells and Sidewalk_Cells that are part of the navigable path between School_Landmark and House_Landmark.
2. THE City_Generator SHALL place a configurable number of Surprise_Encounters (default: 3, minimum: 1, maximum: 10) defined in City_Config, with a minimum spacing of 3 cells between any two Surprise_Encounters.
3. THE Surprise_Encounter SHALL have a BoxCollider marked as trigger with dimensions covering the full Cell area (default 4x1x4 units).
4. THE Surprise_Encounter SHALL have a TriggerZone_Encounter component configured with a combat scene name string and a forced dialogue data reference to a DialogueData ScriptableObject, both defined in City_Config.
5. THE City_Generator SHALL ensure no Surprise_Encounter is placed within 5 cells of School_Landmark or House_Landmark to avoid immediate forced combat at start or end.
6. THE Surprise_Encounter trigger zone SHALL be invisible (no MeshRenderer or SpriteRenderer enabled) so that the player cannot see the encounter zone before entering.
7. THE City_Generator SHALL not place a Surprise_Encounter on a cell that is already occupied by a prop, NPC, or landmark entrance.

### Requirement 13: Building Transparency When Occluding Player

**User Story:** As a player, I want buildings that block my view to become transparent so that I can always see my character.

#### Acceptance Criteria

1. WHEN the City_Generator executes Stage 4, THE City_Generator SHALL add the Building_Transparency_System component to the Isometric_Camera.
2. WHILE the Player_Controller is behind a Placeholder_Building relative to the Isometric_Camera, THE Building_Transparency_System SHALL set that building's material rendering mode to support alpha transparency and reduce its material alpha to a configurable value (default: 0.3) defined in City_Config.
3. WHEN the Player_Controller is no longer behind a Placeholder_Building, THE Building_Transparency_System SHALL restore that building's material alpha to 1.0 and revert its material rendering mode to opaque.
4. THE Building_Transparency_System SHALL detect occlusion every frame by casting a ray from the Isometric_Camera world position to the Player_Controller world position and checking for intersections with colliders on the CityGeometry layer.
5. THE Building_Transparency_System SHALL transition alpha values over a configurable duration (default: 0.2 seconds) defined in City_Config using linear interpolation so that neither fade-out nor fade-in produces an instantaneous alpha change.
6. WHILE multiple buildings occlude the player simultaneously, THE Building_Transparency_System SHALL apply transparency to all occluding buildings independently.
7. IF no Placeholder_Building colliders exist on the CityGeometry layer between the Isometric_Camera and the Player_Controller, THEN THE Building_Transparency_System SHALL not modify any building materials.

### Requirement 14: Decorative Variation and Procedural Distribution

**User Story:** As a developer, I want to add visual variation and procedural distribution to the city so that it feels less repetitive and more organic.

#### Acceptance Criteria

1. WHEN the City_Generator executes Stage 5, THE City_Generator SHALL apply random height variation to all Placeholder_Buildings except School_Landmark and House_Landmark, setting each building's Y-scale to a random value within a configurable range (default: 2.0 to 5.0 units) defined in City_Config.
2. WHEN the City_Generator executes Stage 5, THE City_Generator SHALL apply random color variation to all Placeholder_Buildings except School_Landmark and House_Landmark, selecting each building's material color from a configurable color palette (minimum 5 colors) defined in City_Config.
3. WHEN the City_Generator executes Stage 5, THE City_Generator SHALL convert a configurable percentage (default: 20%, maximum: 50%) of empty cells into additional Building_Cells, where an empty cell is any cell not assigned as Street_Cell, Sidewalk_Cell, Building_Cell, or landmark, and only cells adjacent to at least one existing Building_Cell are eligible for conversion. THE City_Generator SHALL instantiate a Placeholder_Building with a BoxCollider on each newly converted Building_Cell.
4. WHEN the City_Generator executes Stage 5, THE City_Generator SHALL apply random rotation offsets (0, 90, 180, or 270 degrees on Y-axis) to Placeholder_Tree canopies to break visual repetition.
5. WHEN the City_Generator executes Stage 5, THE City_Generator SHALL apply random scale variation (0.8x to 1.2x uniform scale) to all props placed in Stage 2.
6. THE City_Generator SHALL use a configurable seed value in City_Config so that the same seed produces identical results across multiple generation runs, including building heights, building colors, cell conversions, tree rotations, and prop scales.
7. THE City_Generator SHALL expose a "Regenerate" button in the Unity Inspector that destroys all GameObjects under the hierarchy parents (Ground, Streets, Sidewalks, Buildings, Props, NPCs, Encounters) and regenerates the city from Stage 1 using the current City_Config values.

### Requirement 15: City Configuration ScriptableObject

**User Story:** As a developer, I want all city generation parameters in a single ScriptableObject so that I can tune the city without modifying code.

#### Acceptance Criteria

1. THE City_Config SHALL be a ScriptableObject that can be created via Unity's Create Asset menu under "Flipit/City Config".
2. THE City_Config SHALL store grid dimensions as two integer fields (width and depth) with default values of 10 and 10 and minimum values of 5.
3. THE City_Config SHALL store cell size as a float field with a default value of 4.0 and a minimum value of 1.0.
4. THE City_Config SHALL store density values as float fields for trees (default 0.33, minimum 0.0, maximum 1.0), cars (default 0.2, minimum 0.0, maximum 1.0), lampposts (default 1.0, minimum 0.0), benches (default 1.0, minimum 0.0, maximum 1.0), and trash cans (default 0.125, minimum 0.0, maximum 1.0), where density represents the probability of placement per eligible cell.
5. THE City_Config SHALL store NPC counts as integer fields for vendors (default 3, minimum 0), challengers (default 4, minimum 0), and surprise encounters (default 3, minimum 0).
6. THE City_Config SHALL store building height range as two float fields (min and max) with defaults of 2.0 and 5.0, a minimum value of 0.5 for the min field, and a constraint that the max field must be greater than or equal to the min field.
7. THE City_Config SHALL store a color palette as a list of Color values with a minimum of 5 entries.
8. THE City_Config SHALL store distinct colors for School_Landmark, House_Landmark, Vendor_NPC, Challenger_NPC, streets, sidewalks, and the ground plane.
9. THE City_Config SHALL store camera orthographic size (default 8.0, minimum 1.0), camera follow offset (default 10.0, minimum 1.0), camera smoothing speed (default 5.0, minimum 0.1), player move speed (default 5.0, minimum 0.1), and transparency alpha value (default 0.3, minimum 0.0, maximum 1.0) as float fields.
10. THE City_Config SHALL store a random seed as an integer field with a default value of 0 (where 0 indicates use of a random seed at runtime).
11. IF any City_Config numeric field is set below its defined minimum or above its defined maximum, THEN THE City_Generator SHALL clamp the value to the nearest valid bound at generation time and log a warning to the Unity Console indicating the field name, the invalid value, and the clamped value.
12. IF the City_Config building height max field is less than the min field, THEN THE City_Generator SHALL set max equal to min at generation time and log a warning indicating the corrected values.
13. IF the City_Config color palette contains fewer than 5 entries, THEN THE City_Generator SHALL abort generation and log an error indicating the required minimum of 5 palette colors.

### Requirement 16: Stage Completion Confirmation

**User Story:** As a developer, I want the system to pause after completing each stage and ask for confirmation before proceeding so that I can inspect the results and decide whether to continue.

#### Acceptance Criteria

1. WHEN the City_Generator completes Stage 1 generation, THE City_Generator SHALL pause execution and display a confirmation dialog in the Unity Editor asking the developer whether to proceed to Stage 2.
2. WHEN the City_Generator completes Stage 2 generation, THE City_Generator SHALL pause execution and display a confirmation dialog in the Unity Editor asking the developer whether to proceed to Stage 3.
3. WHEN the City_Generator completes Stage 3 generation, THE City_Generator SHALL pause execution and display a confirmation dialog in the Unity Editor asking the developer whether to proceed to Stage 4.
4. WHEN the City_Generator completes Stage 4 generation, THE City_Generator SHALL pause execution and display a confirmation dialog in the Unity Editor asking the developer whether to proceed to Stage 5.
5. IF the developer selects "Cancel" in the confirmation dialog, THEN THE City_Generator SHALL stop generation at the current stage and preserve all GameObjects created during the current and all previous stages in the scene hierarchy without modification.
6. IF the developer selects "OK" in the confirmation dialog, THEN THE City_Generator SHALL proceed to execute the next stage immediately.
7. WHEN a stage completes successfully, THE City_Generator SHALL log to the Unity Console the completed stage number, the total count of GameObjects created during that stage, and a per-category count (e.g., buildings, streets, props, NPCs) before displaying the confirmation dialog.
8. WHEN the City_Generator completes Stage 5 generation, THE City_Generator SHALL log the Stage 5 summary to the Unity Console and display a completion dialog indicating all stages are finished, without offering a "proceed" option.
9. IF a stage encounters an error during generation, THEN THE City_Generator SHALL log the error to the Unity Console, preserve all GameObjects created by previously completed stages, and display an error dialog indicating which stage failed without proceeding to the next stage.

### Requirement 17: Scene Structure and Assembly Organization

**User Story:** As a developer, I want the city generation code organized in its own assembly and scene so that it integrates cleanly with the existing project modules.

#### Acceptance Criteria

1. THE City_Generator scripts SHALL reside in a folder "Assets/Scripts/CityTerrain" with its own assembly definition "Flipit.CityTerrain.asmdef" using rootNamespace "Flipit.CityTerrain".
2. THE Flipit.CityTerrain assembly SHALL reference Flipit.Dialogue and Flipit.Combat assemblies so that NPC components can be attached during Stage 3.
3. THE City_Generator SHALL generate all city content into a dedicated Unity scene named "CityExplorationScene" stored in "Assets/Scenes/CityExplorationScene.unity".
4. WHEN the City_Generator generates the scene, THE City_Generator SHALL organize generated objects under root-level parent GameObjects named "Ground", "Streets", "Sidewalks", "Buildings", "Props", "NPCs", and "Encounters".
5. THE City_Generator SHALL provide an Editor script located in "Assets/Editor" with its own assembly definition "Flipit.CityTerrain.Editor.asmdef" referencing Flipit.CityTerrain, constrained to the Editor platform, exposing a menu item "Flipit/Generate City" that executes city generation in Edit Mode and saves the scene.
6. WHEN the City_Generator executes and the target scene already contains previously generated content, THE City_Generator SHALL destroy all existing child objects under the root-level parent GameObjects ("Ground", "Streets", "Sidewalks", "Buildings", "Props", "NPCs", "Encounters") before generating new content.
