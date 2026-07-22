using UnityEngine;
using Flipit.Dialogue;

namespace Flipit.Combat
{
    /// <summary>
    /// NPC de combate estático. Hereda de NPC_Interactable para reutilizar
    /// la detección por proximidad del Player_Interactor.
    /// El DialogueData asignado contendrá opciones de aceptar/rechazar combate.
    /// Overrides Interact() to register with CombatDialogue_Handler before starting dialogue.
    /// </summary>
    public class FlipCombat_NPC : NPC_Interactable
    {
        [SerializeField] private string _combatSceneName;
        [SerializeField] private string _npcDisplayName;

        /// <summary>
        /// Name of the scene to load when combat is accepted.
        /// </summary>
        public string CombatSceneName => _combatSceneName;

        /// <summary>
        /// Display name shown in the combat dialogue UI.
        /// </summary>
        public string NpcDisplayName => _npcDisplayName;

        /// <summary>
        /// Overrides NPC_Interactable.Interact() to register this NPC with the
        /// CombatDialogue_Handler before starting the normal dialogue flow.
        /// This allows the handler to intercept [COMBAT_ACCEPT]/[COMBAT_REJECT] markers.
        /// </summary>
        public override void Interact()
        {
            // Register with the CombatDialogue_Handler so it knows which NPC
            // is being interacted with (for CombatSceneName retrieval)
            if (CombatDialogue_Handler.Instance != null)
            {
                CombatDialogue_Handler.Instance.RegisterCombatNPC(this);
            }
            else
            {
                Debug.LogWarning("[FlipCombat_NPC] CombatDialogue_Handler.Instance is null. Combat interception will not work.");
            }

            // Use the normal dialogue flow from base class
            base.Interact();
        }
    }
}
