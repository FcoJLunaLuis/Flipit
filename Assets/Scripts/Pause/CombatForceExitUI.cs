using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Permite al jugador forzar la salida del combate desde el overlay de pausa.
/// Se usa cuando el combate se atora (fichas que no voltean o atraviesan el suelo).
/// Nadie gana ni pierde — el combate se cancela limpiamente.
/// 
/// Setup:
/// - Agregar este componente al GameObject del CombatPauseOverlay.
/// - Asignar el botón de salida forzada y la referencia al PauseManager.
/// </summary>
public class CombatForceExitUI : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Botón que el jugador presiona para forzar la salida del combate.")]
    [SerializeField] private Button _forceExitButton;

    [Tooltip("Referencia al PauseManager para reanudar el juego antes de salir.")]
    [SerializeField] private PauseManager _pauseManager;

    private void OnEnable()
    {
        if (_forceExitButton != null)
        {
            _forceExitButton.onClick.AddListener(OnForceExitPressed);
        }
    }

    private void OnDisable()
    {
        if (_forceExitButton != null)
        {
            _forceExitButton.onClick.RemoveListener(OnForceExitPressed);
        }
    }

    private void OnForceExitPressed()
    {
        Debug.Log("[CombatForceExitUI] Forzando salida del combate.");

        // Reanudar el juego (restaura timeScale y oculta paneles de pausa)
        if (_pauseManager != null)
        {
            _pauseManager.Resume();
        }
        else
        {
            // Fallback: restaurar timeScale manualmente si no hay PauseManager
            Time.timeScale = 1f;
            Debug.LogWarning("[CombatForceExitUI] PauseManager no asignado. TimeScale restaurado manualmente.");
        }

        // Salir del combate sin consecuencias
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.SalirDelCombate();
        }
        else
        {
            Debug.LogWarning("[CombatForceExitUI] CombatManager no encontrado. No se pudo salir del combate.");
        }
    }
}
