using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI para seleccionar fichas de apuesta y ficha lanzadora.
/// Muestra las fichas del álbum del jugador y permite seleccionar hasta el máximo.
/// </summary>
public class BetSelectionUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Transform _contenedorFichas;
    [SerializeField] private GameObject _fichaPrefab;
    [SerializeField] private TextMeshProUGUI _contadorTexto;
    [SerializeField] private TextMeshProUGUI _lanzadoraTexto;
    [SerializeField] private Button _botonConfirmar;
    [SerializeField] private Button _botonCancelar;
    [SerializeField] private GameObject _panelSeleccion;

    [Header("Colores de selección")]
    [SerializeField] private Color _colorNormal = Color.white;
    [SerializeField] private Color _colorApuesta = Color.yellow;
    [SerializeField] private Color _colorLanzadora = Color.cyan;

    private CombatManager _combatManager;
    private AlbumData _albumJugador;
    private List<BetSlotUI> _slotsUI = new List<BetSlotUI>();

    private void OnEnable()
    {
        if (_botonConfirmar != null)
            _botonConfirmar.onClick.AddListener(OnConfirmar);

        if (_botonCancelar != null)
            _botonCancelar.onClick.AddListener(OnCancelar);
    }

    private void OnDisable()
    {
        if (_botonConfirmar != null)
            _botonConfirmar.onClick.RemoveListener(OnConfirmar);

        if (_botonCancelar != null)
            _botonCancelar.onClick.RemoveListener(OnCancelar);
    }

    public void Mostrar(CombatManager combatManager, AlbumData album)
    {
        _combatManager = combatManager;
        _albumJugador = album;

        if (_panelSeleccion != null)
            _panelSeleccion.SetActive(true);

        GenerarSlots();
        ActualizarUI();
    }

    public void Ocultar()
    {
        if (_panelSeleccion != null)
            _panelSeleccion.SetActive(false);

        LimpiarSlots();
    }

    private void GenerarSlots()
    {
        LimpiarSlots();

        if (_albumJugador == null || _contenedorFichas == null || _fichaPrefab == null) return;

        var entradas = _albumJugador.ObtenerTodasLasFichas();

        foreach (var entrada in entradas)
        {
            if (entrada.ficha.estaRoto) continue;

            var slotGO = Instantiate(_fichaPrefab, _contenedorFichas);
            var slotUI = slotGO.GetComponent<BetSlotUI>();

            if (slotUI == null)
                slotUI = slotGO.AddComponent<BetSlotUI>();

            slotUI.Configurar(entrada.ficha, OnFichaClickeada);
            _slotsUI.Add(slotUI);
        }
    }

    private void LimpiarSlots()
    {
        foreach (var slot in _slotsUI)
        {
            if (slot != null && slot.gameObject != null)
                Destroy(slot.gameObject);
        }
        _slotsUI.Clear();
    }

    private void OnFichaClickeada(FichaData ficha, BetSlotUI slot)
    {
        if (_combatManager == null) return;

        var betLogic = _combatManager.BetSelection;

        // Si es la lanzadora actual, deseleccionar
        if (betLogic.EsLanzadora(ficha.templateId))
        {
            // No hay método para deseleccionar lanzadora, simplemente la ignoramos
            return;
        }

        // Si ya está seleccionada como apuesta, deseleccionar
        if (betLogic.EstaSeleccionada(ficha.templateId))
        {
            betLogic.DeseleccionarFicha(ficha);
        }
        else
        {
            // Intentar agregar como apuesta
            betLogic.SeleccionarFicha(ficha);
        }

        ActualizarUI();
    }

    public void OnFichaLanzadoraClickeada(FichaData ficha)
    {
        if (_combatManager == null) return;

        var betLogic = _combatManager.BetSelection;

        // No puede ser lanzadora si ya está apostada
        if (betLogic.EstaSeleccionada(ficha.templateId)) return;

        betLogic.SeleccionarFichaLanzadora(ficha);
        ActualizarUI();
    }

    private void ActualizarUI()
    {
        if (_combatManager == null) return;

        var betLogic = _combatManager.BetSelection;
        int max = _combatManager.Config.maxFichasApuesta;

        // Actualizar contador
        if (_contadorTexto != null)
            _contadorTexto.text = $"Fichas apostadas: {betLogic.CantidadSeleccionada}/{max}";

        // Actualizar texto de lanzadora
        if (_lanzadoraTexto != null)
        {
            if (betLogic.FichaLanzadora != null)
                _lanzadoraTexto.text = $"Lanzadora: {betLogic.FichaLanzadora.nombre}";
            else
                _lanzadoraTexto.text = "Lanzadora: (selecciona una)";
        }

        // Actualizar colores de slots
        foreach (var slot in _slotsUI)
        {
            if (slot == null || slot.Ficha == null) continue;

            if (betLogic.EsLanzadora(slot.Ficha.templateId))
                slot.SetColor(_colorLanzadora);
            else if (betLogic.EstaSeleccionada(slot.Ficha.templateId))
                slot.SetColor(_colorApuesta);
            else
                slot.SetColor(_colorNormal);
        }

        // Habilitar botón confirmar solo si la selección es válida
        if (_botonConfirmar != null)
            _botonConfirmar.interactable = betLogic.ApuestaCompleta;
    }

    private void OnConfirmar()
    {
        if (_combatManager != null)
        {
            _combatManager.ConfirmarApuestas();
            Ocultar();
        }
    }

    private void OnCancelar()
    {
        if (_combatManager != null)
        {
            _combatManager.BetSelection.Limpiar();
            ActualizarUI();
        }
    }
}
