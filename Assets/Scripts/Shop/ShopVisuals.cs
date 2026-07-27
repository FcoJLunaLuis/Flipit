using UnityEngine;
using Flipit.Core;

namespace Flipit.Shop
{
    public class ShopVisuals : MonoBehaviour
    {
        [SerializeField] private ShopManager shopManager;
        [SerializeField] private ShopUIController uiController;

        private PurchaseResult currentResult;
        private BagConfig currentConfirmBag;
        private string statusMessage = "";

        private void OnEnable()
        {
            if (uiController != null)
            {
                uiController.OnConfirmationShown += HandleConfirmationShown;
                uiController.OnConfirmationClosed += HandleConfirmationClosed;
                uiController.OnResultsShown += HandleResultsShown;
            }
        }

        private void OnDisable()
        {
            if (uiController != null)
            {
                uiController.OnConfirmationShown -= HandleConfirmationShown;
                uiController.OnConfirmationClosed -= HandleConfirmationClosed;
                uiController.OnResultsShown -= HandleResultsShown;
            }
        }

        private void HandleConfirmationShown(BagConfig config)
        {
            currentConfirmBag = config;
        }

        private void HandleConfirmationClosed()
        {
            currentConfirmBag = null;
        }

        private void HandleResultsShown(PurchaseResult result)
        {
            currentResult = result;
        }

        private void OnGUI()
        {
            if (shopManager == null || uiController == null) return;

            float x = Screen.width / 2f - 150f;
            float y = 20f;

            // Title
            GUI.skin.label.fontSize = 20;
            GUI.Label(new Rect(x, y, 400, 30), "=== TIENDA DE BOLSAS ===");
            y += 40f;

            GUI.skin.label.fontSize = 14;

            // Show wallet balance
            DrawWalletBalance(ref x, ref y);

            y += 10f;

            // Draw bag grid or panels
            if (uiController.IsResultsOpen)
            {
                DrawResultsPanel(ref x, ref y);
            }
            else if (uiController.IsConfirmationOpen)
            {
                DrawConfirmationPanel(ref x, ref y);
            }
            else
            {
                DrawBagGrid(ref x, ref y);
            }

            // Exit button
            y = Screen.height - 60f;
            float exitBtnWidth = 200f;
            float exitBtnX = (Screen.width - exitBtnWidth) / 2f;
            if (GUI.Button(new Rect(exitBtnX, y, exitBtnWidth, 40f), "SALIR DE LA TIENDA"))
            {
                uiController.RequestExit();
            }

            // Status message
            if (!string.IsNullOrEmpty(statusMessage))
            {
                GUI.Label(new Rect(x, Screen.height - 100f, 500, 25), statusMessage);
            }
        }

        private void DrawWalletBalance(ref float x, ref float y)
        {
            GUI.Label(new Rect(x, y, 400, 25), "Tu dinero:");
            y += 22f;
            // Access wallet through reflection or shopManager
            // For now, show via ShopManager's wallet reference
            GUI.Label(new Rect(x + 10, y, 400, 20), $"Usa los botones del WalletTester para agregar dinero");
            y += 25f;
        }

        private void DrawBagGrid(ref float x, ref float y)
        {
            GUI.Label(new Rect(x, y, 400, 25), "Selecciona una bolsa (WASD + Space):");
            y += 25f;

            int tierCount = shopManager.TierCount;
            int bagsPerTier = shopManager.GetBagsPerTier();

            for (int row = 0; row < tierCount; row++)
            {
                var config = shopManager.GetBagConfig(row);
                if (config == null) continue;

                string tierLabel = config.BagName + $" ({config.CostPejecoins} PC)";
                GUI.Label(new Rect(x, y, 300, 20), tierLabel);
                y += 22f;

                for (int col = 0; col < bagsPerTier; col++)
                {
                    float btnX = x + col * 55f;
                    bool isSelected = (row == uiController.CurrentRow && col == uiController.CurrentColumn);
                    bool canAfford = shopManager.CanAffordBag(row);

                    string label = isSelected ? "[X]" : "[ ]";
                    Color originalColor = GUI.backgroundColor;

                    if (isSelected)
                        GUI.backgroundColor = Color.yellow;
                    else if (!canAfford)
                        GUI.backgroundColor = Color.gray;
                    else
                        GUI.backgroundColor = GetTierColor(config.Tier);

                    GUI.Box(new Rect(btnX, y, 50f, 35f), label);
                    GUI.backgroundColor = originalColor;
                }
                y += 42f;
            }

            y += 10f;
            GUI.Label(new Rect(x, y, 500, 20), "Space=Comprar | Escape=Salir");
        }

        private void DrawConfirmationPanel(ref float x, ref float y)
        {
            if (currentConfirmBag == null) return;

            float panelX = Screen.width / 2f - 150f;
            float panelY = Screen.height / 2f - 80f;

            GUI.Box(new Rect(panelX - 10, panelY - 10, 320, 180), "");

            GUI.Label(new Rect(panelX, panelY, 300, 25), "--- CONFIRMAR COMPRA ---");
            panelY += 30f;

            GUI.Label(new Rect(panelX, panelY, 300, 25), $"Bolsa: {currentConfirmBag.BagName}");
            panelY += 22f;

            string priceStr = FormatPrice(currentConfirmBag);
            GUI.Label(new Rect(panelX, panelY, 300, 25), $"Precio: {priceStr}");
            panelY += 22f;

            GUI.Label(new Rect(panelX, panelY, 300, 25), $"Fichas: {currentConfirmBag.MinChips}-{currentConfirmBag.MaxChips}");
            panelY += 30f;

            bool canAfford = shopManager.CanAffordBag(uiController.CurrentRow);
            if (canAfford)
            {
                GUI.Label(new Rect(panelX, panelY, 300, 25), "Space = Confirmar | Escape = Cancelar");
            }
            else
            {
                GUI.Label(new Rect(panelX, panelY, 300, 25), "NO TIENES SUFICIENTE DINERO");
                panelY += 22f;
                GUI.Label(new Rect(panelX, panelY, 300, 25), "Escape = Cancelar");
            }
        }

        private void DrawResultsPanel(ref float x, ref float y)
        {
            float panelX = Screen.width / 2f - 150f;
            float panelY = Screen.height / 2f - 100f;

            GUI.Box(new Rect(panelX - 10, panelY - 10, 320, 250), "");

            if (currentResult == null) return;

            if (currentResult.Success)
            {
                GUI.Label(new Rect(panelX, panelY, 300, 25), "--- FICHAS OBTENIDAS ---");
                panelY += 30f;

                foreach (var chip in currentResult.Chips)
                {
                    string rarityStr = GetRarityLabel(chip.Rarity);
                    Color originalColor = GUI.contentColor;
                    GUI.contentColor = GetRarityColor(chip.Rarity);
                    GUI.Label(new Rect(panelX + 10, panelY, 300, 22), $"* {rarityStr}");
                    GUI.contentColor = originalColor;
                    panelY += 22f;
                }

                panelY += 15f;
                GUI.Label(new Rect(panelX, panelY, 300, 25), "Space = Cerrar");
            }
            else
            {
                GUI.Label(new Rect(panelX, panelY, 300, 25), "--- COMPRA FALLIDA ---");
                panelY += 30f;
                GUI.Label(new Rect(panelX, panelY, 300, 25), currentResult.FailReason);
                panelY += 30f;
                GUI.Label(new Rect(panelX, panelY, 300, 25), "Space = Cerrar");
            }
        }

        private string FormatPrice(BagConfig config)
        {
            if (config.CostAjolopesos > 0)
                return $"{config.CostAjolopesos} Ajolopesos";
            if (config.CostPejecoins > 0)
                return $"{config.CostPejecoins} Pejecoins";
            return $"{config.CostSheintavos} Sheintavos";
        }

        private string GetRarityLabel(ChipRarity rarity)
        {
            switch (rarity)
            {
                case ChipRarity.Common: return "Ficha Comun";
                case ChipRarity.Rare: return "Ficha Rara";
                case ChipRarity.UltraRare: return "Ficha Ultra Rara!";
                default: return "???";
            }
        }

        private Color GetRarityColor(ChipRarity rarity)
        {
            switch (rarity)
            {
                case ChipRarity.Common: return Color.white;
                case ChipRarity.Rare: return Color.cyan;
                case ChipRarity.UltraRare: return Color.magenta;
                default: return Color.white;
            }
        }

        private Color GetTierColor(BagTier tier)
        {
            switch (tier)
            {
                case BagTier.Green: return Color.green;
                case BagTier.Red: return Color.red;
                case BagTier.White: return Color.white;
                default: return Color.gray;
            }
        }
    }
}