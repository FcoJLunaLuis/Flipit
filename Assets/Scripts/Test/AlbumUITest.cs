using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Test que muestra el álbum con datos de prueba al iniciar.
/// Verifica que la UI se llena correctamente.
/// </summary>
public class AlbumUITest : MonoBehaviour
{
    public AlbumUI albumUI;

    void Start()
    {
        if (albumUI == null)
        {
            albumUI = FindObjectOfType<AlbumUI>(true);
        }

        if (albumUI == null)
        {
            Debug.LogError("[AlbumUITest] No se encontró AlbumUI en la escena.");
            return;
        }

        Debug.Log("[AlbumUITest] === Iniciando prueba de UI ===");

        // Crear datos de prueba
        var testEntries = new List<AlbumEntry>();
        string[] names = { "Piedra", "Cristal", "Obsidiana", "Madera", "Hierro", "Diamante" };
        Rareza[] rarezas = { Rareza.Comun, Rareza.Raro, Rareza.UltraRaro, Rareza.Comun, Rareza.Raro, Rareza.UltraRaro };
        int[] cantidades = { 3, 2, 1, 5, 1, 1 };

        for (int i = 0; i < names.Length; i++)
        {
            var ficha = new FichaData
            {
                templateId = i + 1,
                nombre = names[i],
                rareza = rarezas[i],
                rango = i + 1,
                experienciaDeRango = i * 25f,
                estaRoto = i == 3,
                desgaste = i * 15f,
                perk = "Perk " + names[i],
                peso = 1f + i * 0.5f,
                suerte = 1f + i * 2f
            };
            testEntries.Add(new AlbumEntry(ficha, cantidades[i]));
        }

        // Mostrar álbum con datos
        albumUI.Mostrar();
        albumUI.ActualizarLista(testEntries, 0, 1);
        albumUI.SetIndiceSeleccionado(0);

        Debug.Log($"[AlbumUITest] Álbum mostrado con {testEntries.Count} fichas.");
        Debug.Log("[AlbumUITest] La primera ficha debería estar seleccionada y sus detalles visibles.");
        Debug.Log("[AlbumUITest] === Prueba de UI lista ===");
    }
}
