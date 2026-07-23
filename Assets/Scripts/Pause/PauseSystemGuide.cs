/// <summary>
/// ╔══════════════════════════════════════════════════════════════════╗
/// ║           GUÍA DE SETUP - SISTEMA DE MENÚ DE PAUSA             ║
/// ╚══════════════════════════════════════════════════════════════════╝
/// 
/// Este archivo es solo documentación. No necesita estar en ningún GameObject.
/// 
/// ═══════════════════════════════════════════════════
/// OPCIÓN A: SETUP AUTOMÁTICO (Recomendado para testing rápido)
/// ═══════════════════════════════════════════════════
/// 
/// 1. Crear GameObject vacío: "--- PAUSE SYSTEM ---"
/// 2. Agregar componente: PauseSystemSetup
/// 3. Entrar en Play Mode → La UI se crea automáticamente.
/// 4. IMPORTANTE: Después del primer Play, asignar manualmente en PauseManager:
///    - Input Handler
///    - Pause Menu Panel
///    - Combat Pause Overlay
/// 
/// ═══════════════════════════════════════════════════
/// OPCIÓN B: SETUP MANUAL (Recomendado para producción)
/// ═══════════════════════════════════════════════════
/// 
/// JERARQUÍA DE LA ESCENA:
/// 
/// --- PAUSE SYSTEM ---
///   ├── [GameStateManager]           ← Agregar componente GameStateManager
///   ├── [PauseInputHandler]          ← Agregar componente PauseInputHandler (configurar tecla)
///   ├── [PauseManager]               ← Agregar componente PauseManager (asignar referencias)
///   ├── [MockPlayerDataProvider]     ← Agregar componente MockPlayerDataProvider
///   └── [DebugStateToggle]           ← SOLO TESTING, eliminar en release
/// 
/// PauseCanvas (Canvas, Sort Order: 100, Screen Space - Overlay)
///   ├── PauseMenuPanel               ← Agregar PauseMenuUI, SaveAndQuitButton, AlbumFichasButton
///   │   ├── PlayerNameText           (TextMeshProUGUI)
///   │   ├── CurrencyText             (TextMeshProUGUI)
///   │   ├── Btn_AlbumFichas          (Button)
///   │   ├── Btn_Opciones             (Button)
///   │   ├── Btn_GuardarSalir         (Button)
///   │   ├── Btn_VolverMenu           (Button)
///   │   ├── Btn_VolverMenu           (Button)
///   │   └── FeedbackText             (TextMeshProUGUI, inicia desactivado)
///   │
///   ├── CombatPauseOverlay           ← Agregar CombatPauseOverlay
///   │   ├── Background               (Image: negro, alpha 0.7, stretch full)
///   │   └── PauseText                (TextMeshProUGUI: "Pausa", 72pt, centrado, blanco)
///   │
///   ├── OptionsPanel                 ← Agregar OptionsPanel
///   │   ├── Título                   (TextMeshProUGUI: "Opciones")
///   │   ├── MusicVolumeSlider        (Slider: 0-100)
///   │   ├── SFXVolumeSlider          (Slider: 0-100)
///   │   ├── BrightnessSlider         (Slider: 0-100)
///   │   ├── ResolutionDropdown       (TMP_Dropdown)
///   │   ├── FullscreenToggle         (Toggle)
///   │   └── Btn_Back                 (Button)
///   │
///   └── ConfirmationDialog           ← Agregar ConfirmationDialog
///       ├── MessageText              (TextMeshProUGUI)
///       ├── Btn_Confirm              (Button: "Sí")
///       └── Btn_Cancel               (Button: "No")
/// 
/// ═══════════════════════════════════════════════════
/// ASIGNACIÓN DE REFERENCIAS EN EL INSPECTOR:
/// ═══════════════════════════════════════════════════
/// 
/// PauseManager:
///   - Input Handler → PauseInputHandler
///   - Pause Menu Panel → PauseMenuPanel
///   - Combat Pause Overlay → CombatPauseOverlay
/// 
/// PauseMenuUI:
///   - Player Name Text → PlayerNameText
///   - Player Currency Text → CurrencyText
///   - Album Fichas Button → Btn_AlbumFichas
///   - Options Button → Btn_Opciones
///   - Save And Quit Button → Btn_GuardarSalir
///   - Return To Main Menu Button → Btn_VolverMenu
///   - Options Panel → OptionsPanel
///   - Confirmation Panel → ConfirmationDialog
///   - Pause Manager → PauseManager
/// 
/// OptionsPanel:
///   - Music Volume Slider → MusicVolumeSlider
///   - SFX Volume Slider → SFXVolumeSlider
///   - Brightness Slider → BrightnessSlider
///   - Resolution Dropdown → ResolutionDropdown
///   - Fullscreen Toggle → FullscreenToggle
///   - Pause Menu Panel → PauseMenuPanel
///   - Back Button → Btn_Back
/// 
/// ConfirmationDialog:
///   - Message Text → MessageText
///   - Confirm Button → Btn_Confirm
///   - Cancel Button → Btn_Cancel
/// 
/// CombatPauseOverlay:
///   - Background Image → Background
///   - Pause Text → PauseText
/// 
/// SaveAndQuitButton:
///   - Save And Quit Button → Btn_GuardarSalir
///   - Feedback Text → FeedbackText
/// 
/// AlbumFichasButton:
///   - Album Button → Btn_AlbumFichas
///   - Album Fichas Prefab → (asignar después del merge)
///   - Parent Transform → PauseCanvas
///   - Feedback Text → FeedbackText
/// 
/// ═══════════════════════════════════════════════════
/// INTEGRACIÓN CON OTRAS BRANCHES:
/// ═══════════════════════════════════════════════════
/// 
/// ALBUM DE FICHAS (post-merge):
///   1. Hacer merge con la branch de Album de Fichas.
///   2. Asignar el prefab del Álbum en AlbumFichasButton → albumFichasPrefab.
///   3. Listo. El botón instanciará el prefab al presionarlo.
/// 
/// SISTEMA DE GUARDADO (post-merge):
///   1. Hacer merge con la branch de Album de Fichas (que contiene SaveSystem).
///   2. En SaveAndQuitButton.cs, descomentar la lógica y conectar con SaveSystem.
///   3. Implementar el flujo: Guardar → Cargar escena MainMenu.
/// 
/// SISTEMA DE DINERO (cuando esté listo):
///   1. Crear una clase que implemente IPlayerDataProvider.
///   2. Reemplazar MockPlayerDataProvider con la implementación real.
///   3. PauseMenuUI lo encontrará automáticamente.
/// 
/// MENÚ PRINCIPAL (cuando exista la escena):
///   1. En PauseMenuUI.OnReturnToMainMenuPressed, cambiar el TODO por:
///      SceneManager.LoadScene("MainMenu");
///   2. Asegurarse de que Time.timeScale se restaure a 1 antes del cambio de escena.
/// 
/// ═══════════════════════════════════════════════════
/// TESTING RÁPIDO:
/// ═══════════════════════════════════════════════════
/// 
/// 1. Agregar DebugStateToggle a un GameObject.
/// 2. Play Mode:
///    - F1 → Alterna entre Exploration y Combat
///    - Escape → Abre/cierra pausa
///    - WASD/Flechas/Stick → Navegar entre opciones del menú
///    - Space/Botón Sur gamepad → Confirmar/Seleccionar
///    - En Exploration: menú completo con botones
///    - En Combat: pantalla negra con "Pausa"
/// 
/// ═══════════════════════════════════════════════════
/// CONFIGURAR CONTROLES DE UI (Navigate/Submit/Cancel):
/// ═══════════════════════════════════════════════════
/// 
/// Los controles de navegación del menú se configuran desde el Input Action Asset:
/// 
/// 1. Abrir Assets/InputSystem_Actions.inputactions en el editor.
/// 2. Seleccionar el Action Map "UI".
/// 3. Para cambiar el botón de confirmar:
///    - Seleccionar la action "Submit"
///    - Modificar/agregar bindings (default: Space en teclado, buttonSouth en gamepad)
/// 4. Para cambiar la navegación:
///    - Seleccionar la action "Navigate"
///    - Modificar los bindings del composite 2DVector (default: WASD + flechas + stick)
/// 5. Para cambiar el botón de cancelar:
///    - Seleccionar la action "Cancel"
///    - Modificar bindings según necesidad
/// 
/// El InputSystemUIInputModule en el EventSystem usa este asset automáticamente.
/// No se necesita tocar código para cambiar los controles de UI.
/// </summary>
public static class PauseSystemGuide
{
    // Este archivo es solo documentación.
    // No tiene funcionalidad ejecutable.
}
