using UnityEngine;

/// <summary>
/// Componente de ficha física para el combate.
/// Detección de volteo por rotación en X:
/// - 90° = cara arriba = posición normal
/// - 270° (-90°) = cruz arriba = VOLTEADA
/// Se auto-verifica cada frame cuando tiene velocidad baja.
/// </summary>
public class PhysicsChip : MonoBehaviour
{
    [Header("Configuración Física")]
    [SerializeField] private float _masa = 1f;
    [SerializeField] private float _drag = 0.5f;
    [SerializeField] private float _angularDrag = 0.5f;
    [SerializeField] private PhysicsMaterial _physicsMaterial;
    [Tooltip("Multiplicador de gravedad extra para que caigan más rápido")]
    [SerializeField] private float _gravedadExtra = 16f;

    [Header("Volteo por Rotación")]
    [Tooltip("Tolerancia en grados alrededor de 270 para considerar volteada")]
    [SerializeField] private float _toleranciaVolteo = 30f;
    [Tooltip("Velocidad máxima para evaluar volteo (evita evaluar en el aire)")]
    [SerializeField] private float _velocidadMaximaEvaluar = 0.5f;

    [Header("Desaparición")]
    [SerializeField] private float _tiempoDesaparicion = 1f;

    private Rigidbody _rb;
    private TowerSlot _slot;
    private bool _evaluada;
    private bool _volteada;
    private bool _tocandoGround;
    private bool _desapareciendo;
    private bool _evaluacionActiva;

    public TowerSlot Slot => _slot;
    public Rigidbody Rb => _rb;
    public bool EstaEvaluada => _evaluada;
    public bool ResultadoVolteada => _volteada;

    public void Configurar(TowerSlot slot, float masa)
    {
        _slot = slot;
        _masa = masa;
        ConfigurarRigidbody();
    }

    /// <summary>
    /// Activa la auto-verificación continua. Llamar después del impacto.
    /// </summary>
    public void ActivarEvaluacion()
    {
        _evaluacionActiva = true;
        _evaluada = false;
        _volteada = false;
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

        if (_physicsMaterial != null)
        {
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.material = _physicsMaterial;
            }
        }
    }

    public void Liberar()
    {
        if (_rb != null)
            _rb.isKinematic = false;
    }

    public void Congelar()
    {
        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;
        }
    }

    private void FixedUpdate()
    {
        if (_rb == null || _rb.isKinematic || _desapareciendo) return;

        // Gravedad extra cuando no toca ground
        if (!_tocandoGround)
        {
            _rb.AddForce(Vector3.down * _gravedadExtra, ForceMode.Acceleration);
        }
    }

    private void Update()
    {
        if (!_evaluacionActiva || _evaluada || _desapareciendo) return;
        if (_rb == null || _rb.isKinematic) return;

        // Solo evaluar cuando la ficha está quieta
        float velocidad = _rb.linearVelocity.magnitude + _rb.angularVelocity.magnitude;
        if (velocidad > _velocidadMaximaEvaluar) return;

        // Verificar si está volteada por rotación
        if (EstaVolteada())
        {
            _volteada = true;
            _evaluada = true;

            if (_slot != null)
            {
                _slot.Voltear();
            }

            IniciarDesaparicion();
            Debug.Log($"[PhysicsChip] {name} VOLTEADA (rotX:{transform.eulerAngles.x:F1}) - desapareciendo.");
        }
    }

    /// <summary>
    /// Verifica si la ficha está volteada por su rotación en X.
    /// Posición normal = 90°. Volteada = 270° (±tolerancia).
    /// </summary>
    public bool EstaVolteada()
    {
        float rotX = transform.eulerAngles.x;

        // Normalizar a 0-360
        if (rotX < 0f) rotX += 360f;

        // Volteada si está cerca de 270° (cara abajo)
        float diferenciaA270 = Mathf.Abs(rotX - 270f);
        if (diferenciaA270 > 180f) diferenciaA270 = 360f - diferenciaA270;

        return diferenciaA270 < _toleranciaVolteo;
    }

    /// <summary>
    /// Evaluación manual forzada (llamada por FlipDetector en timeout).
    /// </summary>
    public void Evaluar()
    {
        if (_evaluada || _desapareciendo) return;

        _volteada = EstaVolteada();
        _evaluada = true;

        if (_volteada && _slot != null)
        {
            _slot.Voltear();
            IniciarDesaparicion();
            Debug.Log($"[PhysicsChip] {name} VOLTEADA (forzado, rotX:{transform.eulerAngles.x:F1}) - desapareciendo.");
        }
    }

    public void ResetearEvaluacion()
    {
        _evaluada = false;
        _volteada = false;
        _evaluacionActiva = false;
    }

    /// <summary>
    /// Reacción en cadena al ser golpeada.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (_rb == null || _rb.isKinematic || _desapareciendo) return;

        var otroRb = collision.rigidbody;
        if (otroRb == null) return;

        bool esFichaLanzada = collision.gameObject.CompareTag("FichaLanzamiento");
        bool esOtraFichaTorre = collision.gameObject.CompareTag("FichaTorre");

        if (!esFichaLanzada && !esOtraFichaTorre) return;

        float velocidadImpacto = otroRb.linearVelocity.magnitude;
        if (velocidadImpacto < 0.5f) return;

        Vector3 direccion = transform.position - collision.transform.position;
        direccion.y = 0f;
        if (direccion.sqrMagnitude < 0.001f)
        {
            direccion = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        }
        direccion.Normalize();

        float multiplicador = esFichaLanzada ? 1.5f : 0.8f;
        float fuerza = velocidadImpacto * multiplicador;

        _rb.AddForce((direccion + Vector3.up * 0.3f).normalized * fuerza, ForceMode.Impulse);
        _rb.AddTorque(Vector3.Cross(Vector3.up, direccion) * fuerza * 0.3f, ForceMode.Impulse);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            _tocandoGround = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            _tocandoGround = false;
    }

    private void IniciarDesaparicion()
    {
        if (_desapareciendo) return;
        _desapareciendo = true;
        Congelar();
        StartCoroutine(DesvanecerYDestruir());
    }

    private System.Collections.IEnumerator DesvanecerYDestruir()
    {
        var renderer = GetComponentInChildren<Renderer>();
        float tiempo = 0f;

        while (tiempo < _tiempoDesaparicion)
        {
            tiempo += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, tiempo / _tiempoDesaparicion);

            if (renderer != null)
            {
                var propBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(propBlock);
                Color color = propBlock.GetColor("_BaseColor");
                if (color == Color.clear) color = Color.white;
                color.a = alpha;
                propBlock.SetColor("_BaseColor", color);
                renderer.SetPropertyBlock(propBlock);
            }

            yield return null;
        }

        gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        bool volteada = EstaVolteada();
        Gizmos.color = volteada ? Color.red : Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
        Gizmos.color = volteada ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position, -transform.forward * 2f);
    }
}
