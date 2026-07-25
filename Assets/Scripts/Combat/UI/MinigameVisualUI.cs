using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI visual del minijuego de lanzamiento.
/// Muestra la mira, la barra de fuerza y el círculo de precisión.
/// La mira se posiciona usando WorldToScreenPoint del punto mundo del AimPhase.
/// </summary>
public class MinigameVisualUI : MonoBehaviour
{
    [Header("Mira (Etapa 1)")]
    [SerializeField] private RectTransform _miraTransform;
    [SerializeField] private GameObject _miraPanel;
    [SerializeField] private RectTransform _areaTorre;

    [Header("Barra de Fuerza (Etapa 2)")]
    [SerializeField] private Slider _barraFuerza;
    [SerializeField] private GameObject _fuerzaPanel;

    [Header("Círculo de Precisión (Etapa 3)")]
    [SerializeField] private RectTransform _circuloTransform;
    [SerializeField] private GameObject _precisionPanel;

    [Header("Referencias")]
    [SerializeField] private AimPhase _aimPhase;
    [SerializeField] private ForcePhase _forcePhase;
    [SerializeField] private PrecisionPhase _precisionPhase;

    [Header("Configuración Visual")]
    [SerializeField] private float _escalaCirculoMultiplicador = 200f;

    private Camera _mainCamera;
    private Canvas _canvas;
    private RectTransform _canvasRect;

    private void Start()
    {
        _mainCamera = Camera.main;
        _canvas = GetComponent<Canvas>();
        if (_canvas != null)
            _canvasRect = _canvas.GetComponent<RectTransform>();
    }

    public void MostrarFaseAim()
    {
        OcultarTodo();
        if (_miraPanel != null) _miraPanel.SetActive(true);
    }

    public void MostrarFaseFuerza()
    {
        OcultarTodo();
        if (_fuerzaPanel != null) _fuerzaPanel.SetActive(true);
    }

    public void MostrarFasePrecision()
    {
        OcultarTodo();
        if (_precisionPanel != null) _precisionPanel.SetActive(true);
    }

    public void OcultarTodo()
    {
        if (_miraPanel != null) _miraPanel.SetActive(false);
        if (_fuerzaPanel != null) _fuerzaPanel.SetActive(false);
        if (_precisionPanel != null) _precisionPanel.SetActive(false);
    }

    private void Update()
    {
        ActualizarMira();
        ActualizarBarraFuerza();
        ActualizarCirculoPrecision();
    }

    private void ActualizarMira()
    {
        if (_aimPhase == null || !_aimPhase.EstaActivo || _miraTransform == null || _mainCamera == null) return;

        // Convertir posición mundo del AimPhase a posición en pantalla
        Vector3 posicionMundo = _aimPhase.PosicionActualMundo;
        Vector3 screenPos = _mainCamera.WorldToScreenPoint(posicionMundo);

        // Convertir screen position a posición en el canvas
        if (_canvasRect != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect, screenPos, null, out localPoint);
            _miraTransform.anchoredPosition = localPoint;
        }
        else
        {
            _miraTransform.position = screenPos;
        }
    }

    private void ActualizarBarraFuerza()
    {
        if (_forcePhase == null || !_forcePhase.EstaActivo || _barraFuerza == null) return;

        _barraFuerza.value = _forcePhase.ValorActual;
    }

    private void ActualizarCirculoPrecision()
    {
        if (_precisionPhase == null || !_precisionPhase.EstaActivo || _circuloTransform == null) return;

        // Escalar el círculo visualmente proporcional al radio mundo
        float radioMundo = _precisionPhase.RadioActualMundo;
        float escalaVisual = radioMundo * _escalaCirculoMultiplicador;
        _circuloTransform.sizeDelta = new Vector2(escalaVisual, escalaVisual);

        // Posicionar el círculo donde quedó la mira (última posición fijada del AimPhase)
        if (_aimPhase != null && _mainCamera != null && _canvasRect != null)
        {
            Vector3 posAim = _aimPhase.PosicionActualMundo;
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(posAim);
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect, screenPos, null, out localPoint);
            _circuloTransform.anchoredPosition = localPoint;
        }
    }
}
