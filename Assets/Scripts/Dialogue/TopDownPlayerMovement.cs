using UnityEngine;
using UnityEngine.InputSystem;

namespace Flipit.Dialogue
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class TopDownPlayerMovement : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 5f;

        private Rigidbody2D _rb;
        private Vector2 _moveInput;
        private PlayerInput _playerInput;
        private InputAction _moveAction;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _playerInput = GetComponent<PlayerInput>();
        }

        private void Start()
        {
            if (_playerInput != null && _playerInput.actions != null)
                _moveAction = _playerInput.actions.FindAction("Player/Move");
        }

        private void Update()
        {
            if (_moveAction != null && _moveAction.enabled)
                _moveInput = _moveAction.ReadValue<Vector2>();
            else
                _moveInput = Vector2.zero;
        }

        private void FixedUpdate()
        {
            _rb.linearVelocity = _moveInput * _moveSpeed;
        }

        // Legacy SendMessages callback
        public void OnMove(InputValue value)
        {
            _moveInput = value.Get<Vector2>();
        }
    }
}
