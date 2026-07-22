using UnityEngine;
using UnityEngine.InputSystem;

namespace Flipit.Dialogue
{
    /// <summary>
    /// Simple top-down 2D player movement using the new Input System.
    /// Reads the Move action from the Player action map and applies velocity to a Rigidbody2D.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class TopDownPlayerMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        private Rigidbody2D _rb;
        private Vector2 _moveInput;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            _rb.linearVelocity = _moveInput * moveSpeed;
        }

        /// <summary>
        /// Called by PlayerInput component via SendMessages for the Move action.
        /// </summary>
        public void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }
    }
}
