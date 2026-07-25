using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Activa la evaluación continua en las fichas después del impacto.
/// Las fichas se auto-evalúan cada frame cuando están quietas.
/// FlipDetector solo espera a que todas se evalúen y pasa al siguiente turno.
/// Timeout como fallback.
/// </summary>
public class FlipDetector : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Timeout máximo antes de forzar evaluación")]
    [SerializeField] private float _timeoutMaximo = 8f;

    [Header("Referencias")]
    [SerializeField] private TowerPhysicsBuilder _towerBuilder;
    [SerializeField] private CombatArena _arena;

    public Action<List<PhysicsChip>> OnEvaluacionCompleta;

    private bool _evaluando;
    private float _tiempoInicioEvaluacion;

    public bool EstaEvaluando => _evaluando;

    /// <summary>
    /// Activa la evaluación en todas las fichas. Ellas se auto-verifican cada frame.
    /// </summary>
    public void IniciarEvaluacion()
    {
        _evaluando = true;
        _tiempoInicioEvaluacion = Time.time;

        // Activar auto-evaluación en cada ficha
        var fichas = _towerBuilder.ObtenerFichasSinVoltear();
        foreach (var chip in fichas)
        {
            if (chip != null)
                chip.ActivarEvaluacion();
        }

        Debug.Log($"[FlipDetector] Evaluación activada en {fichas.Count} fichas.");
    }

    public void CancelarEvaluacion()
    {
        _evaluando = false;
    }

    private void Update()
    {
        if (!_evaluando) return;

        // Timeout: forzar evaluación de todas
        if (Time.time - _tiempoInicioEvaluacion >= _timeoutMaximo)
        {
            Debug.Log("[FlipDetector] Timeout. Forzando evaluación.");
            ForzarEvaluacionTodas();
            CompletarTurno();
            return;
        }

        // Verificar si todas las fichas ya se auto-evaluaron
        var fichas = _towerBuilder.ObtenerFichasSinVoltear();
        bool todasEvaluadas = true;

        foreach (var chip in fichas)
        {
            if (chip != null && chip.gameObject.activeSelf && !chip.EstaEvaluada)
            {
                todasEvaluadas = false;
                break;
            }
        }

        if (todasEvaluadas)
        {
            CompletarTurno();
        }
    }

    private void ForzarEvaluacionTodas()
    {
        var fichas = _towerBuilder.ObtenerFichasSinVoltear();
        foreach (var chip in fichas)
        {
            if (chip != null && chip.gameObject.activeSelf && !chip.EstaEvaluada)
            {
                chip.Evaluar();
            }
        }
    }

    private void CompletarTurno()
    {
        _evaluando = false;

        var volteadas = new List<PhysicsChip>();
        foreach (var chip in _towerBuilder.Fichas)
        {
            if (chip != null && chip.EstaEvaluada && chip.ResultadoVolteada)
            {
                volteadas.Add(chip);
            }
        }

        Debug.Log($"[FlipDetector] Turno completo. {volteadas.Count} fichas volteadas.");
        OnEvaluacionCompleta?.Invoke(volteadas);
    }
}
