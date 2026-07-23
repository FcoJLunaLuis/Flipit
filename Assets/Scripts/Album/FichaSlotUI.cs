using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Componente de un slot individual de ficha en la lista del álbum.
/// Muestra: icono, nombre, cantidad y estado de selección.
/// </summary>
public class FichaSlotUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image iconoImage;
    public TextMeshProUGUI nombreText;
    public TextMeshProUGUI cantidadText;
    public Image fondoImage;

    [Header("Colores de selección")]
    public Color colorNormal = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    public Color colorSeleccionado = new Color(0.4f, 0.6f, 0.9f, 0.9f);

    private AlbumEntry entradaActual;
    private bool estaSeleccionado;

    /// <summary>
    /// Configura el slot con los datos de una entrada del álbum.
    /// </summary>
    public void Configurar(AlbumEntry entrada)
    {
        entradaActual = entrada;

        if (entrada == null)
        {
            Limpiar();
            return;
        }

        if (nombreText != null)
            nombreText.text = $"#{entrada.ficha.templateId} {entrada.ficha.nombre}";

        if (cantidadText != null)
            cantidadText.text = $"x{entrada.cantidad}";

        if (iconoImage != null)
        {
            // Por ahora usa un placeholder blanco, luego se asignará el sprite real
            iconoImage.color = ObtenerColorPorRareza(entrada.ficha.rareza);
        }

        gameObject.SetActive(true);
    }

    /// <summary>
    /// Limpia el slot (lo deja vacío).
    /// </summary>
    public void Limpiar()
    {
        entradaActual = null;

        if (nombreText != null)
            nombreText.text = "";

        if (cantidadText != null)
            cantidadText.text = "";

        if (iconoImage != null)
            iconoImage.color = Color.clear;

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Establece el estado de selección visual del slot.
    /// </summary>
    public void SetSeleccionado(bool seleccionado)
    {
        estaSeleccionado = seleccionado;

        if (fondoImage != null)
            fondoImage.color = seleccionado ? colorSeleccionado : colorNormal;
    }

    /// <summary>
    /// Obtiene la entrada actual del slot.
    /// </summary>
    public AlbumEntry ObtenerEntrada()
    {
        return entradaActual;
    }

    /// <summary>
    /// Retorna un color placeholder según la rareza de la ficha.
    /// </summary>
    private Color ObtenerColorPorRareza(Rareza rareza)
    {
        switch (rareza)
        {
            case Rareza.Comun: return new Color(0.6f, 0.6f, 0.6f); // Gris
            case Rareza.Raro: return new Color(0.3f, 0.5f, 1.0f); // Azul
            case Rareza.UltraRaro: return new Color(1.0f, 0.8f, 0.0f); // Dorado
            default: return Color.white;
        }
    }
}
