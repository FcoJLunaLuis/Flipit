using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinFlip : MonoBehaviour
{
    public enum ResultadoMoneda { Cara, Cruz }

    private enum Estado { Idle, EnAire, Snapping, Terminado }

    [Header("Configuración de Lanzamiento")]
    [Tooltip("Fuerza del impulso vertical al lanzar la moneda")]
    [SerializeField] private float _fuerzaLanzamiento = 18f;

    [Tooltip("Fuerza del torque para hacer girar la moneda")]
    [SerializeField] private float _torqueFuerza = 10f;

    [Tooltip("Tiempo mínimo de vuelo antes de detectar reposo")]
    [SerializeField] private float _tiempoMinimoVuelo = 0.5f;

    [Header("Configuración de Snap")]
    [Tooltip("Velocidad de interpolación para el snap de rotación")]
    [SerializeField] private float _velocidadSnap = 5f;

    [Tooltip("Ángulo mínimo para considerar snap completado")]
    [SerializeField] private float _anguloSnapCompletado = 2f;

    [Header("Configuración de Shuffle Bag")]
    [Tooltip("Cantidad de resultados 'Cara' (Sello) por bolsa")]
    [SerializeField] private int _carasPorBolsa = 3;

    [Tooltip("Cantidad de resultados 'Cruz' (Águila) por bolsa")]
    [SerializeField] private int _crucesPorBolsa = 3;

    [Header("Referencias")]
    [SerializeField] private Rigidbody _rb;

    [Header("Debug")]
    [SerializeField] private float _distanciaGizmos = 2f;

    public Action OnLanzamiento;
    public Action<ResultadoMoneda> OnResultado;

    private Estado _estadoActual = Estado.Idle;
    private ResultadoMoneda _resultadoActual;
    private ResultadoMoneda _apuestaJugador;
    private ShuffleBag<ResultadoMoneda> _bolsa;
    private Vector3 _posicionInicial;
    private Quaternion _rotacionInicial;
    private float _tiempoLanzamiento;
    private Quaternion _rotacionTarget;

    public bool EstaIdle => _estadoActual == Estado.Idle;
    public ResultadoMoneda UltimoResultado => _resultadoActual;
    public ResultadoMoneda ApuestaJugador => _apuestaJugador;
    public bool JugadorGano => _apuestaJugador == _resultadoActual;

    private void Awake()
    {
        if (_rb == null)
        {
            Transform cuerpo = transform.Find("cuerpo");
            if (cuerpo != null)
            {
                _rb = cuerpo.GetComponent<Rigidbody>();
            }
        }

        if (_rb == null)
        {
            Debug.LogError("CoinFlip: No se encontró Rigidbody en el hijo 'cuerpo'.");
            enabled = false;
            return;
        }

        _posicionInicial = _rb.transform.position;
        _rotacionInicial = _rb.transform.rotation;

        CrearBolsa();
    }

    private void Update()
    {
        switch (_estadoActual)
        {
            case Estado.EnAire:
                if (Time.time - _tiempoLanzamiento > _tiempoMinimoVuelo && _rb.linearVelocity.y < 0)
                {
                    _estadoActual = Estado.Snapping;
                }
                break;

            case Estado.Snapping:
                HacerSnap();
                break;
        }
    }

    public void LanzarConApuesta(ResultadoMoneda apuesta)
    {
        if (_estadoActual != Estado.Idle) return;

        _apuestaJugador = apuesta;
        Lanzar();
    }

    private void Lanzar()
    {
        _resultadoActual = _bolsa.Next();
        _rotacionTarget = ObtenerRotacionTarget(_resultadoActual);

        _rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        _rb.AddForce(Vector3.up * _fuerzaLanzamiento, ForceMode.Impulse);
        _rb.AddTorque(_rb.transform.right * _torqueFuerza, ForceMode.Impulse);

        _tiempoLanzamiento = Time.time;
        _estadoActual = Estado.EnAire;

        OnLanzamiento?.Invoke();
        Debug.Log($"[CoinFlip] Apuesta: {_apuestaJugador} | Resultado decidido: {_resultadoActual} (Bolsa restante: {_bolsa.Remaining})");
    }

    private void HacerSnap()
    {
        _rb.angularVelocity = Vector3.Lerp(_rb.angularVelocity, Vector3.zero, Time.deltaTime * _velocidadSnap);
        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, _rotacionTarget, Time.deltaTime * _velocidadSnap));

        float angulo = Quaternion.Angle(_rb.rotation, _rotacionTarget);
        bool detenida = _rb.linearVelocity.magnitude < 0.05f && angulo < _anguloSnapCompletado;

        if (detenida)
        {
            _rb.rotation = _rotacionTarget;
            _rb.angularVelocity = Vector3.zero;
            _estadoActual = Estado.Terminado;
            OnResultado?.Invoke(_resultadoActual);
        }
    }

    private Quaternion ObtenerRotacionTarget(ResultadoMoneda resultado)
    {
        if (resultado == ResultadoMoneda.Cara)
        {
            return Quaternion.LookRotation(Vector3.up, _rb.transform.up);
        }
        else
        {
            return Quaternion.LookRotation(Vector3.down, _rb.transform.up);
        }
    }

    private void CrearBolsa()
    {
        var items = new List<ResultadoMoneda>();
        for (int i = 0; i < _carasPorBolsa; i++) items.Add(ResultadoMoneda.Cara);
        for (int i = 0; i < _crucesPorBolsa; i++) items.Add(ResultadoMoneda.Cruz);
        _bolsa = new ShuffleBag<ResultadoMoneda>(items);
    }

    public void IniciarPartida()
    {
        CrearBolsa();
        ResetearMoneda();
        Debug.Log("[CoinFlip] Partida iniciada. Bolsa reseteada.");
    }

    public void ResetearMoneda()
    {
        StopAllCoroutines();
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.transform.position = _posicionInicial;
        _rb.transform.rotation = _rotacionInicial;
        _rb.constraints = RigidbodyConstraints.None;
        _estadoActual = Estado.Idle;
    }

    private void OnDrawGizmos()
    {
        if (_rb == null) return;

        Vector3 origen = _rb.transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(origen, _rb.transform.forward * _distanciaGizmos);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(origen, -_rb.transform.forward * _distanciaGizmos);
    }
}
