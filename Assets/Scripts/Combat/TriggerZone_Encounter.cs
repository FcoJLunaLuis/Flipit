using UnityEngine;
using Flipit.Dialogue;
using System.Collections;

namespace Flipit.Combat
{
    /// <summary>
    /// NPC de encuentro por zona (FlipCombat_NPC_3 "Campeón").
    /// Invisible al inicio. Aparece cuando el jugador se acerca.
    /// Fuerza diálogo con solo opción "¡Acepto!".
    /// Desaparece después y se repite cada vez que el jugador vuelve.
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
        }

        private IEnumerator Start()
        {
            // Hide immediately
            SetVisible(false);

            // Wait 1 frame for all singletons to initialize
            yield return null;
            yield return null;

            // Find player
            var interactor = FindObjectOfType<Player_Interactor>();
            if (interactor != null)
            {
                _playerTransform = interactor.transform;
                Debug.Log($"[TriggerZone_Encounter] Player found. Detection radius: {_detectionRadius}");
            }
            else
            {
                Debug.LogError("[TriggerZone_Encounter] Player NOT found!");
            }

            // Wait until player is OUTSIDE range before becoming ready
            // (prevents triggering if player spawns inside the zone)
            if (_playerTransform != null)
            {
                float dist = Vector2.Distance(transform.position, _playerTransform.position);
                if (dist <= _detectionRadius)
                {
                    Debug.Log("[TriggerZone_Encounter] Player starts inside zone, waiting for exit...");
                    while (_playerTransform != null &&
                           Vector2.Distance(transform.position, _playerTransform.position) <= _detectionRadius)
                    {
                        yield return null;
                    }
                    Debug.Log("[TriggerZone_Encounter] Player exited zone, now ready.");
                }
            }

            _ready = true;
        }

        private void Update()
        {
            if (!_ready || _active || _playerTransform == null)
                return;

            // Cooldown after an encounter to prevent instant re-trigger
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
                return;
            }

            // Check systems are ready
            if (Combat_System.Instance == null ||
                Combat_System.Instance.CurrentState != CombatState.Idle)
                return;

            if (Dialogue_Manager.Instance == null ||
                Dialogue_Manager.Instance.CurrentState != DialogueState.Idle)
                return;

            float distance = Vector2.Distance(transform.position, _playerTransform.position);

            if (distance <= _detectionRadius)
            {
                _active = true;
                Debug.Log($"[TriggerZone_Encounter] Player entered zone! Distance: {distance}");
                ActivateEncounter();
            }
        }

        private void ActivateEncounter()
        {
            SetVisible(true);

            // Register with handler for [COMBAT_ACCEPT] interception
            if (CombatDialogue_Handler.Instance != null && _forcedDialogueData != null)
            {
                CombatDialogue_Handler.Instance.RegisterForcedEncounter(
                    _combatSceneName, _forcedDialogueData);
            }
            else
            {
                Debug.LogError("[TriggerZone_Encounter] CombatDialogue_Handler or dialogue data is null!");
                ResetEncounter();
                return;
            }

            // Start dialogue
            if (Dialogue_Manager.Instance != null)
            {
                Dialogue_Manager.Instance.StartDialogue(_forcedDialogueData);
                Debug.Log("[TriggerZone_Encounter] Dialogue started.");
            }
            else
            {
                Debug.LogError("[TriggerZone_Encounter] Dialogue_Manager is null!");
                ResetEncounter();
                return;
            }

            Combat_Event_Bus.OnCombatEventRaised += OnCombatEvent;
        }

        private void OnCombatEvent(string eventId)
        {
            if (eventId == "combat_dialogue_ended" || eventId == "combat_accepted")
            {
                StartCoroutine(HideAfterTransition());
            }
        }

        private IEnumerator HideAfterTransition()
        {
            Combat_Event_Bus.OnCombatEventRaised -= OnCombatEvent;

            // Wait until Combat_System returns to Idle
            float timeout = 10f;
            float elapsed = 0f;
            while (Combat_System.Instance != null &&
                   Combat_System.Instance.CurrentState != CombatState.Idle &&
                   elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            Debug.Log("[TriggerZone_Encounter] Encounter finished, hiding NPC.");
            ResetEncounter();
        }

        private void ResetEncounter()
        {
            SetVisible(false);
            _active = false;
            _cooldownTimer = 2f; // 2 second cooldown before can trigger again
            Combat_Event_Bus.OnCombatEventRaised -= OnCombatEvent;
        }

        private void SetVisible(bool visible)
        {
            if (_spriteRenderer != null)
                _spriteRenderer.enabled = visible;
            if (_labelGO != null)
                _labelGO.SetActive(visible);
        }

        private void OnDestroy()
        {
            Combat_Event_Bus.OnCombatEventRaised -= OnCombatEvent;
        }
    }
}
