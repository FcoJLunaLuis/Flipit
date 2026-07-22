using UnityEngine;

namespace Flipit.Combat
{
    /// <summary>
    /// NPC de encuentro aleatorio. Se instancia dinámicamente cerca del jugador.
    /// No hereda de NPC_Interactable porque no usa el flujo estándar de detección.
    /// </summary>
    public class RandomEncounter_NPC : MonoBehaviour
    {
        [SerializeField] private string _combatSceneName;
        [SerializeField] private float _autoAcceptTimeout = 30f;

        public string CombatSceneName => _combatSceneName;
        public float AutoAcceptTimeout => _autoAcceptTimeout;
        public bool IsActive { get; private set; }

        /// <summary>
        /// Activates this encounter NPC, disabling player actions and forcing combat.
        /// Calls Combat_System.Instance.ForceCombatEncounter to initiate the forced encounter flow.
        /// </summary>
        public void Activate()
        {
            IsActive = true;

            if (Combat_System.Instance != null)
            {
                Combat_System.Instance.ForceCombatEncounter(this);
            }
            else
            {
                Debug.LogError("[RandomEncounter_NPC] Combat_System.Instance is null. Cannot force combat encounter.");
            }
        }

        /// <summary>
        /// Deactivates this encounter NPC, allowing cleanup by the encounter manager.
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
