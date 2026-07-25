using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Orquesta las 3 fases del minijuego de lanzamiento en secuencia.
/// Recopila los resultados y genera un ThrowResult.
/// Ya no resuelve fichas directamente — las físicas se encargan.
/// </summary>
public class ThrowMinigameController : MonoBehaviour
{
    [Header("Referencias de Fases")]
    [SerializeField] private AimPhase _aimPhase;
    [SerializeField] private ForcePhase _forcePhase;
    [SerializeField] private PrecisionPhase _precisionPhase;

    [Header("Configuración")]
    [SerializeField] private CombatConfig _config;

    [Header("Torre")]
    [SerializeField] private TowerPhysicsBuilder _towerPhysicsBuilder;

    public Action<ThrowResult> OnLanzamientoCompleto;

    private enum MinigameState { Idle, Aim, Force, Precision, Resolviendo }

    private MinigameState _estado = MinigameState.Idle;
    private Vector3 _posicionFijadaMundo;
    private float _fuerzaFijada;
    private float _dispersionFijada;
    private bool _esNPC;
    private TowerSlot.SlotOwner _lanzadorActual;
    private List<TowerSlot> _torreActual;

    public bool EstaActivo => _estado != MinigameState.Idle;

    public void IniciarMinijuego(bool esNPC, TowerSlot.SlotOwner lanzador, List<TowerSlot> torre, CombatConfig config)
    {
        _esNPC = esNPC;
        _lanzadorActual = lanzador;
        _torreActual = torre;
        _config = config;

        IniciarFaseAim();
    }

    public void SetReferencias(AimPhase aim, ForcePhase force, PrecisionPhase precision)
    {
        _aimPhase = aim;
        _forcePhase = force;
        _precisionPhase = precision;
    }

    public void SetTowerBuilder(TowerPhysicsBuilder builder)
    {
        _towerPhysicsBuilder = builder;
    }

    private void IniciarFaseAim()
    {
        _estado = MinigameState.Aim;

        // Configurar centro de la torre para la lemniscata
        Vector3 centroTorre = Vector3.zero;
        float radio = 2f;
        if (_towerPhysicsBuilder != null)
        {
            centroTorre = _towerPhysicsBuilder.CentroTorre;
        }

        _aimPhase.OnPosicionFijada = null;
        _aimPhase.OnPosicionFijada += OnAimComplete;
        _aimPhase.Iniciar(_config.velocidadMira, centroTorre, radio, _esNPC);
    }

    private void OnAimComplete(Vector3 posicionMundo)
    {
        _posicionFijadaMundo = posicionMundo;
        _aimPhase.OnPosicionFijada -= OnAimComplete;

        IniciarFaseForce();
    }

    private void IniciarFaseForce()
    {
        _estado = MinigameState.Force;

        _forcePhase.OnFuerzaFijada = null;
        _forcePhase.OnFuerzaFijada += OnForceComplete;
        _forcePhase.Iniciar(_config.velocidadBarraFuerza, _esNPC, _config.npcFuerzaMin, _config.npcFuerzaMax);
    }

    private void OnForceComplete(float fuerza)
    {
        _fuerzaFijada = fuerza;
        _forcePhase.OnFuerzaFijada -= OnForceComplete;

        IniciarFasePrecision();
    }

    private void IniciarFasePrecision()
    {
        _estado = MinigameState.Precision;

        _precisionPhase.OnPrecisionFijada = null;
        _precisionPhase.OnPrecisionFijada += OnPrecisionComplete;
        _precisionPhase.Iniciar(_config.velocidadCirculoPrecision, _esNPC, _config.npcDispersionMin, _config.npcDispersionMax);
    }

    private void OnPrecisionComplete(float dispersion)
    {
        _dispersionFijada = dispersion;
        _precisionPhase.OnPrecisionFijada -= OnPrecisionComplete;

        ResolverLanzamiento();
    }

    private void ResolverLanzamiento()
    {
        _estado = MinigameState.Resolviendo;

        // Crear resultado con posición mundo directa
        var resultado = new ThrowResult(_posicionFijadaMundo, _fuerzaFijada, _dispersionFijada, _lanzadorActual);

        _estado = MinigameState.Idle;

        Debug.Log($"[ThrowMinigame] {resultado}");
        OnLanzamientoCompleto?.Invoke(resultado);
    }

    public void Cancelar()
    {
        _aimPhase.Detener();
        _forcePhase.Detener();
        _precisionPhase.Detener();
        _estado = MinigameState.Idle;
    }
}
