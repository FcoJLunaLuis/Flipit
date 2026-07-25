using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CoinFlipUI : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Referencia al script CoinFlip de la moneda")]
    [SerializeField] private CoinFlip _coinFlip;

    [Tooltip("Texto TextMeshPro donde se muestra el resultado")]
    [SerializeField] private TextMeshProUGUI _textoResultado;

    [Tooltip("Panel contenedor de los botones de apuesta")]
    [SerializeField] private GameObject _panelBotones;

    [Tooltip("Botón para apostar Águila (Cruz)")]
    [SerializeField] private Button _botonAguila;

    [Tooltip("Botón para apostar Sello (Cara)")]
    [SerializeField] private Button _botonSello;

    [Header("Configuración")]
    [Tooltip("Tiempo que se muestra el resultado de la cara antes de mostrar el ganador")]
    [SerializeField] private float _tiempoEntreTextos = 2f;

    [Tooltip("Tiempo que se muestra quién ganó")]
    [SerializeField] private float _tiempoVisible = 2f;

    public Action OnSecuenciaTerminada;

    private CoinFlip.ResultadoMoneda _apuestaJugador;
    private Coroutine _secuenciaCoroutine;

    private void OnEnable()
    {
        if (_coinFlip != null)
        {
            _coinFlip.OnResultado += IniciarSecuenciaResultado;
        }

        if (_botonAguila != null)
        {
            _botonAguila.onClick.AddListener(OnClickAguila);
        }

        if (_botonSello != null)
        {
            _botonSello.onClick.AddListener(OnClickSello);
        }
    }

    private void OnDisable()
    {
        if (_coinFlip != null)
        {
            _coinFlip.OnResultado -= IniciarSecuenciaResultado;
        }

        if (_botonAguila != null)
        {
            _botonAguila.onClick.RemoveListener(OnClickAguila);
        }

        if (_botonSello != null)
        {
            _botonSello.onClick.RemoveListener(OnClickSello);
        }

        if (_secuenciaCoroutine != null)
        {
            StopCoroutine(_secuenciaCoroutine);
            _secuenciaCoroutine = null;
        }
    }

    public void OnClickAguila()
    {
        if (_coinFlip == null || !_coinFlip.EstaIdle) return;

        _apuestaJugador = CoinFlip.ResultadoMoneda.Cruz;
        OcultarBotones();
        _coinFlip.LanzarConApuesta(_apuestaJugador);
    }

    public void OnClickSello()
    {
        if (_coinFlip == null || !_coinFlip.EstaIdle) return;

        _apuestaJugador = CoinFlip.ResultadoMoneda.Cara;
        OcultarBotones();
        _coinFlip.LanzarConApuesta(_apuestaJugador);
    }

    public void MostrarBotones()
    {
        if (_panelBotones != null)
        {
            _panelBotones.SetActive(true);
        }

        if (_textoResultado != null)
        {
            _textoResultado.gameObject.SetActive(false);
        }
    }

    private void OcultarBotones()
    {
        if (_panelBotones != null)
        {
            _panelBotones.SetActive(false);
        }
    }

    private void IniciarSecuenciaResultado(CoinFlip.ResultadoMoneda resultado)
    {
        if (_secuenciaCoroutine != null)
        {
            StopCoroutine(_secuenciaCoroutine);
        }

        _secuenciaCoroutine = StartCoroutine(SecuenciaResultado(resultado));
    }

    private IEnumerator SecuenciaResultado(CoinFlip.ResultadoMoneda resultado)
    {
        if (_textoResultado == null) yield break;

        _textoResultado.gameObject.SetActive(true);
        _textoResultado.text = resultado == CoinFlip.ResultadoMoneda.Cara
            ? "¡SELLO!"
            : "¡ÁGUILA!";

        yield return new WaitForSeconds(_tiempoEntreTextos);

        bool jugadorGano = _apuestaJugador == resultado;
        _textoResultado.text = jugadorGano
            ? "Jugador ganó"
            : "NPC ganó";

        yield return new WaitForSeconds(_tiempoVisible);

        _textoResultado.gameObject.SetActive(false);
        _secuenciaCoroutine = null;

        OnSecuenciaTerminada?.Invoke();
    }
}
