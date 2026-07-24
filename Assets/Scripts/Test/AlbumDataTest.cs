using UnityEngine;

/// <summary>
/// Script de prueba para verificar la lógica de AlbumData.
/// Prueba: agregar fichas, agrupación de repetidas, contadores, remoción y paginación.
/// </summary>
public class AlbumDataTest : MonoBehaviour
{
    [Header("Arrastra los FichaTemplates aquí para probar")]
    public FichaTemplate[] fichasTemplates;

    void Start()
    {
        if (fichasTemplates == null || fichasTemplates.Length == 0)
        {
            Debug.LogWarning("[AlbumDataTest] No hay templates asignados.");
            return;
        }

        Debug.Log("[AlbumDataTest] === Iniciando pruebas del álbum ===\n");

        AlbumData album = new AlbumData();

        // Test 1: Agregar fichas
        Debug.Log("--- Test 1: Agregar fichas ---");
        foreach (var template in fichasTemplates)
        {
            album.AgregarFicha(template);
        }
        Debug.Log($"Fichas únicas: {album.ObtenerTotalFichasUnicas()} (esperado: {fichasTemplates.Length})");
        Debug.Log($"Total fichas: {album.ObtenerTotalFichas()} (esperado: {fichasTemplates.Length})");

        // Test 2: Agregar fichas repetidas
        Debug.Log("\n--- Test 2: Agregar fichas repetidas ---");
        album.AgregarFicha(fichasTemplates[0]); // Agregar la primera ficha de nuevo
        album.AgregarFicha(fichasTemplates[0]); // Y otra vez
        album.AgregarFicha(fichasTemplates[1]); // La segunda también
        Debug.Log($"Cantidad de '{fichasTemplates[0].nombre}': {album.ObtenerCantidad(fichasTemplates[0].id)} (esperado: 3)");
        Debug.Log($"Cantidad de '{fichasTemplates[1].nombre}': {album.ObtenerCantidad(fichasTemplates[1].id)} (esperado: 2)");
        Debug.Log($"Fichas únicas: {album.ObtenerTotalFichasUnicas()} (esperado: {fichasTemplates.Length})");
        Debug.Log($"Total fichas: {album.ObtenerTotalFichas()} (esperado: {fichasTemplates.Length + 3})");

        // Test 3: Remover fichas
        Debug.Log("\n--- Test 3: Remover fichas ---");
        bool removida = album.RemoverFicha(fichasTemplates[0].id);
        Debug.Log($"Remover '{fichasTemplates[0].nombre}': {removida} (esperado: True)");
        Debug.Log($"Cantidad de '{fichasTemplates[0].nombre}': {album.ObtenerCantidad(fichasTemplates[0].id)} (esperado: 2)");

        bool removeInexistente = album.RemoverFicha(999);
        Debug.Log($"Remover ficha inexistente (id=999): {removeInexistente} (esperado: False)");

        // Test 4: Paginación
        Debug.Log("\n--- Test 4: Paginación ---");
        Debug.Log($"Total páginas con {album.ObtenerTotalFichasUnicas()} fichas: {album.ObtenerTotalPaginas()} (esperado: 1)");

        var pag0 = album.ObtenerFichasPaginadas(0);
        Debug.Log($"Fichas en página 0: {pag0.Count} (esperado: {album.ObtenerTotalFichasUnicas()})");

        foreach (var entry in pag0)
        {
            Debug.Log($"  - [{entry.ficha.templateId}] {entry.ficha.nombre} x{entry.cantidad}");
        }

        // Test 5: Paginación con más de 10 fichas (simular)
        Debug.Log("\n--- Test 5: Paginación con muchas fichas ---");
        AlbumData albumGrande = new AlbumData();
        for (int i = 1; i <= 25; i++)
        {
            FichaData ficha = new FichaData
            {
                templateId = i,
                nombre = $"Ficha Test {i}",
                rareza = Rareza.Comun,
                rango = 1,
                perk = "Test"
            };
            albumGrande.AgregarFicha(ficha);
        }

        Debug.Log($"Álbum grande - Fichas únicas: {albumGrande.ObtenerTotalFichasUnicas()} (esperado: 25)");
        Debug.Log($"Álbum grande - Total páginas: {albumGrande.ObtenerTotalPaginas()} (esperado: 3)");

        var pagina0 = albumGrande.ObtenerFichasPaginadas(0);
        var pagina1 = albumGrande.ObtenerFichasPaginadas(1);
        var pagina2 = albumGrande.ObtenerFichasPaginadas(2);

        Debug.Log($"Página 0: {pagina0.Count} fichas (esperado: 10)");
        Debug.Log($"Página 1: {pagina1.Count} fichas (esperado: 10)");
        Debug.Log($"Página 2: {pagina2.Count} fichas (esperado: 5)");

        // Test 6: Contiene ficha
        Debug.Log("\n--- Test 6: Contiene ficha ---");
        Debug.Log($"Contiene ficha id={fichasTemplates[0].id}: {album.ContieneFicha(fichasTemplates[0].id)} (esperado: True)");
        Debug.Log($"Contiene ficha id=999: {album.ContieneFicha(999)} (esperado: False)");

        // Test 7: Álbum vacío
        Debug.Log("\n--- Test 7: Álbum vacío ---");
        AlbumData albumVacio = new AlbumData();
        Debug.Log($"Álbum vacío - Total páginas: {albumVacio.ObtenerTotalPaginas()} (esperado: 1)");
        Debug.Log($"Álbum vacío - Fichas en página 0: {albumVacio.ObtenerFichasPaginadas(0).Count} (esperado: 0)");

        Debug.Log("\n[AlbumDataTest] === Pruebas finalizadas ===");
    }
}
