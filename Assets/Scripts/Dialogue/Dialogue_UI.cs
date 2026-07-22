using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Flipit.Dialogue
{
    /// <summary>
    /// Concrete Canvas-based uGUI implementation of IDialogueUI using TextMeshPro.
    /// Manages dialogue display, option buttons, and choice navigation with wrapping highlight.
    /// </summary>
    public class Dialogue_UI : MonoBehaviour, IDialogueUI
    {
        [SerializeField] private Canvas dialogueCanvas;
        [SerializeField] private TMP_Text speakerNameText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private GameObject advanceIndicator;
        [SerializeField] private GameObject typingIndicator;
        [SerializeField] private DialogueOptionButton[] optionButtons; // max 4

        private int _highlightedIndex;
        private int _activeOptionCount;
        private Action<int> _onOptionSelected;

        /// <summary>
        /// The currently highlighted option index. Exposed for testing.
        /// </summary>
        public int HighlightedIndex => _highlightedIndex;

        /// <summary>
        /// The number of currently active options. Exposed for testing.
        /// </summary>
        public int ActiveOptionCount => _activeOptionCount;

        public void Show()
        {
            if (dialogueCanvas != null)
                dialogueCanvas.enabled = true;
        }

        public void Hide(Action onComplete)
        {
            if (dialogueCanvas != null)
                dialogueCanvas.enabled = false;

            onComplete?.Invoke();
        }

        public void SetSpeakerName(string speakerName)
        {
            if (speakerNameText != null)
                speakerNameText.text = speakerName;
        }

        public void SetDialogueText(string text)
        {
            if (dialogueText != null)
                dialogueText.text = text;
        }

        public void SetVisibleCharacterCount(int count)
        {
            if (dialogueText != null)
                dialogueText.maxVisibleCharacters = count;
        }

        public void ShowAdvanceIndicator(bool visible)
        {
            if (advanceIndicator != null)
                advanceIndicator.SetActive(visible);
        }

        public void ShowTypingIndicator(bool visible)
        {
            if (typingIndicator != null)
                typingIndicator.SetActive(visible);
        }

        public void ShowOptions(List<DialogueOption> options, Action<int> onOptionSelected)
        {
            _onOptionSelected = onOptionSelected;
            _activeOptionCount = Mathf.Min(options.Count, optionButtons.Length);
            _highlightedIndex = 0;

            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (i < _activeOptionCount)
                {
                    optionButtons[i].gameObject.SetActive(true);
                    optionButtons[i].Setup(options[i].DisplayText, i == 0);
                }
                else
                {
                    optionButtons[i].gameObject.SetActive(false);
                }
            }
        }

        public void HideOptions()
        {
            _activeOptionCount = 0;
            _highlightedIndex = 0;
            _onOptionSelected = null;

            if (optionButtons != null)
            {
                for (int i = 0; i < optionButtons.Length; i++)
                {
                    optionButtons[i].gameObject.SetActive(false);
                }
            }
        }

        public void ClearText()
        {
            if (dialogueText != null)
            {
                dialogueText.text = string.Empty;
                dialogueText.maxVisibleCharacters = 0;
            }
        }

        /// <summary>
        /// Navigates the highlight down to the next option, wrapping from last to first.
        /// Uses modulo arithmetic: highlightedIndex = (current + 1) % optionCount
        /// </summary>
        public void NavigateDown()
        {
            if (_activeOptionCount <= 0) return;

            optionButtons[_highlightedIndex].SetHighlighted(false);
            _highlightedIndex = (_highlightedIndex + 1) % _activeOptionCount;
            optionButtons[_highlightedIndex].SetHighlighted(true);
        }

        /// <summary>
        /// Navigates the highlight up to the previous option, wrapping from first to last.
        /// Uses modulo arithmetic: highlightedIndex = (current - 1 + optionCount) % optionCount
        /// </summary>
        public void NavigateUp()
        {
            if (_activeOptionCount <= 0) return;

            optionButtons[_highlightedIndex].SetHighlighted(false);
            _highlightedIndex = (_highlightedIndex - 1 + _activeOptionCount) % _activeOptionCount;
            optionButtons[_highlightedIndex].SetHighlighted(true);
        }

        /// <summary>
        /// Confirms the currently highlighted option, invoking the selection callback.
        /// </summary>
        public void ConfirmSelection()
        {
            if (_activeOptionCount > 0 && _onOptionSelected != null)
            {
                _onOptionSelected.Invoke(_highlightedIndex);
            }
        }
    }
}
