using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Resultado de un lanzamiento en el combate.
/// Almacena los datos del minijuego y las fichas afectadas.
/// PuntoDeImpacto es ahora un Vector3 en espacio mundo.
/// </summary>
[Serializable]
public class ThrowResult
{
    /// <summary>Posición de impacto en espacio mundo (etapa 1: mira).</summary>
    public Vector3 PuntoDeImpacto { get; private set; }

    /// <summary>Fuerza del lanzamiento normalizada 0-1 (etapa 2: barra).</summary>
    public float Fuerza { get; private set; }

    /// <summary>Dispersión en unidades mundo (etapa 3: círculo). Mayor = más disperso.</summary>
    public float Dispersion { get; private set; }

    /// <summary>Lista de slots de la torre que fueron volteados con este lanzamiento.</summary>
    public List<TowerSlot> FichasVolteadas { get; private set; }

    /// <summary>Quién realizó el lanzamiento.</summary>
    public TowerSlot.SlotOwner Lanzador { get; private set; }

    public int CantidadVolteadas => FichasVolteadas != null ? FichasVolteadas.Count : 0;

    public ThrowResult(Vector3 puntoDeImpacto, float fuerza, float dispersion, TowerSlot.SlotOwner lanzador)
    {
        PuntoDeImpacto = puntoDeImpacto;
        Fuerza = Mathf.Clamp01(fuerza);
        Dispersion = dispersion;
        Lanzador = lanzador;
        FichasVolteadas = new List<TowerSlot>();
    }

    public void AsignarFichasVolteadas(List<TowerSlot> fichas)
    {
        FichasVolteadas = fichas ?? new List<TowerSlot>();
    }

    public override string ToString()
    {
        return $"Lanzamiento [{Lanzador}] Impacto:{PuntoDeImpacto} Fuerza:{Fuerza:F2} Dispersión:{Dispersion:F2} Volteadas:{CantidadVolteadas}";
    }
}
