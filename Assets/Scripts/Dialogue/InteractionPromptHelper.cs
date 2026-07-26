using UnityEngine;

namespace Flipit.Dialogue
{
    /// <summary>
    /// Shows/hides the interaction prompt text based on whether 
    /// the Player_Interactor has a valid target.
    /// Attach to a child object of the Player that has a TextMesh.
    /// </summary>
    public class InteractionPromptHelper : MonoBehaviour
    {
        private Player_Interactor _interactor;
        private MeshRenderer _renderer;

        private void Start()
        {
            _interactor = GetComponentInParent<Player_Interactor>();
            _renderer = GetComponent<MeshRenderer>();

            if (_renderer != null)
                _renderer.enabled = false;
        }

        private void Update()
        {
            if (_interactor == null || _renderer == null) return;

            bool hasTarget = _interactor.CurrentTarget != null
                && _interactor.CurrentTarget.HasValidDialogue
                && Dialogue_Manager.Instance != null
                && Dialogue_Manager.Instance.CurrentState == DialogueState.Idle;

            _renderer.enabled = hasTarget;
        }
    }
}
