using System;
using System.Collections;
using UnityEngine;

namespace Flipit.Dialogue
{
    /// <summary>
    /// Handles character-by-character text reveal using IDialogueUI.SetVisibleCharacterCount.
    /// Uses a coroutine that increments visible characters each characterDelay seconds.
    /// </summary>
    public class Typewriter_Effect : MonoBehaviour
    {
        [SerializeField, Min(0.01f)]
        private float characterDelay = 0.03f;

        /// <summary>
        /// Fired when all characters have been revealed, either naturally or via SkipToEnd.
        /// </summary>
        public event Action OnTypingComplete;

        private Coroutine _typingCoroutine;
        private IDialogueUI _currentUI;
        private int _totalCharacters;
        private bool _isTyping;

        /// <summary>
        /// Begins revealing text character-by-character on the provided UI.
        /// Sets the full text, resets visible count to 0, and starts the reveal coroutine.
        /// </summary>
        /// <param name="text">The full dialogue line text to reveal.</param>
        /// <param name="ui">The UI implementation to drive character visibility on.</param>
        public void StartTyping(string text, IDialogueUI ui)
        {
            // Stop any existing typing without notification
            Stop();

            _currentUI = ui;
            _totalCharacters = text.Length;

            // Set the full text for layout/sizing, then hide all characters
            _currentUI.SetDialogueText(text);
            _currentUI.SetVisibleCharacterCount(0);

            _isTyping = true;
            _typingCoroutine = StartCoroutine(TypeTextCoroutine());
        }

        /// <summary>
        /// Immediately reveals all remaining characters and fires OnTypingComplete exactly once.
        /// Guards against double-fire if typing has already completed.
        /// </summary>
        public void SkipToEnd()
        {
            if (!_isTyping)
                return;

            StopTypingCoroutine();

            _currentUI.SetVisibleCharacterCount(_totalCharacters);
            _isTyping = false;

            OnTypingComplete?.Invoke();
        }

        /// <summary>
        /// Halts the typing coroutine without firing OnTypingComplete.
        /// </summary>
        public void Stop()
        {
            if (!_isTyping)
                return;

            StopTypingCoroutine();
            _isTyping = false;
        }

        private IEnumerator TypeTextCoroutine()
        {
            var delay = new WaitForSeconds(characterDelay);

            for (int i = 1; i <= _totalCharacters; i++)
            {
                _currentUI.SetVisibleCharacterCount(i);
                yield return delay;
            }

            // Natural completion
            _isTyping = false;
            _typingCoroutine = null;

            OnTypingComplete?.Invoke();
        }

        private void StopTypingCoroutine()
        {
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _typingCoroutine = null;
            }
        }
    }
}
