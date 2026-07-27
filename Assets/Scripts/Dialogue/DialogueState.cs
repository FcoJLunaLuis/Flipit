namespace Flipit.Dialogue
{
    /// <summary>
    /// Represents the possible states of the Dialogue_Manager state machine.
    /// </summary>
    public enum DialogueState
    {
        Idle,
        Typing,
        WaitingForInput,
        ShowingChoices,
        Transitioning,
        Closing
    }
}
