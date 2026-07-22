using UnityEngine;

namespace Flipit.Combat
{
    [CreateAssetMenu(fileName = "NewEncounterConfig", menuName = "Flipit/Encounter Config")]
    public class EncounterConfig : ScriptableObject
    {
        [SerializeField, Range(1f, 60f)] private float _checkInterval = 5f;
        [SerializeField, Range(5f, 300f)] private float _minTimeBetweenEncounters = 15f;
        [SerializeField, Range(0f, 1f)] private float _encounterProbability = 0.15f;
        [SerializeField, Range(1f, 50f)] private float _spawnRadius = 10f;
        [SerializeField, Range(2f, 4f)] private float _spawnDistanceFromPlayer = 3f;

        public float CheckInterval
        {
            get => Mathf.Clamp(_checkInterval, 1f, 60f);
            set => _checkInterval = Mathf.Clamp(value, 1f, 60f);
        }
        public float MinTimeBetweenEncounters
        {
            get => Mathf.Clamp(_minTimeBetweenEncounters, 5f, 300f);
            set => _minTimeBetweenEncounters = Mathf.Clamp(value, 5f, 300f);
        }
        public float EncounterProbability
        {
            get => Mathf.Clamp(_encounterProbability, 0f, 1f);
            set => _encounterProbability = Mathf.Clamp(value, 0f, 1f);
        }
        public float SpawnRadius
        {
            get => Mathf.Clamp(_spawnRadius, 1f, 50f);
            set => _spawnRadius = Mathf.Clamp(value, 1f, 50f);
        }
        public float SpawnDistanceFromPlayer
        {
            get => Mathf.Clamp(_spawnDistanceFromPlayer, 2f, 4f);
            set => _spawnDistanceFromPlayer = Mathf.Clamp(value, 2f, 4f);
        }
    }
}
