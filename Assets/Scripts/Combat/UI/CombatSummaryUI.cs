using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI de resumen al final del combate.
/// Muestra fichas ganadas/perdidas con color por rareza, XP, desgaste y botón para continuar.
/// </summary>
public class CombatSummaryUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject _panelResumen;

    [Header("Textos Principales")]
    [SerializeField] private TextMeshProUGUI _tituloTexto;
    [SerializeField] private TextMeshProUGUI _xpTexto;
    [SerializeField] private TextMeshProUGUI _desgasteTexto;
    [SerializeField] private TextMeshProUGUI _lanzadoraTexto;

    [Header("Fichas Ganadas")]
    [SerializeField] private TextMeshProUGUI _tituloGanadasTexto;
    [SerializeField] private Transform _contenedorFichasGanadas;

    [Header("Fichas Perdidas")]
    [SerializeField] private TextMeshProUGUI _tituloPerdidasTexto;
    [SerializeField] private Transform _contenedorFichasPerdidas;

    [Header("Prefab")]
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

        // XP
        if (_xpTexto != null)
            _xpTexto.text = $"XP ganada: +{summary.XPGanada:F0}";

        // Desgaste
        if (_desgasteTexto != null)
        {
            string desgasteMsg = $"Desgaste: +{summary.DesgasteAplicado:F0}";
            if (summary.FichaSeRompio)
                desgasteMsg += " <color=#FF4444>(¡ROTA!)</color>";
            _desgasteTexto.text = desgasteMsg;
        }

        // Lanzadora
        if (_lanzadoraTexto != null && summary.FichaLanzadora != null)
            _lanzadoraTexto.text = $"Lanzadora: {summary.FichaLanzadora.nombre} (Desgaste: {summary.FichaLanzadora.desgaste:F0}/100)";

        // Títulos de listas
        if (_tituloGanadasTexto != null)
            _tituloGanadasTexto.text = $"Fichas Ganadas: {summary.FichasGanadas.Count}";

        if (_tituloPerdidasTexto != null)
            _tituloPerdidasTexto.text = $"Fichas Perdidas: {summary.FichasPerdidasAlNPC.Count}";

        // Generar listas visuales
        GenerarListaFichas(summary.FichasGanadas, _contenedorFichasGanadas);
        GenerarListaFichas(summary.FichasPerdidasAlNPC, _contenedorFichasPerdidas);
    }

    public void Ocultar()
    {
        if (_panelResumen != null)
            _panelResumen.SetActive(false);

        LimpiarContenedor(_contenedorFichasGanadas);
        LimpiarContenedor(_contenedorFichasPerdidas);
    }

    private void GenerarListaFichas(List<FichaData> fichas, Transform contenedor)
    {
        if (contenedor == null || _fichaItemPrefab == null) return;

        LimpiarContenedor(contenedor);

        foreach (var ficha in fichas)
        {
            var item = Instantiate(_fichaItemPrefab, contenedor);
            var fichaItemUI = item.GetComponent<FichaItemUI>();
            if (fichaItemUI != null)
            {
                fichaItemUI.Configurar(ficha);
            }
            else
            {
                // Fallback si no tiene FichaItemUI
                var texto = item.GetComponentInChildren<TextMeshProUGUI>();
                if (texto != null)
                    texto.text = $"{ficha.nombre} ({ficha.rareza})";
            }
        }
    }

    private void LimpiarContenedor(Transform contenedor)
    {
        if (contenedor == null) return;

        for (int i = contenedor.childCount - 1; i >= 0; i--)
        {
            Destroy(contenedor.GetChild(i).gameObject);
        }
    }

    private void OnContinuar()
    {
        Ocultar();

        if (_combatManager != null)
            _combatManager.SalirDelCombate();
    }
}
