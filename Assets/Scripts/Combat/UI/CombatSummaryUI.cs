using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI de resumen al final del combate.
/// Muestra fichas ganadas, XP, desgaste y botón para continuar.
/// </summary>
public class CombatSummaryUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject _panelResumen;

    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI _tituloTexto;
    [SerializeField] private TextMeshProUGUI _fichasGanadasTexto;
    [SerializeField] private TextMeshProUGUI _fichasPerdidasTexto;
    [SerializeField] private TextMeshProUGUI _xpTexto;
    [SerializeField] private TextMeshProUGUI _desgasteTexto;
    [SerializeField] private TextMeshProUGUI _lanzadoraTexto;

    [Header("Lista de fichas ganadas")]
    [SerializeField] private Transform _contenedorFichasGanadas;
    [SerializeField] private GameObject _fichaItemPrefab;

    [Header("Botones")]
    [SerializeField] private Button _botonContinuar;

    private CombatManager _combatManager;

    private void OnEnable()
    {
        if (_botonContinuar != null)
            _botonContinuar.onClick.AddListener(OnContinuar);
    }

    private void OnDisable()
    {
        if (_botonContinuar != null)
            _botonContinuar.onClick.RemoveListener(OnContinuar);
    }

    public void Mostrar(CombatSummaryData summary, CombatManager combatManager)
    {
        _combatManager = combatManager;

        if (_panelResumen != null)
            _panelResumen.SetActive(true);

        // Título
        if (_tituloTexto != null)
            _tituloTexto.text = summary.JugadorGano ? "¡Victoria!" : "Derrota";

        // Fichas ganadas
        if (_fichasGanadasTexto != null)
            _fichasGanadasTexto.text = $"Fichas ganadas: {summary.FichasGanadas.Count}";

        // Fichas perdidas
        if (_fichasPerdidasTexto != null)
            _fichasPerdidasTexto.text = $"Fichas perdidas: {summary.FichasPerdidasAlNPC.Count}";

        // XP
        if (_xpTexto != null)
            _xpTexto.text = $"XP ganada: +{summary.XPGanada:F0}";

        // Desgaste
        if (_desgasteTexto != null)
        {
            string desgasteMsg = $"Desgaste: +{summary.DesgasteAplicado:F0}";
            if (summary.FichaSeRompio)
                desgasteMsg += " (¡ROTA!)";
            _desgasteTexto.text = desgasteMsg;
        }

        // Lanzadora
        if (_lanzadoraTexto != null && summary.FichaLanzadora != null)
            _lanzadoraTexto.text = $"Lanzadora: {summary.FichaLanzadora.nombre} (Desgaste: {summary.FichaLanzadora.desgaste:F0}/100)";

        // Generar lista visual de fichas ganadas
        GenerarListaFichas(summary.FichasGanadas);
    }

    public void Ocultar()
    {
        if (_panelResumen != null)
            _panelResumen.SetActive(false);

        LimpiarListaFichas();
    }

    private void GenerarListaFichas(List<FichaData> fichas)
    {
        LimpiarListaFichas();

        if (_contenedorFichasGanadas == null || _fichaItemPrefab == null) return;

        foreach (var ficha in fichas)
        {
            var item = Instantiate(_fichaItemPrefab, _contenedorFichasGanadas);
            var texto = item.GetComponentInChildren<TextMeshProUGUI>();
            if (texto != null)
                texto.text = $"{ficha.nombre} ({ficha.rareza})";
        }
    }

    private void LimpiarListaFichas()
    {
        if (_contenedorFichasGanadas == null) return;

        for (int i = _contenedorFichasGanadas.childCount - 1; i >= 0; i--)
        {
            Destroy(_contenedorFichasGanadas.GetChild(i).gameObject);
        }
    }

    private void OnContinuar()
    {
        Ocultar();

        if (_combatManager != null)
            _combatManager.SalirDelCombate();
    }
}
