# Flipit — Project Structure

## Top-Level Layout

```
Assets/
├── DialogueData/          # ScriptableObject assets for NPC dialogue content
├── Editor/                # Editor-only scripts (scene builders, menu items)
├── Prefabs/               # Reusable prefab assets
├── Scenes/                # Unity scenes
├── Screenshots/           # Captured screenshots from editor
└── Scripts/               # All runtime code (assembly-separated)
    ├── CityTerrain/       # City generation & runtime behaviors
    ├── Combat/            # Combat system
    └── Dialogue/          # Dialogue system
```

## Scripts Architecture

### CityTerrain (`Flipit.CityTerrain`)
```
Scripts/CityTerrain/
├── City_Config.cs          # ScriptableObject: all generation parameters
├── City_Generator.cs       # Top-level generation orchestrator
├── Data/                   # Data structures (Grid_Layout, CellData, CellType)
├── Generators/             # Stage-specific generators (Grid, Geometry, Landmarks, NPCs, Props, Encounters)
├── Runtime/                # Runtime MonoBehaviours (Player_Controller, Isometric_Camera, Building_Transparency_System)
├── Shaders/                # Custom shaders (CharacterSilhouette)
└── Utilities/              # Helper classes (PathFinder)
```

### Combat (`Flipit.Combat`)
```
Scripts/Combat/
├── Combat_System.cs          # Singleton orchestrator with state machine
├── Combat_Event_Bus.cs       # Static event service
├── CombatState.cs            # State enum
├── EncounterConfig.cs        # ScriptableObject for encounter data
├── Combat_Transition_UI.cs   # Transition effects
├── CombatDialogue_Handler.cs # Bridge between combat and dialogue
├── CombatScene_Setup.cs      # Combat scene initialization
├── FlipCombat_NPC.cs         # Voluntary combat NPC
├── RandomEncounter_NPC.cs    # Random encounter NPC
├── Random_Encounter_Manager.cs # Manages random encounters
└── TriggerZone_Encounter.cs  # Hidden trigger zone for forced combat
```

### Dialogue (`Flipit.Dialogue`)
```
Scripts/Dialogue/
├── Dialogue_Manager.cs       # Singleton orchestrator with state machine
├── Dialogue_Event_Bus.cs     # Static event tracking service
├── Dialogue_UI.cs            # Concrete uGUI implementation
├── IDialogueUI.cs            # Interface abstraction for UI
├── DialogueData.cs           # ScriptableObject for conversation data
├── DialogueLine.cs           # Single line data
├── DialogueOption.cs         # Branching option data
├── DialogueOptionButton.cs   # UI button for options
├── DialogueState.cs          # State enum
├── NPC_Interactable.cs       # NPC-side interaction component
├── Player_Interactor.cs      # Player-side proximity detection
├── Typewriter_Effect.cs      # Character-by-character text reveal
└── InteractionPromptHelper.cs # Interaction prompt display
```

## Scenes

| Scene | Purpose |
|-------|---------|
| CityExplorationScene | Main gameplay — procedurally generated city |
| CombatScene | Dedicated combat encounters |
| DialogueDemoScene | Testing/demo for dialogue system |
| SampleScene | Default Unity scene |

## Coding Conventions

- **Namespaces**: `Flipit.Dialogue`, `Flipit.Combat`, `Flipit.CityTerrain`
- **Class naming**: PascalCase with underscore separation for compound names (`Combat_System`, `Dialogue_Manager`, `City_Generator`)
- **Private fields**: `_camelCase` with underscore prefix
- **SerializeField**: Always `[SerializeField] private`, never public fields
- **Properties**: PascalCase, often with clamping/validation in getters
- **Constants**: PascalCase (`MaxDisplayedOptions`)
- **Documentation**: XML `<summary>` blocks on public members
- **Logging**: `Debug.Log/LogWarning/LogError` with `[ClassName]` prefix
- **Patterns**: Singleton (Instance property), State Machine (enum + transition table), Event Bus (static class + HashSet), ScriptableObject data, Interface abstraction

## Assembly Dependency Graph

```
Flipit.Dialogue  (base — no game-assembly dependencies)
       ↑
Flipit.Combat    (depends on Dialogue)
       ↑
Flipit.CityTerrain (depends on Dialogue + Combat)
       ↑
Flipit.Dialogue.Editor (depends on all — Editor only)
```
