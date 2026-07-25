using UnityEngine;

/// <summary>
/// Define el área de juego del combate.
/// Los muros se asignan desde el Inspector y se acomodan automáticamente
/// al polígono guía con el método AcomodarMuros() (botón en Editor).
/// El gizmo muestra el polígono y se adapta a la cantidad de muros asignados.
/// </summary>
public class CombatArena : MonoBehaviour
{
    [Header("Suelo")]
    [Tooltip("Referencia al GameObject del suelo (hijo de este objeto). Configurar manualmente.")]
    [SerializeField] private GameObject _suelo;

    [Header("Muros")]
    [Tooltip("Arrastra aquí los GameObjects con BoxCollider que servirán como muros.")]
    [SerializeField] private GameObject[] _muros;

    [Header("Configuración del Polígono")]
    [Tooltip("Radio del polígono (distancia del centro a cada muro)")]
    [SerializeField] private float _radioPoligono = 5f;
    [Tooltip("Altura de los muros")]
    [SerializeField] private float _alturaMuros = 3f;

    [Header("Debug - Gizmo")]
    [SerializeField] private bool _mostrarGizmos = true;
    [SerializeField] private Color _colorGizmo = new Color(1f, 0f, 0f, 0.3f);

    private Vector3 _centroArena;

    public Vector3 CentroArena => _centroArena;
    public float RadioPoligono => _radioPoligono;
    public int CantidadLados => _muros != null ? _muros.Length : 0;

    /// <summary>
    /// Inicializa el centro de la arena.
    /// </summary>
    public void InicializarArena(Vector3 centro)
    {
        _centroArena = centro;
        _centroArena.y = transform.position.y;
    }

    /// <summary>
    /// Método vacío — los muros físicos se encargan de contener.
    /// </summary>
    public void ContenerFicha(PhysicsChip chip)
    {
    }

    /// <summary>
    /// Posiciona y rota cada muro al centro de cada lado del polígono.
    /// Ajusta el BoxCollider de cada muro al ancho del lado.
    /// Llamar desde el Editor (botón custom) o desde código.
    /// </summary>
    public void AcomodarMuros()
    {
        if (_muros == null || _muros.Length == 0)
        {
            Debug.LogWarning("[CombatArena] No hay muros asignados.");
            return;
        }

        int lados = _muros.Length;
        float anguloPorLado = 360f / lados;
        Vector3 centro = transform.position;

        for (int i = 0; i < lados; i++)
        {
            if (_muros[i] == null) continue;

            float angulo = i * anguloPorLado * Mathf.Deg2Rad;
            float siguienteAngulo = (i + 1) * anguloPorLado * Mathf.Deg2Rad;

            // Punto medio del lado
            float midX = (_radioPoligono * Mathf.Cos(angulo) + _radioPoligono * Mathf.Cos(siguienteAngulo)) / 2f;
            float midZ = (_radioPoligono * Mathf.Sin(angulo) + _radioPoligono * Mathf.Sin(siguienteAngulo)) / 2f;

            // Posicionar en el punto medio del lado, a media altura
            _muros[i].transform.position = new Vector3(
                centro.x + midX,
                centro.y + _alturaMuros / 2f,
                centro.z + midZ
            );

            // Rotar para que el muro quede perpendicular al radio (mirando hacia el centro)
            Vector3 direccionAlCentro = (centro - _muros[i].transform.position);
            direccionAlCentro.y = 0f;
            if (direccionAlCentro.sqrMagnitude > 0.001f)
            {
                _muros[i].transform.rotation = Quaternion.LookRotation(direccionAlCentro.normalized, Vector3.up);
            }

            // Ajustar el BoxCollider si existe
            var boxCol = _muros[i].GetComponent<BoxCollider>();
            if (boxCol != null)
            {
                float v1X = _radioPoligono * Mathf.Cos(angulo);
                float v1Z = _radioPoligono * Mathf.Sin(angulo);
                float v2X = _radioPoligono * Mathf.Cos(siguienteAngulo);
                float v2Z = _radioPoligono * Mathf.Sin(siguienteAngulo);
                float ancho = Vector2.Distance(new Vector2(v1X, v1Z), new Vector2(v2X, v2Z));

                boxCol.size = new Vector3(ancho, _alturaMuros, 0.2f);
                boxCol.center = Vector3.zero;
            }
        }

        Debug.Log($"[CombatArena] {lados} muros acomodados. Radio:{_radioPoligono} Altura:{_alturaMuros}");
    }

    private void OnDrawGizmos()
    {
        if (!_mostrarGizmos) return;

        Vector3 centro = Application.isPlaying ? _centroArena : transform.position;
        int lados = (_muros != null && _muros.Length > 2) ? _muros.Length : 8;
        float anguloPorLado = 360f / lados;

        Gizmos.color = _colorGizmo;

        for (int i = 0; i < lados; i++)
        {
            float angulo = i * anguloPorLado * Mathf.Deg2Rad;
            float siguienteAngulo = (i + 1) * anguloPorLado * Mathf.Deg2Rad;

            Vector3 v1 = centro + new Vector3(Mathf.Cos(angulo) * _radioPoligono, 0f, Mathf.Sin(angulo) * _radioPoligono);
            Vector3 v2 = centro + new Vector3(Mathf.Cos(siguienteAngulo) * _radioPoligono, 0f, Mathf.Sin(siguienteAngulo) * _radioPoligono);

            Gizmos.DrawLine(v1, v2);
            Gizmos.DrawLine(v1, v1 + Vector3.up * _alturaMuros);
            Gizmos.DrawLine(v2, v2 + Vector3.up * _alturaMuros);
            Gizmos.DrawLine(v1 + Vector3.up * _alturaMuros, v2 + Vector3.up * _alturaMuros);
        }
    }
}
