using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Script de debug para testing del sistema de pausa.
/// Permite alternar entre estados Exploration y Combat presionando una tecla.
/// 
/// SOLO PARA TESTING — Eliminar o desactivar antes de release.
/// Usa este script para verificar que el menú de pausa se comporta correctamente
/// en ambos estados sin necesitar un sistema de combate real.
/// </summary>
public class DebugStateToggle : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Tecla para alternar entre Exploration y Combat.")]
    [SerializeField] private Key toggleKey = Key.F1;

    [Header("Estado Visual (solo lectura)")]
    [SerializeField] private string currentStateDisplay = "Esperando GameStateManager...";

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard[toggleKey].wasPressedThisFrame)
        {
            ToggleState();
        }

        // Actualizar display
        if (GameStateManager.Instance != null)
        {
            currentStateDisplay = $"Estado: {GameStateManager.Instance.CurrentState}";
        }
    }

    private void ToggleState()
    {
        if (GameStateManager.Instance == null)
        {
            Debug.LogWarning("[DebugStateToggle] GameStateManager no encontrado.");
            return;
        }

        if (GameStateManager.Instance.CurrentState == GameStateManager.GameState.Exploration)
        {
            GameStateManager.Instance.SetCombat();
            Debug.Log("[DebugStateToggle] Estado cambiado a: COMBAT");
        }
        else
        {
            GameStateManager.Instance.SetExploration();
            Debug.Log("[DebugStateToggle] Estado cambiado a: EXPLORATION");
        }
    }

    private void OnGUI()
    {
        // Mostrar estado actual en esquina superior izquierda para debug
        string state = GameStateManager.Instance != null
            ? GameStateManager.Instance.CurrentState.ToString()
            : "N/A";

        GUI.Label(new Rect(10, 10, 300, 25), $"Estado: {state} (F1 para cambiar)");
        GUI.Label(new Rect(10, 35, 300, 25), $"Pausa: Presiona Escape");
    }
}
