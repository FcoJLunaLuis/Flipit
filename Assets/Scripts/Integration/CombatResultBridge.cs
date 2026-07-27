using UnityEngine;

/// <summary>
/// Bridges combat results to the album and shows the CombatSummaryUI.
/// When combat ends:
/// 1. Calls CombatResultApplier.AplicarResultados() to update the album correctly
/// 2. Finds and shows CombatSummaryUI with the results
/// 3. CombatSummaryUI's "Continuar" button calls SalirDelCombate() to return to exploration
/// 
/// Subscribes to CombatManager.OnCombateTerminado.
/// Lives in Assembly-CSharp (Integration folder).
/// </summary>
public class CombatResultBridge : MonoBehaviour
{
    private CombatSummaryUI _summaryUI;
    private CombatConfig _combatConfig;

    private void Start()
    {
        CacheReferences();
        SubscribeToCombat();
    }

    private void OnEnable()
    {
        SubscribeToCombat();
    }

    private void OnDisable()
    {
        UnsubscribeFromCombat();
    }

    private void OnDestroy()
    {
        UnsubscribeFromCombat();
    }

    private void CacheReferences()
    {
        // Find CombatSummaryUI (may be on an inactive child of the combat system)
        if (_summaryUI == null)
            _summaryUI = FindAnyObjectByType<CombatSummaryUI>(FindObjectsInactive.Include);

        // Get CombatConfig from CombatManager
        if (_combatConfig == null && CombatManager.Instance != null)
            _combatConfig = CombatManager.Instance.Config;
    }

    private void SubscribeToCombat()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombateTerminado -= OnCombateTerminado;
            CombatManager.Instance.OnCombateTerminado += OnCombateTerminado;
        }
    }

    private void UnsubscribeFromCombat()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombateTerminado -= OnCombateTerminado;
        }
    }

    /// <summary>
    /// Called when combat ends. Applies results to album and shows summary UI.
    /// </summary>
    private void OnCombateTerminado(CombatData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[CombatResultBridge] CombatData is null.");
            FallbackExit();
            return;
        }

        // Ensure references are cached
        CacheReferences();

        if (AlbumManager.Instance == null)
        {
            Debug.LogWarning("[CombatResultBridge] AlbumManager.Instance is null. Cannot update album.");
            FallbackExit();
            return;
        }

        AlbumData albumData = AlbumManager.Instance.ObtenerAlbumData();
        if (albumData == null)
        {
            Debug.LogWarning("[CombatResultBridge] AlbumData is null.");
            FallbackExit();
            return;
        }

        if (_combatConfig == null)
        {
            Debug.LogWarning("[CombatResultBridge] CombatConfig is null. Using fallback exit.");
            FallbackExit();
            return;
        }

        // Apply results using the centralized applier (handles ownership filtering)
        var summary = CombatResultApplier.AplicarResultados(data, albumData, _combatConfig);

        // Show summary UI
        if (_summaryUI != null)
        {
            _summaryUI.Mostrar(summary, CombatManager.Instance);
            Debug.Log("[CombatResultBridge] Combat summary UI shown. Waiting for player to dismiss.");
        }
        else
        {
            Debug.LogWarning("[CombatResultBridge] CombatSummaryUI not found. Exiting combat directly.");
            FallbackExit();
        }
    }

    /// <summary>
    /// Fallback: exit combat directly if summary UI cannot be shown.
    /// </summary>
    private void FallbackExit()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.SalirDelCombate();
        }
    }
}
