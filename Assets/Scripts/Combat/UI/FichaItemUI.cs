using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Componente visual para un item de ficha en las listas del resumen de combate.
/// Muestra nombre + rareza con un fondo coloreado según la rareza.
/// </summary>
public class FichaItemUI : MonoBehaviour
{
    [SerializeField] private Image _fondoImage;
    [SerializeField] private TextMeshProUGUI _nombreTexto;

    public void Configurar(FichaData ficha)
    {
        if (_nombreTexto != null)
            _nombreTexto.text = $"{ficha.nombre} ({ficha.rareza})";

        if (_fondoImage != null)
            _fondoImage.color = ObtenerColorPorRareza(ficha.rareza);
    }

    public static Color ObtenerColorPorRareza(Rareza rareza)
    {
        switch (rareza)
        {
            case Rareza.Comun: return new Color(0.5f, 0.5f, 0.5f, 0.8f);
            case Rareza.Raro: return new Color(0.2f, 0.4f, 0.9f, 0.8f);
            case Rareza.UltraRaro: return new Color(0.9f, 0.75f, 0.0f, 0.8f);
            default: return new Color(0.4f, 0.4f, 0.4f, 0.8f);
        }
    }
}
