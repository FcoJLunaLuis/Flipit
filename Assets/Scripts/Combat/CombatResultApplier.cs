using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Aplica los resultados del combate al álbum del jugador.
/// 
/// Lógica de ownership:
/// - Fichas ganadas por el jugador que eran del NPC → agregar al álbum (nuevas adquisiciones)
/// - Fichas ganadas por el NPC que eran del jugador → remover del álbum (pérdidas)
/// - Fichas ganadas por el jugador que eran suyas → ya están en el álbum (no hacer nada)
/// - Fichas ganadas por el NPC que eran suyas → nunca estuvieron en el álbum (no hacer nada)
/// 
/// También aplica XP a la ficha lanzadora y calcula desgaste.
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

        // Obtener sets de templateId para comparación rápida
        var idsApostadasJugador = new HashSet<int>(
            combatData.FichasApostadasJugador.Select(f => f.templateId));
        var idsApostadasNPC = new HashSet<int>(
            combatData.FichasApostadasNPC.Select(f => f.templateId));

        // Fichas que el jugador ganó y que eran del NPC → agregar al álbum
        var fichasNuevasParaJugador = combatData.FichasGanadasJugador
            .Where(f => idsApostadasNPC.Contains(f.templateId))
            .ToList();

        // Fichas que el NPC ganó y que eran del jugador → remover del álbum
        var fichasPerdidasPorJugador = combatData.FichasGanadasNPC
            .Where(f => idsApostadasJugador.Contains(f.templateId))
            .ToList();

        // Aplicar al álbum
        foreach (var ficha in fichasNuevasParaJugador)
        {
            album.AgregarFicha(ficha);
        }

        foreach (var ficha in fichasPerdidasPorJugador)
        {
            album.RemoverFicha(ficha.templateId);
        }

        // Preparar summary para la UI
        summary.FichasGanadas = fichasNuevasParaJugador;
        summary.FichasPerdidasAlNPC = fichasPerdidasPorJugador;

        // Aplicar XP a la ficha lanzadora
        var lanzadora = combatData.FichaLanzadoraJugador;
        if (lanzadora != null)
        {
            float xpGanada = fichasNuevasParaJugador.Count * config.xpPorFichaGanada;
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
        summary.JugadorGano = fichasNuevasParaJugador.Count > fichasPerdidasPorJugador.Count;

        Debug.Log($"[CombatResultApplier] Resultados aplicados. Ganadas del NPC:{fichasNuevasParaJugador.Count} Perdidas al NPC:{fichasPerdidasPorJugador.Count} XP:{summary.XPGanada} Desgaste:{summary.DesgasteAplicado} Rota:{summary.FichaSeRompio}");

        return summary;
    }
}
