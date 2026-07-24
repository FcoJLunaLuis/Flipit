# Requirements Document

## Introduction

Sistema de validación de exploración para la ciudad generada en Flipit. Este feature proporciona una escena de validación donde el jugador es colocado automáticamente en la posición de spawn definida (PlayerSpawn), puede moverse con controles WASD con aceleración suave, y es seguido por una cámara isométrica con zoom ajustable y prevención de clipping. Incluye un sistema de oclusión para edificios, colisiones con todos los objetos del entorno, gizmos de validación visual, y una ventana de Editor dedicada ("Flipit → Validate Exploration") que orquesta todo el flujo. Este feature extiende la infraestructura existente (Player_Controller, Isometric_Camera, Building_Transparency_System) sin duplicarla, y no incluye NPCs, combate ni diálogos.

## Glossary

- **Exploration_Validator**: MonoBehaviour principal que orquesta la escena de validación de exploración, responsable de inicializar el spawn del jugador, la cámara, y los sistemas de validación.
- **Player_Controller**: El MonoBehaviour existente basado en CharacterController que maneja movimiento WASD en el plano XZ usando el New Input System (ubicado en `Assets/Scripts/CityTerrain/Runtime/Player_Controller.cs`).
- **Smooth_Movement**: Extensión del movimiento del jugador que añade aceleración y desaceleración gradual en lugar de velocidad instantánea.
- **Isometric_Camera**: La cámara existente en ángulo isométrico fijo (30° X, 45° Y) con seguimiento suave y proyección ortográfica (ubicada en `Assets/Scripts/CityTerrain/Runtime/Isometric_Camera.cs`).
- **Camera_Zoom**: Funcionalidad de zoom ajustable que modifica el orthographicSize de la cámara isométrica mediante input del usuario.
- **Camera_Clipping_Prevention**: Sistema que impide que la cámara penetre la geometría de edificios durante el seguimiento al jugador.
- **Building_Transparency_System**: El sistema existente que detecta edificios ocluidos entre cámara y jugador y les aplica transparencia gradual (ubicado en `Assets/Scripts/CityTerrain/Runtime/Building_Transparency_System.cs`).
- **Validation_Gizmo_Renderer**: MonoBehaviour que dibuja gizmos de depuración en el Scene view para spawn point, bounds de navegación, colliders de edificios y target de la cámara.
- **Exploration_Validator_Window**: Ventana de Editor personalizada accesible desde el menú "Flipit → Validate Exploration" que permite generar la ciudad, spawnear el jugador, posicionar la cámara e iniciar el modo de validación.
- **Navigation_Bounds**: La región rectangular del mundo que delimita el área navegable por el jugador, definida por las dimensiones del Grid_Layout.
- **PlayerSpawn**: La posición de spawn definida en la escena (SpawnPoint child del School_Landmark) donde el jugador aparece automáticamente al iniciar la validación.
- **CityGeometry_Layer**: El Physics Layer existente asignado a edificios y props generados para filtrado de colisiones y raycasts.

## Requirements

### Requirement 1: Spawn Automático del Jugador

**User Story:** As a developer, I want the player to spawn automatically at the defined PlayerSpawn position when validation starts, so that I can immediately begin exploring without manual setup.

#### Acceptance Criteria

1. WHEN the Exploration_Validator initializes, THE Exploration_Validator SHALL locate the PlayerSpawn GameObject in the scene hierarchy (child of School_Landmark named "SpawnPoint") and position the Player_Controller at that world position.
2. IF the PlayerSpawn GameObject does not exist in the scene, THEN THE Exploration_Validator SHALL log an error to the Unity Console indicating "SpawnPoint not found" and abort validation initialization.
3. WHEN the player is spawned, THE Exploration_Validator SHALL ensure the Player_Controller GameObject has a CharacterController component (radius 0.5, height 2.0, center at (0, 1, 0)) and a Player_Controller component configured for movement.
4. WHEN the player is spawned, THE Exploration_Validator SHALL orient the Player_Controller with a Y-rotation of 0 degrees facing the default forward direction.

### Requirement 2: Movimiento con Aceleración Suave

**User Story:** As a developer, I want the player character to accelerate and decelerate smoothly when moving, so that movement feels polished during exploration validation.

#### Acceptance Criteria

1. WHILE the Player_Controller receives Move input from the Player action map (WASD keys), THE Smooth_Movement SHALL increase the character velocity from zero to the configured maximum speed using linear acceleration at a configurable rate (default: 20 units per second squared) defined in the Exploration_Validator configuration.
2. WHEN the Player_Controller stops receiving Move input, THE Smooth_Movement SHALL decrease the character velocity from the current speed to zero using linear deceleration at a configurable rate (default: 15 units per second squared) defined in the Exploration_Validator configuration.
3. THE Smooth_Movement SHALL use the existing CharacterController.Move method from the Player_Controller to apply the smoothed velocity vector on the XZ plane each frame.
4. THE Smooth_Movement SHALL clamp the maximum velocity to the configured move speed (default: 5 units per second) so that the character never exceeds the speed limit regardless of input duration.
5. WHILE the character velocity is below 0.01 units per second, THE Smooth_Movement SHALL set velocity to zero to prevent micro-drift.

### Requirement 3: Cámara Isométrica con Zoom Ajustable

**User Story:** As a developer, I want the isometric camera to support adjustable zoom, so that I can inspect the city at different scales during validation.

#### Acceptance Criteria

1. THE Isometric_Camera SHALL maintain the fixed isometric rotation of 30 degrees on the X-axis and 45 degrees on the Y-axis at all times, without rotation in response to player input.
2. THE Isometric_Camera SHALL follow the Player_Controller position each frame using linear interpolation toward the target position with a configurable smooth speed (default: 5 units per second).
3. WHEN the user scrolls the mouse wheel up, THE Camera_Zoom SHALL decrease the orthographic size of the camera by a configurable step (default: 0.5 units per scroll tick), clamped to a minimum value of 3.
4. WHEN the user scrolls the mouse wheel down, THE Camera_Zoom SHALL increase the orthographic size of the camera by a configurable step (default: 0.5 units per scroll tick), clamped to a maximum value of 20.
5. THE Camera_Zoom SHALL apply the orthographic size change smoothly using linear interpolation over a configurable duration (default: 0.15 seconds) so that zoom transitions are not abrupt.

### Requirement 4: Prevención de Clipping de Cámara

**User Story:** As a developer, I want the camera to never clip through buildings, so that the camera view remains clean during exploration validation.

#### Acceptance Criteria

1. WHILE the Isometric_Camera follows the player, THE Camera_Clipping_Prevention SHALL cast a ray from the player position toward the desired camera position along the camera's back-vector.
2. IF the ray intersects a collider on the CityGeometry_Layer before reaching the desired camera distance, THEN THE Camera_Clipping_Prevention SHALL position the camera at the hit point offset by 0.5 units toward the player so that the camera remains outside the building geometry.
3. WHEN the ray no longer intersects a building collider, THE Camera_Clipping_Prevention SHALL smoothly return the camera to the full configured follow offset distance using linear interpolation at the configured smooth speed.
4. THE Camera_Clipping_Prevention SHALL use SphereCast with a radius of 0.3 units instead of a simple Raycast to prevent near-plane clipping against thin geometry.

### Requirement 5: Oclusión de Edificios

**User Story:** As a developer, I want buildings that block my view of the player to become semi-transparent, so that the player character remains visible at all times during exploration validation.

#### Acceptance Criteria

1. THE Building_Transparency_System SHALL detect occlusion each frame by casting a ray from the Isometric_Camera world position to the Player_Controller world position, checking for intersections with colliders on the CityGeometry_Layer.
2. WHILE a building collider is detected between the Isometric_Camera and the Player_Controller, THE Building_Transparency_System SHALL reduce that building's material alpha to the configured target alpha (default: 0.3) using linear interpolation over the configured fade duration (default: 0.2 seconds).
3. WHEN a building is no longer between the Isometric_Camera and the Player_Controller, THE Building_Transparency_System SHALL restore that building's material alpha to 1.0 using linear interpolation over the configured fade duration.
4. WHILE multiple buildings simultaneously occlude the player, THE Building_Transparency_System SHALL apply transparency to all occluding buildings independently.
5. THE Building_Transparency_System SHALL ensure the Player_Controller remains visible at all times by verifying at least one unobstructed line of sight exists from the camera to the player after applying transparency.

### Requirement 6: Colisiones Físicas del Jugador

**User Story:** As a developer, I want the player to collide with all solid objects in the city, so that I can validate that the navigation mesh is correct and objects are properly blocking.

#### Acceptance Criteria

1. WHILE the Player_Controller moves, THE Player_Controller SHALL collide with all GameObjects on the CityGeometry_Layer including buildings, trees, vehicles, and props, preventing the character from passing through them.
2. THE Player_Controller SHALL move freely over Street_Cells, Sidewalk_Cells, and Park_Cells without collision obstruction on the walking surface.
3. IF the Player_Controller contacts a collider on the CityGeometry_Layer, THEN THE Player_Controller SHALL stop movement in the direction of contact and allow sliding along the surface using the CharacterController built-in collision response.
4. THE Player_Controller SHALL apply gravity of -9.81 m/s² each frame via CharacterController.Move so that the character remains grounded on the walking surface.

### Requirement 7: Gizmos de Validación

**User Story:** As a developer, I want to see visual debug gizmos in the Scene view, so that I can verify spawn positions, navigation boundaries, colliders, and camera target during validation.

#### Acceptance Criteria

1. WHILE the Validation_Gizmo_Renderer is active, THE Validation_Gizmo_Renderer SHALL draw a sphere gizmo of radius 1.0 at the PlayerSpawn position using green color in the Unity Scene view.
2. WHILE the Validation_Gizmo_Renderer is active, THE Validation_Gizmo_Renderer SHALL draw a wireframe rectangle gizmo representing the Navigation_Bounds defined by the Grid_Layout dimensions (width × cellSize, depth × cellSize) using yellow color in the Unity Scene view.
3. WHILE the Validation_Gizmo_Renderer is active, THE Validation_Gizmo_Renderer SHALL draw wireframe cube gizmos matching the BoxCollider size of each building on the CityGeometry_Layer using cyan color in the Unity Scene view.
4. WHILE the Validation_Gizmo_Renderer is active, THE Validation_Gizmo_Renderer SHALL draw a wireframe sphere gizmo of radius 0.5 at the Isometric_Camera target position (the interpolation target the camera moves toward) using magenta color in the Unity Scene view.
5. WHILE the Unity Editor is not in Play Mode, THE Validation_Gizmo_Renderer SHALL still draw the spawn point and navigation bounds gizmos using OnDrawGizmos so that they are visible during scene editing.

### Requirement 8: Ventana de Editor "Flipit → Validate Exploration"

**User Story:** As a developer, I want a dedicated Editor window that orchestrates the full validation flow, so that I can generate the city, spawn the player, position the camera, and start validation with a single tool.

#### Acceptance Criteria

1. THE Exploration_Validator_Window SHALL be accessible from the Unity Editor menu at the path "Flipit/Validate Exploration".
2. WHEN the developer clicks the "Generate City" button in the Exploration_Validator_Window, THE Exploration_Validator_Window SHALL invoke the existing City_Generator generation pipeline (equivalent to "Flipit → Generate City") to produce the city in the CityExplorationScene.
3. WHEN the developer clicks the "Spawn Player" button in the Exploration_Validator_Window, THE Exploration_Validator_Window SHALL locate the PlayerSpawn position and instantiate or reposition the Player_Controller at that location with all required components (CharacterController, PlayerInput, Player_Controller, Smooth_Movement).
4. WHEN the developer clicks the "Position Camera" button in the Exploration_Validator_Window, THE Exploration_Validator_Window SHALL instantiate or reposition the Isometric_Camera at the correct offset from the player with the Camera_Zoom and Camera_Clipping_Prevention components attached.
5. WHEN the developer clicks the "Start Validation" button in the Exploration_Validator_Window, THE Exploration_Validator_Window SHALL enter Play Mode in the Unity Editor with the CityExplorationScene loaded, enabling all runtime validation systems (movement, camera follow, transparency, gizmos).
6. THE Exploration_Validator_Window SHALL display status labels indicating whether each step has been completed (city generated, player spawned, camera positioned) so the developer knows the current validation state.
7. IF the CityExplorationScene does not exist when any button is pressed, THEN THE Exploration_Validator_Window SHALL display an error dialog indicating "CityExplorationScene not found. Run 'Flipit → Generate City' first." and abort the operation.

### Requirement 9: Restricciones de Alcance

**User Story:** As a developer, I want the validation scene to include only exploration mechanics, so that testing remains focused and uncontaminated by other systems.

#### Acceptance Criteria

1. THE Exploration_Validator SHALL not instantiate any NPC GameObjects (Vendor_NPC, Challenger_NPC) during validation mode.
2. THE Exploration_Validator SHALL not instantiate any combat-related systems (FlipCombat_NPC, TriggerZone_Encounter, EncounterConfig) during validation mode.
3. THE Exploration_Validator SHALL not instantiate any dialogue systems (NPC_Interactable, Dialogue_Manager) during validation mode.
4. THE Exploration_Validator SHALL execute only Stage 1 (Core Layout) and Stage 2 (Urban Props) of the City_Generator pipeline when generating the city for validation, skipping Stages 3 (NPC Placement), 4 (Building Transparency), and 5 (Decorative Variation).
5. THE Exploration_Validator SHALL manually attach the Building_Transparency_System and apply decorative variation (building heights and colors only) after Stages 1 and 2 complete, so that visual quality is maintained without NPC or encounter systems.
