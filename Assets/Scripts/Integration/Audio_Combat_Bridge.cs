using UnityEngine;
using UnityEngine.SceneManagement;
using Flipit.Core;

/// <summary>
/// Bridge between GameStateManager and Audio_Manager.
/// Listens for Combat/Exploration state changes and triggers the appropriate music.
/// Lives in Assembly-CSharp (no asmdef) so it can access both systems.
/// Re-attempts subscription on scene load in case GameStateManager initializes later.
/// </summary>
public class Audio_Combat_Bridge : MonoBehaviour
{
    [Header("Combat Music")]
    [SerializeField] private AudioClip _combatMusic;
    [SerializeField, Range(0f, 1f)] private float _combatVolume = 0.7f;

    private bool _subscribed;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Subscribe();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Unsubscribe();
    }

    private void Start()
    {
        Subscribe();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Retry subscription when a new scene loads (GameStateManager may now exist)
        if (!_subscribed)
        {
            Subscribe();
        }
    }

    private void Subscribe()
    {
        if (_subscribed) return;

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
            _subscribed = true;
            Debug.Log("[Audio_Combat_Bridge] Subscribed to GameStateManager.");
        }
    }

    private void Unsubscribe()
    {
        if (!_subscribed) return;

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }
        _subscribed = false;
    }

    private void OnGameStateChanged(GameStateManager.GameState previous, GameStateManager.GameState current)
    {
        if (Audio_Manager.Instance == null) return;

        if (current == GameStateManager.GameState.Combat)
        {
            if (_combatMusic != null)
            {
                Audio_Manager.Instance.PlayMusic(_combatMusic, _combatVolume);
                Debug.Log("[Audio_Combat_Bridge] Switched to combat music.");
            }
        }
        else if (current == GameStateManager.GameState.Exploration)
        {
            Audio_Manager.Instance.ResumeLastTrack();
            Debug.Log("[Audio_Combat_Bridge] Resumed exploration music.");
        }
    }
}
