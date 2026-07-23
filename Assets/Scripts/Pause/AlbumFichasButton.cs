using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Maneja la apertura del Álbum de Fichas desde el menú de pausa.
/// El Álbum de Fichas se integrará como prefab desde la branch correspondiente.
/// 
/// INTEGRACIÓN POST-MERGE:
/// 1. Hacer merge con la branch de Album de Fichas.
/// 2. Asignar el prefab del Álbum de Fichas en el campo 'albumFichasPrefab' desde el Inspector.
/// 3. El prefab se instanciará al presionar el botón y se destruirá al cerrar el menú.
/// 
/// NOTA: El prefab del Álbum ya maneja su propia lógica interna.
/// Este script solo se encarga de instanciarlo/destruirlo.
/// </summary>
public class AlbumFichasButton : MonoBehaviour
{
    [Header("Prefab del Álbum")]
    [Tooltip("Asignar el prefab del Álbum de Fichas aquí después del merge con su branch.")]
    [SerializeField] private GameObject albumFichasPrefab;

    [Header("Configuración")]
    [Tooltip("Transform padre donde se instanciará el prefab (normalmente el Canvas).")]
    [SerializeField] private Transform parentTransform;

    [Header("Referencias UI")]
    [SerializeField] private Button albumButton;

    [Tooltip("Texto de feedback cuando el álbum no está disponible.")]
    [SerializeField] private TextMeshProUGUI feedbackText;

    [SerializeField] private float feedbackDuration = 2f;

    /// <summary>
    /// Referencia a la instancia actual del álbum (para poder destruirla al cerrar).
    /// </summary>
    private GameObject albumInstance;
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

        // Destruir la instancia del álbum si se cierra el menú de pausa
        DestroyAlbumInstance();
    }

    private void OnAlbumPressed()
    {
        if (albumFichasPrefab != null)
        {
            OpenAlbum();
        }
        else
        {
            Debug.Log("[AlbumFichas] Prefab no asignado. Álbum de Fichas no disponible.");
            ShowFeedback("Álbum de Fichas no disponible");
        }
    }

    /// <summary>
    /// Instancia el prefab del Álbum de Fichas.
    /// </summary>
    private void OpenAlbum()
    {
        if (albumInstance != null)
        {
            // Ya está abierto, no instanciar de nuevo
            Debug.Log("[AlbumFichas] El álbum ya está abierto.");
            return;
        }

        Transform parent = parentTransform != null ? parentTransform : transform.root;
        albumInstance = Instantiate(albumFichasPrefab, parent);
        albumInstance.name = "AlbumFichas_Instance";

        Debug.Log("[AlbumFichas] Álbum de Fichas abierto.");
    }

    /// <summary>
    /// Destruye la instancia del Álbum de Fichas.
    /// Se llama al cerrar el menú de pausa o al presionar un botón de cerrar dentro del álbum.
    /// </summary>
    public void CloseAlbum()
    {
        DestroyAlbumInstance();
        Debug.Log("[AlbumFichas] Álbum de Fichas cerrado.");
    }

    private void DestroyAlbumInstance()
    {
        if (albumInstance != null)
        {
            Destroy(albumInstance);
            albumInstance = null;
        }
    }

    /// <summary>
    /// Indica si el álbum está actualmente abierto.
    /// </summary>
    public bool IsAlbumOpen => albumInstance != null;

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
