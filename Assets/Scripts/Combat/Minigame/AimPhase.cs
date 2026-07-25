using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Etapa 1 del minijuego: Mira que se mueve en forma de lemniscata (infinito)
/// directamente en espacio mundo (plano X/Z), centrada en la torre.
/// La lemniscata rota lentamente para dar variación sin ser errática.
/// Output: Vector3 posición mundo donde caerá la ficha.
/// </summary>
public class AimPhase : MonoBehaviour
{
    [Header("Lemniscata")]
    [SerializeField] private float _radio = 1f;
    [SerializeField] private float _velocidad = 2f;
    [SerializeField] private float _velocidadRotacion = 0.3f;

    [Header("NPC")]
    [SerializeField] private float _npcDelayMin = 0.8f;
    [SerializeField] private float _npcDelayMax = 2.5f;

    public Action<Vector3> OnPosicionFijada;

    private Vector3 _posicionActualMundo;
    private Vector3 _centroTorre;
    private bool _activo;
    private bool _modoNPC;
    private float _tiempoInicio;
    private float _npcDelay;
    private float _tiempo;
    private float _anguloRotacion;

    public Vector3 PosicionActualMundo => _posicionActualMundo;
    public bool EstaActivo => _activo;

    /// <summary>
    /// Inicia la fase de mira centrada en la posición de la torre.
    /// </summary>
    public void Iniciar(float velocidad, Vector3 centroTorre, float radio, bool esNPC = false)
    {
        _velocidad = velocidad;
        _centroTorre = centroTorre;
        _radio = radio;
        _modoNPC = esNPC;
        _activo = true;
        _tiempoInicio = Time.time;
        _tiempo = 0f;
        _anguloRotacion = 0f;

        if (_modoNPC)
        {
            _npcDelay = UnityEngine.Random.Range(_npcDelayMin, _npcDelayMax);
        }
    }

    /// <summary>
    /// Sobrecarga para compatibilidad con la firma anterior.
    /// </summary>
    public void Iniciar(float velocidad, bool esNPC = false)
    {
        Iniciar(velocidad, _centroTorre, _radio, esNPC);
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

        ActualizarPosicion();

        if (_modoNPC)
        {
            if (Time.time - _tiempoInicio >= _npcDelay)
            {
                FijarPosicion();
            }
        }
        else
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
            {
                FijarPosicion();
            }
        }
    }

    private void ActualizarPosicion()
    {
        _tiempo += Time.deltaTime * _velocidad;
        _anguloRotacion += Time.deltaTime * _velocidadRotacion;

        // Lemniscata de Bernoulli: x = cos(t) / (1 + sin²(t)), y = sin(t)*cos(t) / (1 + sin²(t))
        float sinT = Mathf.Sin(_tiempo);
        float cosT = Mathf.Cos(_tiempo);
        float denominador = 1f + sinT * sinT;

        float localX = (_radio * cosT) / denominador;
        float localZ = (_radio * sinT * cosT) / denominador;

        // Aplicar rotación lenta al patrón
        float rotCos = Mathf.Cos(_anguloRotacion);
        float rotSin = Mathf.Sin(_anguloRotacion);

        float rotatedX = localX * rotCos - localZ * rotSin;
        float rotatedZ = localX * rotSin + localZ * rotCos;

        // Posición final en espacio mundo (plano X/Z centrado en la torre)
        _posicionActualMundo = new Vector3(
            _centroTorre.x + rotatedX,
            _centroTorre.y,
            _centroTorre.z + rotatedZ
        );
    }

    private void FijarPosicion()
    {
        _activo = false;
        OnPosicionFijada?.Invoke(_posicionActualMundo);
        Debug.Log($"[AimPhase] Posición fijada: {_posicionActualMundo}");
    }
}
