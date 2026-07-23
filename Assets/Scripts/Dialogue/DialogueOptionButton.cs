using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Flipit.Dialogue
{
    /// <summary>
    /// A reusable component for each option button in the dialogue choice UI.
    /// Displays option text and manages visual highlight state.
    /// </summary>
    public class DialogueOptionButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private Image highlightImage;

        /// <summary>
        /// Configures the button with display text and initial highlight state.
        /// </summary>
        /// <param name="text">The option text to display.</param>
        /// <param name="highlighted">Whether this option should be visually highlighted.</param>
        public void Setup(string text, bool highlighted)
        {
            labelText.text = text;
            SetHighlighted(highlighted);
        }

        /// <summary>
        /// Sets the visual highlight state of this option button.
        /// </summary>
        /// <param name="highlighted">Whether to show the highlight.</param>
        public void SetHighlighted(bool highlighted)
        {
            highlightImage.enabled = highlighted;
        }
    }
}
