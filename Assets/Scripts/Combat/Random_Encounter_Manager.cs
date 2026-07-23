using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Flipit.Dialogue;

namespace Flipit.Combat
{
    public class Random_Encounter_Manager : MonoBehaviour
    {
        [SerializeField] private EncounterConfig _encounterConfig;
        [SerializeField] private RandomEncounter_NPC _encounterPrefab;
        [SerializeField] private Transform[] _spawnPoints;

        public bool IsEncounterActive { get; private set; }
        private float _lastEncounterTime = float.NegativeInfinity;
        private Coroutine _encounterCoroutine;

        private void OnEnable() => _encounterCoroutine = StartCoroutine(EncounterTimerRoutine());
        private void OnDisable() { if (_encounterCoroutine != null) StopCoroutine(_encounterCoroutine); }

        private IEnumerator EncounterTimerRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_encounterConfig.CheckInterval);
                EvaluateEncounter();
            }
        }

        private void EvaluateEncounter()
        {
            if (Dialogue_Manager.Instance == null || Dialogue_Manager.Instance.CurrentState != DialogueState.Idle) return;
            if (IsEncounterActive) return;
            if (Time.time - _lastEncounterTime < _encounterConfig.MinTimeBetweenEncounters) return;
            if (Random.value > _encounterConfig.EncounterProbability) return;

            Transform sp = FindValidSpawnPoint();
            if (sp == null) { Debug.LogWarning("[Random_Encounter_Manager] No valid spawn point."); return; }

            var npc = Instantiate(_encounterPrefab, sp.position, Quaternion.identity);
            npc.Activate();
            IsEncounterActive = true;
            _lastEncounterTime = Time.time;
        }

        private Transform FindValidSpawnPoint()
        {
            if (_spawnPoints == null || _spawnPoints.Length == 0) return null;
            var player = FindObjectOfType<Player_Interactor>();
            if (player == null) return null;

            Vector2 playerPos = player.transform.position;
            var valid = new List<Transform>();
            for (int i = 0; i < _spawnPoints.Length; i++)
            {
                if (_spawnPoints[i] == null) continue;
                float dist = Vector2.Distance(playerPos, _spawnPoints[i].position);
                if (dist > _encounterConfig.SpawnRadius) continue;
                var cols = Physics2D.OverlapCircleAll(_spawnPoints[i].position, 0.5f);
                bool blocked = false;
                foreach (var c in cols) { if (!c.isTrigger) { blocked = true; break; } }
                if (!blocked) valid.Add(_spawnPoints[i]);
            }
            return valid.Count > 0 ? valid[Random.Range(0, valid.Count)] : null;
        }

        public void NotifyEncounterEnded() => IsEncounterActive = false;

        [ContextMenu("Force Encounter Now")]
        public void ForceEncounterNow()
        {
            if (IsEncounterActive || Combat_System.Instance == null || Combat_System.Instance.CurrentState != CombatState.Idle) return;
            var player = FindObjectOfType<Player_Interactor>();
            if (player == null || _encounterPrefab == null) return;
            var npc = Instantiate(_encounterPrefab, player.transform.position + new Vector3(2f, 0, 0), Quaternion.identity);
            npc.Activate();
            IsEncounterActive = true;
            _lastEncounterTime = Time.time;
        }
    }
}
