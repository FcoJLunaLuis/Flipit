using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representación visual de la torre de fichas en el combate.
/// Instancia el modelo 3D de ficha (Ficha.fbx) apilándolas verticalmente.
/// Las fichas volteadas cambian su rotación y color.
/// </summary>
public class TowerVisual : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private GameObject _fichaPrefab;
    [SerializeField] private float _espaciadoVertical = 0.15f;

    [Header("Feedback Visual")]
    [SerializeField] private Color _colorJugador = new Color(0.2f, 0.6f, 1f);
    [SerializeField] private Color _colorNPC = new Color(1f, 0.4f, 0.3f);
    [SerializeField] private Color _colorVolteada = new Color(0.5f, 0.5f, 0.5f, 0.6f);

    [Header("Animación")]
    [SerializeField] private float _velocidadVolteo = 5f;

    private List<TowerSlotVisual> _slotVisuals = new List<TowerSlotVisual>();
    private List<TowerSlot> _torreData;

    public void ConstruirTorreVisual(List<TowerSlot> torre)
    {
        LimpiarTorre();
        _torreData = torre;

        if (_fichaPrefab == null)
        {
            Debug.LogWarning("[TowerVisual] No hay prefab de ficha asignado.");
            return;
        }

        foreach (var slot in torre)
        {
            var go = Instantiate(_fichaPrefab, transform);

            // Apilar verticalmente: cada ficha sube en Y según su índice
            float posY = slot.PosicionEnTorre.y * _espaciadoVertical;
            go.transform.localPosition = new Vector3(0f, posY, 0f);

            // Configurar visual
            var slotVisual = go.AddComponent<TowerSlotVisual>();
            slotVisual.Configurar(slot, ObtenerColorPorDueno(slot.Dueno), _colorVolteada, _velocidadVolteo);

            _slotVisuals.Add(slotVisual);
        }

        Debug.Log($"[TowerVisual] Torre construida: {torre.Count} fichas apiladas.");
    }

    public void ActualizarVisuales()
    {
        foreach (var visual in _slotVisuals)
        {
            if (visual != null)
                visual.ActualizarEstado();
        }
    }

    public void AnimarVolteo(List<TowerSlot> fichasVolteadas)
    {
        foreach (var slotVolteado in fichasVolteadas)
        {
            var visual = _slotVisuals.Find(v => v != null && v.Slot == slotVolteado);
            if (visual != null)
            {
                visual.AnimarVolteo();
            }
        }
    }

    public void LimpiarTorre()
    {
        foreach (var visual in _slotVisuals)
        {
            if (visual != null && visual.gameObject != null)
                Destroy(visual.gameObject);
        }
        _slotVisuals.Clear();
    }

    private Color ObtenerColorPorDueno(TowerSlot.SlotOwner dueno)
    {
        return dueno == TowerSlot.SlotOwner.Jugador ? _colorJugador : _colorNPC;
    }
}
