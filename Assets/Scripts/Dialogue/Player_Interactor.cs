using UnityEngine;
using UnityEngine.InputSystem;

namespace Flipit.Dialogue
{
    /// <summary>
    /// Attached to the player GameObject. Detects nearby NPC_Interactable components
    /// and handles interaction input by polling the Interact action directly.
    /// </summary>
    public class Player_Interactor : MonoBehaviour
    {
        [SerializeField, Min(0.1f)]
        private float interactionRadius = 2.0f;

        private InteractionPromptHelper _prompt;
        private PlayerInput _playerInput;
        private InputAction _interactAction;

        public NPC_Interactable CurrentTarget { get; private set; }

        public float InteractionRadius
        {
            get => interactionRadius;
            set => interactionRadius = Mathf.Max(0.1f, value);
        }

        private void Awake()
        {
            _prompt = GetComponentInChildren<InteractionPromptHelper>(true);
            _playerInput = GetComponent<PlayerInput>();
        }

        private void Start()
        {
            if (_playerInput != null && _playerInput.actions != null)
                _interactAction = _playerInput.actions.FindAction("Player/Interact");
        }

        private void Update()
        {
            // Poll the Interact action directly every frame
            if (_interactAction != null && _interactAction.WasPerformedThisFrame())
            {
                TryInteract();
            }
        }

        private void FixedUpdate()
        {
            DetectNearestNPC();
        }

        private void TryInteract()
        {
            if (CurrentTarget == null)
                return;

            if (!CurrentTarget.HasValidDialogue)
                return;

            CurrentTarget.Interact();
        }

        private void DetectNearestNPC()
        {
            float radius = Mathf.Max(0.1f, interactionRadius);
            var colliders = new System.Collections.Generic.List<Collider2D>();
            Physics2D.OverlapCircle(transform.position, radius, new ContactFilter2D().NoFilter(), colliders);

            NPC_Interactable nearest = null;
            float nearestDistance = float.MaxValue;

            for (int i = 0; i < colliders.Count; i++)
            {
                NPC_Interactable npc = colliders[i].GetComponent<NPC_Interactable>();
                if (npc == null)
                    continue;

                float distance = Vector2.Distance(transform.position, npc.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = npc;
                }
            }

            if (nearest != CurrentTarget)
            {
                CurrentTarget = nearest;
                UpdatePrompt();
            }
        }

        private void UpdatePrompt()
        {
            if (_prompt == null) return;

            if (CurrentTarget != null)
                _prompt.Show("[E] Retar");
            else
                _prompt.Hide();
        }

        // Legacy SendMessages callback (kept as fallback)
        public void OnInteract(InputValue value)
        {
            TryInteract();
        }
    }
}
