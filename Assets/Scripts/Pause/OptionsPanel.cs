using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Flipit.Core;

/// <summary>
/// Panel de opciones/configuración del juego (placeholder).
/// Todos los controles logean su valor en consola por ahora.
/// 
/// TODO: Conectar sliders con AudioMixer para volumen.
/// TODO: Conectar brillo con post-processing o shader global.
/// TODO: Conectar resolución con Screen.SetResolution().
/// TODO: Conectar pantalla completa con Screen.fullScreen.
/// 
/// Setup en el Canvas:
/// - Slider: Volumen Música (0-100)
/// - Slider: Volumen Efectos (0-100)
/// - Slider: Brillo (0-100)
/// - Dropdown: Resolución
/// - Toggle: Pantalla Completa
/// - Botón: Atrás (regresa al menú de pausa)
/// </summary>
public class OptionsPanel : MonoBehaviour
{
    [Header("Controles de Audio")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Controles de Video")]
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Navegación")]
    [Tooltip("Referencia al panel principal del menú de pausa para regresar.")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button backButton;

    [Tooltip("Primer elemento seleccionado al abrir el panel (para navegación con teclado/gamepad).")]
    [SerializeField] private GameObject firstSelected;

    private void OnEnable()
    {
        SetupListeners();
        LoadPlaceholderValues();
        SelectFirstElement();
    }

    private void OnDisable()
    {
        RemoveListeners();
    }

    private void SetupListeners()
    {
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        if (brightnessSlider != null) brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        if (backButton != null) backButton.onClick.AddListener(OnBackPressed);
    }

    private void RemoveListeners()
    {
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        if (brightnessSlider != null) brightnessSlider.onValueChanged.RemoveListener(OnBrightnessChanged);
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
        if (backButton != null) backButton.onClick.RemoveListener(OnBackPressed);
    }

    /// <summary>
    /// Carga valores por defecto en los controles.
    /// TODO: Reemplazar con PlayerPrefs o sistema de configuración real.
    /// </summary>
    private void LoadPlaceholderValues()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0;
            musicVolumeSlider.maxValue = 100;
            musicVolumeSlider.value = Audio_Manager.Instance != null
                ? Audio_Manager.Instance.MasterVolume * 100f
                : 80;
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = 0;
            sfxVolumeSlider.maxValue = 100;
            sfxVolumeSlider.value = 80;
        }

        if (brightnessSlider != null)
        {
            brightnessSlider.minValue = 0;
            brightnessSlider.maxValue = 100;
            brightnessSlider.value = 50;
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "1920x1080",
                "1280x720",
                "1600x900",
                "2560x1440"
            });
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
        }
    }

    // --- Handlers ---

    private void OnMusicVolumeChanged(float value)
    {
        Debug.Log($"[Opciones] Volumen Música: {value}");
        if (Audio_Manager.Instance != null)
            Audio_Manager.Instance.MasterVolume = value / 100f;
    }

    private void OnSFXVolumeChanged(float value)
    {
        Debug.Log($"[Opciones] Volumen Efectos: {value}");
        // TODO: Conectar con AudioMixer.SetFloat("SFXVolume", ConvertToDecibels(value));
    }

    private void OnBrightnessChanged(float value)
    {
        Debug.Log($"[Opciones] Brillo: {value}");
        // TODO: Conectar con post-processing o shader de brillo global.
    }

    private void OnResolutionChanged(int index)
    {
        string selected = resolutionDropdown.options[index].text;
        Debug.Log($"[Opciones] Resolución seleccionada: {selected}");
        // TODO: Parsear resolución y llamar Screen.SetResolution(width, height, fullscreen);
    }

    private void OnFullscreenChanged(bool isFullscreen)
    {
        Debug.Log($"[Opciones] Pantalla Completa: {isFullscreen}");
        // TODO: Screen.fullScreen = isFullscreen;
    }

    private void OnBackPressed()
    {
        // Regresar al menú de pausa principal
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
        gameObject.SetActive(false);
    }

    private void SelectFirstElement()
    {
        if (firstSelected != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }
    }
}
