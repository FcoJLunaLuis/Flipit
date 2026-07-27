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
    private HashSet<PhysicsChip> _volteadasAntesDeTurno = new HashSet<PhysicsChip>();

    public bool EstaEvaluando => _evaluando;

    /// <summary>
    /// Activa la evaluación en todas las fichas. Ellas se auto-verifican cada frame.
    /// </summary>
    public void IniciarEvaluacion()
    {
        _evaluando = true;
        _tiempoInicioEvaluacion = Time.time;

        // Guardar snapshot de fichas ya volteadas antes de este turno
        _volteadasAntesDeTurno.Clear();
        foreach (var chip in _towerBuilder.Fichas)
        {
            if (chip != null && chip.ResultadoVolteada)
                _volteadasAntesDeTurno.Add(chip);
        }

        // Activar auto-evaluación en cada ficha que aún no se volteó
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

        // Solo reportar fichas que se voltearon EN ESTE TURNO (no las previas)
        var volteadasEsteTurno = new List<PhysicsChip>();
        foreach (var chip in _towerBuilder.Fichas)
        {
            if (chip != null && chip.EstaEvaluada && chip.ResultadoVolteada
                && !_volteadasAntesDeTurno.Contains(chip))
            {
                volteadasEsteTurno.Add(chip);
            }
        }

        Debug.Log($"[FlipDetector] Turno completo. {volteadasEsteTurno.Count} fichas volteadas este turno.");
        OnEvaluacionCompleta?.Invoke(volteadasEsteTurno);
    }
}
