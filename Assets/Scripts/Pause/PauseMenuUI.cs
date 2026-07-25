using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Controlador del menú de pausa completo (mostrado en Exploración).
/// Gestiona la visualización de datos del jugador y la navegación entre sub-paneles.
/// 
/// Setup en el Canvas:
/// - Panel principal con fondo semi-transparente
/// - Sección superior: Nombre del personaje + Dinero
/// - Botones verticales: Álbum de Fichas, Opciones, Guardar y Salir, Volver al Menú
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("Información del Jugador")]
    [Tooltip("Texto que muestra el nombre del personaje.")]
    [SerializeField] private TextMeshProUGUI playerNameText;

    [Tooltip("Texto que muestra el dinero del jugador.")]
    [SerializeField] private TextMeshProUGUI playerCurrencyText;

    [Header("Botones del Menú")]
    [SerializeField] private Button albumFichasButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button saveAndQuitButton;
    [SerializeField] private Button returnToMainMenuButton;

    [Header("Sub-Paneles")]
    [Tooltip("Panel de opciones (se activa al presionar Opciones).")]
    [SerializeField] private GameObject optionsPanel;

    [Tooltip("Panel de confirmación reutilizable.")]
    [SerializeField] private GameObject confirmationPanel;

    [Header("Referencia al PauseManager")]
    [SerializeField] private PauseManager pauseManager;

    [Header("Navegación")]
    [Tooltip("Primer elemento seleccionado al abrir el menú (para navegación con teclado/gamepad).")]
    [SerializeField] private GameObject firstSelected;

    private void OnEnable()
    {
        SetupButtonListeners();
        RefreshPlayerInfo();
        SelectFirstElement();
    }

    private void OnDisable()
    {
        RemoveButtonListeners();
    }

    private void SetupButtonListeners()
    {
        if (optionsButton != null) optionsButton.onClick.AddListener(OnOptionsPressed);
        if (saveAndQuitButton != null) saveAndQuitButton.onClick.AddListener(OnSaveAndQuitPressed);
        if (returnToMainMenuButton != null) returnToMainMenuButton.onClick.AddListener(OnReturnToMainMenuPressed);
        if (albumFichasButton != null) albumFichasButton.onClick.AddListener(OnAlbumFichasPressed);
    }

    private void RemoveButtonListeners()
    {
        if (optionsButton != null) optionsButton.onClick.RemoveListener(OnOptionsPressed);
        if (saveAndQuitButton != null) saveAndQuitButton.onClick.RemoveListener(OnSaveAndQuitPressed);
        if (returnToMainMenuButton != null) returnToMainMenuButton.onClick.RemoveListener(OnReturnToMainMenuPressed);
        if (albumFichasButton != null) albumFichasButton.onClick.RemoveListener(OnAlbumFichasPressed);
    }

    /// <summary>
    /// Actualiza la información del jugador mostrada en el menú.
    /// Se llama cada vez que el menú se abre.
    /// </summary>
    public void RefreshPlayerInfo()
    {
        var providers = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        IPlayerDataProvider dataProvider = null;

        foreach (var provider in providers)
        {
            if (provider is IPlayerDataProvider playerData)
            {
                dataProvider = playerData;
                break;
            }
        }

        if (dataProvider != null)
        {
            SetPlayerName(dataProvider.PlayerName);
            SetCurrency(dataProvider.Ajolopesos, dataProvider.Pejecoins, dataProvider.Sheintavos);
        }
        else
        {
            SetPlayerName("Jugador");
            SetCurrency(0, 0, 0);
        }
    }

    /// <summary>
    /// Actualiza el nombre del personaje en la UI.
    /// </summary>
    /// <param name="name">Nombre del personaje.</param>
    public void SetPlayerName(string name)
    {
        if (playerNameText != null)
        {
            playerNameText.text = name;
        }
    }

    /// <summary>
    /// Actualiza el dinero mostrado en la UI con las 3 denominaciones.
    /// </summary>
    public void SetCurrency(int ajolopesos, int pejecoins, int sheintavos)
    {
        if (playerCurrencyText != null)
        {
            playerCurrencyText.text = $"{ajolopesos} Ajolopesos | {pejecoins} Pejecoins | {sheintavos} Sheintavos";
        }
    }

    // --- Handlers de Botones ---

    private void OnOptionsPressed()
    {
        if (optionsPanel != null)
        {
            gameObject.SetActive(false);
            optionsPanel.SetActive(true);
        }
        else
        {
            Debug.Log("[PauseMenuUI] Panel de opciones no asignado.");
        }
    }

    private void OnSaveAndQuitPressed()
    {
        // Delegado al componente SaveAndQuitButton que se encarga de mostrar feedback en UI.
        // Si no existe el componente, solo logea.
        var saveButton = GetComponentInChildren<SaveAndQuitButton>(true);
        if (saveButton == null)
        {
            Debug.Log("[PauseMenuUI] Guardar y Salir presionado. SaveAndQuitButton no encontrado.");
        }
    }

    private void OnReturnToMainMenuPressed()
    {
        // Mostrar diálogo de confirmación antes de volver al menú principal
        var dialog = FindAnyObjectByType<ConfirmationDialog>(FindObjectsInactive.Include);
        if (dialog != null)
        {
            dialog.Show(
                "¿Seguro que deseas volver al menú principal?\nEl progreso no guardado se perderá.",
                onConfirm: () =>
                {
                    Debug.Log("[PauseMenuUI] Confirmado: Volver al menú principal.");
                    // TODO: SceneManager.LoadScene("MainMenu");
                },
                onCancel: () =>
                {
                    // Regresar al menú de pausa
                    gameObject.SetActive(true);
                }
            );
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("[PauseMenuUI] ConfirmationDialog no encontrado en la escena.");
        }
    }

    private void OnAlbumFichasPressed()
    {
        // Delegado al componente AlbumFichasButton que se encarga de instanciar el prefab.
        // Si no existe el componente, solo logea.
        var albumBtn = GetComponentInChildren<AlbumFichasButton>(true);
        if (albumBtn == null)
        {
            Debug.Log("[PauseMenuUI] Álbum de Fichas presionado. AlbumFichasButton no encontrado.");
        }
    }

    /// <summary>
    /// Muestra de nuevo este panel (usado cuando se regresa desde sub-paneles).
    /// </summary>
    public void ShowMainPanel()
    {
        gameObject.SetActive(true);
    }

    private void SelectFirstElement()
    {
        if (firstSelected != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }
    }
}
