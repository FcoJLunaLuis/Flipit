using System;
using System.Collections.Generic;

namespace Flipit.Dialogue
{
    /// <summary>
    /// Abstraction layer between the Dialogue_Manager and any UI implementation.
    /// Allows different UI implementations to be swapped without modifying core dialogue logic.
    /// </summary>
    public interface IDialogueUI
    {
        /// <summary>
        /// Makes the dialogue UI visible.
        /// </summary>
        void Show();

        /// <summary>
        /// Hides the dialogue UI, invoking the callback when the hide operation completes.
        /// </summary>
        /// <param name="onComplete">Callback invoked when hiding is complete.</param>
        void Hide(Action onComplete);

        /// <summary>
        /// Sets the displayed speaker name.
        /// </summary>
        /// <param name="speakerName">The name of the current speaker.</param>
        void SetSpeakerName(string speakerName);

        /// <summary>
        /// Sets the full dialogue text content (used for layout/sizing before typewriter reveal).
        /// </summary>
        /// <param name="text">The full dialogue line text.</param>
        void SetDialogueText(string text);

        /// <summary>
        /// Sets the number of visible characters for typewriter text reveal via TMP maxVisibleCharacters.
        /// </summary>
        /// <param name="count">Number of characters to display.</param>
        void SetVisibleCharacterCount(int count);

        /// <summary>
        /// Shows or hides the advance indicator (signaling the player can proceed).
        /// </summary>
        /// <param name="visible">Whether the indicator should be visible.</param>
        void ShowAdvanceIndicator(bool visible);

        /// <summary>
        /// Shows or hides the typing indicator (signaling text is being revealed).
        /// </summary>
        /// <param name="visible">Whether the indicator should be visible.</param>
        void ShowTypingIndicator(bool visible);

        /// <summary>
        /// Displays dialogue options for player selection.
        /// </summary>
        /// <param name="options">The list of available dialogue options to display.</param>
        /// <param name="onOptionSelected">Callback invoked with the selected option index.</param>
        void ShowOptions(List<DialogueOption> options, Action<int> onOptionSelected);

        /// <summary>
        /// Hides all dialogue option buttons.
        /// </summary>
        void HideOptions();

        /// <summary>
        /// Clears the dialogue text display.
        /// </summary>
        void ClearText();
    }
}
