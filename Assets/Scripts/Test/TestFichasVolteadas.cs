using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Test de raycast en fichas.
/// Clic en la escena → instancia FichaTorre en esa posición con rotación aleatoria (cara o cruz).
/// La ficha se vuelve roja si raycast cara (forward) toca Ground, azul si cruz (-forward) toca Ground.
/// </summary>
public class TestFichasVolteadas : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject _fichaTorrePrefab;

    [Header("Raycast Config")]
    [SerializeField] private float _raycastDistancia = 5f;

    [Header("Colores")]
    [SerializeField] private Color _colorCara = Color.red;
    [SerializeField] private Color _colorCruz = Color.blue;
    [SerializeField] private Color _colorNeutro = Color.white;

    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || _mainCamera == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            InstanciarFichaEnClick(mouse.position.ReadValue());
        }

        // Actualizar colores de todas las fichas instanciadas
        ActualizarColoresFichas();
    }

    private void InstanciarFichaEnClick(Vector2 screenPos)
    {
        if (_fichaTorrePrefab == null) return;

        Ray ray = _mainCamera.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0f));
        Plane plano = new Plane(Vector3.up, Vector3.zero);
        float distancia;

        if (plano.Raycast(ray, out distancia))
        {
            Vector3 punto = ray.GetPoint(distancia);

            // Rotación aleatoria: 90 (cara arriba) o 270 (cruz arriba, equivale a -90/270)
            float rotX = Random.Range(0, 2) == 0 ? 90f : 270f;
            Quaternion rotacion = Quaternion.Euler(rotX, 0f, 0f);

            var ficha = Instantiate(_fichaTorrePrefab, punto + Vector3.up * 0.5f, rotacion, transform);
            ficha.name = $"TestFicha_{rotX}_{transform.childCount}";

            Debug.Log($"[TestFichas] Instanciada en {punto} con rotX={rotX}");
        }
    }

    private void ActualizarColoresFichas()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var ficha = transform.GetChild(i);
            var renderer = ficha.GetComponentInChildren<Renderer>();
            if (renderer == null) continue;

            bool caraTocaGround = RaycastTocaGround(ficha, ficha.forward);
            bool cruzTocaGround = RaycastTocaGround(ficha, -ficha.forward);

            Color color = _colorNeutro;
            if (caraTocaGround) color = _colorCara;
            else if (cruzTocaGround) color = _colorCruz;

            var propBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_BaseColor", color);
            renderer.SetPropertyBlock(propBlock);
        }
    }

    private bool RaycastTocaGround(Transform ficha, Vector3 direccion)
    {
        RaycastHit hit;
        if (Physics.Raycast(ficha.position, direccion, out hit, _raycastDistancia))
        {
            return hit.collider.CompareTag("Ground");
        }
        return false;
    }
}
