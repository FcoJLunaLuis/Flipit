using UnityEngine;

/// <summary>
/// Define el área de juego del combate: suelo sólido con BoxCollider
/// y radio límite invisible que contiene las fichas.
/// </summary>
public class CombatArena : MonoBehaviour
{
    [Header("Suelo")]
    [SerializeField] private float _sueloTamano = 5f;
    [SerializeField] private float _sueloGrosor = 0.1f;

    [Header("Radio Límite")]
    [Tooltip("Radio máximo donde las fichas pueden estar. Configurable para pruebas.")]
    [SerializeField] private float _radioLimite = 2f;

    [Header("Debug")]
    [SerializeField] private bool _mostrarGizmos = true;
    [SerializeField] private Color _colorGizmoRadio = new Color(1f, 0f, 0f, 0.3f);

    private GameObject _suelo;
    private Vector3 _centroArena;

    public float RadioLimite => _radioLimite;
    public Vector3 CentroArena => _centroArena;

    /// <summary>
    /// Inicializa la arena centrada en la posición dada (centro de la torre).
    /// </summary>
    public void InicializarArena(Vector3 centro)
    {
        _centroArena = centro;
        _centroArena.y = transform.position.y;

        CrearSuelo();
    }

    private void CrearSuelo()
    {
        if (_suelo != null) Destroy(_suelo);

        _suelo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _suelo.name = "Suelo_Arena";
        _suelo.transform.SetParent(transform);
        _suelo.transform.position = new Vector3(_centroArena.x, _centroArena.y - _sueloGrosor / 2f, _centroArena.z);
        _suelo.transform.localScale = new Vector3(_sueloTamano, _sueloGrosor, _sueloTamano);

        // Remover MeshCollider si existe y usar BoxCollider
        var meshCol = _suelo.GetComponent<MeshCollider>();
        if (meshCol != null) Object.Destroy(meshCol);

        var boxCol = _suelo.GetComponent<BoxCollider>();
        if (boxCol == null) boxCol = _suelo.AddComponent<BoxCollider>();

        // Material gris oscuro
        var renderer = _suelo.GetComponent<Renderer>();
        if (renderer != null)
        {
            var propBlock = new MaterialPropertyBlock();
            propBlock.SetColor("_BaseColor", new Color(0.3f, 0.3f, 0.3f, 1f));
            renderer.SetPropertyBlock(propBlock);
        }
    }

    /// <summary>
    /// Mantiene las fichas dentro del radio. Llamar en FixedUpdate.
    /// Clampea la posición horizontal de cada ficha al radio límite.
    /// </summary>
    public void ContenerFicha(PhysicsChip chip)
    {
        if (chip == null || chip.Rb == null || chip.Rb.isKinematic) return;

        Vector3 pos = chip.transform.position;
        Vector3 offset = new Vector3(pos.x - _centroArena.x, 0f, pos.z - _centroArena.z);

        if (offset.magnitude > _radioLimite)
        {
            Vector3 clamped = offset.normalized * _radioLimite;
            chip.transform.position = new Vector3(
                _centroArena.x + clamped.x,
                pos.y,
                _centroArena.z + clamped.z
            );

            // Anular velocidad horizontal para que no siga empujando
            Vector3 vel = chip.Rb.linearVelocity;
            chip.Rb.linearVelocity = new Vector3(0f, vel.y, 0f);
        }
    }

    private void OnDrawGizmos()
    {
        if (!_mostrarGizmos) return;

        Vector3 centro = Application.isPlaying ? _centroArena : transform.position;

        // Dibujar radio
        Gizmos.color = _colorGizmoRadio;
        DrawCircleGizmo(centro, _radioLimite, 32);

        // Dibujar suelo
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
        Gizmos.DrawCube(centro, new Vector3(_sueloTamano, 0.05f, _sueloTamano));
    }

    private void DrawCircleGizmo(Vector3 centro, float radio, int segmentos)
    {
        float paso = 360f / segmentos;
        Vector3 prev = centro + new Vector3(radio, 0f, 0f);

        for (int i = 1; i <= segmentos; i++)
        {
            float angulo = i * paso * Mathf.Deg2Rad;
            Vector3 next = centro + new Vector3(Mathf.Cos(angulo) * radio, 0f, Mathf.Sin(angulo) * radio);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}
