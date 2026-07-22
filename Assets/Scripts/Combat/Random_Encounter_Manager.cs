using System.Collections;
using UnityEngine;
using Flipit.Dialogue;

namespace Flipit.Combat
{
    /// <summary>
    /// Manages the timing and spawning of random encounters.
    /// Evaluates encounter probability at configurable intervals using a coroutine-based timer.
    /// </summary>
    public class Random_Encounter_Manager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private EncounterConfig _encounterConfig;

        [Header("Spawning")]
        [SerializeField] private RandomEncounter_NPC _encounterPrefab;
        [SerializeField] private Transform[] _spawnPoints;

        /// <summary>
        /// Whether a random encounter NPC is currently active in the world.
        /// </summary>
        public bool IsEncounterActive { get; private set; }

        private float _lastEncounterTime = float.NegativeInfinity;
        private Coroutine _encounterCoroutine;

        private void OnEnable()
        {
            _encounterCoroutine = StartCoroutine(EncounterTimerRoutine());
        }

        private void OnDisable()
        {
            if (_encounterCoroutine != null)
            {
                StopCoroutine(_encounterCoroutine);
                _encounterCoroutine = null;
            }
        }

        /// <summary>
        /// Coroutine that periodically calls EvaluateEncounter based on the configured check interval.
        /// </summary>
        private IEnumerator EncounterTimerRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_encounterConfig.CheckInterval);
                EvaluateEncounter();
            }
        }

        /// <summary>
        /// Evaluates whether a random encounter should trigger.
        /// Checks: dialogue state is Idle, no active encounter, cooldown elapsed, probability roll passes.
        /// If all conditions pass, finds a valid spawn point and instantiates the encounter prefab.
        /// </summary>
        private void EvaluateEncounter()
        {
            // Guard: Dialogue_Manager must be Idle
            if (Dialogue_Manager.Instance == null ||
                Dialogue_Manager.Instance.CurrentState != DialogueState.Idle)
            {
                return;
            }

            // Guard: No encounter already active
            if (IsEncounterActive)
            {
                return;
            }

            // Guard: Cooldown between encounters
            float timeSinceLastEncounter = Time.time - _lastEncounterTime;
            if (timeSinceLastEncounter < _encounterConfig.MinTimeBetweenEncounters)
            {
                return;
            }

            // Probability roll
            if (Random.value > _encounterConfig.EncounterProbability)
            {
                return;
            }

            // Find a valid spawn point
            Transform spawnPoint = FindValidSpawnPoint();
            if (spawnPoint == null)
            {
                Debug.LogWarning("[Random_Encounter_Manager] No valid spawn point found within configured radius. Encounter cancelled.");
                return;
            }

            // Spawn the encounter NPC
            RandomEncounter_NPC encounterNPC = Instantiate(_encounterPrefab, spawnPoint.position, Quaternion.identity);
            encounterNPC.Activate();

            IsEncounterActive = true;
            _lastEncounterTime = Time.time;
        }

        /// <summary>
        /// Finds a valid spawn point within the configured spawn radius of the player.
        /// Filters out points that are obstructed by colliders (Physics2D.OverlapCircle check).
        /// Returns a random valid point, or null if none exist.
        /// </summary>
        private Transform FindValidSpawnPoint()
        {
            if (_spawnPoints == null || _spawnPoints.Length == 0)
                return null;

            Transform player = FindPlayerTransform();
            if (player == null)
                return null;

            Vector2 playerPosition = player.position;
            float spawnRadius = _encounterConfig.SpawnRadius;

            // Collect valid spawn points: within radius and unobstructed
            var validPoints = new System.Collections.Generic.List<Transform>();

            for (int i = 0; i < _spawnPoints.Length; i++)
            {
                if (_spawnPoints[i] == null)
                    continue;

                Vector2 pointPosition = _spawnPoints[i].position;
                float distance = Vector2.Distance(playerPosition, pointPosition);

                // Must be within configured spawn radius
                if (distance > spawnRadius)
                    continue;

                // Validate no solid (non-trigger) collider obstruction at the spawn point
                Collider2D[] colliders = Physics2D.OverlapCircleAll(pointPosition, 0.5f);
                bool obstructed = false;
                for (int j = 0; j < colliders.Length; j++)
                {
                    if (!colliders[j].isTrigger)
                    {
                        obstructed = true;
                        break;
                    }
                }
                if (obstructed)
                    continue;

                validPoints.Add(_spawnPoints[i]);
            }

            if (validPoints.Count == 0)
                return null;

            // Return a random valid point
            return validPoints[Random.Range(0, validPoints.Count)];
        }

        /// <summary>
        /// Finds the player Transform in the scene. Looks for the Player_Interactor component
        /// as that is always on the player GameObject.
        /// </summary>
        private Transform FindPlayerTransform()
        {
            var playerInteractor = FindObjectOfType<Player_Interactor>();
            if (playerInteractor != null)
                return playerInteractor.transform;

            return null;
        }

        /// <summary>
        /// Called externally (e.g., by RandomEncounter_NPC.Deactivate flow) to mark the encounter as no longer active.
        /// </summary>
        public void NotifyEncounterEnded()
        {
            IsEncounterActive = false;
        }

        /// <summary>
        /// Forces an encounter immediately, bypassing probability and cooldown checks.
        /// Useful for testing. Can be called from the Inspector context menu.
        /// </summary>
        [ContextMenu("Force Encounter Now")]
        public void ForceEncounterNow()
        {
            if (IsEncounterActive)
            {
                Debug.LogWarning("[Random_Encounter_Manager] Cannot force encounter: one is already active.");
                return;
            }

            if (Combat_System.Instance != null && Combat_System.Instance.CurrentState != CombatState.Idle)
            {
                Debug.LogWarning("[Random_Encounter_Manager] Cannot force encounter: Combat_System is not Idle.");
                return;
            }

            Transform player = FindPlayerTransform();
            if (player == null)
            {
                Debug.LogWarning("[Random_Encounter_Manager] Cannot force encounter: player not found.");
                return;
            }

            // Spawn near the player (offset 2 units to the right)
            Vector3 spawnPos = player.position + new Vector3(2f, 0, 0);

            if (_encounterPrefab == null)
            {
                Debug.LogWarning("[Random_Encounter_Manager] Cannot force encounter: no prefab assigned.");
                return;
            }

            RandomEncounter_NPC encounterNPC = Instantiate(_encounterPrefab, spawnPos, Quaternion.identity);
            encounterNPC.Activate();

            IsEncounterActive = true;
            _lastEncounterTime = Time.time;

            Debug.Log("[Random_Encounter_Manager] Forced encounter spawned!");
        }
    }
}
