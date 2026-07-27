using System;
using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// Entrada individual de una ficha en juego durante un combate activo.
/// Se utiliza para el registro de crash recovery, permitiendo restaurar
/// fichas al álbum si la aplicación se cierra inesperadamente.
/// </summary>
[Serializable]
public class FichaEnJuegoEntry
{
    /// <summary>
    /// Template ID de la ficha en juego.
    /// </summary>
    [JsonProperty("templateId")]
    public int templateId;

    /// <summary>
    /// Cantidad de fichas de este tipo removidas del álbum.
    /// </summary>
    [JsonProperty("cantidad")]
    public int cantidad;

    /// <summary>
    /// Si es true, esta entrada corresponde a la ficha lanzadora del jugador.
    /// La ficha lanzadora no forma parte de la apuesta y siempre se restaura.
    /// </summary>
    [JsonProperty("esLanzadora")]
    public bool esLanzadora;

    /// <summary>
    /// Datos mutables completos de la ficha para restauración.
    /// Contiene toda la información necesaria para reconstruir la ficha en el álbum.
    /// </summary>
    [JsonProperty("fichaData")]
    public FichaData fichaData;
}

/// <summary>
/// Contenedor principal para el archivo de crash recovery en disco.
/// Se serializa a JSON en Application.persistentDataPath/fichas_en_juego.json
/// cuando el jugador confirma su apuesta, y se elimina al finalizar el combate.
/// </summary>
[Serializable]
public class FichasEnJuegoData
{
    /// <summary>
    /// Timestamp del momento en que se confirmó la apuesta.
    /// Formato: "yyyy-MM-dd HH:mm:ss"
    /// </summary>
    [JsonProperty("timestampConfirmacion")]
    public string timestampConfirmacion;

    /// <summary>
    /// Lista de fichas removidas del álbum durante la confirmación de apuesta.
    /// Incluye tanto fichas apostadas como la ficha lanzadora.
    /// </summary>
    [JsonProperty("fichas")]
    public List<FichaEnJuegoEntry> fichas = new List<FichaEnJuegoEntry>();
}
