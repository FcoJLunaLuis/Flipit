using UnityEngine;

/// <summary>
/// Aplica los resultados del combate al álbum del jugador.
/// Agrega fichas ganadas, aplica XP a la ficha lanzadora y calcula desgaste.
/// </summary>
public static class CombatResultApplier
{
    /// <summary>
    /// Aplica todos los resultados del combate.
    /// </summary>
    /// <param name="combatData">Datos del combate terminado.</param>
    /// <param name="album">Álbum del jugador.</param>
    /// <param name="config">Configuración de combate.</param>
    public static CombatSummaryData AplicarResultados(CombatData combatData, AlbumData album, CombatConfig config)
    {
        var summary = new CombatSummaryData();

        // Fichas ganadas por el jugador
        summary.FichasGanadas = combatData.FichasGanadasJugador;
        summary.FichasPerdidasAlNPC = combatData.FichasGanadasNPC;

        // Agregar fichas ganadas al álbum
        foreach (var ficha in combatData.FichasGanadasJugador)
        {
            album.AgregarFicha(ficha);
        }

        // Remover fichas perdidas del álbum (las que ganó el NPC eran del jugador)
        foreach (var ficha in combatData.FichasGanadasNPC)
        {
            // Solo remover si era ficha del jugador originalmente
            album.RemoverFicha(ficha.templateId);
        }

        // Aplicar XP a la ficha lanzadora
        var lanzadora = combatData.FichaLanzadoraJugador;
        if (lanzadora != null)
        {
            float xpGanada = combatData.FichasGanadasJugador.Count * config.xpPorFichaGanada;
            lanzadora.experienciaDeRango += xpGanada;
            summary.XPGanada = xpGanada;

            // Aplicar desgaste
            float desgasteTotal = combatData.LanzamientosJugador * config.desgastePorLanzamiento;
            lanzadora.desgaste += desgasteTotal;
            summary.DesgasteAplicado = desgasteTotal;

            // Verificar si se rompió
            if (lanzadora.desgaste >= config.desgasteMaximo)
            {
                lanzadora.estaRoto = true;
                summary.FichaSeRompio = true;
            }

            summary.FichaLanzadora = lanzadora;
        }

        summary.LanzamientosRealizados = combatData.LanzamientosJugador;
        summary.JugadorGano = combatData.FichasGanadasJugador.Count > combatData.FichasGanadasNPC.Count;

        Debug.Log($"[CombatResultApplier] Resultados aplicados. Ganadas:{summary.FichasGanadas.Count} XP:{summary.XPGanada} Desgaste:{summary.DesgasteAplicado} Rota:{summary.FichaSeRompio}");

        return summary;
    }
}
