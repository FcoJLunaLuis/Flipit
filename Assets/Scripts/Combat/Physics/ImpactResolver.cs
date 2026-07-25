using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lanza una ficha física contra la torre.
/// Instancia el prefab FichaLanzable (pre-configurado con MeshCollider convex,
/// Rigidbody, y LaunchableChip) y le indica la velocidad de lanzamiento.
/// </summary>
public class ImpactResolver : MonoBehaviour
{
    [Header("Prefab")]
    [Tooltip("Prefab de la ficha lanzable (Assets/Prefabs/Ficha Combate/FichaLanzable)")]
    [SerializeField] private GameObject _fichaLanzablePrefab;

    [Header("Configuración de Lanzamiento")]
    [Tooltip("Velocidad base del lanzamiento (se multiplica por el valor del minijuego 0-1)")]
    [SerializeField] private float _velocidadBase = 20f;
    [Tooltip("Altura desde donde se lanza la ficha respecto al centro de la torre")]
    [SerializeField] private float _alturaLanzamiento = 5f;

    private GameObject _ultimaFichaLanzada;

    /// <summary>
    /// Lanza una ficha contra la torre.
    /// </summary>
    /// <param name="puntoImpactoMundo">Posición mundo donde debe caer (del AimPhase).</param>
    /// <param name="fuerza">Valor de fuerza 0-1 (del ForcePhase).</param>
    /// <param name="dispersion">Dispersión en unidades mundo (del PrecisionPhase).</param>
    /// <param name="centroTorre">Centro de la torre para calcular altura de spawn.</param>
    /// <param name="fichas">No usado, el impacto es por colisión física.</param>
    public void AplicarImpacto(Vector3 puntoImpactoMundo, float fuerza, float dispersion, Vector3 centroTorre, List<PhysicsChip> fichas)
    {
        if (_fichaLanzablePrefab == null)
        {
            Debug.LogWarning("[ImpactResolver] No hay prefab de ficha lanzable asignado.");
            return;
        }

        // Destruir ficha anterior si existe
        if (_ultimaFichaLanzada != null)
            Destroy(_ultimaFichaLanzada);

        // Aplicar dispersión al punto de impacto
        float offsetX = Random.Range(-dispersion, dispersion);
        float offsetZ = Random.Range(-dispersion, dispersion);
        Vector3 puntoFinal = puntoImpactoMundo + new Vector3(offsetX, 0f, offsetZ);

        // Posición de spawn: arriba del punto de impacto
        Vector3 posOrigen = new Vector3(puntoFinal.x, centroTorre.y + _alturaLanzamiento, puntoFinal.z);

        // Instanciar el prefab lanzable
        _ultimaFichaLanzada = Instantiate(_fichaLanzablePrefab, posOrigen, Quaternion.Euler(90f, 0f, 0f));

        // Lanzar
        var launchable = _ultimaFichaLanzada.GetComponent<LaunchableChip>();
        if (launchable != null)
        {
            float velocidadFinal = _velocidadBase * fuerza;
            launchable.Lanzar(Vector3.down * velocidadFinal);
        }

        Debug.Log($"[ImpactResolver] Ficha lanzada desde {posOrigen} hacia ({puntoFinal.x:F2}, {puntoFinal.z:F2}). Velocidad:{_velocidadBase * fuerza:F1}");
    }
}
