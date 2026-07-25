using System;
using UnityEngine;

/// <summary>
/// Representa una posición en la torre de fichas durante el combate.
/// Contiene la ficha, su dueño original, posición lógica y estado de volteo.
/// </summary>
[Serializable]
public class TowerSlot
{
    public enum SlotOwner { Jugador, NPC }
    public enum SlotState { BocaArriba, Volteada }

    public FichaData Ficha { get; private set; }
    public SlotOwner Dueno { get; private set; }
    public SlotState Estado { get; private set; }
    public Vector2Int PosicionEnTorre { get; private set; }

    public bool EstaVolteada => Estado == SlotState.Volteada;

    public TowerSlot(FichaData ficha, SlotOwner dueno, Vector2Int posicion)
    {
        Ficha = ficha;
        Dueno = dueno;
        Estado = SlotState.BocaArriba;
        PosicionEnTorre = posicion;
    }

    public void Voltear()
    {
        Estado = SlotState.Volteada;
    }

    public void ResetearEstado()
    {
        Estado = SlotState.BocaArriba;
    }

    public override string ToString()
    {
        return $"[{PosicionEnTorre}] {Ficha.nombre} ({Dueno}) - {Estado}";
    }
}
