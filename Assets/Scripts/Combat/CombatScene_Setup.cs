using UnityEngine;
using UnityEngine.InputSystem;
using Flipit.Dialogue;

namespace Flipit.Combat
{
    public class CombatScene_Setup : MonoBehaviour
    {
        [SerializeField] private Transform _playerSpawnPoint;
        [SerializeField] private GameObject _playerPrefab;

        private void Awake() => SetupPlayer();

        private void SetupPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                var movement = FindObjectOfType<TopDownPlayerMovement>();
                if (movement != null) player = movement.gameObject;
            }

            if (player == null && _playerPrefab != null)
                player = Instantiate(_playerPrefab, GetSpawnPosition(), Quaternion.identity);
            else if (player != null)
                player.transform.position = GetSpawnPosition();

            if (player == null) { Debug.LogError("[CombatScene_Setup] No player found."); return; }

            var mov = player.GetComponent<TopDownPlayerMovement>();
            if (mov != null) mov.enabled = true;
            var inter = player.GetComponent<Player_Interactor>();
            if (inter != null) inter.enabled = true;
            var pi = player.GetComponent<PlayerInput>();
            if (pi != null)
            {
                pi.notificationBehavior = PlayerNotifications.SendMessages;
                pi.SwitchCurrentActionMap("Player");
            }
        }

        private Vector3 GetSpawnPosition() =>
            _playerSpawnPoint != null ? _playerSpawnPoint.position : Vector3.zero;
    }
}
