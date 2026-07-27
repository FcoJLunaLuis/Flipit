using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla toda la UI del álbum: muestra/oculta, actualiza slots, panel de detalles, paginación.
/// </summary>
public class AlbumUI : MonoBehaviour
{
    [Header("Panel Principal")]
    public GameObject albumPanel;

    [Header("Panel Izquierdo - Detalles")]
    public TextMeshProUGUI detalleIdText;
    public TextMeshProUGUI detalleNombreText;
    public TextMeshProUGUI detalleRarezaText;
    public TextMeshProUGUI detalleRangoText;
    public TextMeshProUGUI detalleExpText;
    public TextMeshProUGUI detalleDesgasteText;
    public TextMeshProUGUI detallePerkText;
    public TextMeshProUGUI detallePesoText;
    public TextMeshProUGUI detalleSuerteText;
    public TextMeshProUGUI detalleEstadoText;
    public Image detalleIconoImage;

    [Header("Panel Derecho - Lista de Fichas")]
    public FichaSlotUI[] fichaSlots;

    [Header("Paginación")]
    public TextMeshProUGUI paginaText;
    public Button botonSiguiente;
    public Button botonAnterior;

    [Header("Botón Cerrar")]
    public Button botonCerrar;

    [Header("Mensaje vacío")]
    public TextMeshProUGUI mensajeVacioText;

    private int indiceSeleccionado = 0;
    private int paginaActual = 0;
    private int totalPaginas = 1;
    private List<AlbumEntry> entradasActuales = new List<AlbumEntry>();

    /// <summary>
    /// Muestra el álbum.
    /// </summary>
    public void Mostrar()
    {
        if (albumPanel != null)
            albumPanel.SetActive(true);
    }

    /// <summary>
    /// Oculta el álbum.
    /// </summary>
    public void Ocultar()
    {
        if (albumPanel != null)
            albumPanel.SetActive(false);
    }

    /// <summary>
    /// Verifica si el álbum está visible.
    /// </summary>
    public bool EstaVisible()
    {
        return albumPanel != null && albumPanel.activeSelf;
    }

    /// <summary>
    /// Actualiza la lista de fichas con los datos de una página.
    /// </summary>
    public void ActualizarLista(List<AlbumEntry> entradas, int pagina, int totalPags)
    {
        entradasActuales = entradas ?? new List<AlbumEntry>();
        paginaActual = pagina;
        totalPaginas = totalPags;

        // Actualizar slots
        for (int i = 0; i < fichaSlots.Length; i++)
        {
            if (fichaSlots[i] == null) continue;

            if (i < entradasActuales.Count)
            {
                fichaSlots[i].Configurar(entradasActuales[i]);
            }
            else
            {
                fichaSlots[i].Limpiar();
            }
        }

        // Actualizar indicador de página
        if (paginaText != null)
        {
            paginaText.text = $"Página {paginaActual + 1}/{totalPaginas}";
            paginaText.gameObject.SetActive(totalPaginas > 1);
        }

        // Mostrar/ocultar botones de paginación
        if (botonSiguiente != null)
            botonSiguiente.gameObject.SetActive(totalPaginas > 1);

        if (botonAnterior != null)
            botonAnterior.gameObject.SetActive(totalPaginas > 1);

        // Mostrar mensaje vacío si no hay fichas
        if (mensajeVacioText != null)
            mensajeVacioText.gameObject.SetActive(entradasActuales.Count == 0);

        // Resetear selección si es necesario
        if (entradasActuales.Count == 0)
        {
            indiceSeleccionado = 0;
            LimpiarDetalles();
        }
        else if (indiceSeleccionado >= entradasActuales.Count)
        {
            indiceSeleccionado = 0;
        }

        ActualizarSeleccion();
    }

    /// <summary>
    /// Mueve la selección en la dirección indicada.
    /// direction: 1 = arriba (índice menor), -1 = abajo (índice mayor)
    /// </summary>
    public void MoverSeleccion(int direction)
    {
        if (entradasActuales.Count == 0) return;

        // Arriba (1) = índice menor, Abajo (-1) = índice mayor
        indiceSeleccionado -= direction;

        // Clampar dentro de los límites
        if (indiceSeleccionado < 0)
            indiceSeleccionado = 0;
        if (indiceSeleccionado >= entradasActuales.Count)
            indiceSeleccionado = entradasActuales.Count - 1;

        ActualizarSeleccion();
    }

    /// <summary>
    /// Obtiene el índice de selección actual.
    /// </summary>
    public int ObtenerIndiceSeleccionado()
    {
        return indiceSeleccionado;
    }

    /// <summary>
    /// Establece el índice de selección.
    /// </summary>
    public void SetIndiceSeleccionado(int indice)
    {
        indiceSeleccionado = Mathf.Clamp(indice, 0, Mathf.Max(0, entradasActuales.Count - 1));
        ActualizarSeleccion();
    }

    /// <summary>
    /// Obtiene la entrada actualmente seleccionada.
    /// </summary>
    public AlbumEntry ObtenerEntradaSeleccionada()
    {
        if (entradasActuales.Count == 0 || indiceSeleccionado >= entradasActuales.Count)
            return null;

        return entradasActuales[indiceSeleccionado];
    }

    /// <summary>
    /// Muestra los detalles completos de una ficha en el panel izquierdo.
    /// </summary>
    public void MostrarDetalles(AlbumEntry entrada)
    {
        if (entrada == null)
        {
            LimpiarDetalles();
            return;
        }

        var ficha = entrada.ficha;

        if (detalleIdText != null) detalleIdText.text = $"#{ficha.templateId}";
        if (detalleNombreText != null) detalleNombreText.text = ficha.nombre;
        if (detalleRarezaText != null) detalleRarezaText.text = $"Rareza: {ficha.rareza}";
        if (detalleRangoText != null) detalleRangoText.text = $"Rango: {ficha.rango}";
        if (detalleExpText != null) detalleExpText.text = $"Exp: {ficha.experienciaDeRango}";
        if (detalleDesgasteText != null) detalleDesgasteText.text = $"Desgaste: {ficha.desgaste:F0}%";
        if (detallePerkText != null) detallePerkText.text = $"Perk: {ficha.perk}";
        if (detallePesoText != null) detallePesoText.text = $"Peso: {ficha.peso:F1}";
        if (detalleSuerteText != null) detalleSuerteText.text = $"Suerte: {ficha.suerte:F1}";
        if (detalleEstadoText != null) detalleEstadoText.text = ficha.estaRoto ? "Estado: ROTO" : "Estado: Normal";

        if (detalleIconoImage != null)
        {
            detalleIconoImage.color = ObtenerColorPorRareza(ficha.rareza);
        }
    }

    /// <summary>
    /// Limpia el panel de detalles.
    /// </summary>
    public void LimpiarDetalles()
    {
        if (detalleIdText != null) detalleIdText.text = "";
        if (detalleNombreText != null) detalleNombreText.text = "Selecciona una ficha";
        if (detalleRarezaText != null) detalleRarezaText.text = "";
        if (detalleRangoText != null) detalleRangoText.text = "";
        if (detalleExpText != null) detalleExpText.text = "";
        if (detalleDesgasteText != null) detalleDesgasteText.text = "";
        if (detallePerkText != null) detallePerkText.text = "";
        if (detallePesoText != null) detallePesoText.text = "";
        if (detalleSuerteText != null) detalleSuerteText.text = "";
        if (detalleEstadoText != null) detalleEstadoText.text = "";
        if (detalleIconoImage != null) detalleIconoImage.color = Color.clear;
    }

    /// <summary>
    /// Actualiza el highlight visual de los slots según la selección actual.
    /// </summary>
    private void ActualizarSeleccion()
    {
        for (int i = 0; i < fichaSlots.Length; i++)
        {
            if (fichaSlots[i] == null) continue;
            fichaSlots[i].SetSeleccionado(i == indiceSeleccionado);
        }

        // Actualizar detalles con la ficha seleccionada
        MostrarDetalles(ObtenerEntradaSeleccionada());
    }

    private Color ObtenerColorPorRareza(Rareza rareza)
    {
        switch (rareza)
        {
            case Rareza.Comun: return new Color(0.6f, 0.6f, 0.6f);
            case Rareza.Raro: return new Color(0.3f, 0.5f, 1.0f);
            case Rareza.UltraRaro: return new Color(1.0f, 0.8f, 0.0f);
            default: return Color.white;
        }
    }
}
