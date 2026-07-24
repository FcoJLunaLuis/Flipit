using UnityEngine;

namespace Flipit.NPC
{
    /// <summary>
    /// Temporary trigger for testing the Collector NPC.
    /// Click the cube to open the NPC interface.
    /// Will be replaced by an action button interaction post-merge.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CollectorNPCTrigger : MonoBehaviour
    {
        [SerializeField] private CollectorNPCUIController uiController;

        private void OnMouseDown()
        {
            if (uiController == null)
            {
                Debug.LogWarning("[CollectorNPCTrigger] UIController not assigned!");
                return;
            }

            if (uiController.CurrentState == NPCUIState.Closed)
            {
                uiController.OpenMenu();
            }
        }
    }
}
