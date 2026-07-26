using UnityEngine;
using UnityEngine.InputSystem;

namespace Flipit.Dialogue
{
    /// <summary>
    /// Attached to the player GameObject. Detects nearby NPC_Interactable components
    /// via Physics2D.OverlapCircleAll each FixedUpdate and dispatches interaction requests
    /// to the Dialogue_Manager.
    /// </summary>
    public class Player_Interactor : MonoBehaviour
    {
        [SerializeField, Min(0.1f)]
        private float interactionRadius = 2.0f;

        /// <summary>
        /// The currently detected nearest NPC_Interactable within the interaction radius,
        /// or null if none is in range.
        /// </summary>
        public NPC_Interactable CurrentTarget { get; private set; }

        /// <summary>
        /// The effective interaction radius. Values below 0.1 are clamped to 0.1.
        /// </summary>
        public float InteractionRadius
        {
            get => interactionRadius;
            set => interactionRadius = Mathf.Max(0.1f, value);
        }

        private void FixedUpdate()
        {
            DetectNearestNPC();
        }

        /// <summary>
        /// Performs a 3D overlap sphere centered on the player's position,
        /// filters for NPC_Interactable components, and selects the nearest one.
        /// Falls back to 2D overlap circle if no 3D colliders found.
        /// Clears the target when no NPC_Interactable is within radius.
        /// </summary>
        private void DetectNearestNPC()
        {
            float radius = Mathf.Max(0.1f, interactionRadius);

            NPC_Interactable nearest = null;
            float nearestDistance = float.MaxValue;

            // Try 3D detection first (for city exploration scene)
            Collider[] colliders3D = Physics.OverlapSphere(transform.position, radius);
            for (int i = 0; i < colliders3D.Length; i++)
            {
                NPC_Interactable npc = colliders3D[i].GetComponent<NPC_Interactable>();
                if (npc == null)
                    continue;

                float distance = Vector3.Distance(transform.position, npc.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = npc;
                }
            }

            // Fall back to 2D detection if no 3D results (for dialogue demo scene)
            if (nearest == null)
            {
                Collider2D[] colliders2D = Physics2D.OverlapCircleAll(transform.position, radius);
                for (int i = 0; i < colliders2D.Length; i++)
                {
                    NPC_Interactable npc = colliders2D[i].GetComponent<NPC_Interactable>();
                    if (npc == null)
                        continue;

                    float distance = Vector2.Distance(transform.position, npc.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = npc;
                    }
                }
            }

            CurrentTarget = nearest;
        }

        /// <summary>
        /// Called by the Input System when the Interact action is performed.
        /// Compatible with PlayerInput in SendMessages mode (receives InputValue).
        /// </summary>
        public void OnInteract(InputValue value)
        {
            HandleInteract();
        }

        /// <summary>
        /// Overload without parameters for SendMessages compatibility.
        /// </summary>
        public void OnInteract()
        {
            HandleInteract();
        }

        private void HandleInteract()
        {
            if (CurrentTarget == null)
                return;

            if (!CurrentTarget.HasValidDialogue)
                return;

            if (Dialogue_Manager.Instance == null)
                return;

            if (Dialogue_Manager.Instance.CurrentState != DialogueState.Idle)
                return;

            Dialogue_Manager.Instance.StartDialogue(CurrentTarget.DialogueData);
        }
    }
}
