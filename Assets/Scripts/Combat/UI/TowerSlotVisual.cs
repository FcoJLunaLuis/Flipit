using UnityEngine;

/// <summary>
/// Visual individual de un slot en la torre.
/// Controla la apariencia y animación de volteo de una ficha.
/// </summary>
public class TowerSlotVisual : MonoBehaviour
{
    private TowerSlot _slot;
    private Color _colorOriginal;
    private Color _colorVolteada;
    private float _velocidadVolteo;
    private bool _animandoVolteo;
    private float _anguloTarget;
    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;

    public TowerSlot Slot => _slot;

    public void Configurar(TowerSlot slot, Color colorDueno, Color colorVolteada, float velocidadVolteo)
    {
        _slot = slot;
        _colorOriginal = colorDueno;
        _colorVolteada = colorVolteada;
        _velocidadVolteo = velocidadVolteo;
        _animandoVolteo = false;

        _renderer = GetComponentInChildren<Renderer>();
        _propBlock = new MaterialPropertyBlock();

        AplicarColor(_colorOriginal);
    }

    public void ActualizarEstado()
    {
        if (_slot == null) return;

        if (_slot.EstaVolteada && !_animandoVolteo)
        {
            AplicarColor(_colorVolteada);
        }
    }

    public void AnimarVolteo()
    {
        _animandoVolteo = true;
        _anguloTarget = 180f;
    }

    private void Update()
    {
        if (!_animandoVolteo) return;

        float anguloActual = transform.localEulerAngles.x;
        float nuevoAngulo = Mathf.MoveTowards(anguloActual, _anguloTarget, _velocidadVolteo * Time.deltaTime * 100f);
        transform.localEulerAngles = new Vector3(nuevoAngulo, transform.localEulerAngles.y, transform.localEulerAngles.z);

        if (Mathf.Abs(nuevoAngulo - _anguloTarget) < 0.1f)
        {
            _animandoVolteo = false;
            transform.localEulerAngles = new Vector3(_anguloTarget, transform.localEulerAngles.y, transform.localEulerAngles.z);
            AplicarColor(_colorVolteada);
        }
    }

    private void AplicarColor(Color color)
    {
        if (_renderer == null) return;

        _renderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor("_BaseColor", color);
        _renderer.SetPropertyBlock(_propBlock);
    }
}
