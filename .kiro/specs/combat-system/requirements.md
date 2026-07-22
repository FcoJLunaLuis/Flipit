# Requirements Document

## Introduction

Sistema de combate para Flipit que permite al jugador entrar en partidas de combate Flipit de dos formas: interacción voluntaria con NPCs estáticos (flip_combat) o encuentros aleatorios forzados. Ambas formas conducen a una transición de escena con una viñeta "RETO ACEPTADO" y cargan una escena de combate dedicada. La escena de combate es una pequeña ciudad con vista top-down donde el jugador puede moverse libremente y encontrar múltiples NPCs de combate.

## Glossary

- **Combat_System**: Módulo central que gestiona el flujo de combate, incluyendo la detección de NPCs de combate, la decisión del jugador, la transición de escena y la carga de la escena de combate.
- **FlipCombat_NPC**: NPC de combate estático ubicado en el mundo. El jugador puede acercarse voluntariamente e iniciar una partida de Flipit.
- **RandomEncounter_NPC**: NPC de combate que aparece de forma aleatoria, bloquea las acciones del jugador y fuerza la aceptación del combate.
- **Combat_Transition_UI**: Componente de UI que muestra la viñeta de cómic "RETO ACEPTADO" durante la transición entre escenas.
- **Combat_Scene**: Escena dedicada con una pequeña ciudad en vista top-down que contiene múltiples NPCs de combate estáticos y aleatorios.
- **Player_Controller**: El GameObject del jugador con capacidad de movimiento top-down y detección de interacciones.
- **Random_Encounter_Manager**: Sistema que gestiona la aparición y temporización de encuentros aleatorios.

## Requirements

### Requirement 1: Interacción con NPC de Combate Estático

**User Story:** Como jugador, quiero acercarme a un NPC de combate estático y decidir si acepto o rechazo la partida de Flipit, para poder explorar libremente y combatir cuando lo desee.

#### Acceptance Criteria

1. WHEN the Player_Controller enters within 2.0 Unity units of a FlipCombat_NPC, THE Combat_System SHALL display a visible interaction prompt indicating a Flipit challenge is available.
2. WHEN the Player_Controller moves beyond 2.0 Unity units from the FlipCombat_NPC, THE Combat_System SHALL hide the interaction prompt.
3. WHEN the Player_Controller presses the interact action while within range of a FlipCombat_NPC and the Dialogue_Manager is in Idle state, THE Combat_System SHALL present a dialogue with options to accept or reject the Flipit challenge.
4. WHEN the player selects the accept option, THE Combat_System SHALL transition the Dialogue_Manager to Closing state, close the dialogue UI, and load the combat scene within 2 seconds.
5. WHEN the player selects the reject option, THE Combat_System SHALL close the dialogue, return the Dialogue_Manager to Idle state, restore the Player action map, and preserve the player's current position and game state unchanged.
6. WHILE the combat dialogue is open, THE Player_Controller SHALL have movement input disabled by switching to the UI action map.
7. IF the Player_Controller presses the interact action while the Dialogue_Manager is not in Idle state, THEN THE Combat_System SHALL ignore the input and not open a second dialogue.

### Requirement 2: Encuentro Aleatorio Forzado

**User Story:** Como jugador, quiero experimentar encuentros aleatorios sorpresivos que me obliguen a combatir, para añadir tensión y variedad al gameplay.

#### Acceptance Criteria

1. WHEN a random encounter triggers, THE Random_Encounter_Manager SHALL instantiate a RandomEncounter_NPC at a position within 2 to 4 world units from the Player_Controller position, ensuring the spawn point is not obstructed by colliders.
2. WHEN a RandomEncounter_NPC spawns, THE Combat_System SHALL disable all Player_Controller movement and interaction inputs within the same frame, leaving only the accept combat input active.
3. WHEN a RandomEncounter_NPC spawns, THE Combat_System SHALL display a forced dialogue panel containing exactly one visible option to accept combat, with no close or cancel button available.
4. WHEN the player selects the accept option from a forced encounter dialogue, THE Combat_System SHALL initiate the combat transition sequence by loading the combat scene or activating the combat state within 1 second.
5. WHILE a RandomEncounter_NPC is active (from the moment it spawns until the combat transition sequence completes), THE Player_Controller SHALL remain unable to move, interact with other objects, or open menus.
6. IF a random encounter triggers while the Dialogue_Manager is not in Idle state or another RandomEncounter_NPC is already active, THEN THE Random_Encounter_Manager SHALL discard the encounter and not spawn a new RandomEncounter_NPC.
7. IF the forced encounter dialogue remains on screen without player input for more than 30 seconds, THEN THE Combat_System SHALL auto-accept the combat and initiate the combat transition sequence.

### Requirement 3: Transición de Escena con Viñeta

**User Story:** Como jugador, quiero ver una viñeta de cómic que diga "RETO ACEPTADO" al aceptar un combate, para sentir la emoción del momento antes de la batalla.

#### Acceptance Criteria

1. WHEN the player accepts a combat challenge (from either a FlipCombat_NPC dialogue or a RandomEncounter_NPC forced dialogue), THE Combat_Transition_UI SHALL display a full-screen Canvas overlay with the text "RETO ACEPTADO" rendered in bold, uppercase font at minimum 72pt size with a contrasting outline or shadow.
2. WHEN the Combat_Transition_UI overlay is displayed, THE Combat_System SHALL hold the overlay visible for a duration between 1.5 and 3.0 seconds (configurable via serialized field, default 2.0 seconds) before initiating scene load.
3. WHEN the overlay display duration completes, THE Combat_System SHALL call SceneManager.LoadSceneAsync with the Combat_Scene name and transition to that scene.
4. WHILE the transition overlay is active, THE Player_Controller SHALL have all input disabled and no gameplay state changes shall occur.
5. IF SceneManager.LoadSceneAsync fails or throws an exception, THEN THE Combat_System SHALL log an error via Debug.LogError, destroy the overlay, restore the Player action map, and return the player to normal gameplay in the current scene.

### Requirement 4: Escena de Combate con Ciudad

**User Story:** Como jugador, quiero explorar una pequeña ciudad en vista top-down con múltiples NPCs de combate, para tener un espacio dedicado donde buscar partidas de Flipit.

#### Acceptance Criteria

1. THE Combat_Scene SHALL contain a city layout bounded by collision boundaries that prevent the Player_Controller from exiting the playable area, with exterior areas where NPCs are placed.
2. THE Combat_Scene SHALL include a minimum of 4 FlipCombat_NPC instances positioned outside of buildings, each with a Collider2D that allows detection by the Player_Interactor system.
3. THE Combat_Scene SHALL include a minimum of 3 RandomEncounter_NPC spawn points distributed across the city with a minimum separation of 5 units between any two spawn points.
4. WHEN the Combat_Scene loads, THE Player_Controller SHALL spawn at a designated entry point located within the city boundaries with the TopDownPlayerMovement component and Player_Interactor component active.
5. WHILE in the Combat_Scene, THE Player_Controller SHALL use the same TopDownPlayerMovement system as the existing dialogue scene, receiving input via PlayerInput in SendMessages mode with the Player action map.
6. WHEN the Player_Controller interacts with a FlipCombat_NPC via the Player_Interactor system, THE Combat_Scene SHALL trigger a combat initiation event that external combat systems can subscribe to.

### Requirement 5: Sistema de Encuentros Aleatorios

**User Story:** Como diseñador, quiero configurar la frecuencia y condiciones de los encuentros aleatorios, para poder balancear la experiencia de juego.

#### Acceptance Criteria

1. THE Random_Encounter_Manager SHALL evaluate encounter probability every time its configurable check interval elapses, where the check interval is a serialized float field clamped between 1.0 and 60.0 seconds (default 5.0 seconds).
2. WHEN the Random_Encounter_Manager decides to trigger an encounter, THE Random_Encounter_Manager SHALL select a spawn point from the available spawn points within a configurable radius (serialized float, clamped between 1.0 and 50.0 Unity units, default 10.0 units) of the player's current position.
3. WHILE Dialogue_Manager.Instance.CurrentState is not Idle, THE Random_Encounter_Manager SHALL suppress new random encounters and skip the probability check until the state returns to Idle.
4. THE Random_Encounter_Manager SHALL expose serialized fields for: minimum interval between encounters (float, clamped between 5.0 and 300.0 seconds, default 15.0 seconds) and encounter probability per check (float, clamped between 0.0 and 1.0, default 0.15).
5. IF the Random_Encounter_Manager triggers an encounter and no spawn points exist within the configured radius of the player, THEN THE Random_Encounter_Manager SHALL cancel the encounter for the current check and log a warning message to the console.

### Requirement 6: Integración con Sistemas Existentes

**User Story:** Como desarrollador, quiero que el sistema de combate se integre con el sistema de diálogo y movimiento existentes, para mantener consistencia y reutilizar código.

#### Acceptance Criteria

1. THE FlipCombat_NPC class SHALL inherit from NPC_Interactable and override the Interact method to trigger combat dialogue via the existing Dialogue_Manager API.
2. THE Combat_System SHALL invoke Player_Interactor.DetectNearbyInteractables (using Physics2D.OverlapCircle with the same radius parameter) for detecting FlipCombat_NPC proximity.
3. THE Combat_System SHALL call PlayerInput.SwitchCurrentActionMap("UI") when combat dialogue opens and PlayerInput.SwitchCurrentActionMap("Player") when combat dialogue closes without acceptance.
4. WHEN the player accepts combat from a FlipCombat_NPC dialogue, THE Combat_System SHALL call PlayerInput.SwitchCurrentActionMap("UI") and keep it in that state until the combat scene fully loads.
5. THE Combat_System SHALL publish events through the existing Dialogue_Event_Bus when combat dialogue starts and when combat dialogue ends, using event names consistent with the existing naming pattern.
