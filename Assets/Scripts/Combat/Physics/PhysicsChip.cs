using UnityEngine;

/// <summary>
/// Componente de ficha física para el combate.
/// Maneja el Rigidbody, detección de volteo por raycast en forward,
/// y detección de reposo (velocidad bajo umbral).
/// </summary>
public class PhysicsChip : MonoBehaviour
{
    [Header("Configuración Física")]
    [SerializeField] private float _masa = 1f;
    [SerializeField] private float _drag = 0.5f;
    [SerializeField] private float _angularDrag = 0.5f;

    [Header("Detección de Reposo")]
    [SerializeField] private float _umbralVelocidad = 0.05f;
    [SerializeField] private float _umbralAngular = 0.1f;

    [Header("Debug")]
    [SerializeField] private float _raycastDistancia = 0.5f;

    private Rigidbody _rb;
    private TowerSlot _slot;
    private bool _evaluada;
    private bool _volteada;

    public TowerSlot Slot => _slot;
    public Rigidbody Rb => _rb;
    public bool EstaEvaluada => _evaluada;
    public bool ResultadoVolteada => _volteada;

    public void Configurar(TowerSlot slot, float masa)
    {
        _slot = slot;
        _masa = masa;

        ConfigurarRigidbody();
        ConfigurarCollider();
    }

    private void ConfigurarRigidbody()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
            _rb = gameObject.AddComponent<Rigidbody>();

        _rb.mass = _masa;
        _rb.linearDamping = _drag;
        _rb.angularDamping = _angularDrag;
        _rb.isKinematic = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void ConfigurarCollider()
    {
        // No tocar colliders — el prefab FichaTorre ya viene con el collider correcto
    }

    /// <summary>
    /// Libera la ficha para que las físicas actúen.
    /// </summary>
    public void Liberar()
    {
        if (_rb != null)
            _rb.isKinematic = false;
    }

    /// <summary>
    /// Congela la ficha (kinematic).
    /// </summary>
    public void Congelar()
    {
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// Verifica si la ficha está volteada usando el forward del transform.
    /// Si forward apunta mayormente hacia abajo (dot con Vector3.down > umbral), está volteada.
    /// Con rotación inicial de 90° en X, forward apunta hacia arriba cuando está "boca arriba".
    /// Solo cuenta como volteada si está casi completamente boca abajo.
    /// </summary>
    public bool EstaVolteada()
    {
        float dot = Vector3.Dot(transform.forward, Vector3.down);
        return dot > 0.7f;
    }

    /// <summary>
    /// Verifica si la ficha está en reposo (velocidad linear y angular bajo umbral).
    /// </summary>
    public bool EstaEnReposo()
    {
        if (_rb == null) return true;
        if (_rb.isKinematic) return true;

        return _rb.linearVelocity.magnitude < _umbralVelocidad &&
               _rb.angularVelocity.magnitude < _umbralAngular;
    }

    /// <summary>
    /// Evalúa el estado final de la ficha (llamado por FlipDetector cuando se alcanza reposo).
    /// </summary>
    public void Evaluar()
    {
        _volteada = EstaVolteada();
        _evaluada = true;

        if (_volteada && _slot != null)
        {
            _slot.Voltear();
        }
    }

    /// <summary>
    /// Resetea el estado de evaluación para un nuevo turno.
    /// </summary>
    public void ResetearEvaluacion()
    {
        _evaluada = false;
        _volteada = false;
    }

    private void OnDrawGizmos()
    {
        // Dibujar raycast forward
        Gizmos.color = EstaVolteada() ? Color.red : Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * _raycastDistancia);
    }
}
