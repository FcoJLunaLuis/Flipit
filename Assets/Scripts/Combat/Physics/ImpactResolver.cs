using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lanza una ficha física contra la torre.
/// Recibe directamente la posición mundo del punto de impacto (del AimPhase),
/// la fuerza (del ForcePhase), y la dispersión en unidades mundo (del PrecisionPhase).
/// </summary>
public class ImpactResolver : MonoBehaviour
{
    [Header("Configuración de Lanzamiento")]
    [SerializeField] private GameObject _fichaLanzadoraPrefab;
    [SerializeField] private float _fuerzaBase = 5f;
    [SerializeField] private float _alturaLanzamiento = 2f;

    [Header("Ficha Lanzada")]
    [SerializeField] private float _masaFichaLanzada = 1.5f;
    [SerializeField] private float _tiempoVidaFichaLanzada = 5f;

    private GameObject _ultimaFichaLanzada;

    /// <summary>
    /// Lanza una ficha contra la torre usando posición mundo directa.
    /// </summary>
    /// <param name="puntoImpactoMundo">Posición mundo donde la ficha debe caer (del AimPhase).</param>
    /// <param name="fuerza">Valor de fuerza 0-1 (del ForcePhase).</param>
    /// <param name="dispersion">Dispersión en unidades mundo (del PrecisionPhase).</param>
    /// <param name="centroTorre">Centro de la torre (para calcular altura de spawn).</param>
    /// <param name="fichas">No usado directamente, el impacto es por colisión física.</param>
    public void AplicarImpacto(Vector3 puntoImpactoMundo, float fuerza, float dispersion, Vector3 centroTorre, List<PhysicsChip> fichas)
    {
        if (_fichaLanzadoraPrefab == null)
        {
            Debug.LogWarning("[ImpactResolver] No hay prefab de ficha lanzadora asignado.");
            return;
        }

        // Destruir ficha lanzada anterior
        if (_ultimaFichaLanzada != null)
            Destroy(_ultimaFichaLanzada);

        // Aplicar dispersión: offset aleatorio en X/Z proporcional al valor de dispersión
        float offsetX = Random.Range(-dispersion, dispersion);
        float offsetZ = Random.Range(-dispersion, dispersion);
        Vector3 puntoFinal = puntoImpactoMundo + new Vector3(offsetX, 0f, offsetZ);

        // Posición de spawn: arriba del punto de impacto
        Vector3 posOrigen = new Vector3(puntoFinal.x, centroTorre.y + _alturaLanzamiento, puntoFinal.z);

        // Instanciar ficha lanzadora (rotada 90 en X para quedar plana)
        _ultimaFichaLanzada = Instantiate(_fichaLanzadoraPrefab, posOrigen, Quaternion.Euler(90f, 0f, 0f));
        _ultimaFichaLanzada.name = "FichaLanzada";

        // Configurar Rigidbody
        var rb = _ultimaFichaLanzada.GetComponent<Rigidbody>();
        if (rb == null) rb = _ultimaFichaLanzada.AddComponent<Rigidbody>();
        rb.mass = _masaFichaLanzada;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Configurar Collider
        var existingColliders = _ultimaFichaLanzada.GetComponentsInChildren<Collider>();
        foreach (var col in existingColliders)
            Destroy(col);
        var boxCol = _ultimaFichaLanzada.AddComponent<BoxCollider>();
        boxCol.size = new Vector3(0.011f, 0.016f, 0.05f);

        // Lanzar hacia abajo con fuerza
        float magnitudFinal = _fuerzaBase * fuerza;
        rb.AddForce(Vector3.down * magnitudFinal, ForceMode.Impulse);

        // Destruir después de un tiempo
        Destroy(_ultimaFichaLanzada, _tiempoVidaFichaLanzada);

        Debug.Log($"[ImpactResolver] Ficha lanzada desde {posOrigen} hacia ({puntoFinal.x:F2}, {puntoFinal.z:F2}). Fuerza:{magnitudFinal:F1} Dispersión:{dispersion:F2}");
    }
}
