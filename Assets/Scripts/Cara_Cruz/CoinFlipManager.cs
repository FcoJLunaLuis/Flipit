using System;
using UnityEngine;

public class CoinFlipManager : MonoBehaviour
{
    private static CoinFlipManager _instance;
    public static CoinFlipManager Instance => _instance;

    [Header("Referencias")]
    [Tooltip("GameObject contenedor de todo el sistema de flip (moneda, UI, cámara)")]
    [SerializeField] private GameObject _contenedorFlip;

    [Tooltip("Referencia al script CoinFlip")]
    [SerializeField] private CoinFlip _coinFlip;

    [Tooltip("Referencia al script CoinFlipUI")]
    [SerializeField] private CoinFlipUI _coinFlipUI;

    public Action<bool> OnFlipCompletado;

    private Action<bool> _callbackActual;
    private int _totalCaras;
    private int _totalCruces;
    private GameObject _camaraPrincipal;

    public int TotalCaras => _totalCaras;
    public int TotalCruces => _totalCruces;
    public int TotalFlips => _totalCaras + _totalCruces;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (_contenedorFlip != null)
        {
            _contenedorFlip.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (_coinFlipUI != null)
        {
            _coinFlipUI.OnSecuenciaTerminada += OnSecuenciaTerminada;
        }
    }

    private void OnDisable()
    {
        if (_coinFlipUI != null)
        {
            _coinFlipUI.OnSecuenciaTerminada -= OnSecuenciaTerminada;
        }
    }

    public void IniciarFlip(Action<bool> onCompletado = null)
    {
        _callbackActual = onCompletado;

        _camaraPrincipal = Camera.main?.gameObject;

        if (_camaraPrincipal != null)
        {
            _camaraPrincipal.SetActive(false);
        }

        if (_contenedorFlip != null)
        {
            _contenedorFlip.SetActive(true);
        }

        if (_coinFlip != null)
        {
            _coinFlip.ResetearMoneda();
        }

        if (_coinFlipUI != null)
        {
            _coinFlipUI.MostrarBotones();
        }

        Debug.Log("[CoinFlipManager] Flip iniciado. Esperando elección del jugador...");
    }

    public void IniciarPartida()
    {
        _totalCaras = 0;
        _totalCruces = 0;

        if (_coinFlip != null)
        {
            _coinFlip.IniciarPartida();
        }

        Debug.Log("[CoinFlipManager] Partida iniciada. Estadísticas reseteadas.");
    }

    private void OnSecuenciaTerminada()
    {
        if (_coinFlip == null) return;

        bool jugadorGano = _coinFlip.JugadorGano;
        CoinFlip.ResultadoMoneda resultado = _coinFlip.UltimoResultado;

        if (resultado == CoinFlip.ResultadoMoneda.Cara)
            _totalCaras++;
        else
            _totalCruces++;

        Debug.Log($"[CoinFlipManager] Flip completado. Jugador {(jugadorGano ? "ganó" : "perdió")} | Estadísticas totales: Cara={_totalCaras}, Cruz={_totalCruces}");

        DesactivarFlip();

        _callbackActual?.Invoke(jugadorGano);
        OnFlipCompletado?.Invoke(jugadorGano);
        _callbackActual = null;
    }

    private void DesactivarFlip()
    {
        if (_contenedorFlip != null)
        {
            _contenedorFlip.SetActive(false);
        }

        if (_camaraPrincipal != null)
        {
            _camaraPrincipal.SetActive(true);
            _camaraPrincipal = null;
        }

    }
}
