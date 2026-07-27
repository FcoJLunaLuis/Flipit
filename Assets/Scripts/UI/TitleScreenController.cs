using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the title screen menu. Handles Play, Credits, and Quit buttons.
/// Attach to the TitleCanvas root in TitleScene.
/// </summary>
public class TitleScreenController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private GameObject _creditsPanel;

    [Header("Scene to Load")]
    [SerializeField] private string _gameSceneName = "CityExplorationScene";

    /// <summary>
    /// Loads the game scene.
    /// </summary>
    public void OnPlayClicked()
    {
        SceneManager.LoadScene(_gameSceneName);
    }

    /// <summary>
    /// Shows the credits panel, hides the main menu.
    /// </summary>
    public void OnCreditsClicked()
    {
        if (_mainMenuPanel != null) _mainMenuPanel.SetActive(false);
        if (_creditsPanel != null) _creditsPanel.SetActive(true);
    }

    /// <summary>
    /// Returns from credits to main menu.
    /// </summary>
    public void OnBackClicked()
    {
        if (_creditsPanel != null) _creditsPanel.SetActive(false);
        if (_mainMenuPanel != null) _mainMenuPanel.SetActive(true);
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
