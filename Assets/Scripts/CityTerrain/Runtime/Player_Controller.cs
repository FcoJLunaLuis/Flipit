using UnityEngine;
using UnityEngine.InputSystem;

namespace Flipit.CityTerrain
{
    /// <summary>
    /// CharacterController-based movement on XZ plane.
    /// Reads Move action from New Input System Player action map.
    /// Clamps position to city bounds.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class Player_Controller : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private Vector2 _boundaryMin = new Vector2(0f, 0f);
        [SerializeField] private Vector2 _boundaryMax = new Vector2(120f, 120f);

        private CharacterController _cc;
        private Vector2 _moveInput;
        private float _verticalVelocity;
        private const float Gravity = -9.81f;
        private bool _movementLocked;

        public Vector2 BoundaryMin { get => _boundaryMin; set => _boundaryMin = value; }
        public Vector2 BoundaryMax { get => _boundaryMax; set => _boundaryMax = value; }

        /// <summary>
        /// Lock/unlock player movement (e.g., during dialogue).
        /// </summary>
        public bool MovementLocked
        {
            get => _movementLocked;
            set
            {
                _movementLocked = value;
                if (_movementLocked) _moveInput = Vector2.zero;
            }
        }

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
        }

        private void Update()
        {
            // Apply gravity
            if (_cc.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -0.5f;
            }
            else
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }

            // Calculate XZ movement (zero if locked)
            Vector3 move;
            if (_movementLocked)
            {
                move = Vector3.zero;
            }
            else
            {
                move = new Vector3(_moveInput.x, 0f, _moveInput.y);
                move = move.normalized * _moveSpeed;
            }

            // Add vertical velocity
            move.y = _verticalVelocity;

            _cc.Move(move * Time.deltaTime);

            // Clamp position to city boundaries
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, _boundaryMin.x, _boundaryMax.x);
            pos.z = Mathf.Clamp(pos.z, _boundaryMin.y, _boundaryMax.y);
            transform.position = pos;
        }

        /// <summary>
        /// Input System callback for the Move action.
        /// </summary>
        public void OnMove(InputValue value)
        {
            if (!_movementLocked)
                _moveInput = value.Get<Vector2>();
        }

        /// <summary>
        /// Called via SendMessage by Dialogue_Manager to lock movement during dialogue.
        /// </summary>
        public void LockMovement()
        {
            MovementLocked = true;
        }

        /// <summary>
        /// Called via SendMessage by Dialogue_Manager to unlock movement after dialogue.
        /// </summary>
        public void UnlockMovement()
        {
            MovementLocked = false;
        }
    }
}
