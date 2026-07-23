using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Diálogo de confirmación reutilizable para acciones que requieren verificación del usuario.
/// Puede usarse para "Volver al Menú Principal", "Salir del juego", etc.
/// 
/// Setup en el Canvas:
/// - Panel con fondo semi-transparente
/// - TextMeshProUGUI para el mensaje
/// - Botón "Sí" (confirmar)
/// - Botón "No" (cancelar)
/// 
/// Uso desde código:
///   confirmationDialog.Show(
///       "¿Seguro que deseas volver al menú principal?",
///       onConfirm: () => { /* acción al confirmar */ },
///       onCancel: () => { /* acción al cancelar */ }
///   );
/// </summary>
public class ConfirmationDialog : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [Header("Textos de Botones (opcional)")]
    [SerializeField] private TextMeshProUGUI confirmButtonText;
    [SerializeField] private TextMeshProUGUI cancelButtonText;

    private Action onConfirmAction;
    private Action onCancelAction;

    private void OnEnable()
    {
        if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirm);
        if (cancelButton != null) cancelButton.onClick.AddListener(OnCancel);
    }

    private void OnDisable()
    {
        if (confirmButton != null) confirmButton.onClick.RemoveListener(OnConfirm);
        if (cancelButton != null) cancelButton.onClick.RemoveListener(OnCancel);
    }

    /// <summary>
    /// Muestra el diálogo de confirmación con el mensaje y acciones especificadas.
    /// </summary>
    /// <param name="message">Mensaje a mostrar al usuario.</param>
    /// <param name="onConfirm">Acción a ejecutar si el usuario confirma.</param>
    /// <param name="onCancel">Acción a ejecutar si el usuario cancela (opcional).</param>
    /// <param name="confirmText">Texto del botón de confirmar (default: "Sí").</param>
    /// <param name="cancelText">Texto del botón de cancelar (default: "No").</param>
    public void Show(string message, Action onConfirm, Action onCancel = null,
                     string confirmText = "Sí", string cancelText = "No")
    {
        onConfirmAction = onConfirm;
        onCancelAction = onCancel;

        if (messageText != null)
        {
            messageText.text = message;
        }

        if (confirmButtonText != null)
        {
            confirmButtonText.text = confirmText;
        }

        if (cancelButtonText != null)
        {
            cancelButtonText.text = cancelText;
        }

        gameObject.SetActive(true);

        // Seleccionar "No" por defecto (más seguro para acciones destructivas)
        if (cancelButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(cancelButton.gameObject);
        }
    }

    /// <summary>
    /// Oculta el diálogo.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
        onConfirmAction = null;
        onCancelAction = null;
    }

    private void OnConfirm()
    {
        onConfirmAction?.Invoke();
        Hide();
    }

    private void OnCancel()
    {
        onCancelAction?.Invoke();
        Hide();
    }
}
