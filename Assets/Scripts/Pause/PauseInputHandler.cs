using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Detecta la tecla de pausa configurada desde el Inspector y dispara un evento.
/// Usa el nuevo Input System Package.
/// Funciona independientemente de Time.timeScale (Update sigue ejecutándose con timeScale = 0).
/// </summary>
public class PauseInputHandler : MonoBehaviour
{
    [Header("Configuración de Input")]
    [Tooltip("Tecla que activa/desactiva la pausa. Configurable desde el Inspector.")]
    [SerializeField] private Key pauseKey = Key.Escape;

    /// <summary>
    /// Evento que se dispara cuando se presiona la tecla de pausa.
    /// </summary>
    public event Action OnPausePressed;

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard[pauseKey].wasPressedThisFrame)
        {
            OnPausePressed?.Invoke();
        }
    }

    /// <summary>
    /// Permite cambiar la tecla de pausa en runtime si fuera necesario en el futuro.
    /// </summary>
    /// <param name="newKey">Nueva tecla de pausa.</param>
    public void SetPauseKey(Key newKey)
    {
        pauseKey = newKey;
    }

    /// <summary>
    /// Retorna la tecla de pausa actual.
    /// </summary>
    public Key GetCurrentPauseKey()
    {
        return pauseKey;
    }
}
