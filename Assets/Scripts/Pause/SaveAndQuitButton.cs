using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Maneja la funcionalidad del botón "Guardar y Salir" en el menú de pausa.
/// Por ahora muestra un mensaje placeholder.
/// 
/// IMPORTANTE: Después del merge con la branch de Album de Fichas,
/// conectar este botón con el SaveSystem existente en esa branch.
/// El flujo esperado es: Guardar estado del día → Volver al menú principal.
/// 
/// Para conectar:
/// 1. Obtener referencia al SaveSystem de la branch Album de Fichas.
/// 2. En OnSaveAndQuit(), llamar al método de guardado del SaveSystem.
/// 3. Una vez confirmado el guardado, cargar la escena del menú principal.
/// </summary>
public class SaveAndQuitButton : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Button saveAndQuitButton;

    [Tooltip("Texto temporal para mostrar el mensaje de 'no disponible'.")]
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Tooltip("Tiempo en segundos que se muestra el mensaje de feedback.")]
    [SerializeField] private float feedbackDuration = 2f;

    private Coroutine feedbackCoroutine;

    private void OnEnable()
    {
        if (saveAndQuitButton != null)
        {
            saveAndQuitButton.onClick.AddListener(OnSaveAndQuit);
        }

        // Ocultar texto de feedback al abrir
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (saveAndQuitButton != null)
        {
            saveAndQuitButton.onClick.RemoveListener(OnSaveAndQuit);
        }

        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = null;
        }
    }

    private void OnSaveAndQuit()
    {
        Debug.Log("[SaveAndQuit] Botón presionado. Función no disponible aún.");

        // TODO: Después del merge con la branch de Album de Fichas,
        // descomentar y adaptar el siguiente código:
        //
        // var saveSystem = FindObjectOfType<SaveSystem>(); // o la referencia que corresponda
        // if (saveSystem != null)
        // {
        //     saveSystem.SaveGame();
        //     // Opcionalmente esperar confirmación antes de cambiar de escena
        //     SceneManager.LoadScene("MainMenu");
        // }

        ShowFeedback("Función no disponible aún");
    }

    /// <summary>
    /// Muestra un mensaje temporal en la UI.
    /// </summary>
    /// <param name="message">Mensaje a mostrar.</param>
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
        // Usar WaitForSecondsRealtime porque Time.timeScale está en 0 durante la pausa
        yield return new WaitForSecondsRealtime(feedbackDuration);

        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }
        feedbackCoroutine = null;
    }
}
