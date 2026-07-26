using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bootstrap para la escena de integración.
/// Genera fichas mock en el álbum del jugador para poder hacer apuestas.
/// Solo se ejecuta si el álbum está vacío.
/// </summary>
public class IntegrationBootstrap : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int _fichasIniciales = 8;

    private void Start()
    {
        // Esperar un frame para que todos los singletons se inicialicen
        Invoke(nameof(SetupDatos), 0.1f);
    }

    private void SetupDatos()
    {
        // Verificar que AlbumManager existe y tiene fichas
        if (AlbumManager.Instance == null)
        {
            Debug.LogWarning("[IntegrationBootstrap] AlbumManager no encontrado.");
            return;
        }

        var album = AlbumManager.Instance.ObtenerAlbumData();
        if (album == null)
        {
            Debug.LogWarning("[IntegrationBootstrap] AlbumData es null.");
            return;
        }

        if (album.ObtenerTotalFichas() > 0)
        {
            Debug.Log($"[IntegrationBootstrap] Álbum ya tiene {album.ObtenerTotalFichas()} fichas. No se generan datos mock.");
            return;
        }

        // Generar fichas de prueba
        string[] nombres = { "Dragón Dorado", "Fénix Rojo", "Lobo Plateado", "Águila Real",
                            "Serpiente Jade", "Tigre Blanco", "León Celeste", "Oso Negro" };

        for (int i = 0; i < _fichasIniciales && i < nombres.Length; i++)
        {
            var ficha = new FichaData
            {
                templateId = i + 1,
                nombre = nombres[i],
                rareza = (Rareza)(i % 3),
                rango = 1,
                experienciaDeRango = 0,
                estaRoto = false,
                desgaste = Random.Range(0f, 25f),
                perk = "Ninguno",
                peso = Random.Range(5f, 20f),
                suerte = Random.Range(0.1f, 1f)
            };
            album.AgregarFicha(ficha);
        }

        Debug.Log($"[IntegrationBootstrap] {_fichasIniciales} fichas de prueba agregadas al álbum.");
    }
}
