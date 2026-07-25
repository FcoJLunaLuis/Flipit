using UnityEngine;
using Flipit.Core;

/// <summary>
/// Bootstrap script for the Integration Test scene.
/// Pre-loads the album with some chips and initializes the wallet
/// so there's data to work with when testing the full flow.
/// 
/// Execution order: runs after AlbumManager but before CollectorNPCRealInitializer.
/// </summary>
[DefaultExecutionOrder(50)]
public class IntegrationTestBootstrap : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WalletData walletData;
    [SerializeField] private FichaTemplate[] fichasToPreload;
    [SerializeField] private ShopAlbumBridge shopAlbumBridge;

    [Header("Test Configuration")]
    [Tooltip("How many of each preloaded ficha to add.")]
    [SerializeField] private int copiesPerFicha = 3;

    private void Start()
    {
        InitializeWallet();
        PreloadAlbum();
        LogStatus();
    }

    private void InitializeWallet()
    {
        if (walletData != null)
        {
            walletData.InitializeNewGame(); // Reset + 25 Pejecoins
            walletData.Add(500, 10, 0); // Extra money for testing
            Debug.Log($"[IntegrationTestBootstrap] Wallet initialized: {walletData.Ajolopesos} Ajp | {walletData.Pejecoins} Pjc | {walletData.Sheintavos} Sht");
        }
    }

    private void PreloadAlbum()
    {
        if (AlbumManager.Instance == null || fichasToPreload == null)
        {
            Debug.LogWarning("[IntegrationTestBootstrap] AlbumManager or fichas not available for preload.");
            return;
        }

        foreach (var template in fichasToPreload)
        {
            if (template == null) continue;

            for (int i = 0; i < copiesPerFicha; i++)
            {
                AlbumManager.Instance.AgregarFicha(template);
            }
        }

        var albumData = AlbumManager.Instance.ObtenerAlbumData();
        Debug.Log($"[IntegrationTestBootstrap] Album preloaded: {albumData.ObtenerTotalFichasUnicas()} unique, {albumData.ObtenerTotalFichas()} total");
    }

    /// <summary>
    /// Simulates buying a bag from the shop and adding results to album.
    /// Call from OnGUI tester or externally.
    /// </summary>
    public void SimulateBagPurchase()
    {
        if (shopAlbumBridge == null)
        {
            Debug.LogWarning("[IntegrationTestBootstrap] ShopAlbumBridge not assigned.");
            return;
        }

        // Simulate a purchase result with random rarities
        var chips = new ChipResult[]
        {
            new ChipResult(ChipRarity.Common),
            new ChipResult(ChipRarity.Common),
            new ChipResult(ChipRarity.Rare)
        };

        var result = PurchaseResult.Succeeded(chips);
        shopAlbumBridge.ProcessPurchaseResult(result);
        Debug.Log("[IntegrationTestBootstrap] Simulated bag purchase: 2 Common + 1 Rare added to album.");
    }

    private void LogStatus()
    {
        Debug.Log("=== INTEGRATION TEST READY ===");
        Debug.Log("  Click the green cube to open NPC Collector.");
        Debug.Log("  Use the OnGUI tester to sell/trade/simulate purchases.");
        Debug.Log("  All systems connected: Wallet <-> Pause, Shop <-> Album, Album <-> NPC");
    }
}
