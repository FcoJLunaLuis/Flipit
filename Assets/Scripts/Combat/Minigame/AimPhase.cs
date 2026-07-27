using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Etapa 1 del minijuego: Mira libre.
/// Jugador: mueve la mira con mouse (posición en mundo) o WASD. Confirma con Space o Click izquierdo.
/// NPC: escoge punto aleatorio o apunta a una ficha (según precisión configurable).
/// </summary>
public class AimPhase : MonoBehaviour
{
    [Header("Jugador - Movimiento")]
    [SerializeField] private float _velocidadWASD = 5f;
    [SerializeField] private float _radioLimite = 5f;

    [Header("NPC - Targeting")]
    [Tooltip("Probabilidad (0-1) de que el NPC apunte directo a una ficha. 0 = siempre random, 1 = siempre preciso.")]
    [SerializeField] private float _npcPrecision = 0.4f;
    [SerializeField] private float _npcDelayMin = 0.8f;
    [SerializeField] private float _npcDelayMax = 2.0f;

    public Action<Vector3> OnPosicionFijada;

    private Vector3 _posicionActualMundo;
    private Vector3 _centroTorre;
    private float _radio;
    private bool _activo;
    private bool _modoNPC;
    private float _tiempoInicio;
    private float _npcDelay;
    private Camera _mainCamera;
    private List<PhysicsChip> _fichasSinVoltear;

    public Vector3 PosicionActualMundo => _posicionActualMundo;
    public bool EstaActivo => _activo;

    public void Iniciar(float velocidad, Vector3 centroTorre, float radio, bool esNPC = false, List<PhysicsChip> fichasSinVoltear = null)
    {
        _velocidadWASD = velocidad;
        _centroTorre = centroTorre;
        _radio = radio;
        _radioLimite = radio;
        _modoNPC = esNPC;
        _activo = true;
        _tiempoInicio = Time.time;
        _posicionActualMundo = centroTorre;
        _mainCamera = Camera.main;
        _fichasSinVoltear = fichasSinVoltear;

        if (_modoNPC)
        {
            _npcDelay = UnityEngine.Random.Range(_npcDelayMin, _npcDelayMax);
            _posicionActualMundo = CalcularPuntoNPC();
        }
    }

    /// <summary>
    /// Sobrecarga para compatibilidad.
    /// </summary>
    public void Iniciar(float velocidad, bool esNPC = false)
    {
        Iniciar(velocidad, _centroTorre, _radio, esNPC, null);
    }

    public void SetCentroTorre(Vector3 centro)
    {
        _centroTorre = centro;
    }

    public void Detener()
    {
        _activo = false;
    }

    private void Update()
    {
        if (!_activo) return;

        if (_modoNPC)
        {
            if (Time.time - _tiempoInicio >= _npcDelay)
            {
                FijarPosicion();
            }
        }
        else
        {
            ActualizarPosicionJugador();

            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            bool confirmar = false;
            if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame) confirmar = true;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame) confirmar = true;

            if (confirmar)
            {
                FijarPosicion();
            }
        }
    }

    private void ActualizarPosicionJugador()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;

        // Mouse: raycast desde cámara al plano Y del centro de la torre
        if (mouse != null && _mainCamera != null)
        {
            Vector2 mousePos = mouse.position.ReadValue();
            Ray ray = _mainCamera.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0f));

            // Plano horizontal a la altura del centro de la torre
            Plane plano = new Plane(Vector3.up, _centroTorre);
            float distancia;
            if (plano.Raycast(ray, out distancia))
            {
                _posicionActualMundo = ray.GetPoint(distancia);
            }
        }

        // WASD: movimiento alternativo
        if (keyboard != null)
        {
            Vector3 movimiento = Vector3.zero;
            if (keyboard.wKey.isPressed) movimiento.z += 1f;
            if (keyboard.sKey.isPressed) movimiento.z -= 1f;
            if (keyboard.aKey.isPressed) movimiento.x -= 1f;
            if (keyboard.dKey.isPressed) movimiento.x += 1f;

            if (movimiento.sqrMagnitude > 0f)
            {
                _posicionActualMundo += movimiento.normalized * _velocidadWASD * Time.deltaTime;
            }
        }

        // Clampear: no permitir que la mira pase a través de los muros (colliders)
        // Hacemos un raycast desde el centro hacia la posición de la mira
        // Si hay un collider entre el centro y la mira, limitamos ahí
        Vector3 direccion = _posicionActualMundo - _centroTorre;
        direccion.y = 0f;

        if (direccion.sqrMagnitude > 0.01f)
        {
            RaycastHit hit;
            float distancia = direccion.magnitude;
            Ray ray = new Ray(_centroTorre + Vector3.up * 0.5f, direccion.normalized);

            if (Physics.Raycast(ray, out hit, distancia))
            {
                // Limitar la mira justo antes del collider
                Vector3 puntoLimite = hit.point - direccion.normalized * 0.1f;
                _posicionActualMundo = new Vector3(puntoLimite.x, _centroTorre.y, puntoLimite.z);
            }
        }
    }

    private Vector3 CalcularPuntoNPC()
    {
        // Decidir si apunta a una ficha o random
        bool apuntarAFicha = UnityEngine.Random.value < _npcPrecision && _fichasSinVoltear != null && _fichasSinVoltear.Count > 0;

        if (apuntarAFicha)
        {
            // Escoger una ficha aleatoria de las que quedan
            int index = UnityEngine.Random.Range(0, _fichasSinVoltear.Count);
            Vector3 posFicha = _fichasSinVoltear[index].transform.position;
            // Agregar un poco de offset para no ser perfecto
            float offsetX = UnityEngine.Random.Range(-0.3f, 0.3f);
            float offsetZ = UnityEngine.Random.Range(-0.3f, 0.3f);
            return new Vector3(posFicha.x + offsetX, _centroTorre.y, posFicha.z + offsetZ);
        }
        else
        {
            // Punto random dentro del radio
            float angulo = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = UnityEngine.Random.Range(0f, _radioLimite * 0.7f);
            return _centroTorre + new Vector3(Mathf.Cos(angulo) * dist, 0f, Mathf.Sin(angulo) * dist);
        }
    }

    private void FijarPosicion()
    {
        _activo = false;
        OnPosicionFijada?.Invoke(_posicionActualMundo);
        Debug.Log($"[AimPhase] Posición fijada: {_posicionActualMundo} (NPC:{_modoNPC})");
    }
}
