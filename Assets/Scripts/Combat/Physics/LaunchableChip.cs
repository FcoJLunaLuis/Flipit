using UnityEngine;

/// <summary>
/// Ficha lanzable en combate. Se instancia, recibe velocidad, y al colisionar
/// con fichas de la torre (PhysicsChip) amplifica el impacto.
/// Si la ficha impactada está en el suelo (plana), aplica más fuerza horizontal
/// para que reaccione al golpe.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class LaunchableChip : MonoBehaviour
{
    [Header("Amplificación de Impacto")]
    [Tooltip("Multiplicador de fuerza aplicada a las fichas impactadas")]
    [SerializeField] private float _amplificacionFuerza = 15f;
    [Tooltip("Fuerza mínima para que se aplique amplificación")]
    [SerializeField] private float _velocidadMinimaImpacto = 0.5f;
    [Tooltip("Componente vertical adicional al impacto (para que las fichas salten)")]
    [SerializeField] private float _fuerzaVertical = 1f;

    [Header("Fichas en Suelo")]
    [Tooltip("Multiplicador extra cuando la ficha impactada está plana en el suelo")]
    [SerializeField] private float _amplificacionSuelo = 5f;
    [Tooltip("Altura máxima para considerar que una ficha está en el suelo")]
    [SerializeField] private float _alturaMaximaSuelo = 1f;

    [Header("Autodestrucción")]
    [SerializeField] private float _tiempoVida = 5f;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Lanzar(Vector3 velocidad)
    {
        _rb.isKinematic = false;
        _rb.linearVelocity = velocidad;

        Destroy(gameObject, _tiempoVida);

        Debug.Log($"[LaunchableChip] Lanzada con velocidad {velocidad.magnitude:F1} m/s");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_rb.linearVelocity.magnitude < _velocidadMinimaImpacto) return;

        // Solo impactar fichas de torre (por tag)
        if (!collision.gameObject.CompareTag("FichaTorre")) return;

        var chip = collision.gameObject.GetComponent<PhysicsChip>();
        if (chip == null) return;
        if (chip.Rb == null || chip.Rb.isKinematic) return;

        // Dirección: siempre horizontal, desde este objeto hacia la ficha objetivo (proyección XZ)
        Vector3 direccion = chip.transform.position - transform.position;
        direccion.y = 0f;
        if (direccion.sqrMagnitude < 0.001f)
        {
            direccion = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        }
        direccion = direccion.normalized;

        // Agregar componente vertical para que levante un poco
        Vector3 fuerzaFinal = direccion + Vector3.up * _fuerzaVertical;
        fuerzaFinal.Normalize();

        // Calcular magnitud: velocidad de impacto × amplificación
        float magnitud = _rb.linearVelocity.magnitude * _amplificacionFuerza;

        // Si está en el suelo, multiplicar extra
        bool fichaEnSuelo = chip.transform.position.y < _alturaMaximaSuelo;
        if (fichaEnSuelo)
        {
            magnitud *= _amplificacionSuelo;
        }

        // Aplicar fuerza
        chip.Rb.AddForce(fuerzaFinal * magnitud, ForceMode.Impulse);

        // Torque para que gire
        Vector3 torque = Vector3.Cross(Vector3.up, direccion) * magnitud * 0.5f;
        chip.Rb.AddTorque(torque, ForceMode.Impulse);

        // Desactivar collider después del impacto
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Debug.Log($"[LaunchableChip] Impacto con {chip.name}. Fuerza:{magnitud:F1} Dir:{fuerzaFinal} EnSuelo:{fichaEnSuelo}");
    }
}
