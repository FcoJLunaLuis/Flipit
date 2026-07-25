using System.Collections.Generic;
using UnityEngine;
using Flipit.Core;

namespace Flipit.NPC
{
    /// <summary>
    /// Visual tester for the Collector NPC system (OnGUI).
    /// Shows NPC state, inventory, wallet, trade offers, and allows quick operations.
    /// Attach to any GameObject in the NPC_Test scene alongside CollectorNPCInitializer.
    /// </summary>
    public class CollectorNPCTester : MonoBehaviour
    {
        [SerializeField] private CollectorNPCManager npcManager;
        [SerializeField] private CollectorNPCUIController uiController;
        [SerializeField] private WalletData walletData;

        private CollectorNPCInitializer initializer;
        private string lastOperationResult = "---";
        private Vector2 scrollPosition;

        private void Start()
        {
            initializer = GetComponent<CollectorNPCInitializer>();
            if (initializer == null)
                initializer = FindObjectOfType<CollectorNPCInitializer>();
        }

        private void OnGUI()
        {
            if (npcManager == null || walletData == null)
            {
                GUI.Label(new Rect(10, 10, 500, 30), "ERROR: References not assigned in CollectorNPCTester!");
                return;
            }

            float x = 10f;
            float y = 10f;
            float colWidth = 320f;
            float buttonWidth = 280f;
            float buttonHeight = 30f;
            float spacing = 5f;

            GUI.skin.label.fontSize = 12;
            GUI.skin.button.fontSize = 11;

            // === COLUMN 1: State & Wallet ===
            GUI.Label(new Rect(x, y, colWidth, 25), "=== NPC COLECCIONISTA TESTER ===");
            y += 30f;

            // UI State
            string stateText = uiController != null ? uiController.CurrentState.ToString() : "N/A";
            GUI.Label(new Rect(x, y, colWidth, 20), $"UI State: {stateText}");
            y += 22f;

            // Wallet
            GUI.Label(new Rect(x, y, colWidth, 20), "--- WALLET ---");
            y += 20f;
            GUI.Label(new Rect(x, y, colWidth, 20), $"  Ajolopesos: {walletData.Ajolopesos}");
            y += 18f;
            GUI.Label(new Rect(x, y, colWidth, 20), $"  Pejecoins: {walletData.Pejecoins}");
            y += 18f;
            GUI.Label(new Rect(x, y, colWidth, 20), $"  Sheintavos: {walletData.Sheintavos}");
            y += 18f;
            GUI.Label(new Rect(x, y, colWidth, 20), $"  Total: {walletData.GetTotalInSheintavos()} sheintavos");
            y += 25f;

            // Cycle Prices
            GUI.Label(new Rect(x, y, colWidth, 20), "--- PRECIOS CICLO ---");
            y += 20f;
            GUI.Label(new Rect(x, y, colWidth, 20), $"  Common: {npcManager.GetCycleSellPrice(ChipRarity.Common)} sht");
            y += 18f;
            GUI.Label(new Rect(x, y, colWidth, 20), $"  Rare: {npcManager.GetCycleSellPrice(ChipRarity.Rare)} sht");
            y += 18f;
            GUI.Label(new Rect(x, y, colWidth, 20), $"  UltraRare: {npcManager.GetCycleSellPrice(ChipRarity.UltraRare)} sht");
            y += 25f;

            // Actions
            GUI.Label(new Rect(x, y, colWidth, 20), "--- ACCIONES ---");
            y += 22f;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Refresh Offers"))
            {
                npcManager.RefreshOffers();
                lastOperationResult = "Ofertas refrescadas";
            }
            y += buttonHeight + spacing;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Refresh Cycle Prices"))
            {
                npcManager.RefreshCyclePrices();
                lastOperationResult = "Precios de ciclo refrescados";
            }
            y += buttonHeight + spacing;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Add 100 Sheintavos"))
            {
                walletData.Add(100, 0, 0);
                lastOperationResult = "+100 Sheintavos";
            }
            y += buttonHeight + spacing;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Open NPC Menu"))
            {
                if (uiController != null)
                {
                    uiController.OpenMenu();
                    lastOperationResult = "Menu abierto";
                }
            }
            y += buttonHeight + spacing + 10f;

            GUI.Label(new Rect(x, y, 500, 20), $"Ultima operacion: {lastOperationResult}");
            y += 25f;

            // === COLUMN 2: Inventory ===
            float col2X = x + colWidth + 20f;
            float col2Y = 10f;

            GUI.Label(new Rect(col2X, col2Y, colWidth, 25), "--- INVENTARIO ---");
            col2Y += 25f;

            IChipInventory inventory = npcManager.GetInventory();
            IChipCatalog catalog = npcManager.GetCatalog();

            if (inventory != null)
            {
                var owned = inventory.GetOwnedChipIds();
                for (int i = 0; i < owned.Count && i < 15; i++)
                {
                    string chipId = owned[i];
                    int count = inventory.GetChipCount(chipId);
                    string rarity = catalog != null ? catalog.GetChipRarity(chipId).ToString().Substring(0, 1) : "?";
                    GUI.Label(new Rect(col2X, col2Y, colWidth, 18), $"  [{rarity}] {chipId}: x{count}");
                    col2Y += 18f;
                }

                // Quick sell buttons
                col2Y += 10f;
                if (owned.Count > 0)
                {
                    string firstChip = owned[0];
                    if (GUI.Button(new Rect(col2X, col2Y, buttonWidth, buttonHeight), $"Vender 1x {firstChip}"))
                    {
                        var result = npcManager.TrySell(firstChip, 1);
                        lastOperationResult = result.Success
                            ? $"Vendido 1x {firstChip} por {result.MoneyEarned} sht"
                            : $"FALLO: {result.FailReason}";
                    }
                    col2Y += buttonHeight + spacing;
                }
            }
            else
            {
                GUI.Label(new Rect(col2X, col2Y, colWidth, 20), "  (inventario no inicializado)");
            }

            // === COLUMN 3: Trade Offers ===
            float col3X = col2X + colWidth + 20f;
            float col3Y = 10f;

            GUI.Label(new Rect(col3X, col3Y, colWidth + 100, 25), "--- OFERTAS DE INTERCAMBIO ---");
            col3Y += 25f;

            var offers = npcManager.CurrentOffers;
            if (offers != null)
            {
                for (int i = 0; i < offers.Length; i++)
                {
                    var offer = offers[i];
                    bool canAfford = npcManager.CanAffordTrade(offer);
                    string affordText = canAfford ? "[OK]" : "[X]";
                    string rarityStr = offer.OfferedRarity.ToString().Substring(0, 1);

                    GUI.Label(new Rect(col3X, col3Y, colWidth + 100, 18),
                        $"  {affordText} [{rarityStr}] {offer.OfferedChipId}");
                    col3Y += 18f;

                    // Show requirements
                    for (int j = 0; j < offer.Requirements.Length; j++)
                    {
                        var req = offer.Requirements[j];
                        int playerHas = inventory != null ? inventory.GetChipCount(req.RequiredChipId) : 0;
                        string hasEnough = playerHas >= req.RequiredAmount ? "v" : "x";
                        GUI.Label(new Rect(col3X + 20, col3Y, colWidth + 80, 18),
                            $"    ({hasEnough}) Requiere: {req.RequiredAmount}x {req.RequiredChipId} (tienes {playerHas})");
                        col3Y += 16f;
                    }

                    // Trade button
                    if (canAfford)
                    {
                        if (GUI.Button(new Rect(col3X + 20, col3Y, 200, 22), $"Intercambiar #{i + 1}"))
                        {
                            var result = npcManager.TryTrade(offer);
                            lastOperationResult = result.Success
                                ? $"Intercambiado! Obtenido: {result.ChipObtained}"
                                : $"FALLO: {result.FailReason}";
                            npcManager.RefreshOffers();
                        }
                        col3Y += 24f;
                    }

                    col3Y += 8f;
                }
            }
            else
            {
                GUI.Label(new Rect(col3X, col3Y, colWidth, 20), "  (no hay ofertas)");
            }
        }
    }
}
