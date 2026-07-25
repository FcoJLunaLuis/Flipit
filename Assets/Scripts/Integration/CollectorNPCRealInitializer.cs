using UnityEngine;
using Flipit.NPC;

/// <summary>
/// Initializes the Collector NPC with real Album data instead of mocks.
/// Replaces CollectorNPCInitializer for production use.
/// Requires AlbumManager and ShopAlbumBridge to be present in the scene.
/// Execution order 100 ensures AlbumManager (default) and Bootstrap (50) run first.
/// </summary>
[DefaultExecutionOrder(100)]
public class CollectorNPCRealInitializer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CollectorNPCManager npcManager;
    [SerializeField] private FichaTemplate[] allFichaTemplates;

    private AlbumChipInventory inventory;
    private AlbumChipCatalog catalog;

    private void Start()
    {
        if (npcManager == null)
        {
            npcManager = GetComponent<CollectorNPCManager>();
        }

        if (npcManager == null)
        {
            Debug.LogError("[CollectorNPCRealInitializer] CollectorNPCManager not found!");
            return;
        }

        if (AlbumManager.Instance == null)
        {
            Debug.LogError("[CollectorNPCRealInitializer] AlbumManager.Instance not found! Make sure AlbumManager exists in the scene.");
            return;
        }

        InitializeWithRealData();
    }

    private void InitializeWithRealData()
    {
        // Get album data from AlbumManager
        AlbumData albumData = AlbumManager.Instance.ObtenerAlbumData();

        // Create catalog adapter from FichaTemplates
        catalog = new AlbumChipCatalog(allFichaTemplates);

        // Create inventory adapter from AlbumData
        inventory = new AlbumChipInventory(albumData, catalog);

        // Initialize NPC with real adapters
        npcManager.Initialize(inventory, catalog);

        Debug.Log($"[CollectorNPCRealInitializer] NPC initialized with real data.");
        Debug.Log($"  - Catalog: {catalog.GetAllChipIds().Count} fichas");
        Debug.Log($"  - Inventory: {inventory.GetOwnedChipIds().Count} tipos de fichas poseidas");
        Debug.Log($"  - Trade offers: {npcManager.CurrentOffers.Length}");
    }

    /// <summary>
    /// Gets the catalog adapter (for external use if needed).
    /// </summary>
    public AlbumChipCatalog GetCatalog() => catalog;

    /// <summary>
    /// Gets the inventory adapter (for external use if needed).
    /// </summary>
    public AlbumChipInventory GetInventory() => inventory;
}
