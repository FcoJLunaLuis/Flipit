using UnityEngine;
using Flipit.Dialogue;
using System.Collections;

namespace Flipit.Combat
{
    /// <summary>
    /// NPC invisible que aparece cuando el jugador se acerca.
    /// Fuerza diálogo con solo "¡Acepto!" y muestra "RETO ACEPTADO".
    /// Se repite cada vez que el jugador vuelve a la zona.
    /// </summary>
    public class TriggerZone_Encounter : MonoBehaviour
    {
        [SerializeField] private string _combatSceneName = "CombatScene";
        [SerializeField] private string _npcDisplayName = "???";
        [SerializeField] private DialogueData _forcedDialogueData;
        [SerializeField] private float _detectionRadius = 3f;

        private SpriteRenderer _spriteRenderer;
        private GameObject _labelGO;
        private Transform _playerTransform;
        private bool _active;
        private bool _ready;
        private float _cooldownTimer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _labelGO = transform.Find("Label")?.gameObject;
            // Hide immediately at Awake (before first frame renders)
            SetVisible(false);
        }

        private IEnumerator Start()
        {
            // Wait for singletons to initialize
            yield return null;
            yield return null;

            var interactor = FindObjectOfType<Player_Interactor>();
            if (interactor != null)
                _playerTransform = interactor.transform;
            else
                Debug.LogError("[TriggerZone_Encounter] Player not found!");

            // If player starts inside zone, wait until they leave
            if (_playerTransform != null)
            {
                float dist = Vector2.Distance(transform.position, _playerTransform.position);
                while (_playerTransform != null && dist <= _detectionRadius)
                {
                    yield return null;
                    dist = Vector2.Distance(transform.position, _playerTransform.position);
                }
            }
            _ready = true;
        }

        private void Update()
        {
            if (!_ready || _active || _playerTransform == null) return;

            if (_cooldownTimer > 0f) { _cooldownTimer -= Time.deltaTime; return; }

            if (Combat_System.Instance == null || Combat_System.Instance.CurrentState != CombatState.Idle) return;
            if (Dialogue_Manager.Instance == null || Dialogue_Manager.Instance.CurrentState != DialogueState.Idle) return;

            float distance = Vector2.Distance(transform.position, _playerTransform.position);
            if (distance <= _detectionRadius)
            {
                _active = true;
                ActivateEncounter();
            }
        }

        private void ActivateEncounter()
        {
            SetVisible(true);

            if (CombatDialogue_Handler.Instance != null && _forcedDialogueData != null)
                CombatDialogue_Handler.Instance.RegisterForcedEncounter(_combatSceneName, _forcedDialogueData);

            if (Dialogue_Manager.Instance != null && _forcedDialogueData != null)
                Dialogue_Manager.Instance.StartDialogue(_forcedDialogueData);
            else
            { ResetEncounter(); return; }

            Combat_Event_Bus.OnCombatEventRaised += OnCombatEvent;
        }

        private void OnCombatEvent(string eventId)
        {
            if (eventId == "combat_dialogue_ended" || eventId == "combat_accepted")
                StartCoroutine(HideAfterTransition());
        }

        private IEnumerator HideAfterTransition()
        {
            Combat_Event_Bus.OnCombatEventRaised -= OnCombatEvent;
            float timeout = 10f, elapsed = 0f;
            while (Combat_System.Instance != null &&
                   Combat_System.Instance.CurrentState != CombatState.Idle &&
                   elapsed < timeout)
            { elapsed += Time.deltaTime; yield return null; }
            ResetEncounter();
        }

        private void ResetEncounter()
        {
            SetVisible(false);
            _active = false;
            _cooldownTimer = 3f;
            Combat_Event_Bus.OnCombatEventRaised -= OnCombatEvent;
        }

        private void SetVisible(bool visible)
        {
            if (_spriteRenderer != null) _spriteRenderer.enabled = visible;
            if (_labelGO != null) _labelGO.SetActive(visible);
        }

        private void OnDestroy()
        {
            Combat_Event_Bus.OnCombatEventRaised -= OnCombatEvent;
        }
    }
}
