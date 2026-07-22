namespace Flipit.Combat
{
    /// <summary>
    /// Represents the possible states of the Combat_System state machine.
    /// </summary>
    public enum CombatState
    {
        Idle,
        DialogueOpen,
        ForcedEncounter,
        Transitioning
    }
}
