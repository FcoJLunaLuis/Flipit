using System;
using UnityEngine;

/// <summary>
/// Singleton que gestiona el estado actual del juego.
/// Otros sistemas (como PauseManager) consultan este estado para decidir su comportamiento.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    /// <summary>
    /// Estados posibles del juego.
    /// </summary>
    public enum GameState
    {
        Exploration,
        Combat
    }

    [SerializeField] private GameState initialState = GameState.Exploration;

    private GameState currentState;

    /// <summary>
    /// Estado actual del juego. Al asignarlo se dispara OnGameStateChanged.
    /// </summary>
    public GameState CurrentState
    {
        get => currentState;
        set
        {
            if (currentState == value) return;

            GameState previousState = currentState;
            currentState = value;
            OnGameStateChanged?.Invoke(previousState, currentState);
        }
    }

    /// <summary>
    /// Evento que se dispara cuando cambia el estado del juego.
    /// Parámetros: (estadoAnterior, estadoNuevo)
    /// </summary>
    public event Action<GameState, GameState> OnGameStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        currentState = initialState;
    }

    /// <summary>
    /// Cambia el estado a Exploration.
    /// </summary>
    public void SetExploration()
    {
        CurrentState = GameState.Exploration;
    }

    /// <summary>
    /// Cambia el estado a Combat.
    /// </summary>
    public void SetCombat()
    {
        CurrentState = GameState.Combat;
    }
}
