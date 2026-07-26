using UnityEngine;

/// <summary>
/// Herramienta de desarrollo: resetea el álbum del jugador al iniciar la escena.
/// Limpia todas las fichas y agrega 1 de cada FichaTemplate asignada en el inspector.
/// Se auto-destruye después de ejecutar para no persistir en la escena.
///
/// USO: Arrastrar el prefab AlbumResetTool a cualquier escena → Play → álbum reseteado → prefab se destruye.
/// IMPORTANTE: No dejar este prefab en escenas de producción.
/// </summary>
public class AlbumResetTool : MonoBehaviour
{
    [Header("Fichas a incluir en el álbum reseteado (1 de cada una)")]
    [SerializeField] private FichaTemplate[] _fichaTemplates;

    private void Start()
    {
        Invoke(nameof(EjecutarReset), 0.2f);
    }

    private void EjecutarReset()
    {
        if (AlbumManager.Instance == null)
        {
            Debug.LogError("[AlbumResetTool] AlbumManager no encontrado. No se puede resetear el álbum.");
            Destroy(gameObject);
            return;
        }

        var album = AlbumManager.Instance.ObtenerAlbumData();
        if (album == null)
        {
            Debug.LogError("[AlbumResetTool] AlbumData es null.");
            Destroy(gameObject);
            return;
        }

        // Limpiar todo
        album.Limpiar();

        // Agregar 1 de cada template
        int agregadas = 0;
        if (_fichaTemplates != null)
        {
            foreach (var template in _fichaTemplates)
            {
                if (template == null) continue;
                album.AgregarFicha(template);
                agregadas++;
            }
        }

        // Guardar inmediatamente
        SaveData saveData = new SaveData();
        saveData.album = AlbumSaveData.CrearDesdeAlbum(album);
        SaveSystem.Guardar(saveData);

        Debug.Log($"[AlbumResetTool] Álbum reseteado. {agregadas} fichas únicas agregadas y guardadas. Auto-destruyendo.");
        Destroy(gameObject);
    }
}
