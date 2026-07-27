# Implementation Plan: Flipit Betting Album Integration

## Overview

This plan implements the bridge between the Album system and the Combat/Betting system. The central component is `CombatAlbumBridge`, a MonoBehaviour that observes CombatManager events without modifying existing files (CombatManager.cs, AlbumManager.cs). Implementation uses C# 9.0 / Unity 6, Newtonsoft.Json for crash recovery serialization, and the existing `AlbumChipInventory` adapter for chip operations.

## Tasks

- [x] 1. Create data models for crash recovery
  - [x] 1.1 Create FichaEnJuegoEntry and FichasEnJuegoData classes
    - Create `Assets/Scripts/Integration/FichaEnJuegoEntry.cs`
    - Implement `[Serializable] public class FichaEnJuegoEntry` with fields: `int templateId`, `int cantidad`, `bool esLanzadora`, `FichaData fichaData`
    - Implement `[Serializable] public class FichasEnJuegoData` with fields: `string timestampConfirmacion`, `List<FichaEnJuegoEntry> fichas`
    - Use `[Newtonsoft.Json.JsonProperty]` attributes for serialization compatibility
    - Add XML `<summary>` documentation on each class and field
    - _Requirements: 3.2, 3.4_

- [x] 2. Implement CombatAlbumBridge core component
  - [x] 2.1 Create CombatAlbumBridge MonoBehaviour with lifecycle and event subscription
    - Create `Assets/Scripts/Integration/CombatAlbumBridge.cs`
    - Implement MonoBehaviour with `[SerializeField] private CombatConfig _combatConfig`
    - Implement `[SerializeField] private GameObject _mensajeValidacionPanel` and `[SerializeField] private TMPro.TextMeshProUGUI _mensajeValidacionTexto` for UI feedback
    - Implement `public Action<CombatData> OnAlbumActualizado` event
    - In `OnEnable()`: subscribe to `CombatManager.Instance.OnCombateIniciado`, `OnCombateTerminado`, `OnFaseCambiada`
    - In `OnDisable()`: unsubscribe from all events
    - In `Awake()`: call `IntentarRecuperacionCrash()`
    - Store internal state: `_chipInventory` (AlbumChipInventory), `_chipCatalog` (AlbumChipCatalog), `_fichasEnJuego` (List), `_apuestaConfirmada` (bool)
    - All event handlers wrapped in try/catch per Requirement 10.5
    - _Requirements: 10.1, 10.5_

  - [x] 2.2 Implement ValidarPreCombate and ObtenerFichasDisponiblesParaApuesta
    - Implement `public bool ValidarPreCombate()`: access `AlbumManager.Instance.ObtenerAlbumData()`, count fichas where `estaRoto == false` and `cantidad >= 1`, require count >= `_combatConfig.minFichasApuesta + 1`
    - If AlbumManager.Instance is null or ObtenerTodasLasFichas() returns null: return false, `Debug.LogError("[CombatAlbumBridge]...")`
    - If all fichas rotas: show message "No posees fichas en condiciones de combate"
    - If fichas < minFichasApuesta + 1: show message indicating requirement
    - If fichas == minFichasApuesta (no lanzadora available): show specific message about needing one more ficha
    - Implement `public List<FichaData> ObtenerFichasDisponiblesParaApuesta()`: filter from AlbumData excluding `estaRoto == true` and `cantidad <= 0`
    - Implement `MostrarMensajeValidacion(string mensaje)` and `OcultarMensajeValidacion()` helpers
    - _Requirements: 1.2, 1.3, 1.4, 1.5, 11.1, 11.2, 11.3, 11.4, 11.5_

  - [x] 2.3 Implement ConfirmarApuesta with transactional rollback
    - Implement `public bool ConfirmarApuesta(List<FichaData> fichasApostadas, FichaData fichaLanzadora)`
    - Initialize `AlbumChipInventory` from `AlbumManager.Instance.ObtenerAlbumData()` and `AlbumChipCatalog`
    - Track removed chips in `List<(string chipId, int cantidad)>` for rollback
    - For each ficha in fichasApostadas: call `_chipInventory.RemoveChips(templateId.ToString(), 1)`. If fails → rollback all previously removed and return false
    - Remove lanzadora: `_chipInventory.RemoveChips(lanzadoraId, 1)`. If fails → rollback all and return false
    - On success: build `_fichasEnJuego` list, call `PersistirFichasEnJuego()`, set `_apuestaConfirmada = true`
    - Wrap entire operation in try/catch with rollback in catch block
    - _Requirements: 3.1, 3.3, 3.5, 10.5_

  - [x] 2.4 Implement ProcesarResultadoCombate
    - Implement `public void ProcesarResultadoCombate(CombatData combatData)`
    - If `combatData.FichasGanadasJugador` is not null: call `AlbumManager.Instance.AgregarFicha(ficha)` for each
    - Fichas perdidas (in FichasGanadasNPC belonging to jugador): already removed, just clear from `_fichasEnJuego`
    - Discard fichas in FichasGanadasNPC whose original owner is NPC (no álbum action)
    - Restore FichaLanzadoraJugador: `AlbumManager.Instance.AgregarFicha(fichaLanzadora)` if not null
    - Call `EliminarArchivoCrashRecovery()`
    - Trigger save via coroutine `GuardarConReintentos()`
    - Fire `OnAlbumActualizado?.Invoke(combatData)`
    - Handle null FichasGanadasJugador and null FichaLanzadora with `Debug.LogWarning`
    - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5, 8.1, 8.2, 8.3, 8.4, 8.5, 9.1, 9.2, 9.5_

  - [x] 2.5 Implement crash recovery (PersistirFichasEnJuego, IntentarRecuperacionCrash, EliminarArchivoCrashRecovery)
    - Implement `PersistirFichasEnJuego()`: serialize `FichasEnJuegoData` to JSON using `Newtonsoft.Json.JsonConvert.SerializeObject`, write to `Application.persistentDataPath/fichas_en_juego.json`
    - Implement `IntentarRecuperacionCrash()`: check if file exists, deserialize, restore each ficha to AlbumData via `AlbumManager.Instance.AgregarFicha()`, delete file after restore
    - Handle corrupted file: wrap deserialization in try/catch, delete file on failure with `Debug.LogWarning`
    - Implement `EliminarArchivoCrashRecovery()`: delete file if exists
    - Use `System.IO.File.Exists`, `File.WriteAllText`, `File.ReadAllText`, `File.Delete`
    - _Requirements: 3.2, 3.4, 3.6_

  - [x] 2.6 Implement event handlers (OnCombateIniciado, OnCombateTerminado, OnFaseCambiada) and GuardarConReintentos
    - `OnCombateIniciado()`: reset internal state, prepare for new combat
    - `OnFaseCambiada(CombatData.CombatPhase fase)`: when fase == BetSelection, call `ObtenerFichasDisponiblesParaApuesta()` and provide to BetSelectionLogic
    - `OnCombateTerminado(CombatData data)`: call `ProcesarResultadoCombate(data)` wrapped in try/catch
    - Implement `GuardarConReintentos()` coroutine: attempt `SaveSystem.Guardar()` up to 3 times with 1s intervals; log success/failure per attempt
    - _Requirements: 1.1, 7.4, 9.1, 9.3, 9.4, 10.5_

- [x] 3. Checkpoint — Core Bridge Validation
  - Ensure all tests pass, ask the user if questions arise.

- [x] 4. Create BettingAlbumIntegrationSceneBuilder (Editor script)
  - [x] 4.1 Create BettingAlbumIntegrationSceneBuilder
    - Create `Assets/Editor/BettingAlbumIntegrationSceneBuilder.cs`
    - Add `[MenuItem("Flipit/Build Betting Album Integration Test Scene")]`
    - Follow pattern of existing `CombatSceneBuilder.cs`
    - Scene path: `Assets/Scenes/BettingAlbumIntegrationTestScene.unity`
    - Create scene contents:
      - Camera (orthographic, positioned for 2D view)
      - Ground + Walls (BoxCollider2D boundaries)
      - AlbumManager with test `FichaTemplate[]` (3-5 test fichas) via SerializeField
      - CombatManager with CombatConfig asset (default min=1, max=5)
      - CombatAlbumBridge component with _combatConfig reference and UI panel references
      - A FlipCombat_NPC ("FlipCombat_NPC_Test") with NPC pool (3-5 fichas)
      - UI Canvas for validation messages (panel + TextMeshProUGUI)
      - Player with TopDownPlayerMovement, PlayerInput, Player_Interactor
      - Placeholder combat components (TowerPhysicsBuilder, ImpactResolver, FlipDetector — can be null refs)
      - ThrowMinigameController (null refs acceptable)
      - CombatArena (null ref acceptable)
    - Add scene to Build Settings
    - Use `EnsureFolder`, `EditorSceneManager.SaveScene`, `AssetDatabase.SaveAssets` pattern
    - _Requirements: 10.1, 10.2, 10.3, 11.1_

- [x] 5. Checkpoint — Scene Builder Validation
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 6. Write unit tests for CombatAlbumBridge
  - [x] 6.1 Create test assembly and test infrastructure
    - Create `Assets/Tests/EditMode/Flipit.Integration.Tests.EditMode.asmdef` referencing: Assembly-CSharp, UnityEngine.TestRunner, NUnit, Newtonsoft.Json
    - If asmdef already exists (Flipit.Dialogue.Tests.EditMode), add Integration assembly reference or create a separate asmdef
    - Create test helper `CombatAlbumBridgeTestHelper.cs` with factory methods for creating test AlbumData, FichaData, FichaTemplates, and CombatData instances
    - _Requirements: 10.5_

  - [x] 6.2 Write unit tests for ValidarPreCombate
    - Create `Assets/Tests/EditMode/CombatAlbumBridgeValidationTests.cs`
    - Test: álbum with 0 fichas → returns false
    - Test: álbum with all fichas rotas → returns false
    - Test: álbum with exactly minFichasApuesta fichas (no lanzadora possible) → returns false
    - Test: álbum with minFichasApuesta + 1 fichas no rotas → returns true
    - Test: álbum with many fichas including mixed rotas/no-rotas → correct count
    - Test: AlbumManager null → returns false with LogError
    - _Requirements: 1.3, 11.2, 11.3, 11.4_

  - [x] 6.3 Write unit tests for ConfirmarApuesta (success + rollback)
    - Create `Assets/Tests/EditMode/CombatAlbumBridgeConfirmationTests.cs`
    - Test: successful confirmation with 3 fichas + lanzadora → álbum quantities reduced
    - Test: ficha not in álbum → rollback, all previously removed fichas restored, returns false
    - Test: lanzadora removal fails → rollback all apostadas, returns false
    - Test: crash file created after successful confirmation
    - Test: _apuestaConfirmada set to true only on success
    - _Requirements: 3.1, 3.3, 3.5_

  - [x] 6.4 Write unit tests for ProcesarResultadoCombate
    - Create `Assets/Tests/EditMode/CombatAlbumBridgePostCombatTests.cs`
    - Test: victoria total — all fichas ganadas added to álbum
    - Test: derrota total — fichas apostadas stay removed, none restored
    - Test: partial result — mix of ganadas/perdidas correctly handled
    - Test: lanzadora always restored regardless of result
    - Test: FichasGanadasJugador null → no error, continues
    - Test: FichaLanzadoraJugador null → warning logged, continues
    - Test: OnAlbumActualizado event fired with correct CombatData
    - _Requirements: 7.1, 7.2, 7.3, 7.5, 8.1, 8.4_

  - [x] 6.5 Write unit tests for crash recovery
    - Create `Assets/Tests/EditMode/CombatAlbumBridgeCrashRecoveryTests.cs`
    - Test: PersistirFichasEnJuego writes valid JSON file
    - Test: IntentarRecuperacionCrash reads file and restores fichas to álbum
    - Test: corrupted file → deleted without crash, warning logged
    - Test: no file exists → no-op, no errors
    - Test: EliminarArchivoCrashRecovery removes file
    - Test: round-trip (write → read) preserves all FichaEnJuegoEntry data
    - _Requirements: 3.2, 3.4, 3.6_

  - [ ]* 6.6 Write property test: Filtrado excluye fichas rotas y sin cantidad
    - **Property 1: Filtrado de fichas excluye rotas y sin cantidad**
    - For random AlbumData with varied estaRoto/cantidad values, ObtenerFichasDisponiblesParaApuesta() contains only fichas where estaRoto == false AND cantidad >= 1
    - Use [Test, Repeat(100)] with randomized AlbumEntry generation
    - **Validates: Requirements 1.2, 1.4**

  - [ ]* 6.7 Write property test: Validación pre-combate bloquea cuando insuficientes
    - **Property 2: Validación pre-combate bloquea cuando fichas insuficientes**
    - For random AlbumData with N fichas no rotas where N < minFichasApuesta + 1, ValidarPreCombate() returns false without modifying álbum state
    - Use [Test, Repeat(100)] with randomized N in [0, minFichasApuesta]
    - **Validates: Requirements 1.3, 11.2, 11.4, 11.5**

  - [ ]* 6.8 Write property test: Confirmación remueve fichas correctamente
    - **Property 4: Confirmación de apuesta remueve todas las fichas del álbum**
    - For random valid selection (apostadas + lanzadora), after ConfirmarApuesta() success, álbum total == previous total - (apostadas.Count + 1)
    - Use [Test, Repeat(100)] with randomized selections
    - **Validates: Requirements 3.1, 3.5**

  - [ ]* 6.9 Write property test: Round-trip crash recovery preserva fichas
    - **Property 5: Round-trip de crash recovery preserva fichas**
    - For random FichasEnJuegoData, serialize → deserialize produces equivalent set; restore increments álbum quantities exactly
    - Use [Test, Repeat(100)] with randomized FichaEnJuegoEntry lists
    - **Validates: Requirements 3.2, 3.4**

  - [ ]* 6.10 Write property test: Rollback transaccional ante fallo
    - **Property 6: Rollback transaccional ante fallo de remoción**
    - For any ConfirmarApuesta where RemoveChips returns false mid-operation, álbum state after == álbum state before
    - Use [Test, Repeat(100)] with deliberately insufficient quantities at random positions
    - **Validates: Requirements 3.3**

  - [ ]* 6.11 Write property test: Estado post-combate correcto
    - **Property 10: Estado del álbum post-combate es correcto**
    - For random CombatData results, after ProcesarResultadoCombate: álbum contains all FichasGanadasJugador, does NOT contain fichas perdidas, contains lanzadora restored
    - Use [Test, Repeat(100)] with randomized combat outcomes
    - **Validates: Requirements 7.1, 7.2, 7.3, 8.1, 8.5**

- [ ] 7. Final Checkpoint — Full System Validation
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation
- Property tests validate universal correctness properties from the design document
- Unit tests validate specific examples and edge cases
- No existing files are modified (CombatManager.cs, AlbumManager.cs remain untouched)
- All new runtime code goes in `Assets/Scripts/Integration/`
- Editor script goes in `Assets/Editor/`
- Tests go in `Assets/Tests/EditMode/`
- Newtonsoft.Json is already available in the project
- The test scene builder is the primary way to verify integration in Unity Editor

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1"] },
    { "id": 1, "tasks": ["2.1"] },
    { "id": 2, "tasks": ["2.2", "2.5"] },
    { "id": 3, "tasks": ["2.3", "2.4"] },
    { "id": 4, "tasks": ["2.6"] },
    { "id": 5, "tasks": ["4.1", "6.1"] },
    { "id": 6, "tasks": ["6.2", "6.3", "6.4", "6.5"] },
    { "id": 7, "tasks": ["6.6", "6.7", "6.8", "6.9", "6.10", "6.11"] }
  ]
}
```
