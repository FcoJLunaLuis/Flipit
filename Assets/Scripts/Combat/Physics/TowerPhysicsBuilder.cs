using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Genera la torre de fichas con físicas reales.
/// Instancia Ficha.fbx con PhysicsChip + Rigidbody apiladas verticalmente.
/// Las fichas inician como kinematic y se liberan solo cuando se llama LiberarTorre().
/// </summary>
public class TowerPhysicsBuilder : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private GameObject _fichaPrefab;
    [SerializeField] private float _espaciadoVertical = 7f;
    [SerializeField] private float _masaBase = 1f;
    [SerializeField] private float _offsetSueloY = 1f;

    [Header("Delay antes del primer turno")]
    [SerializeField] private float _delayPostConstruccion = 1.5f;

    [Header("Colores por dueño")]
    [SerializeField] private Color _colorJugador = new Color(0.2f, 0.6f, 1f);
    [SerializeField] private Color _colorNPC = new Color(1f, 0.4f, 0.3f);

    private List<PhysicsChip> _fichas = new List<PhysicsChip>();
    private Vector3 _centroTorre;
    private bool _torreLibre;

    public List<PhysicsChip> Fichas => _fichas;
    public Vector3 CentroTorre => _centroTorre;
    public bool TorreLibre => _torreLibre;
    public float DelayPostConstruccion => _delayPostConstruccion;

    /// <summary>
    /// Construye la torre con fichas físicas apiladas.
    /// Las fichas quedan kinematic hasta que se llame LiberarTorre().
    /// La escala del modelo se ajusta para que quepan bien.
    /// </summary>
    public void ConstruirTorre(List<TowerSlot> slots)
    {
        LimpiarTorre();
        _torreLibre = false;

        if (_fichaPrefab == null)
        {
            Debug.LogWarning("[TowerPhysicsBuilder] No hay prefab de ficha asignado.");
            return;
        }

        // Calcular altura real de cada ficha (el modelo es 0.03 de alto con Y)
        float alturaFicha = 0.03f;
        float pasoY = alturaFicha + _espaciadoVertical;

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];

            var go = Instantiate(_fichaPrefab, transform);

            // NO escalar - usar tamaño original del modelo
            // Posicionar apilada
            float posY = _offsetSueloY + (i * pasoY);
            go.transform.localPosition = new Vector3(0f, posY, 0f);

            // Rotar 90 grados en X para que la ficha quede plana (boca arriba)
            // El modelo viene de canto, esta rotación lo acuesta correctamente
            go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            go.name = $"Ficha_{slot.Ficha.nombre}_{i}";

            // Agregar PhysicsChip
            var chip = go.AddComponent<PhysicsChip>();
            float masa = _masaBase * (slot.Ficha.peso / 10f);
            chip.Configurar(slot, Mathf.Max(masa, 0.5f));

            // Aplicar color según dueño
            AplicarColor(go, slot.Dueno);

            _fichas.Add(chip);
        }

        // Calcular centro de la torre (punto medio en Y)
        float alturaTotal = slots.Count * pasoY;
        _centroTorre = transform.position + new Vector3(0f, alturaTotal / 2f, 0f);

        Debug.Log($"[TowerPhysicsBuilder] Torre construida: {slots.Count} fichas. PasoY:{pasoY:F4} Centro:{_centroTorre}");
    }

    /// <summary>
    /// Libera todas las fichas (isKinematic = false) para que las físicas actúen.
    /// Llamar justo antes del primer impacto, NO al construir la torre.
    /// </summary>
    public void LiberarTorre()
    {
        if (_torreLibre) return;

        foreach (var chip in _fichas)
        {
            if (chip != null)
                chip.Liberar();
        }

        _torreLibre = true;
        Debug.Log("[TowerPhysicsBuilder] Torre liberada - físicas activas.");
    }

    /// <summary>
    /// Congela todas las fichas (kinematic).
    /// </summary>
    public void CongelarTorre()
    {
        foreach (var chip in _fichas)
        {
            if (chip != null)
                chip.Congelar();
        }
        _torreLibre = false;
    }

    /// <summary>
    /// Obtiene las fichas que aún no han sido evaluadas como volteadas.
    /// </summary>
    public List<PhysicsChip> ObtenerFichasSinVoltear()
    {
        var sinVoltear = new List<PhysicsChip>();
        foreach (var chip in _fichas)
        {
            if (chip != null && chip.Slot != null && !chip.Slot.EstaVolteada)
                sinVoltear.Add(chip);
        }
        return sinVoltear;
    }

    /// <summary>
    /// Obtiene todas las fichas que fueron volteadas.
    /// </summary>
    public List<PhysicsChip> ObtenerFichasVolteadas()
    {
        var volteadas = new List<PhysicsChip>();
        foreach (var chip in _fichas)
        {
            if (chip != null && chip.Slot != null && chip.Slot.EstaVolteada)
                volteadas.Add(chip);
        }
        return volteadas;
    }

    public void LimpiarTorre()
    {
        foreach (var chip in _fichas)
        {
            if (chip != null && chip.gameObject != null)
                Destroy(chip.gameObject);
        }
        _fichas.Clear();
        _torreLibre = false;
    }

    private void AplicarColor(GameObject go, TowerSlot.SlotOwner dueno)
    {
        var renderer = go.GetComponentInChildren<Renderer>();
        if (renderer == null) return;

        var propBlock = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_BaseColor", dueno == TowerSlot.SlotOwner.Jugador ? _colorJugador : _colorNPC);
        renderer.SetPropertyBlock(propBlock);
    }
}
