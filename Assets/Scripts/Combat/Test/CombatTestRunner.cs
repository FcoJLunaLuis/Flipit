using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Script de prueba para el sistema de combate.
/// Genera fichas mock para jugador y NPC, e inicia un combate completo.
/// Colocar en la escena TestVoltearFlchas para probar la mecánica.
/// 
/// Controles:
/// - Space: Acción en el minijuego (fijar mira, fuerza, precisión)
/// - C: Iniciar combate con fichas mock (salta selección de apuesta)
/// - F: Iniciar combate completo (con selección de apuesta manual)
/// </summary>
public class CombatTestRunner : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CombatManager _combatManager;
    [SerializeField] private CombatConfig _config;
    [SerializeField] private TowerVisual _towerVisual;
    [SerializeField] private CombatSummaryUI _summaryUI;

    [Header("Test Config")]
    [SerializeField] private int _fichasJugadorTest = 4;
    [SerializeField] private int _fichasNPCTest = 4;
    [SerializeField] private bool _autoIniciar = false;

    private AlbumData _albumTest;
    private List<FichaData> _fichasNPCPool;
    private bool _combateActivo;

    private void Start()
    {
        CrearDatosMock();

        if (_combatManager != null)
        {
            _combatManager.OnCombateIniciado += OnCombateIniciado;
            _combatManager.OnCombateTerminado += OnCombateTerminado;
            _combatManager.OnFaseCambiada += OnFaseCambiada;
            _combatManager.OnLanzamientoResuelto += OnLanzamientoResuelto;
            _combatManager.OnTurnoCambiado += OnTurnoCambiado;
        }

        if (_autoIniciar)
        {
            IniciarCombateRapido();
        }
        else
        {
            Debug.Log("[TestRunner] Presiona C para combate rápido, F para combate completo.");
        }
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.cKey.wasPressedThisFrame && !_combateActivo)
        {
            IniciarCombateRapido();
        }

        if (keyboard.fKey.wasPressedThisFrame && !_combateActivo)
        {
            IniciarCombateCompleto();
        }
    }

    private void IniciarCombateRapido()
    {
        Debug.Log("[TestRunner] Iniciando combate rápido con fichas mock...");

        _combatManager.IniciarCombate(_fichasNPCPool);

        var betLogic = _combatManager.BetSelection;
        var todasLasFichas = _albumTest.ObtenerTodasLasFichas();

        int seleccionadas = 0;
        FichaData lanzadora = null;

        foreach (var entrada in todasLasFichas)
        {
            if (seleccionadas < _fichasJugadorTest)
            {
                betLogic.SeleccionarFicha(entrada.ficha);
                seleccionadas++;
            }
            else if (lanzadora == null)
            {
                lanzadora = entrada.ficha;
            }
        }

        if (lanzadora != null)
        {
            betLogic.SeleccionarFichaLanzadora(lanzadora);
        }

        _combatManager.ConfirmarApuestas();
    }

    private void IniciarCombateCompleto()
    {
        Debug.Log("[TestRunner] Iniciando combate completo...");
        _combatManager.IniciarCombate(_fichasNPCPool);
    }

    private void CrearDatosMock()
    {
        _albumTest = new AlbumData();
        _fichasNPCPool = new List<FichaData>();

        string[] nombresJugador = { "Dragón Dorado", "Fénix Rojo", "Lobo Plateado", "Águila Real", "Serpiente Jade", "Tigre Blanco" };
        for (int i = 0; i < nombresJugador.Length; i++)
        {
            var ficha = new FichaData
            {
                templateId = i + 1,
                nombre = nombresJugador[i],
                rareza = (Rareza)(i % 3),
                rango = 1,
                experienciaDeRango = 0,
                estaRoto = false,
                desgaste = Random.Range(0f, 30f),
                perk = "Ninguno",
                peso = Random.Range(5f, 20f),
                suerte = Random.Range(0.1f, 1f)
            };
            _albumTest.AgregarFicha(ficha);
        }

        string[] nombresNPC = { "Murciélago Oscuro", "Rata Nocturna", "Cuervo Negro", "Araña Venenosa", "Escorpión Rojo", "Gato Sombra" };
        for (int i = 0; i < nombresNPC.Length; i++)
        {
            var ficha = new FichaData
            {
                templateId = 100 + i,
                nombre = nombresNPC[i],
                rareza = (Rareza)(i % 3),
                rango = 1,
                experienciaDeRango = 0,
                estaRoto = false,
                desgaste = Random.Range(0f, 20f),
                perk = "Ninguno",
                peso = Random.Range(5f, 20f),
                suerte = Random.Range(0.1f, 1f)
            };
            _fichasNPCPool.Add(ficha);
        }

        Debug.Log($"[TestRunner] Datos mock creados. Álbum: {_albumTest.ObtenerTotalFichas()} fichas. NPC Pool: {_fichasNPCPool.Count} fichas.");
    }

    private void OnCombateIniciado()
    {
        _combateActivo = true;
        Debug.Log("[TestRunner] >>> COMBATE INICIADO <<<");
    }

    private void OnCombateTerminado(CombatData data)
    {
        _combateActivo = false;
        Debug.Log($"[TestRunner] >>> COMBATE TERMINADO <<< Jugador ganó: {data.FichasGanadasJugador.Count} | NPC ganó: {data.FichasGanadasNPC.Count}");

        var summary = CombatResultApplier.AplicarResultados(data, _albumTest, _config);

        if (_summaryUI != null)
        {
            _summaryUI.Mostrar(summary, _combatManager);
        }

        if (_towerVisual != null)
        {
            _towerVisual.ActualizarVisuales();
        }

        Debug.Log($"[TestRunner] Resultados: XP={summary.XPGanada} Desgaste={summary.DesgasteAplicado} Rota={summary.FichaSeRompio}");
        Debug.Log("[TestRunner] Presiona C o F para otro combate.");
    }

    private void OnFaseCambiada(CombatData.CombatPhase fase)
    {
        Debug.Log($"[TestRunner] Fase: {fase}");

        if (fase == CombatData.CombatPhase.ThrowTurn && _towerVisual != null)
        {
            if (_combatManager.DatosCombate != null)
            {
                _towerVisual.ConstruirTorreVisual(_combatManager.DatosCombate.Torre);
            }
        }
    }

    private void OnLanzamientoResuelto(ThrowResult resultado)
    {
        Debug.Log($"[TestRunner] Lanzamiento: {resultado}");

        if (_towerVisual != null && resultado.FichasVolteadas != null)
        {
            _towerVisual.AnimarVolteo(resultado.FichasVolteadas);
        }
    }

    private void OnTurnoCambiado()
    {
        if (_combatManager.DatosCombate != null)
        {
            Debug.Log($"[TestRunner] Turno de: {_combatManager.DatosCombate.TurnoActual} | Fichas restantes: {_combatManager.DatosCombate.FichasRestantes}");
        }
    }

    private void OnDestroy()
    {
        if (_combatManager != null)
        {
            _combatManager.OnCombateIniciado -= OnCombateIniciado;
            _combatManager.OnCombateTerminado -= OnCombateTerminado;
            _combatManager.OnFaseCambiada -= OnFaseCambiada;
            _combatManager.OnLanzamientoResuelto -= OnLanzamientoResuelto;
            _combatManager.OnTurnoCambiado -= OnTurnoCambiado;
        }
    }
}
