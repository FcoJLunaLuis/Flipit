using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Flipit.NPC
{
    /// <summary>
    /// Provides visual feedback for the NPC menu navigation.
    /// Highlights the currently selected option by changing button colors.
    /// Attach to the same GameObject as CollectorNPCUIController.
    /// </summary>
    public class NPCMenuVisualFeedback : MonoBehaviour
    {
        [Header("Main Menu Buttons")]
        [SerializeField] private Image sellButtonImage;
        [SerializeField] private Image tradeButtonImage;

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.3f, 1f);
        [SerializeField] private Color selectedColor = new Color(0.3f, 0.5f, 0.8f, 1f);

        [Header("Selection Indicator")]
        [SerializeField] private string selectedPrefix = "> ";
        [SerializeField] private string normalPrefix = "  ";

        private CollectorNPCUIController uiController;
        private TMPro.TextMeshProUGUI sellText;
        private TMPro.TextMeshProUGUI tradeText;

        private void Awake()
        {
            uiController = GetComponent<CollectorNPCUIController>();

            if (sellButtonImage != null)
                sellText = sellButtonImage.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tradeButtonImage != null)
                tradeText = tradeButtonImage.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            if (uiController != null)
            {
                uiController.OnMainMenuCursorMoved += UpdateMainMenuHighlight;
                uiController.OnMenuOpened += HandleMenuOpened;
                uiController.OnMenuClosed += HandleMenuClosed;
            }
        }

        private void OnDisable()
        {
            if (uiController != null)
            {
                uiController.OnMainMenuCursorMoved -= UpdateMainMenuHighlight;
                uiController.OnMenuOpened -= HandleMenuOpened;
                uiController.OnMenuClosed -= HandleMenuClosed;
            }
        }

        private void HandleMenuOpened()
        {
            UpdateMainMenuHighlight(0);
        }

        private void HandleMenuClosed()
        {
            ResetAllHighlights();
        }

        private void UpdateMainMenuHighlight(int selectedIndex)
        {
            // Sell button (index 0)
            if (sellButtonImage != null)
                sellButtonImage.color = (selectedIndex == 0) ? selectedColor : normalColor;
            if (sellText != null)
                sellText.text = (selectedIndex == 0) ? selectedPrefix + "VENDER" : normalPrefix + "VENDER";

            // Trade button (index 1)
            if (tradeButtonImage != null)
                tradeButtonImage.color = (selectedIndex == 1) ? selectedColor : normalColor;
            if (tradeText != null)
                tradeText.text = (selectedIndex == 1) ? selectedPrefix + "INTERCAMBIAR" : normalPrefix + "INTERCAMBIAR";
        }

        private void ResetAllHighlights()
        {
            if (sellButtonImage != null)
                sellButtonImage.color = normalColor;
            if (tradeButtonImage != null)
                tradeButtonImage.color = normalColor;

            if (sellText != null)
                sellText.text = normalPrefix + "VENDER";
            if (tradeText != null)
                tradeText.text = normalPrefix + "INTERCAMBIAR";
        }
    }
}
