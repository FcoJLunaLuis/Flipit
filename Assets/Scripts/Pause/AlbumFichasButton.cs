using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Maneja la apertura del Álbum de Fichas desde el menú de pausa.
/// Usa AlbumManager.Instance directamente (debe existir en la escena).
/// Al cerrar el álbum, el menú de pausa se reactiva automáticamente.
/// </summary>
public class AlbumFichasButton : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Button albumButton;

    [Tooltip("Panel del menú de pausa que se oculta al abrir el álbum.")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Tooltip("Texto de feedback cuando el álbum no está disponible.")]
    [SerializeField] private TextMeshProUGUI feedbackText;

    [SerializeField] private float feedbackDuration = 2f;

    private Coroutine feedbackCoroutine;

    private void OnEnable()
    {
        if (albumButton != null)
        {
            albumButton.onClick.AddListener(OnAlbumPressed);
        }

        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }

        // Suscribirse al evento de cierre del álbum
        if (AlbumManager.Instance != null)
        {
            AlbumManager.Instance.OnAlbumCerrado += OnAlbumCerrado;
        }
    }

    private void OnDisable()
    {
        if (albumButton != null)
        {
            albumButton.onClick.RemoveListener(OnAlbumPressed);
        }

        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = null;
        }

        // Desuscribirse del evento
        if (AlbumManager.Instance != null)
        {
            AlbumManager.Instance.OnAlbumCerrado -= OnAlbumCerrado;
        }
    }

    private void OnAlbumPressed()
    {
        if (AlbumManager.Instance != null)
        {
            // Suscribirse por si no se hizo en OnEnable (el Instance puede no existir al momento del OnEnable)
            AlbumManager.Instance.OnAlbumCerrado -= OnAlbumCerrado;
            AlbumManager.Instance.OnAlbumCerrado += OnAlbumCerrado;

            // Ocultar menú de pausa
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(false);
            }

            // Abrir álbum
            AlbumManager.Instance.AbrirAlbum();
            Debug.Log("[AlbumFichasButton] Álbum abierto desde menú de pausa.");
        }
        else
        {
            Debug.Log("[AlbumFichasButton] AlbumManager no encontrado en la escena.");
            ShowFeedback("Álbum de Fichas no disponible");
        }
    }

    /// <summary>
    /// Callback cuando el álbum se cierra. Reactiva el menú de pausa.
    /// </summary>
    private void OnAlbumCerrado()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
        Debug.Log("[AlbumFichasButton] Álbum cerrado. Menú de pausa reactivado.");
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText == null) return;

        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);

        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }
        feedbackCoroutine = StartCoroutine(HideFeedbackAfterDelay());
    }

    private System.Collections.IEnumerator HideFeedbackAfterDelay()
    {
        yield return new WaitForSecondsRealtime(feedbackDuration);

        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }
        feedbackCoroutine = null;
    }
}
