using UnityEngine;

namespace Flipit.Combat
{
    public class RandomEncounter_NPC : MonoBehaviour
    {
        [SerializeField] private string _combatSceneName;
        [SerializeField] private float _autoAcceptTimeout = 30f;

        public string CombatSceneName => _combatSceneName;
        public float AutoAcceptTimeout => _autoAcceptTimeout;
        public bool IsActive { get; private set; }

        public void Activate()
        {
            IsActive = true;
            if (Combat_System.Instance != null)
                Combat_System.Instance.ForceCombatEncounter(this);
            else
                Debug.LogError("[RandomEncounter_NPC] Combat_System.Instance is null.");
        }

        public void Deactivate() => IsActive = false;
    }
}
