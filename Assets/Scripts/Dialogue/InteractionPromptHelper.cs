using UnityEngine;

namespace Flipit.Dialogue
{
    /// <summary>
    /// Shows/hides an interaction prompt (e.g. "[E] Retar") based on player proximity.
    /// Attach to a child TextMesh of the player.
    /// </summary>
    public class InteractionPromptHelper : MonoBehaviour
    {
        private TextMesh _textMesh;

        private void Awake()
        {
            _textMesh = GetComponent<TextMesh>();
            Hide();
        }

        public void Show(string text = null)
        {
            if (_textMesh != null && text != null)
                _textMesh.text = text;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
