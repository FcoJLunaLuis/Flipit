using UnityEngine;

/// <summary>
/// Ficha lanzable en combate. Se instancia, recibe velocidad, y al colisionar
/// con fichas de la torre (PhysicsChip) amplifica el impacto para que
/// las fichas salgan despedidas satisfactoriamente.
/// Pre-configurado en un prefab con MeshCollider(convex) y Rigidbody.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class LaunchableChip : MonoBehaviour
{
    [Header("Amplificación de Impacto")]
    [Tooltip("Multiplicador de fuerza aplicada a las fichas impactadas")]
    [SerializeField] private float _amplificacionFuerza = 5f;
    [Tooltip("Fuerza mínima para que se aplique amplificación")]
    [SerializeField] private float _velocidadMinimaImpacto = 1f;
    [Tooltip("Componente vertical adicional al impacto (para que las fichas salten)")]
    [SerializeField] private float _fuerzaVertical = 2f;

    [Header("Autodestrucción")]
    [SerializeField] private float _tiempoVida = 5f;

    private Rigidbody _rb;
    private bool _impactoRealizado;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Lanza la ficha con la velocidad indicada.
    /// Llamado por ImpactResolver después de instanciar.
    /// </summary>
    public void Lanzar(Vector3 velocidad)
    {
        _rb.isKinematic = false;
        _rb.linearVelocity = velocidad;
        _impactoRealizado = false;

        Destroy(gameObject, _tiempoVida);

        Debug.Log($"[LaunchableChip] Lanzada con velocidad {velocidad.magnitude:F1} m/s");
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Solo amplificar si tiene suficiente velocidad
        if (_rb.linearVelocity.magnitude < _velocidadMinimaImpacto) return;

        var chip = collision.gameObject.GetComponent<PhysicsChip>();
        if (chip == null) return;

        // Calcular dirección del impacto
        Vector3 puntoContacto = collision.contacts[0].point;
        Vector3 direccionImpacto = (collision.transform.position - puntoContacto).normalized;

        // Añadir componente vertical para que las fichas salten
        direccionImpacto.y += _fuerzaVertical;
        direccionImpacto.Normalize();

        // Amplificar la fuerza basada en la velocidad de impacto
        float fuerzaImpacto = _rb.linearVelocity.magnitude * _amplificacionFuerza;

        // Aplicar fuerza a la ficha impactada
        if (chip.Rb != null && !chip.Rb.isKinematic)
        {
            chip.Rb.AddForce(direccionImpacto * fuerzaImpacto, ForceMode.Impulse);
        }

        Debug.Log($"[LaunchableChip] Impacto con {chip.name}. Fuerza aplicada: {fuerzaImpacto:F1}");
    }
}
