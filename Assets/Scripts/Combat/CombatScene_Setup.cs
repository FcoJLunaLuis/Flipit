using UnityEngine;
using UnityEngine.InputSystem;
using Flipit.Dialogue;

namespace Flipit.Combat
{
    /// <summary>
    /// Runs when the CombatScene loads. Finds or instantiates the Player at the
    /// designated spawn point and ensures all required components are active.
    /// </summary>
    public class CombatScene_Setup : MonoBehaviour
    {
        [SerializeField] private Transform _playerSpawnPoint;
        [SerializeField] private GameObject _playerPrefab;

        private void Awake()
        {
            SetupPlayer();
        }

        private void SetupPlayer()
        {
            GameObject player = FindExistingPlayer();

            if (player == null && _playerPrefab != null)
            {
                Vector3 spawnPosition = GetSpawnPosition();
                player = Instantiate(_playerPrefab, spawnPosition, Quaternion.identity);
            }
            else if (player != null)
            {
                // Player already exists (e.g., via DontDestroyOnLoad), reposition it
                player.transform.position = GetSpawnPosition();
            }

            if (player == null)
            {
                Debug.LogError("[CombatScene_Setup] No player found in scene and no prefab assigned.");
                return;
            }

            EnsurePlayerComponents(player);
            ConfigurePlayerInput(player);
        }

        private Vector3 GetSpawnPosition()
        {
            if (_playerSpawnPoint != null)
                return _playerSpawnPoint.position;

            Debug.LogWarning("[CombatScene_Setup] _playerSpawnPoint is not assigned. Using Vector3.zero.");
            return Vector3.zero;
        }

        private GameObject FindExistingPlayer()
        {
            // Try finding by tag first
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                return player;

            // Fallback: find by component
            TopDownPlayerMovement movement = FindFirstObjectByType<TopDownPlayerMovement>();
            if (movement != null)
                return movement.gameObject;

            return null;
        }

        private void EnsurePlayerComponents(GameObject player)
        {
            // Ensure TopDownPlayerMovement is present and active
            TopDownPlayerMovement movement = player.GetComponent<TopDownPlayerMovement>();
            if (movement != null)
            {
                movement.enabled = true;
            }
            else
            {
                Debug.LogWarning("[CombatScene_Setup] Player is missing TopDownPlayerMovement component.");
            }

            // Ensure Player_Interactor is present and active
            Player_Interactor interactor = player.GetComponent<Player_Interactor>();
            if (interactor != null)
            {
                interactor.enabled = true;
            }
            else
            {
                Debug.LogWarning("[CombatScene_Setup] Player is missing Player_Interactor component.");
            }
        }

        private void ConfigurePlayerInput(GameObject player)
        {
            PlayerInput playerInput = player.GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                Debug.LogWarning("[CombatScene_Setup] Player is missing PlayerInput component.");
                return;
            }

            // Set notification behavior to SendMessages
            playerInput.notificationBehavior = PlayerNotifications.SendMessages;

            // Switch to "Player" action map
            playerInput.SwitchCurrentActionMap("Player");
        }
    }
}
