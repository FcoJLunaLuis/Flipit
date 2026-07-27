using UnityEngine;
using Flipit.Dialogue;

namespace Flipit.Combat
{
    public class FlipCombat_NPC : NPC_Interactable
    {
        [SerializeField] private string _combatSceneName;
        [SerializeField] private string _npcDisplayName;

        public string CombatSceneName => _combatSceneName;
        public string NpcDisplayName => _npcDisplayName;

        public override void Interact()
        {
            if (CombatDialogue_Handler.Instance != null)
                CombatDialogue_Handler.Instance.RegisterCombatNPC(this);

            base.Interact();
        }
    }
}
