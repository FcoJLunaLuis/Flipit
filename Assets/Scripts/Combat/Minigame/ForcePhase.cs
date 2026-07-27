using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Etapa 2 del minijuego: Barra de fuerza que oscila entre 0 y 1 (ping-pong).
/// Confirmar con Space o Click izquierdo.
/// </summary>
public class ForcePhase : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float _velocidad = 2.5f;

    [Header("NPC")]
    [SerializeField] private float _npcFuerzaMin = 0.4f;
    [SerializeField] private float _npcFuerzaMax = 0.9f;
    [SerializeField] private float _npcDelayMin = 0.3f;
    [SerializeField] private float _npcDelayMax = 1.5f;

    public Action<float> OnFuerzaFijada;

    private float _valorActual;
    private bool _activo;
    private bool _modoNPC;
    private float _tiempoInicio;
    private float _npcDelay;
    private float _tiempo;

    public float ValorActual => _valorActual;
    public bool EstaActivo => _activo;

    public void Iniciar(float velocidad, bool esNPC = false, float npcMin = 0.4f, float npcMax = 0.9f)
    {
        _velocidad = velocidad;
        _modoNPC = esNPC;
        _npcFuerzaMin = npcMin;
        _npcFuerzaMax = npcMax;
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

    private void Update()
    {
        if (!_activo) return;

        ActualizarValor();

        if (_modoNPC)
        {
            if (Time.time - _tiempoInicio >= _npcDelay)
            {
                FijarFuerza();
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
                FijarFuerza();
            }
        }
    }

    private void ActualizarValor()
    {
        _tiempo += Time.deltaTime * _velocidad;
        _valorActual = Mathf.PingPong(_tiempo, 1f);
    }

    private void FijarFuerza()
    {
        _activo = false;
        OnFuerzaFijada?.Invoke(_valorActual);
    }
}
