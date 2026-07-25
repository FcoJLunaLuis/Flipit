using UnityEngine;

/// <summary>
/// Bloquea la pausa durante el combate y la desbloquea al terminar.
/// Similar a NPCPauseBridge pero para el sistema de combate.
/// </summary>
public class CombatPauseBridge : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PauseManager _pauseManager;

    private void OnEnable()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombateIniciado += HandleCombateIniciado;
            CombatManager.Instance.OnCombateTerminado += HandleCombateTerminado;
        }
    }

    private void OnDisable()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombateIniciado -= HandleCombateIniciado;
            CombatManager.Instance.OnCombateTerminado -= HandleCombateTerminado;
        }
    }

    private void HandleCombateIniciado()
    {
        if (_pauseManager != null)
        {
            _pauseManager.BlockPause();
        }
    }

    private void HandleCombateTerminado(CombatData data)
    {
        if (_pauseManager != null)
        {
            _pauseManager.UnblockPause();
        }
    }
}
