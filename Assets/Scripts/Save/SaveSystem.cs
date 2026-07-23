using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// Sistema de guardado y carga de datos usando Newtonsoft JSON.
/// Guarda en Application.persistentDataPath con escritura segura (archivo temporal + renombrar).
/// </summary>
public static class SaveSystem
{
    private const string SAVE_FILE_NAME = "flipit_save.json";
    private const string TEMP_FILE_SUFFIX = ".tmp";

    private static readonly JsonSerializerSettings settings = new JsonSerializerSettings
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Include,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
    };

    /// <summary>
    /// Obtiene la ruta completa del archivo de guardado.
    /// </summary>
    public static string ObtenerRutaGuardado()
    {
        return Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
    }

    /// <summary>
    /// Guarda los datos al disco.
    /// Usa escritura segura: escribe a archivo temporal y luego renombra.
    /// </summary>
    public static bool Guardar(SaveData data)
    {
        try
        {
            data.fechaGuardado = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string json = JsonConvert.SerializeObject(data, settings);
            string rutaFinal = ObtenerRutaGuardado();
            string rutaTemporal = rutaFinal + TEMP_FILE_SUFFIX;

            // Escribir a archivo temporal primero
            File.WriteAllText(rutaTemporal, json);

            // Si el archivo final existe, eliminarlo
            if (File.Exists(rutaFinal))
            {
                File.Delete(rutaFinal);
            }

            // Renombrar temporal a final
            File.Move(rutaTemporal, rutaFinal);

            Debug.Log($"[SaveSystem] Guardado exitoso en: {rutaFinal}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Error al guardar: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Carga los datos del disco.
    /// Retorna null si no existe archivo o hay un error.
    /// </summary>
    public static SaveData Cargar()
    {
        string ruta = ObtenerRutaGuardado();

        if (!File.Exists(ruta))
        {
            Debug.Log("[SaveSystem] No existe archivo de guardado. Se creará uno nuevo.");
            return null;
        }

        try
        {
            string json = File.ReadAllText(ruta);
            SaveData data = JsonConvert.DeserializeObject<SaveData>(json, settings);
            Debug.Log($"[SaveSystem] Carga exitosa. Fecha del guardado: {data.fechaGuardado}");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Error al cargar: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Verifica si existe un archivo de guardado.
    /// </summary>
    public static bool ExisteSave()
    {
        return File.Exists(ObtenerRutaGuardado());
    }

    /// <summary>
    /// Elimina el archivo de guardado.
    /// </summary>
    public static bool BorrarSave()
    {
        string ruta = ObtenerRutaGuardado();

        if (!File.Exists(ruta))
        {
            Debug.Log("[SaveSystem] No hay archivo de guardado para borrar.");
            return false;
        }

        try
        {
            File.Delete(ruta);
            Debug.Log("[SaveSystem] Archivo de guardado eliminado.");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Error al borrar: {e.Message}");
            return false;
        }
    }
}
