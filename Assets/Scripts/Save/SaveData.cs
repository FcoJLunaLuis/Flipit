using System;
using System.Collections.Generic;

/// <summary>
/// Representa una entrada guardada del álbum: templateId + cantidad + datos mutables de la ficha.
/// </summary>
[Serializable]
public class AlbumEntrySaveData
{
    public int templateId;
    public int cantidad;
    public FichaData fichaData;

    public AlbumEntrySaveData() { }

    public AlbumEntrySaveData(AlbumEntry entry)
    {
        templateId = entry.ficha.templateId;
        cantidad = entry.cantidad;
        fichaData = entry.ficha;
    }
}

/// <summary>
/// Datos de guardado del álbum.
/// </summary>
[Serializable]
public class AlbumSaveData
{
    public List<AlbumEntrySaveData> entradas = new List<AlbumEntrySaveData>();

    /// <summary>
    /// Crea AlbumSaveData a partir de un AlbumData en runtime.
    /// </summary>
    public static AlbumSaveData CrearDesdeAlbum(AlbumData album)
    {
        var saveData = new AlbumSaveData();
        var todasLasFichas = album.ObtenerTodasLasFichas();

        foreach (var entry in todasLasFichas)
        {
            saveData.entradas.Add(new AlbumEntrySaveData(entry));
        }

        return saveData;
    }

    /// <summary>
    /// Restaura un AlbumData a partir de los datos guardados.
    /// </summary>
    public AlbumData RestaurarAlbum()
    {
        var album = new AlbumData();

        foreach (var entrada in entradas)
        {
            // Restaurar la ficha con la cantidad guardada
            for (int i = 0; i < entrada.cantidad; i++)
            {
                album.AgregarFicha(entrada.fichaData);
            }
        }

        return album;
    }
}

/// <summary>
/// Contenedor general de datos de guardado.
/// Extensible: agrega nuevos campos conforme el juego crezca.
/// </summary>
[Serializable]
public class SaveData
{
    /// <summary>
    /// Versión del formato de guardado. Útil para migraciones futuras.
    /// </summary>
    public int version = 1;

    /// <summary>
    /// Fecha y hora del último guardado.
    /// </summary>
    public string fechaGuardado;

    /// <summary>
    /// Datos del álbum de fichas.
    /// </summary>
    public AlbumSaveData album = new AlbumSaveData();

    // --- Extensible: agrega más datos aquí conforme el juego crezca ---
    // public InventarioSaveData inventario;
    // public ProgresoSaveData progreso;
    // public ConfiguracionSaveData configuracion;
}
