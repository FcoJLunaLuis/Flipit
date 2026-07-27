using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Resuelve el resultado de un lanzamiento: determina cuántas y cuáles fichas se voltean.
/// Lógica pura sin dependencia de MonoBehaviour.
/// </summary>
public static class ThrowResolver
{
    /// <summary>
    /// Dado un resultado de minijuego y la torre actual, determina qué fichas se voltean.
    /// Las fichas más cercanas al punto de impacto (que no estén ya volteadas) son las afectadas.
    /// </summary>
    /// <param name="puntoImpacto">Posición de impacto normalizada en el espacio de la torre.</param>
    /// <param name="fuerza">Valor de fuerza (0-1).</param>
    /// <param name="dispersion">Valor de dispersión (0-1, menor = más preciso).</param>
    /// <param name="torre">Lista de slots de la torre.</param>
    /// <returns>Lista de slots volteados.</returns>
    public static List<TowerSlot> ResolverLanzamiento(Vector2 puntoImpacto, float fuerza, float dispersion, List<TowerSlot> torre)
    {
        int cantidadAVoltear = CalcularCantidadAVoltear(fuerza, dispersion);

        var fichasSinVoltear = torre.Where(slot => !slot.EstaVolteada).ToList();

        if (fichasSinVoltear.Count == 0) return new List<TowerSlot>();

        // Aplicar dispersión al punto de impacto
        Vector2 puntoFinal = AplicarDispersion(puntoImpacto, dispersion);

        // Ordenar por proximidad al punto de impacto
        var ordenadas = fichasSinVoltear
            .OrderBy(slot => DistanciaAlSlot(puntoFinal, slot))
            .ToList();

        // Tomar las más cercanas
        int cantidad = Mathf.Min(cantidadAVoltear, ordenadas.Count);
        var fichasVolteadas = ordenadas.Take(cantidad).ToList();

        // Marcar como volteadas
        foreach (var slot in fichasVolteadas)
        {
            slot.Voltear();
        }

        return fichasVolteadas;
    }

    /// <summary>
    /// Calcula cuántas fichas se voltean: 1, 2 o 3 según efectividad.
    /// </summary>
    public static int CalcularCantidadAVoltear(float fuerza, float dispersion)
    {
        float efectividad = (fuerza + (1f - dispersion)) / 2f;

        if (efectividad >= 0.75f) return 3;
        if (efectividad >= 0.45f) return 2;
        return 1;
    }

    private static Vector2 AplicarDispersion(Vector2 punto, float dispersion)
    {
        // La dispersión agrega un offset aleatorio proporcional al valor
        float offsetX = Random.Range(-dispersion, dispersion) * 0.5f;
        float offsetY = Random.Range(-dispersion, dispersion) * 0.5f;
        return punto + new Vector2(offsetX, offsetY);
    }

    private static float DistanciaAlSlot(Vector2 punto, TowerSlot slot)
    {
        // Convertir posición lógica del slot a espacio normalizado
        Vector2 posSlot = new Vector2(slot.PosicionEnTorre.x, slot.PosicionEnTorre.y);
        return Vector2.Distance(punto, posSlot);
    }
}
