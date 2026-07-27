using UnityEngine;
using TMPro;

/// <summary>
/// Conecta los eventos del CombatManager con la UI del minijuego.
/// Activa/desactiva paneles según la fase y actualiza textos de turno.
/// </summary>
public class CombatUIController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CombatManager _combatManager;
    [SerializeField] private MinigameVisualUI _minigameVisualUI;
    [SerializeField] private TextMeshProUGUI _turnoTexto;
    [SerializeField] private TextMeshProUGUI _infoTexto;

    private void OnEnable()
    {
        if (_combatManager == null) return;

        _combatManager.OnFaseCambiada += OnFaseCambiada;
        _combatManager.OnTurnoCambiado += OnTurnoCambiado;
        _combatManager.OnCombateTerminado += OnCombateTerminado;
    }

    private void OnDisable()
    {
        if (_combatManager == null) return;

        _combatManager.OnFaseCambiada -= OnFaseCambiada;
        _combatManager.OnTurnoCambiado -= OnTurnoCambiado;
        _combatManager.OnCombateTerminado -= OnCombateTerminado;
    }

    private void OnFaseCambiada(CombatData.CombatPhase fase)
    {
        switch (fase)
        {
            case CombatData.CombatPhase.ThrowTurn:
                if (_minigameVisualUI != null)
                    _minigameVisualUI.MostrarFaseAim();
                ActualizarInfo("Apunta a la torre - SPACE para fijar");
                break;

            case CombatData.CombatPhase.Summary:
                if (_minigameVisualUI != null)
                    _minigameVisualUI.OcultarTodo();
                ActualizarInfo("Combate terminado");
                break;

            default:
                break;
        }
    }

    private void OnTurnoCambiado()
    {
        if (_combatManager.DatosCombate == null) return;

        var turno = _combatManager.DatosCombate.TurnoActual;
        ActualizarTurno(turno.ToString());
    }

    private void OnCombateTerminado(CombatData data)
    {
        if (_minigameVisualUI != null)
            _minigameVisualUI.OcultarTodo();

        ActualizarTurno("Terminado");
        string resultado = data.FichasGanadasJugador.Count > data.FichasGanadasNPC.Count ? "Victoria!" : "Derrota";
        ActualizarInfo(resultado);
    }

    private void Update()
    {
        // Detectar cambio de fase del minijuego para activar paneles correctos
        if (_combatManager == null || _combatManager.DatosCombate == null) return;
        if (_combatManager.DatosCombate.FaseActual != CombatData.CombatPhase.ThrowTurn) return;

        // Verificar en qué etapa del minijuego estamos
        var aim = _combatManager.GetComponentInChildren<AimPhase>();
        var force = _combatManager.GetComponentInChildren<ForcePhase>();
        var precision = _combatManager.GetComponentInChildren<PrecisionPhase>();

        if (aim != null && aim.EstaActivo)
        {
            if (_minigameVisualUI != null) _minigameVisualUI.MostrarFaseAim();
            ActualizarInfo("Apunta - SPACE para fijar posicion");
        }
        else if (force != null && force.EstaActivo)
        {
            if (_minigameVisualUI != null) _minigameVisualUI.MostrarFaseFuerza();
            ActualizarInfo("Fuerza - SPACE para fijar");
        }
        else if (precision != null && precision.EstaActivo)
        {
            if (_minigameVisualUI != null) _minigameVisualUI.MostrarFasePrecision();
            ActualizarInfo("Precision - SPACE para fijar");
        }
    }

    private void ActualizarTurno(string turno)
    {
        if (_turnoTexto != null)
            _turnoTexto.text = "Turno: " + turno;
    }

    private void ActualizarInfo(string info)
    {
        if (_infoTexto != null)
            _infoTexto.text = info;
    }
}
