using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Construye la torre de fichas para el combate.
/// Toma las fichas apostadas de ambos lados, las mezcla y las apila
/// verticalmente (una sobre otra) formando una torre.
/// </summary>
public static class TowerBuilder
{
    /// <summary>
    /// Genera la torre mezclando las fichas de jugador y NPC.
    /// Las fichas se apilan verticalmente: posición Y incrementa hacia arriba.
    /// </summary>
    /// <param name="fichasJugador">Fichas apostadas por el jugador.</param>
    /// <param name="fichasNPC">Fichas apostadas por el NPC.</param>
    /// <returns>Lista de TowerSlots con posiciones asignadas (apiladas).</returns>
    public static List<TowerSlot> ConstruirTorre(List<FichaData> fichasJugador, List<FichaData> fichasNPC)
    {
        var todosLosSlots = new List<TowerSlot>();

        // Crear slots con dueño temporal (posición se asigna después)
        var slotsJugador = fichasJugador.Select(f =>
            new TowerSlot(f, TowerSlot.SlotOwner.Jugador, Vector2Int.zero)).ToList();

        var slotsNPC = fichasNPC.Select(f =>
            new TowerSlot(f, TowerSlot.SlotOwner.NPC, Vector2Int.zero)).ToList();

        // Mezclar todas las fichas aleatoriamente
        var slotsMezclados = new List<TowerSlot>();
        slotsMezclados.AddRange(slotsJugador);
        slotsMezclados.AddRange(slotsNPC);
        slotsMezclados = slotsMezclados.OrderBy(_ => Random.Range(0f, 1f)).ToList();

        // Apilar verticalmente: cada ficha en posición (0, altura)
        for (int i = 0; i < slotsMezclados.Count; i++)
        {
            var slotOriginal = slotsMezclados[i];
            var posicion = new Vector2Int(0, i);

            var slotConPosicion = new TowerSlot(
                slotOriginal.Ficha,
                slotOriginal.Dueno,
                posicion
            );

            todosLosSlots.Add(slotConPosicion);
        }

        return todosLosSlots;
    }

    /// <summary>
    /// Obtiene la altura total de la torre (cantidad de fichas).
    /// </summary>
    public static int ObtenerAlturaTorre(int totalFichas)
    {
        return totalFichas;
    }
}
