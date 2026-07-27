using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper factory methods for creating test data in CombatAlbumBridge tests.
/// </summary>
public static class CombatAlbumBridgeTestHelper
{
    /// <summary>
    /// Creates a FichaData with the given parameters.
    /// </summary>
    public static FichaData CrearFicha(int templateId, string nombre = null, bool estaRoto = false, Rareza rareza = Rareza.Comun)
    {
        return new FichaData
        {
            templateId = templateId,
            nombre = nombre ?? $"Ficha_{templateId}",
            rareza = rareza,
            rango = 1,
            experienciaDeRango = 0f,
            estaRoto = estaRoto,
            desgaste = 0f,
            perk = "",
            peso = 1f,
            suerte = 1f
        };
    }

    /// <summary>
    /// Creates an AlbumData populated with the given fichas (each with cantidad = 1).
    /// </summary>
    public static AlbumData CrearAlbumConFichas(params FichaData[] fichas)
    {
        var albumData = new AlbumData();
        foreach (var ficha in fichas)
        {
            albumData.AgregarFicha(ficha);
        }
        return albumData;
    }

    /// <summary>
    /// Creates an AlbumData populated with N non-broken fichas (templateId 1..n).
    /// </summary>
    public static AlbumData CrearAlbumConNFichas(int cantidad)
    {
        var albumData = new AlbumData();
        for (int i = 1; i <= cantidad; i++)
        {
            albumData.AgregarFicha(CrearFicha(i));
        }
        return albumData;
    }

    /// <summary>
    /// Creates a CombatData with the given results for post-combat testing.
    /// Uses CombatData's public API to set up apostadas and lanzadoras.
    /// FichasGanadas must be populated via RegistrarLanzamiento.
    /// </summary>
    public static CombatData CrearCombatDataConApuestas(
        List<FichaData> apostadasJugador,
        List<FichaData> apostadasNPC,
        FichaData lanzadoraJugador = null,
        FichaData lanzadoraNPC = null)
    {
        var combatData = new CombatData();
        combatData.SetFichasApostadas(apostadasJugador, apostadasNPC);
        combatData.SetFichaLanzadora(lanzadoraJugador, lanzadoraNPC);
        return combatData;
    }

    /// <summary>
    /// Creates a list of FichaData from template IDs.
    /// </summary>
    public static List<FichaData> CrearListaFichas(params int[] templateIds)
    {
        var list = new List<FichaData>();
        foreach (int id in templateIds)
        {
            list.Add(CrearFicha(id));
        }
        return list;
    }

    /// <summary>
    /// Creates a FichasEnJuegoData for crash recovery testing.
    /// </summary>
    public static FichasEnJuegoData CrearFichasEnJuegoData(List<FichaData> fichasApostadas, FichaData fichaLanzadora)
    {
        var data = new FichasEnJuegoData();
        data.timestampConfirmacion = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        data.fichas = new List<FichaEnJuegoEntry>();

        foreach (var ficha in fichasApostadas)
        {
            data.fichas.Add(new FichaEnJuegoEntry
            {
                templateId = ficha.templateId,
                cantidad = 1,
                esLanzadora = false,
                fichaData = ficha
            });
        }

        if (fichaLanzadora != null)
        {
            data.fichas.Add(new FichaEnJuegoEntry
            {
                templateId = fichaLanzadora.templateId,
                cantidad = 1,
                esLanzadora = true,
                fichaData = fichaLanzadora
            });
        }

        return data;
    }
}
