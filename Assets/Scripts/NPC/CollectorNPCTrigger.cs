using UnityEngine;
using UnityEngine.InputSystem;

namespace Flipit.NPC
{
    /// <summary>
    /// Temporary trigger for testing the Collector NPC.
    /// Click the cube to open the NPC interface.
    /// Uses New Input System for mouse click detection + Physics.Raycast.
    /// Will be replaced by an action button interaction post-merge.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CollectorNPCTrigger : MonoBehaviour
    {
        [SerializeField] private CollectorNPCUIController uiController;
        [SerializeField] private float interactionDistance = 100f;

        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (mainCamera == null) return;

            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                HandleClick(mouse.position.ReadValue());
            }
        }

        private void HandleClick(Vector2 screenPosition)
        {
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    TryOpenMenu();
                }
            }
        }

        private void TryOpenMenu()
        {
            if (uiController == null)
            {
                Debug.LogWarning("[CollectorNPCTrigger] UIController not assigned!");
                return;
            }

            if (uiController.CurrentState == NPCUIState.Closed)
            {
                Debug.Log("[CollectorNPCTrigger] Cube clicked - opening NPC menu.");
                uiController.OpenMenu();
            }
        }
    }
}
