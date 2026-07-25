using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Genera la apuesta del NPC de forma aleatoria.
/// El NPC apuesta ±1 ficha respecto al jugador (si el jugador no apuesta el máximo).
/// También selecciona una ficha lanzadora aleatoria.
/// </summary>
public class NPCBetGenerator
{
    private readonly int _maxFichas;

    public NPCBetGenerator(int maxFichas = 5)
    {
        _maxFichas = maxFichas;
    }

    /// <summary>
    /// Genera la apuesta del NPC basada en la cantidad de fichas que apostó el jugador.
    /// Si el jugador apostó el máximo (5), el NPC apuesta 5.
    /// Si no, el NPC apuesta cantidad ±1 (aleatorio), clampeado a [1, max].
    /// </summary>
    /// <param name="fichasDisponiblesNPC">Pool de fichas que el NPC puede apostar.</param>
    /// <param name="cantidadJugador">Cantidad de fichas que apostó el jugador.</param>
    /// <returns>Lista de fichas seleccionadas por el NPC para apostar.</returns>
    public List<FichaData> GenerarApuesta(List<FichaData> fichasDisponiblesNPC, int cantidadJugador)
    {
        if (fichasDisponiblesNPC == null || fichasDisponiblesNPC.Count == 0)
        {
            Debug.LogWarning("[NPCBetGenerator] El NPC no tiene fichas disponibles.");
            return new List<FichaData>();
        }

        int cantidadNPC = CalcularCantidadApuesta(cantidadJugador, fichasDisponiblesNPC.Count);

        var fichasValidas = fichasDisponiblesNPC.Where(f => !f.estaRoto).ToList();
        if (fichasValidas.Count == 0)
        {
            Debug.LogWarning("[NPCBetGenerator] El NPC no tiene fichas válidas (todas rotas).");
            return new List<FichaData>();
        }

        // Barajar y tomar la cantidad necesaria
        var fichasBarajadas = fichasValidas.OrderBy(_ => Random.Range(0f, 1f)).ToList();
        int cantidadFinal = Mathf.Min(cantidadNPC, fichasBarajadas.Count);

        return fichasBarajadas.Take(cantidadFinal).ToList();
    }

    /// <summary>
    /// Selecciona una ficha lanzadora aleatoria del pool del NPC.
    /// La ficha lanzadora no puede estar entre las fichas apostadas.
    /// </summary>
    /// <param name="fichasDisponiblesNPC">Todas las fichas del NPC.</param>
    /// <param name="fichasApostadas">Fichas que el NPC ya apostó.</param>
    /// <returns>La ficha lanzadora del NPC, o null si no hay disponibles.</returns>
    public FichaData SeleccionarFichaLanzadora(List<FichaData> fichasDisponiblesNPC, List<FichaData> fichasApostadas)
    {
        if (fichasDisponiblesNPC == null) return null;

        var apostadasIds = new HashSet<int>(fichasApostadas.Select(f => f.templateId));

        var candidatas = fichasDisponiblesNPC
            .Where(f => !f.estaRoto && !apostadasIds.Contains(f.templateId))
            .ToList();

        if (candidatas.Count == 0)
        {
            Debug.LogWarning("[NPCBetGenerator] El NPC no tiene fichas disponibles para lanzar.");
            return null;
        }

        int index = Random.Range(0, candidatas.Count);
        return candidatas[index];
    }

    private int CalcularCantidadApuesta(int cantidadJugador, int fichasDisponibles)
    {
        int cantidadNPC;

        if (cantidadJugador >= _maxFichas)
        {
            cantidadNPC = _maxFichas;
        }
        else
        {
            // ±1 aleatorio
            int variacion = Random.Range(0, 2) == 0 ? -1 : 1;
            cantidadNPC = cantidadJugador + variacion;
        }

        // Clampear entre 1 y el máximo permitido
        cantidadNPC = Mathf.Clamp(cantidadNPC, 1, _maxFichas);

        // No puede apostar más fichas de las que tiene
        cantidadNPC = Mathf.Min(cantidadNPC, fichasDisponibles);

        return cantidadNPC;
    }
}
