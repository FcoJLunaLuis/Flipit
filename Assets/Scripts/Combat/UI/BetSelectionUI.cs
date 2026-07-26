using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI de selección de apuesta con 3 zonas:
/// - Panel Derecho: Fichas disponibles del álbum (2 filas × 5, paginado). Click = apostar.
/// - Panel Izquierdo: Fichas apostadas (máx 5). Click = devolver.
/// - Zona Inferior: Fichas candidatas a lanzadora (no apostadas). Click = seleccionar lanzadora.
/// - Botón "Listo": Solo activo cuando la apuesta es válida (min fichas + lanzadora).
///
/// Se muestra automáticamente cuando CombatManager entra en fase BetSelection.
/// </summary>
public class BetSelectionUI : MonoBehaviour
{
    [Header("Panel Principal")]
    [SerializeField] private GameObject _panelSeleccion;

    [Header("Panel Derecho - Fichas Disponibles")]
    [SerializeField] private Transform _contenedorDisponibles;
    [SerializeField] private GameObject _slotPrefab;
    [SerializeField] private TextMeshProUGUI _paginaTexto;
    [SerializeField] private Button _botonPaginaSiguiente;
    [SerializeField] private Button _botonPaginaAnterior;

    [Header("Panel Izquierdo - Fichas Apostadas")]
    [SerializeField] private Transform _contenedorApostadas;

    [Header("Zona Inferior - Selección de Lanzadora")]
    [SerializeField] private Transform _contenedorLanzadora;
    [SerializeField] private TextMeshProUGUI _lanzadoraTexto;

    [Header("Botones")]
    [SerializeField] private Button _botonListo;
    [SerializeField] private TextMeshProUGUI _contadorTexto;

    [Header("Colores")]
    [SerializeField] private Color _colorNormal = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color _colorApuesta = new Color(0.9f, 0.7f, 0.1f, 0.9f);
    [SerializeField] private Color _colorLanzadora = new Color(0.1f, 0.8f, 0.9f, 0.9f);

    private const int FICHAS_POR_PAGINA = 10;

    private CombatManager _combatManager;
    private AlbumData _albumData;
    private List<AlbumEntry> _todasLasFichas = new List<AlbumEntry>();
    private int _paginaActual;
    private int _totalPaginas;

    // Slots instanciados
    private List<BetSlotUI> _slotsDisponibles = new List<BetSlotUI>();
    private List<BetSlotUI> _slotsApostadas = new List<BetSlotUI>();
    private List<BetSlotUI> _slotsLanzadora = new List<BetSlotUI>();

    // === LIFECYCLE ===

    private void OnEnable()
    {
        if (_botonListo != null)
            _botonListo.onClick.AddListener(OnListoPresionado);
        if (_botonPaginaSiguiente != null)
            _botonPaginaSiguiente.onClick.AddListener(PaginaSiguiente);
        if (_botonPaginaAnterior != null)
            _botonPaginaAnterior.onClick.AddListener(PaginaAnterior);
    }

    private void OnDisable()
    {
        if (_botonListo != null)
            _botonListo.onClick.RemoveListener(OnListoPresionado);
        if (_botonPaginaSiguiente != null)
            _botonPaginaSiguiente.onClick.RemoveListener(PaginaSiguiente);
        if (_botonPaginaAnterior != null)
            _botonPaginaAnterior.onClick.RemoveListener(PaginaAnterior);
    }

    // === API PÚBLICA ===

    /// <summary>
    /// Muestra el panel de selección con las fichas del jugador.
    /// </summary>
    public void Mostrar(CombatManager combatManager, AlbumData album)
    {
        _combatManager = combatManager;
        _albumData = album;
        _paginaActual = 0;

        // Obtener fichas no rotas
        _todasLasFichas = album.ObtenerTodasLasFichas()
            .Where(e => !e.ficha.estaRoto)
            .ToList();

        _totalPaginas = Mathf.Max(1, Mathf.CeilToInt((float)_todasLasFichas.Count / FICHAS_POR_PAGINA));

        if (_panelSeleccion != null)
            _panelSeleccion.SetActive(true);

        RefrescarTodo();
    }

    /// <summary>
    /// Oculta el panel de selección y limpia los slots.
    /// </summary>
    public void Ocultar()
    {
        if (_panelSeleccion != null)
            _panelSeleccion.SetActive(false);

        LimpiarSlots(_slotsDisponibles);
        LimpiarSlots(_slotsApostadas);
        LimpiarSlots(_slotsLanzadora);
    }

    // === REFRESCO DE UI ===

    private void RefrescarTodo()
    {
        RefrescarPanelDisponibles();
        RefrescarPanelApostadas();
        RefrescarZonaLanzadora();
        RefrescarEstadoBotones();
    }

    private void RefrescarPanelDisponibles()
    {
        LimpiarSlots(_slotsDisponibles);

        if (_contenedorDisponibles == null || _slotPrefab == null) return;

        var betLogic = _combatManager.BetSelection;

        // Fichas que NO están apostadas ni son lanzadora
        var fichasDisponibles = _todasLasFichas
            .Where(e => !betLogic.EstaSeleccionada(e.ficha.templateId) && !betLogic.EsLanzadora(e.ficha.templateId))
            .ToList();

        // Recalcular paginación sobre fichas disponibles reales
        _totalPaginas = Mathf.Max(1, Mathf.CeilToInt((float)fichasDisponibles.Count / FICHAS_POR_PAGINA));
        if (_paginaActual >= _totalPaginas) _paginaActual = _totalPaginas - 1;

        var fichasVisibles = fichasDisponibles
            .Skip(_paginaActual * FICHAS_POR_PAGINA)
            .Take(FICHAS_POR_PAGINA)
            .ToList();

        foreach (var entrada in fichasVisibles)
        {
            var slotGO = Instantiate(_slotPrefab, _contenedorDisponibles);
            var slot = slotGO.GetComponent<BetSlotUI>();
            if (slot == null) slot = slotGO.AddComponent<BetSlotUI>();

            slot.Configurar(entrada.ficha, OnFichaDisponibleClickeada);
            slot.SetColor(_colorNormal);
            _slotsDisponibles.Add(slot);
        }

        // Paginación
        if (_paginaTexto != null)
            _paginaTexto.text = $"{_paginaActual + 1}/{_totalPaginas}";

        if (_botonPaginaSiguiente != null)
            _botonPaginaSiguiente.interactable = _paginaActual < _totalPaginas - 1;

        if (_botonPaginaAnterior != null)
            _botonPaginaAnterior.interactable = _paginaActual > 0;
    }

    private void RefrescarPanelApostadas()
    {
        LimpiarSlots(_slotsApostadas);

        if (_contenedorApostadas == null || _slotPrefab == null) return;

        var betLogic = _combatManager.BetSelection;
        var fichasApostadas = betLogic.FichasSeleccionadas;

        foreach (var ficha in fichasApostadas)
        {
            var slotGO = Instantiate(_slotPrefab, _contenedorApostadas);
            var slot = slotGO.GetComponent<BetSlotUI>();
            if (slot == null) slot = slotGO.AddComponent<BetSlotUI>();

            slot.Configurar(ficha, OnFichaApostadaClickeada);
            slot.SetColor(_colorApuesta);
            _slotsApostadas.Add(slot);
        }
    }

    private void RefrescarZonaLanzadora()
    {
        LimpiarSlots(_slotsLanzadora);

        if (_contenedorLanzadora == null || _slotPrefab == null) return;

        var betLogic = _combatManager.BetSelection;

        // Candidatas: fichas no apostadas
        var candidatas = _todasLasFichas
            .Where(e => !betLogic.EstaSeleccionada(e.ficha.templateId))
            .ToList();

        foreach (var entrada in candidatas)
        {
            var slotGO = Instantiate(_slotPrefab, _contenedorLanzadora);
            var slot = slotGO.GetComponent<BetSlotUI>();
            if (slot == null) slot = slotGO.AddComponent<BetSlotUI>();

            slot.Configurar(entrada.ficha, OnFichaLanzadoraClickeada);

            bool esLanzadora = betLogic.EsLanzadora(entrada.ficha.templateId);
            slot.SetColor(esLanzadora ? _colorLanzadora : _colorNormal);
            _slotsLanzadora.Add(slot);
        }

        // Texto de lanzadora
        if (_lanzadoraTexto != null)
        {
            if (betLogic.FichaLanzadora != null)
                _lanzadoraTexto.text = $"Lanzadora: {betLogic.FichaLanzadora.nombre}";
            else
                _lanzadoraTexto.text = "Selecciona tu ficha lanzadora:";
        }
    }

    private void RefrescarEstadoBotones()
    {
        var betLogic = _combatManager.BetSelection;
        int max = _combatManager.Config.maxFichasApuesta;

        // Contador
        if (_contadorTexto != null)
            _contadorTexto.text = $"Apostadas: {betLogic.CantidadSeleccionada}/{max}";

        // Botón listo
        if (_botonListo != null)
            _botonListo.interactable = betLogic.ApuestaCompleta;
    }

    // === HANDLERS DE CLICK ===

    private void OnFichaDisponibleClickeada(FichaData ficha, BetSlotUI slot)
    {
        if (_combatManager == null) return;

        var betLogic = _combatManager.BetSelection;
        if (betLogic.SeleccionarFicha(ficha))
        {
            RefrescarTodo();
        }
    }

    private void OnFichaApostadaClickeada(FichaData ficha, BetSlotUI slot)
    {
        if (_combatManager == null) return;

        var betLogic = _combatManager.BetSelection;
        if (betLogic.DeseleccionarFicha(ficha))
        {
            RefrescarTodo();
        }
    }

    private void OnFichaLanzadoraClickeada(FichaData ficha, BetSlotUI slot)
    {
        if (_combatManager == null) return;

        var betLogic = _combatManager.BetSelection;

        // No puede ser lanzadora si ya está apostada
        if (betLogic.EstaSeleccionada(ficha.templateId)) return;

        betLogic.SeleccionarFichaLanzadora(ficha);
        RefrescarTodo();
    }

    // === PAGINACIÓN ===

    private void PaginaSiguiente()
    {
        if (_paginaActual < _totalPaginas - 1)
        {
            _paginaActual++;
            RefrescarTodo();
        }
    }

    private void PaginaAnterior()
    {
        if (_paginaActual > 0)
        {
            _paginaActual--;
            RefrescarTodo();
        }
    }

    // === BOTÓN LISTO ===

    private void OnListoPresionado()
    {
        if (_combatManager == null) return;

        string error;
        if (!_combatManager.BetSelection.ValidarSeleccion(out error))
        {
            Debug.LogWarning($"[BetSelectionUI] Selección inválida: {error}");
            return;
        }

        Debug.Log("[BetSelectionUI] Apuesta confirmada. Iniciando combate...");
        _combatManager.ConfirmarApuestas();
        Ocultar();
    }

    // === UTILIDADES ===

    private void LimpiarSlots(List<BetSlotUI> slots)
    {
        foreach (var slot in slots)
        {
            if (slot != null && slot.gameObject != null)
                Destroy(slot.gameObject);
        }
        slots.Clear();
    }
}
