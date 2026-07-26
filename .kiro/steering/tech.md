# Flipit — Tech Stack

## Engine & Platform

- **Unity 6** (6000.5.4f1)
- **C# 9.0** / .NET Standard 2.1
- **Build Target**: StandaloneWindows64
- **Render Pipeline**: Universal Render Pipeline (URP) 17.5.0

## Key Packages

| Package | Version | Purpose |
|---------|---------|---------|
| com.unity.inputsystem | 1.19.0 | New Input System (Player & UI action maps) |
| com.unity.render-pipelines.universal | 17.5.0 | URP rendering |
| com.unity.ai.navigation | 2.0.13 | NavMesh navigation |
| com.unity.ugui | 2.5.0 | Canvas-based UI |
| Unity.TextMeshPro | (bundled) | Text rendering in UI |
| com.unity.timeline | 1.8.12 | Timeline sequences |
| com.unity.test-framework | 1.7.0 | Unit/integration testing |
| com.unity.visualscripting | 1.9.11 | Visual scripting |

## Assembly Definitions

The project uses `.asmdef` files for compilation isolation:

| Assembly | Namespace | Dependencies |
|----------|-----------|--------------|
| Flipit.Dialogue | `Flipit.Dialogue` | InputSystem, TMP, uGUI |
| Flipit.Combat | `Flipit.Combat` | Flipit.Dialogue, InputSystem, TMP, uGUI |
| Flipit.CityTerrain | `Flipit.CityTerrain` | Flipit.Dialogue, Flipit.Combat, InputSystem, TMP, uGUI |
| Flipit.Dialogue.Editor | (none) | All runtime assemblies (Editor-only) |

## IDE Support

- JetBrains Rider (com.unity.ide.rider 3.0.38)
- Visual Studio (com.unity.ide.visualstudio 2.0.26)

## Commands

There are no CLI build scripts. All generation and testing is done through Unity Editor menu items:

| Menu Item | Purpose |
|-----------|---------|
| Flipit → Generate City | Builds CityExplorationScene with staged generation |
| Flipit → Generate City (No Dialogs) | Same as above, skips confirmation prompts |
| Flipit → Build Dialogue Demo Scene | Builds DialogueDemoScene for testing dialogue |
| Flipit → Build Combat Scene | Builds CombatScene for testing combat |

## Input System

Uses Unity's New Input System with two action maps:
- **Player**: WASD movement, interaction (E key)
- **UI**: Navigate, Submit, Cancel for dialogue/menu navigation
