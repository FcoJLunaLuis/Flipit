using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controlador de jugador simple para la escena de integración.
/// WASD para moverse, sin dependencias de otros assemblies.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class SimplePlayerController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;

    private CharacterController _cc;
    private float _verticalVelocity;
    private bool _movementLocked;

    public bool MovementLocked
    {
        get => _movementLocked;
        set => _movementLocked = value;
    }

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (_movementLocked) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Gravity
        if (_cc.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -0.5f;
        else
            _verticalVelocity += -9.81f * Time.deltaTime;

        // Movement input
        Vector2 input = Vector2.zero;
        if (keyboard.wKey.isPressed) input.y += 1f;
        if (keyboard.sKey.isPressed) input.y -= 1f;
        if (keyboard.aKey.isPressed) input.x -= 1f;
        if (keyboard.dKey.isPressed) input.x += 1f;

        Vector3 move = new Vector3(input.x, 0f, input.y).normalized;
        move *= _moveSpeed * Time.deltaTime;
        move.y = _verticalVelocity * Time.deltaTime;

        _cc.Move(move);
    }
}
