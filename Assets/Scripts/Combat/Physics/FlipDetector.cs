using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Espera a que las fichas se detengan después de un impacto y evalúa cuáles se voltearon.
/// Condiciones de finalización:
/// - Todas las fichas en reposo (velocidad bajo umbral)
/// - Timeout máximo alcanzado
/// - Solo 1 ficha moviéndose después del timeout parcial
/// </summary>
public class FlipDetector : MonoBehaviour
{
    [Header("Configuración de Reposo")]
    [SerializeField] private float _timeoutMaximo = 8f;
    [SerializeField] private float _timeoutUnaFicha = 3f;
    [SerializeField] private float _delayInicialEvaluacion = 0.5f;

    [Header("Referencias")]
    [SerializeField] private TowerPhysicsBuilder _towerBuilder;
    [SerializeField] private CombatArena _arena;

    public Action<List<PhysicsChip>> OnEvaluacionCompleta;

    private bool _evaluando;
    private float _tiempoInicioEvaluacion;
    private float _tiempoUnaFichaMovimiento;
    private bool _unaFichaDetectada;

    public bool EstaEvaluando => _evaluando;

    /// <summary>
    /// Inicia la evaluación. Espera a que las fichas se detengan.
    /// </summary>
    public void IniciarEvaluacion()
    {
        _evaluando = true;
        _tiempoInicioEvaluacion = Time.time;
        _unaFichaDetectada = false;
        _tiempoUnaFichaMovimiento = 0f;

        Debug.Log("[FlipDetector] Evaluación iniciada. Esperando reposo...");
    }

    /// <summary>
    /// Cancela la evaluación en curso.
    /// </summary>
    public void CancelarEvaluacion()
    {
        _evaluando = false;
    }

    private void FixedUpdate()
    {
        if (!_evaluando) return;

        // Contener fichas dentro del radio
        if (_arena != null && _towerBuilder != null)
        {
            foreach (var chip in _towerBuilder.Fichas)
            {
                _arena.ContenerFicha(chip);
            }
        }

        // No evaluar durante el delay inicial (dejar que el impacto se propague)
        if (Time.time - _tiempoInicioEvaluacion < _delayInicialEvaluacion) return;

        // Verificar timeout máximo
        if (Time.time - _tiempoInicioEvaluacion >= _timeoutMaximo)
        {
            Debug.Log("[FlipDetector] Timeout máximo alcanzado. Evaluando...");
            CompletarEvaluacion();
            return;
        }

        // Contar fichas en movimiento
        var fichasSinVoltear = _towerBuilder.ObtenerFichasSinVoltear();
        int enMovimiento = 0;

        foreach (var chip in fichasSinVoltear)
        {
            if (chip != null && !chip.EstaEnReposo())
                enMovimiento++;
        }

        // Todas en reposo
        if (enMovimiento == 0)
        {
            Debug.Log("[FlipDetector] Todas las fichas en reposo. Evaluando...");
            CompletarEvaluacion();
            return;
        }

        // Solo 1 en movimiento - iniciar timeout parcial
        if (enMovimiento == 1)
        {
            if (!_unaFichaDetectada)
            {
                _unaFichaDetectada = true;
                _tiempoUnaFichaMovimiento = Time.time;
            }
            else if (Time.time - _tiempoUnaFichaMovimiento >= _timeoutUnaFicha)
            {
                Debug.Log("[FlipDetector] Timeout de una ficha alcanzado. Evaluando...");
                CompletarEvaluacion();
                return;
            }
        }
        else
        {
            _unaFichaDetectada = false;
        }
    }

    private void CompletarEvaluacion()
    {
        _evaluando = false;

        var fichasSinVoltear = _towerBuilder.ObtenerFichasSinVoltear();
        var nuevasVolteadas = new List<PhysicsChip>();

        foreach (var chip in fichasSinVoltear)
        {
            if (chip == null) continue;

            chip.Evaluar();

            if (chip.ResultadoVolteada)
            {
                nuevasVolteadas.Add(chip);
            }
        }

        Debug.Log($"[FlipDetector] Evaluación completa. {nuevasVolteadas.Count} fichas volteadas en este turno.");

        OnEvaluacionCompleta?.Invoke(nuevasVolteadas);
    }
}
