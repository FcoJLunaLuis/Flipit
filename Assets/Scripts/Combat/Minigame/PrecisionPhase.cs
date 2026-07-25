using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Etapa 3 del minijuego: Círculo que se expande y contrae.
/// Output: radio en unidades mundo. Confirmar con Space o Click izquierdo.
/// </summary>
public class PrecisionPhase : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float _velocidad = 3f;

    [Header("Radio en unidades mundo")]
    [Tooltip("Radio mínimo de dispersión (mejor caso)")]
    [SerializeField] private float _radioMinimoMundo = 0.05f;
    [Tooltip("Radio máximo de dispersión (peor caso)")]
    [SerializeField] private float _radioMaximoMundo = 0.8f;

    [Header("NPC")]
    [SerializeField] private float _npcDispersionMin = 0.2f;
    [SerializeField] private float _npcDispersionMax = 0.7f;
    [SerializeField] private float _npcDelayMin = 0.4f;
    [SerializeField] private float _npcDelayMax = 1.8f;

    public Action<float> OnPrecisionFijada;

    private float _radioActualMundo;
    private float _dispersionNormalizada;
    private bool _activo;
    private bool _modoNPC;
    private float _tiempoInicio;
    private float _npcDelay;
    private float _tiempo;

    public float RadioActualMundo => _radioActualMundo;
    public float DispersionNormalizada => _dispersionNormalizada;
    public float RadioMinimo => _radioMinimoMundo;
    public float RadioMaximo => _radioMaximoMundo;
    public bool EstaActivo => _activo;

    public void Iniciar(float velocidad, bool esNPC = false, float npcMin = 0.2f, float npcMax = 0.7f)
    {
        _velocidad = velocidad;
        _modoNPC = esNPC;
        _npcDispersionMin = npcMin;
        _npcDispersionMax = npcMax;
        _activo = true;
        _tiempoInicio = Time.time;
        _tiempo = 0f;

        if (_modoNPC)
        {
            _npcDelay = UnityEngine.Random.Range(_npcDelayMin, _npcDelayMax);
        }
    }

    public void Detener()
    {
        _activo = false;
    }

    public void SetRadiosMundo(float minimo, float maximo)
    {
        _radioMinimoMundo = minimo;
        _radioMaximoMundo = maximo;
    }

    private void Update()
    {
        if (!_activo) return;

        ActualizarRadio();

        if (_modoNPC)
        {
            if (Time.time - _tiempoInicio >= _npcDelay)
            {
                FijarPrecision();
            }
        }
        else
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            bool confirmar = false;
            if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame) confirmar = true;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame) confirmar = true;

            if (confirmar)
            {
                FijarPrecision();
            }
        }
    }

    private void ActualizarRadio()
    {
        _tiempo += Time.deltaTime * _velocidad;
        _dispersionNormalizada = Mathf.PingPong(_tiempo, 1f);
        _radioActualMundo = Mathf.Lerp(_radioMinimoMundo, _radioMaximoMundo, _dispersionNormalizada);
    }

    private void FijarPrecision()
    {
        _activo = false;
        OnPrecisionFijada?.Invoke(_radioActualMundo);
        Debug.Log($"[PrecisionPhase] Dispersión fijada: {_radioActualMundo:F3} unidades mundo");
    }
}
