using System.Collections;
using UnityEngine;

public class CoinFlipCamera : MonoBehaviour
{
    private enum EstadoCamara { Estatica, Siguiendo, Congelada, Regresando }

    [Header("Referencias")]
    [Tooltip("Referencia al script CoinFlip")]
    [SerializeField] private CoinFlip _coinFlip;

    [Tooltip("Transform del objeto a seguir (cuerpo de la moneda)")]
    [SerializeField] private Transform _target;

    [Header("Configuración de Seguimiento")]
    [Tooltip("Velocidad de rotación del look-at")]
    [SerializeField] private float _velocidadLookAt = 8f;

    [Tooltip("Tiempo que sigue mirando la moneda después de que empieza a caer")]
    [SerializeField] private float _tiempoSeguimientoCaida = 1f;

    [Header("Configuración de Regreso")]
    [Tooltip("Velocidad de interpolación para regresar a la rotación original")]
    [SerializeField] private float _velocidadRegreso = 3f;

    private EstadoCamara _estadoActual = EstadoCamara.Estatica;
    private Quaternion _rotacionOriginal;
    private bool _monedaCayendo;
    private float _tiempoInicioCaida;

    private void Awake()
    {
        _rotacionOriginal = transform.rotation;
    }

    private void OnEnable()
    {
        if (_coinFlip != null)
        {
            _coinFlip.OnLanzamiento += IniciarSeguimiento;
            _coinFlip.OnResultado += OnResultadoRecibido;
        }
    }

    private void OnDisable()
    {
        if (_coinFlip != null)
        {
            _coinFlip.OnLanzamiento -= IniciarSeguimiento;
            _coinFlip.OnResultado -= OnResultadoRecibido;
        }
    }

    private void LateUpdate()
    {
        switch (_estadoActual)
        {
            case EstadoCamara.Siguiendo:
                MirarTarget();
                VerificarCaida();
                break;

            case EstadoCamara.Regresando:
                RegresarAOriginal();
                break;
        }
    }

    private void IniciarSeguimiento()
    {
        _estadoActual = EstadoCamara.Siguiendo;
        _monedaCayendo = false;
    }

    private void MirarTarget()
    {
        if (_target == null) return;

        Vector3 direccion = _target.position - transform.position;
        if (direccion.sqrMagnitude < 0.001f) return;

        Quaternion rotacionDeseada = Quaternion.LookRotation(direccion);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, Time.deltaTime * _velocidadLookAt);
    }

    private void VerificarCaida()
    {
        if (_target == null) return;

        Rigidbody rb = _target.GetComponent<Rigidbody>();
        if (rb == null) return;

        if (!_monedaCayendo && rb.linearVelocity.y < 0)
        {
            _monedaCayendo = true;
            _tiempoInicioCaida = Time.time;
        }

        if (_monedaCayendo && Time.time - _tiempoInicioCaida > _tiempoSeguimientoCaida)
        {
            _estadoActual = EstadoCamara.Congelada;
        }
    }

    private void OnResultadoRecibido(CoinFlip.ResultadoMoneda resultado)
    {
        _estadoActual = EstadoCamara.Regresando;
    }

    private void RegresarAOriginal()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, _rotacionOriginal, Time.deltaTime * _velocidadRegreso);

        if (Quaternion.Angle(transform.rotation, _rotacionOriginal) < 0.5f)
        {
            transform.rotation = _rotacionOriginal;
            _estadoActual = EstadoCamara.Estatica;
        }
    }
}
