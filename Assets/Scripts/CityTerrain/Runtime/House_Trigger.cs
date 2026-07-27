using UnityEngine;
using UnityEngine.InputSystem;
using Flipit.Combat;

namespace Flipit.CityTerrain
{
    /// <summary>
    /// Interaction-based trigger for the player's house.
    /// When the player is within interaction radius and presses E,
    /// fires the level-completion event via Combat_Event_Bus.
    /// No trigger collider needed — uses distance check.
    /// </summary>
    public class House_Trigger : MonoBehaviour
    {
        [SerializeField] private string _eventId = "level_complete";
        [SerializeField] private bool _fireOnce = true;
        [SerializeField] private float _interactionRadius = 3f;

        private bool _hasFired;
        private Transform _player;

        private void Start()
        {
            // Find player
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO == null)
                playerGO = GameObject.Find("Player");
            if (playerGO != null)
                _player = playerGO.transform;
            else
                Debug.LogWarning("[House_Trigger] Player not found.");
        }

        private void Update()
        {
            if (_fireOnce && _hasFired) return;
            if (_player == null) return;

            // Check distance
            float distance = Vector3.Distance(transform.position, _player.position);
            if (distance > _interactionRadius) return;

            // Check E key press
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                _hasFired = true;
                Combat_Event_Bus.RaiseEvent(_eventId);
                Debug.Log($"[House_Trigger] Level-completion event fired: {_eventId}");
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _interactionRadius);
        }
    }
}
