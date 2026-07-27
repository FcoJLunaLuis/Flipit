using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Flipit.Core;

namespace Flipit.Shop
{
    /// <summary>
    /// Canvas-based visual controller for the shop UI.
    /// Reads serialized references to prefab elements and updates them
    /// based on ShopUIController events.
    /// 
    /// Bag button colors are defined in the prefab and NEVER modified by code.
    /// Selection highlight uses scale change.
    /// </summary>
    public class ShopCanvasController : MonoBehaviour
    {
        [Header("Controller Reference")]
        [SerializeField] private ShopUIController _uiController;
        [SerializeField] private ShopManager _shopManager;

        [Header("Texts")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _walletText;
        [SerializeField] private TMP_Text _hintText;
        [SerializeField] private TMP_Text _confirmMsg;
        [SerializeField] private TMP_Text _resultsMsg;

        [Header("Panels")]
        [SerializeField] private GameObject _confirmPanel;
        [SerializeField] private GameObject _resultsPanel;

        [Header("Buttons")]
        [SerializeField] private Button _btnSalir;
        [SerializeField] private Button _btnComprar;
        [SerializeField] private Button _btnCancelar;
        [SerializeField] private Button _btnCerrar;

        [Header("Bag Buttons (3 rows x 5 columns = 15, row-major order)")]
        [Tooltip("Assign 15 buttons: [0-4] = Row 0 (Green), [5-9] = Row 1 (White), [10-14] = Row 2 (Red)")]
        [SerializeField] private Button[] _bagButtons;

        [Header("Selection Highlight")]
        [SerializeField] private float _highlightScale = 1.2f;

        private const int Columns = 5;
        private int _previousRow = -1;
        private int _previousCol = -1;

        private void OnEnable()
        {
            WireButtons();
            SubscribeEvents();
            InitializeVisuals();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void WireButtons()
        {
            if (_btnSalir != null)
                _btnSalir.onClick.AddListener(HandleExitClick);
            if (_btnComprar != null)
                _btnComprar.onClick.AddListener(HandleBuyClick);
            if (_btnCancelar != null)
                _btnCancelar.onClick.AddListener(HandleCancelClick);
            if (_btnCerrar != null)
                _btnCerrar.onClick.AddListener(HandleCloseResultsClick);

            // Wire bag button clicks
            if (_bagButtons != null)
            {
                for (int i = 0; i < _bagButtons.Length; i++)
                {
                    if (_bagButtons[i] == null) continue;
                    int row = i / Columns;
                    int col = i % Columns;
                    _bagButtons[i].onClick.AddListener(() => HandleBagClick(row, col));
                }
            }
        }

        private void SubscribeEvents()
        {
            if (_uiController == null) return;

            _uiController.OnCursorMoved += HandleCursorMoved;
            _uiController.OnConfirmationShown += HandleConfirmationShown;
            _uiController.OnConfirmationClosed += HandleConfirmationClosed;
            _uiController.OnResultsShown += HandleResultsShown;
        }

        private void UnsubscribeEvents()
        {
            if (_uiController == null) return;

            _uiController.OnCursorMoved -= HandleCursorMoved;
            _uiController.OnConfirmationShown -= HandleConfirmationShown;
            _uiController.OnConfirmationClosed -= HandleConfirmationClosed;
            _uiController.OnResultsShown -= HandleResultsShown;

            if (_btnSalir != null) _btnSalir.onClick.RemoveListener(HandleExitClick);
            if (_btnComprar != null) _btnComprar.onClick.RemoveListener(HandleBuyClick);
            if (_btnCancelar != null) _btnCancelar.onClick.RemoveListener(HandleCancelClick);
            if (_btnCerrar != null) _btnCerrar.onClick.RemoveListener(HandleCloseResultsClick);
        }

        private void InitializeVisuals()
        {
            if (_confirmPanel != null) _confirmPanel.SetActive(false);
            if (_resultsPanel != null) _resultsPanel.SetActive(false);

            if (_titleText != null)
                _titleText.text = "TIENDA DE BOLSAS";

            if (_hintText != null)
                _hintText.text = "WASD Navegar | Space Comprar | Esc Salir";

            UpdateWalletDisplay();

            // Highlight initial position
            SetBagHighlight(0, 0, true);
            _previousRow = 0;
            _previousCol = 0;
        }

        // ─── Event Handlers ─────────────────────────────────────────────────────

        private void HandleCursorMoved(int row, int col)
        {
            if (_previousRow >= 0 && _previousCol >= 0)
                SetBagHighlight(_previousRow, _previousCol, false);

            SetBagHighlight(row, col, true);
            _previousRow = row;
            _previousCol = col;
        }

        private void HandleConfirmationShown(BagConfig config)
        {
            if (_confirmPanel != null)
                _confirmPanel.SetActive(true);

            if (_confirmMsg != null)
            {
                bool canAfford = _shopManager != null && _shopManager.CanAffordBag(_uiController.CurrentRow);

                string text = $"Bolsa: {config.BagName}\n" +
                              $"Precio: {FormatPrice(config)}\n" +
                              $"Fichas: {config.MinChips}-{config.MaxChips}";

                if (!canAfford)
                    text += "\n\n<color=red>NO TIENES SUFICIENTE DINERO</color>";

                _confirmMsg.text = text;
            }

            if (_btnComprar != null && _shopManager != null)
                _btnComprar.interactable = _shopManager.CanAffordBag(_uiController.CurrentRow);
        }

        private void HandleConfirmationClosed()
        {
            if (_confirmPanel != null)
                _confirmPanel.SetActive(false);

            UpdateWalletDisplay();
        }

        private void HandleResultsShown(PurchaseResult result)
        {
            if (_confirmPanel != null)
                _confirmPanel.SetActive(false);

            if (_resultsPanel != null)
                _resultsPanel.SetActive(true);

            if (_resultsMsg != null && result != null)
            {
                if (result.Success)
                {
                    string text = "<b>FICHAS OBTENIDAS</b>\n\n";
                    foreach (var chip in result.Chips)
                    {
                        text += $"<color={GetRarityColorHex(chip.Rarity)}>\u2022 {GetRarityLabel(chip.Rarity)}</color>\n";
                    }
                    _resultsMsg.text = text;
                }
                else
                {
                    _resultsMsg.text = $"<b>COMPRA FALLIDA</b>\n\n{result.FailReason}";
                }
            }

            UpdateWalletDisplay();
        }

        // ─── Button Click Handlers ──────────────────────────────────────────────

        private void HandleExitClick()
        {
            if (_uiController != null)
                _uiController.RequestExit();
        }

        private void HandleBuyClick()
        {
            if (_uiController != null)
                _uiController.ConfirmPurchaseFromUI();
        }

        private void HandleCancelClick()
        {
            if (_uiController != null)
                _uiController.CancelConfirmationFromUI();
        }

        private void HandleCloseResultsClick()
        {
            if (_uiController != null)
                _uiController.CloseResultsFromUI();
        }

        private void HandleBagClick(int row, int col)
        {
            if (_uiController != null)
                _uiController.SelectBagFromUI(row, col);
        }

        // ─── Highlight (scale-based, never touches color) ───────────────────────

        private void SetBagHighlight(int row, int col, bool highlighted)
        {
            int index = row * Columns + col;
            if (_bagButtons == null || index < 0 || index >= _bagButtons.Length) return;

            var btn = _bagButtons[index];
            if (btn == null) return;

            btn.transform.localScale = highlighted
                ? Vector3.one * _highlightScale
                : Vector3.one;
        }

        // ─── Wallet Display ─────────────────────────────────────────────────────

        private void UpdateWalletDisplay()
        {
            if (_walletText == null || _shopManager == null) return;

            var walletField = typeof(ShopManager).GetField("wallet",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (walletField != null)
            {
                var wallet = walletField.GetValue(_shopManager) as WalletData;
                if (wallet != null)
                {
                    _walletText.text = $"Sheintavos: {wallet.Sheintavos} | Pejecoins: {wallet.Pejecoins} | Ajolopesos: {wallet.Ajolopesos}";
                    return;
                }
            }

            _walletText.text = "";
        }

        // ─── Helpers ────────────────────────────────────────────────────────────

        private string FormatPrice(BagConfig config)
        {
            if (config.CostAjolopesos > 0) return $"{config.CostAjolopesos} Ajolopesos";
            if (config.CostPejecoins > 0) return $"{config.CostPejecoins} Pejecoins";
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

        private string GetRarityColorHex(ChipRarity rarity)
        {
            switch (rarity)
            {
                case ChipRarity.Common: return "#FFFFFF";
                case ChipRarity.Rare: return "#00FFFF";
                case ChipRarity.UltraRare: return "#FF00FF";
                default: return "#FFFFFF";
            }
        }
    }
}
