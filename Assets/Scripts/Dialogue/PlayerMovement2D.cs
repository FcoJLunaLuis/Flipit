using UnityEngine;
using UnityEngine.InputSystem;

namespace Flipit.Dialogue
{
    /// <summary>
    /// Simple top-down 2D player movement controller.
    /// Reads Move input from the Player action map.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement2D : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        private Rigidbody2D _rb;
        private Vector2 _moveInput;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
        }

        public void OnMove(InputValue value)
        {
            _moveInput = value.Get<Vector2>();
        }

        private void FixedUpdate()
        {
            _rb.linearVelocity = _moveInput * moveSpeed;
        }
    }
}