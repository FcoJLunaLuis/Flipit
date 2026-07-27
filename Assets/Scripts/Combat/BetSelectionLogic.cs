using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Lógica de selección de fichas para apostar y ficha lanzadora.
/// Valida las selecciones del jugador según las reglas del combate.
/// </summary>
public class BetSelectionLogic
{
    private readonly int _maxFichas;
    private readonly int _minFichas;

    private List<FichaData> _fichasSeleccionadas;
    private FichaData _fichaLanzadora;

    public List<FichaData> FichasSeleccionadas => _fichasSeleccionadas;
    public FichaData FichaLanzadora => _fichaLanzadora;
    public int CantidadSeleccionada => _fichasSeleccionadas.Count;
    public bool ApuestaCompleta => _fichasSeleccionadas.Count >= _minFichas && _fichaLanzadora != null;

    public BetSelectionLogic(int maxFichas = 5, int minFichas = 1)
    {
        _maxFichas = maxFichas;
        _minFichas = minFichas;
        _fichasSeleccionadas = new List<FichaData>();
        _fichaLanzadora = null;
    }

    /// <summary>
    /// Intenta agregar una ficha a la apuesta.
    /// Retorna true si se agregó, false si ya está el máximo o la ficha ya fue seleccionada.
    /// </summary>
    public bool SeleccionarFicha(FichaData ficha)
    {
        if (ficha == null) return false;
        if (_fichasSeleccionadas.Count >= _maxFichas) return false;
        if (_fichasSeleccionadas.Any(f => f.templateId == ficha.templateId)) return false;
        if (ficha.estaRoto) return false;

        _fichasSeleccionadas.Add(ficha);
        return true;
    }

    /// <summary>
    /// Remueve una ficha de la selección de apuesta.
    /// </summary>
    public bool DeseleccionarFicha(FichaData ficha)
    {
        if (ficha == null) return false;
        return _fichasSeleccionadas.RemoveAll(f => f.templateId == ficha.templateId) > 0;
    }

    /// <summary>
    /// Selecciona la ficha que se usará para lanzar.
    /// La ficha lanzadora NO debe ser parte de las fichas apostadas y no debe estar rota.
    /// </summary>
    public bool SeleccionarFichaLanzadora(FichaData ficha)
    {
        if (ficha == null) return false;
        if (ficha.estaRoto) return false;
        if (_fichasSeleccionadas.Any(f => f.templateId == ficha.templateId)) return false;

        _fichaLanzadora = ficha;
        return true;
    }

    /// <summary>
    /// Verifica si una ficha ya está seleccionada para apostar.
    /// </summary>
    public bool EstaSeleccionada(int templateId)
    {
        return _fichasSeleccionadas.Any(f => f.templateId == templateId);
    }

    /// <summary>
    /// Verifica si una ficha es la lanzadora actual.
    /// </summary>
    public bool EsLanzadora(int templateId)
    {
        return _fichaLanzadora != null && _fichaLanzadora.templateId == templateId;
    }

    /// <summary>
    /// Valida que la selección actual sea válida para iniciar combate.
    /// </summary>
    public bool ValidarSeleccion(out string mensajeError)
    {
        if (_fichasSeleccionadas.Count < _minFichas)
        {
            mensajeError = $"Debes apostar al menos {_minFichas} ficha(s). Seleccionadas: {_fichasSeleccionadas.Count}";
            return false;
        }

        if (_fichasSeleccionadas.Count > _maxFichas)
        {
            mensajeError = $"No puedes apostar más de {_maxFichas} fichas.";
            return false;
        }

        if (_fichaLanzadora == null)
        {
            mensajeError = "Debes seleccionar una ficha lanzadora.";
            return false;
        }

        if (_fichasSeleccionadas.Any(f => f.templateId == _fichaLanzadora.templateId))
        {
            mensajeError = "La ficha lanzadora no puede ser una de las fichas apostadas.";
            return false;
        }

        mensajeError = string.Empty;
        return true;
    }

    /// <summary>
    /// Limpia toda la selección.
    /// </summary>
    public void Limpiar()
    {
        _fichasSeleccionadas.Clear();
        _fichaLanzadora = null;
    }
}
