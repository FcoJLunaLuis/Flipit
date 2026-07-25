using System;
using UnityEngine;

/// <summary>
/// Coordina la pausa del juego: detiene el tiempo y activa la UI correcta
/// según el estado actual del juego (Exploración o Combate).
/// </summary>
public class PauseManager : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Referencia al PauseInputHandler para suscribirse al evento de pausa.")]
    [SerializeField] private PauseInputHandler inputHandler;

    [Header("Paneles de UI")]
    [Tooltip("Panel del menú de pausa completo (se muestra en Exploración).")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Tooltip("Panel del overlay de pausa en combate (pantalla negra con texto Pausa).")]
    [SerializeField] private GameObject combatPauseOverlay;

    /// <summary>
    /// Indica si el juego está actualmente pausado.
    /// </summary>
    public bool IsPaused { get; private set; }

    /// <summary>
    /// Cuando es false, la pausa no se puede activar (otro sistema tiene prioridad, ej: NPC abierto).
    /// Siempre se permite reanudar aunque CanPause sea false.
    /// </summary>
    private bool canPause = true;

    /// <summary>
    /// Evento que se dispara cuando cambia el estado de pausa.
    /// Parámetro: true si se pausó, false si se reanudó.
    /// </summary>
    public event Action<bool> OnPauseStateChanged;

    private void OnEnable()
    {
        if (inputHandler != null)
        {
            inputHandler.OnPausePressed += TogglePause;
        }
    }

    private void OnDisable()
    {
        if (inputHandler != null)
        {
            inputHandler.OnPausePressed -= TogglePause;
        }
    }

    private void Start()
    {
        // Asegurarse de que los paneles inician desactivados
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (combatPauseOverlay != null) combatPauseOverlay.SetActive(false);

        IsPaused = false;
    }

    /// <summary>
    /// Alterna entre pausar y reanudar el juego.
    /// No permite pausar si otro sistema bloqueó la pausa (ej: NPC abierto).
    /// </summary>
    public void TogglePause()
    {
        if (IsPaused)
        {
            Resume();
        }
        else
        {
            if (!canPause) return;
            Pause();
        }
    }

    /// <summary>
    /// Bloquea la pausa. Llamar cuando otro sistema toma el control (ej: NPC UI abierta).
    /// </summary>
    public void BlockPause()
    {
        canPause = false;
    }

    /// <summary>
    /// Desbloquea la pausa. Llamar cuando el otro sistema libera el control.
    /// </summary>
    public void UnblockPause()
    {
        canPause = true;
    }

    /// <summary>
    /// Pausa el juego y muestra la UI correspondiente al estado actual.
    /// </summary>
    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;

        ShowPauseUI();
        OnPauseStateChanged?.Invoke(true);
    }

    /// <summary>
    /// Reanuda el juego y oculta toda la UI de pausa.
    /// </summary>
    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        HideAllPauseUI();
        OnPauseStateChanged?.Invoke(false);
    }

    private void ShowPauseUI()
    {
        if (GameStateManager.Instance == null)
        {
            Debug.LogWarning("[PauseManager] GameStateManager no encontrado. Mostrando menú de pausa por defecto.");
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
            return;
        }

        switch (GameStateManager.Instance.CurrentState)
        {
            case GameStateManager.GameState.Exploration:
                if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
                break;

            case GameStateManager.GameState.Combat:
                if (combatPauseOverlay != null) combatPauseOverlay.SetActive(true);
                break;
        }
    }

    private void HideAllPauseUI()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (combatPauseOverlay != null) combatPauseOverlay.SetActive(false);
    }
}
