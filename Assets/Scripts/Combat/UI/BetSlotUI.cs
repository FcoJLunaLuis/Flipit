using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Representa un slot individual de ficha en la UI de selección de apuestas.
/// </summary>
public class BetSlotUI : MonoBehaviour
{
    [SerializeField] private Image _imagenFicha;
    [SerializeField] private TextMeshProUGUI _nombreTexto;
    [SerializeField] private Image _fondoSlot;
    [SerializeField] private Button _boton;

    private FichaData _ficha;
    private Action<FichaData, BetSlotUI> _onClickCallback;

    public FichaData Ficha => _ficha;

    public void Configurar(FichaData ficha, Action<FichaData, BetSlotUI> onClick)
    {
        _ficha = ficha;
        _onClickCallback = onClick;

        if (_nombreTexto != null)
            _nombreTexto.text = ficha.nombre;

        if (_boton == null)
            _boton = GetComponent<Button>();

        if (_boton != null)
        {
            _boton.onClick.RemoveAllListeners();
            _boton.onClick.AddListener(OnClick);
        }
    }

    public void SetColor(Color color)
    {
        if (_fondoSlot != null)
            _fondoSlot.color = color;
    }

    private void OnClick()
    {
        _onClickCallback?.Invoke(_ficha, this);
    }
}
