using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Máquina de estados principal del combate.
/// Orquesta el flujo completo: apuestas → coin flip → turnos de lanzamiento (con físicas) → resumen.
/// Usa ImpactResolver para aplicar fuerzas y FlipDetector para evaluar volteo.
/// </summary>
public class CombatManager : MonoBehaviour
{
    private static CombatManager _instance;
    public static CombatManager Instance => _instance;

    [Header("Configuración")]
    [SerializeField] private CombatConfig _config;

    [Header("Timing")]
    [Tooltip("Segundos de espera entre turnos")]
    [SerializeField] private float _delayEntreTurnos = 2.5f;

    [Header("Referencias - Minijuego")]
    [SerializeField] private ThrowMinigameController _minigameController;

    [Header("Referencias - Físicas")]
    [SerializeField] private TowerPhysicsBuilder _towerPhysicsBuilder;
    [SerializeField] private ImpactResolver _impactResolver;
    [SerializeField] private FlipDetector _flipDetector;
    [SerializeField] private CombatArena _arena;

    public Action OnCombateIniciado;
    public Action<CombatData> OnCombateTerminado;
    public Action<CombatData.CombatPhase> OnFaseCambiada;
    public Action<ThrowResult> OnLanzamientoResuelto;
    public Action OnTurnoCambiado;
    public Action OnTorreGenerada;
    public Action<List<PhysicsChip>> OnFichasVolteadas;

    private CombatData _combatData;
    private BetSelectionLogic _betSelectionJugador;
    private NPCBetGenerator _npcBetGenerator;
    private List<FichaData> _fichasNPCPool;
    private ThrowResult _ultimoResultado;

    public CombatData DatosCombate => _combatData;
    public CombatConfig Config => _config;
    public BetSelectionLogic BetSelection => _betSelectionJugador;
    public TowerPhysicsBuilder TorreActual => _towerPhysicsBuilder;
    public bool EnCombate => _combatData != null && _combatData.FaseActual != CombatData.CombatPhase.Summary;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    /// <summary>
    /// Inicia un nuevo combate.
    /// </summary>
    public void IniciarCombate(List<FichaData> fichasNPC)
    {
        _combatData = new CombatData();
        _betSelectionJugador = new BetSelectionLogic(_config.maxFichasApuesta, _config.minFichasApuesta);
        _npcBetGenerator = new NPCBetGenerator(_config.maxFichasApuesta);
        _fichasNPCPool = fichasNPC;

        CambiarFase(CombatData.CombatPhase.BetSelection);
        OnCombateIniciado?.Invoke();

        // Notificar al GameStateManager que estamos en combate
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetCombat();

        Debug.Log("[CombatManager] Combate iniciado. Esperando selección de fichas...");
    }

    /// <summary>
    /// Confirma la apuesta del jugador y genera la del NPC.
    /// Construye la torre física y la arena.
    /// </summary>
    public void ConfirmarApuestas()
    {
        string error;
        if (!_betSelectionJugador.ValidarSeleccion(out error))
        {
            Debug.LogWarning($"[CombatManager] Apuesta inválida: {error}");
            return;
        }

        // Generar apuesta del NPC
        var fichasNPC = _npcBetGenerator.GenerarApuesta(_fichasNPCPool, _betSelectionJugador.CantidadSeleccionada);
        var lanzadoraNPC = _npcBetGenerator.SeleccionarFichaLanzadora(_fichasNPCPool, fichasNPC);

        _combatData.SetFichasApostadas(_betSelectionJugador.FichasSeleccionadas, fichasNPC);
        _combatData.SetFichaLanzadora(_betSelectionJugador.FichaLanzadora, lanzadoraNPC);

        // Construir torre lógica
        var torre = TowerBuilder.ConstruirTorre(_betSelectionJugador.FichasSeleccionadas, fichasNPC);
        _combatData.SetTorre(torre);

        // Construir torre física (queda kinematic hasta el primer impacto)
        if (_towerPhysicsBuilder != null)
        {
            _towerPhysicsBuilder.ConstruirTorre(torre);

            // Inicializar arena centrada en la torre
            if (_arena != null)
            {
                _arena.InicializarArena(_towerPhysicsBuilder.CentroTorre);
            }

            OnTorreGenerada?.Invoke();
        }

        Debug.Log($"[CombatManager] Apuestas confirmadas. Jugador:{_betSelectionJugador.CantidadSeleccionada} NPC:{fichasNPC.Count} Torre:{torre.Count} fichas");

        IniciarCoinFlip();
    }

    private void IniciarCoinFlip()
    {
        CambiarFase(CombatData.CombatPhase.CoinFlip);

        if (CoinFlipManager.Instance != null)
        {
            CoinFlipManager.Instance.IniciarFlip(OnCoinFlipResult);
        }
        else
        {
            Debug.LogWarning("[CombatManager] CoinFlipManager no encontrado. Asignando turno aleatorio.");
            bool jugadorEmpieza = UnityEngine.Random.Range(0, 2) == 0;
            OnCoinFlipResult(jugadorEmpieza);
        }
    }

    private void OnCoinFlipResult(bool jugadorGano)
    {
        var turno = jugadorGano ? CombatData.TurnOwner.Jugador : CombatData.TurnOwner.NPC;
        _combatData.SetTurnoInicial(turno);

        Debug.Log($"[CombatManager] Coin flip: {(jugadorGano ? "Jugador" : "NPC")} empieza.");

        // Delay para dar tiempo a la torre de asentarse visualmente
        float delay = _towerPhysicsBuilder != null ? _towerPhysicsBuilder.DelayPostConstruccion : 0f;
        StartCoroutine(IniciarTurnoConDelay(delay));
    }

    private System.Collections.IEnumerator IniciarTurnoConDelay(float delay)
    {
        if (delay > 0f)
        {
            Debug.Log($"[CombatManager] Esperando {delay}s antes del primer turno...");
            yield return new WaitForSeconds(delay);
        }
        IniciarTurnoLanzamiento();
    }

    private void IniciarTurnoLanzamiento()
    {
        CambiarFase(CombatData.CombatPhase.ThrowTurn);

        bool esNPC = _combatData.TurnoActual == CombatData.TurnOwner.NPC;
        var lanzador = esNPC ? TowerSlot.SlotOwner.NPC : TowerSlot.SlotOwner.Jugador;

        _minigameController.OnLanzamientoCompleto = null;
        _minigameController.OnLanzamientoCompleto += OnMinijuegoCompleto;
        _minigameController.IniciarMinijuego(esNPC, lanzador, _combatData.Torre, _config);

        Debug.Log($"[CombatManager] Turno de {_combatData.TurnoActual}. Minijuego iniciado.");
    }

    private void OnMinijuegoCompleto(ThrowResult resultado)
    {
        _minigameController.OnLanzamientoCompleto -= OnMinijuegoCompleto;
        _ultimoResultado = resultado;

        CambiarFase(CombatData.CombatPhase.ResolveThrow);

        // Aplicar impacto físico a la torre
        if (_impactResolver != null && _towerPhysicsBuilder != null)
        {
            // Liberar torre en el primer impacto (kinematic → dinámico)
            if (!_towerPhysicsBuilder.TorreLibre)
            {
                _towerPhysicsBuilder.LiberarTorre();
            }

            var fichasActivas = _towerPhysicsBuilder.ObtenerFichasSinVoltear();
            _impactResolver.AplicarImpacto(
                resultado.PuntoDeImpacto,
                resultado.Fuerza,
                resultado.Dispersion,
                _towerPhysicsBuilder.CentroTorre,
                fichasActivas
            );
        }

        // Iniciar evaluación de volteo (espera reposo)
        if (_flipDetector != null)
        {
            _flipDetector.OnEvaluacionCompleta = null;
            _flipDetector.OnEvaluacionCompleta += OnEvaluacionVolteoCompleta;
            _flipDetector.IniciarEvaluacion();
        }
        else
        {
            // Fallback sin físicas
            Debug.LogWarning("[CombatManager] FlipDetector no encontrado. Saltando evaluación.");
            OnEvaluacionVolteoCompleta(new List<PhysicsChip>());
        }

        OnLanzamientoResuelto?.Invoke(resultado);
        Debug.Log($"[CombatManager] Impacto aplicado. Esperando que las fichas se asienten...");
    }

    private void OnEvaluacionVolteoCompleta(List<PhysicsChip> fichasVolteadas)
    {
        if (_flipDetector != null)
            _flipDetector.OnEvaluacionCompleta -= OnEvaluacionVolteoCompleta;

        // Registrar fichas volteadas en el turno actual
        var slotsVolteados = new List<TowerSlot>();
        foreach (var chip in fichasVolteadas)
        {
            if (chip != null && chip.Slot != null)
                slotsVolteados.Add(chip.Slot);
        }

        _ultimoResultado.AsignarFichasVolteadas(slotsVolteados);
        _combatData.RegistrarLanzamiento(_ultimoResultado);

        OnFichasVolteadas?.Invoke(fichasVolteadas);

        Debug.Log($"[CombatManager] Evaluación: {fichasVolteadas.Count} fichas volteadas. {_combatData}");

        VerificarFinCombate();
    }

    private void VerificarFinCombate()
    {
        CambiarFase(CombatData.CombatPhase.CheckEnd);

        if (_combatData.TodasLasFichasVolteadas())
        {
            TerminarCombate();
        }
        else
        {
            _combatData.CambiarTurno();
            OnTurnoCambiado?.Invoke();

            Debug.Log($"[CombatManager] Fichas restantes: {_combatData.FichasRestantes}. Cambiando turno a {_combatData.TurnoActual}. Esperando {_delayEntreTurnos}s...");

            StartCoroutine(SiguienteTurnoConDelay());
        }
    }

    private System.Collections.IEnumerator SiguienteTurnoConDelay()
    {
        yield return new WaitForSeconds(_delayEntreTurnos);
        IniciarTurnoLanzamiento();
    }

    private void TerminarCombate()
    {
        CambiarFase(CombatData.CombatPhase.Summary);

        Debug.Log($"[CombatManager] ¡Combate terminado! Jugador ganó {_combatData.FichasGanadasJugador.Count} fichas. NPC ganó {_combatData.FichasGanadasNPC.Count} fichas.");

        OnCombateTerminado?.Invoke(_combatData);
    }

    public void SalirDelCombate()
    {
        // Restaurar estado de exploración
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetExploration();

        if (_towerPhysicsBuilder != null)
            _towerPhysicsBuilder.LimpiarTorre();

        _combatData = null;
        _betSelectionJugador = null;

        Debug.Log("[CombatManager] Regresando al overworld.");
    }

    private void CambiarFase(CombatData.CombatPhase nuevaFase)
    {
        _combatData.AvanzarFase(nuevaFase);
        OnFaseCambiada?.Invoke(nuevaFase);
    }
}
