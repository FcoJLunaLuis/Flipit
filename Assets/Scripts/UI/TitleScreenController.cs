using UnityEngine;
using UnityEngine.SceneManagement;
using Flipit.Core;

/// <summary>
/// Controls the title screen menu. Handles Play, Instructions, Credits, and Quit buttons.
/// Attach to the TitleCanvas root in TitleScene.
/// </summary>
public class TitleScreenController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private GameObject _creditsPanel;
    [SerializeField] private GameObject _instructionsPanel;

    [Header("Scene to Load")]
    [SerializeField] private string _gameSceneName = "CityExplorationScene";

    [Header("Audio")]
    [SerializeField] private AudioClip _creditsMusic;

    /// <summary>
    /// Shows the instructions panel, hides the main menu.
    /// </summary>
    public void OnPlayClicked()
    {
        if (_mainMenuPanel != null) _mainMenuPanel.SetActive(false);
        if (_instructionsPanel != null) _instructionsPanel.SetActive(true);
    }

    /// <summary>
    /// Loads the game scene. Called from the instructions panel "¡EMPEZAR!" button.
    /// </summary>
    public void OnStartGameClicked()
    {
        SceneManager.LoadScene(_gameSceneName);
    }

    /// <summary>
    /// Shows the credits panel, hides the main menu, and plays credits music.
    /// </summary>
    public void OnCreditsClicked()
    {
        if (_mainMenuPanel != null) _mainMenuPanel.SetActive(false);
        if (_creditsPanel != null) _creditsPanel.SetActive(true);

        if (Audio_Manager.Instance != null && _creditsMusic != null)
            Audio_Manager.Instance.PlayMusic(_creditsMusic);
    }

    /// <summary>
    /// Returns from credits to main menu and restores title music.
    /// </summary>
    public void OnBackClicked()
    {
        if (_creditsPanel != null) _creditsPanel.SetActive(false);
        if (_instructionsPanel != null) _instructionsPanel.SetActive(false);
        if (_mainMenuPanel != null) _mainMenuPanel.SetActive(true);

        if (Audio_Manager.Instance != null)
            Audio_Manager.Instance.PlayMusicForScene("TitleScene");
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
