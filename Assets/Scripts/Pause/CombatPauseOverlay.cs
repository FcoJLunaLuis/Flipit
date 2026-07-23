using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Controla el overlay de pausa durante el combate.
/// Muestra una pantalla negra semi-transparente con el texto "Pausa" centrado.
/// 
/// Setup en el Canvas:
/// - Image (fondo): Color negro, Alpha ~0.7, Stretch en todos los lados.
/// - TextMeshProUGUI (hijo): Texto "Pausa", centrado, fuente grande, color blanco.
/// </summary>
public class CombatPauseOverlay : MonoBehaviour
{
    [Header("Referencias UI")]
    [Tooltip("Imagen de fondo del overlay (negro semi-transparente).")]
    [SerializeField] private Image backgroundImage;

    [Tooltip("Texto que muestra 'Pausa' en el centro de la pantalla.")]
    [SerializeField] private TextMeshProUGUI pauseText;

    [Header("Configuración Visual")]
    [Tooltip("Opacidad del fondo negro (0 = transparente, 1 = sólido).")]
    [Range(0f, 1f)]
    [SerializeField] private float backgroundAlpha = 0.7f;

    [Tooltip("Texto que se muestra al pausar en combate.")]
    [SerializeField] private string displayText = "Pausa";

    private void Awake()
    {
        ApplyVisualSettings();
    }

    private void OnEnable()
    {
        ApplyVisualSettings();
    }

    /// <summary>
    /// Aplica la configuración visual al overlay.
    /// Se puede llamar desde el editor para previsualizar cambios.
    /// </summary>
    private void ApplyVisualSettings()
    {
        if (backgroundImage != null)
        {
            Color bgColor = backgroundImage.color;
            bgColor.a = backgroundAlpha;
            backgroundImage.color = bgColor;
        }

        if (pauseText != null)
        {
            pauseText.text = displayText;
        }
    }

    /// <summary>
    /// Permite cambiar la opacidad del fondo en runtime.
    /// </summary>
    /// <param name="alpha">Valor de opacidad (0-1).</param>
    public void SetBackgroundAlpha(float alpha)
    {
        backgroundAlpha = Mathf.Clamp01(alpha);
        ApplyVisualSettings();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ApplyVisualSettings();
    }
#endif
}
