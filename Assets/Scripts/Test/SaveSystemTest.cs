using UnityEngine;

/// <summary>
/// Script de prueba para verificar el sistema de guardado.
/// Prueba: guardar álbum con fichas, cargar en nueva instancia, verificar persistencia.
/// </summary>
public class SaveSystemTest : MonoBehaviour
{
    [Header("Arrastra los FichaTemplates aquí para probar")]
    public FichaTemplate[] fichasTemplates;

    void Start()
    {
        if (fichasTemplates == null || fichasTemplates.Length == 0)
        {
            Debug.LogWarning("[SaveSystemTest] No hay templates asignados.");
            return;
        }

        Debug.Log("[SaveSystemTest] === Iniciando pruebas del sistema de guardado ===\n");
        Debug.Log($"[SaveSystemTest] Ruta de guardado: {SaveSystem.ObtenerRutaGuardado()}");

        // Limpiar save anterior para empezar limpio
        SaveSystem.BorrarSave();

        // Test 1: Guardar álbum con fichas
        Debug.Log("\n--- Test 1: Guardar álbum con fichas ---");
        AlbumData album = new AlbumData();
        album.AgregarFicha(fichasTemplates[0]); // Piedra x3
        album.AgregarFicha(fichasTemplates[0]);
        album.AgregarFicha(fichasTemplates[0]);
        album.AgregarFicha(fichasTemplates[1]); // Cristal x1
        album.AgregarFicha(fichasTemplates[2]); // Obsidiana x2
        album.AgregarFicha(fichasTemplates[2]);

        SaveData saveData = new SaveData();
        saveData.album = AlbumSaveData.CrearDesdeAlbum(album);

        bool guardadoExitoso = SaveSystem.Guardar(saveData);
        Debug.Log($"Guardado exitoso: {guardadoExitoso} (esperado: True)");
        Debug.Log($"Existe save: {SaveSystem.ExisteSave()} (esperado: True)");

        // Test 2: Cargar datos y verificar
        Debug.Log("\n--- Test 2: Cargar datos y verificar ---");
        SaveData datosCargados = SaveSystem.Cargar();
        Debug.Log($"Datos cargados: {datosCargados != null} (esperado: True)");
        Debug.Log($"Versión: {datosCargados.version} (esperado: 1)");
        Debug.Log($"Fecha guardado: {datosCargados.fechaGuardado}");
        Debug.Log($"Entradas en álbum: {datosCargados.album.entradas.Count} (esperado: 3)");

        // Test 3: Restaurar AlbumData desde datos cargados
        Debug.Log("\n--- Test 3: Restaurar AlbumData ---");
        AlbumData albumRestaurado = datosCargados.album.RestaurarAlbum();
        Debug.Log($"Fichas únicas: {albumRestaurado.ObtenerTotalFichasUnicas()} (esperado: 3)");
        Debug.Log($"Total fichas: {albumRestaurado.ObtenerTotalFichas()} (esperado: 6)");
        Debug.Log($"Cantidad '{fichasTemplates[0].nombre}': {albumRestaurado.ObtenerCantidad(fichasTemplates[0].id)} (esperado: 3)");
        Debug.Log($"Cantidad '{fichasTemplates[1].nombre}': {albumRestaurado.ObtenerCantidad(fichasTemplates[1].id)} (esperado: 1)");
        Debug.Log($"Cantidad '{fichasTemplates[2].nombre}': {albumRestaurado.ObtenerCantidad(fichasTemplates[2].id)} (esperado: 2)");

        // Verificar que los datos de la ficha se preservaron
        var entradaPiedra = albumRestaurado.ObtenerEntrada(fichasTemplates[0].id);
        Debug.Log($"Nombre preservado: {entradaPiedra.ficha.nombre} (esperado: {fichasTemplates[0].nombre})");
        Debug.Log($"Rareza preservada: {entradaPiedra.ficha.rareza} (esperado: {fichasTemplates[0].rareza})");
        Debug.Log($"Perk preservado: {entradaPiedra.ficha.perk} (esperado: {fichasTemplates[0].perk})");

        // Test 4: Guardar/cargar con álbum vacío
        Debug.Log("\n--- Test 4: Álbum vacío ---");
        SaveData saveVacio = new SaveData();
        saveVacio.album = AlbumSaveData.CrearDesdeAlbum(new AlbumData());
        bool guardadoVacio = SaveSystem.Guardar(saveVacio);
        Debug.Log($"Guardado álbum vacío: {guardadoVacio} (esperado: True)");

        SaveData cargaVacia = SaveSystem.Cargar();
        AlbumData albumVacio = cargaVacia.album.RestaurarAlbum();
        Debug.Log($"Fichas en álbum vacío restaurado: {albumVacio.ObtenerTotalFichas()} (esperado: 0)");

        // Test 5: Borrar save
        Debug.Log("\n--- Test 5: Borrar save ---");
        bool borrado = SaveSystem.BorrarSave();
        Debug.Log($"Borrado: {borrado} (esperado: True)");
        Debug.Log($"Existe save después de borrar: {SaveSystem.ExisteSave()} (esperado: False)");

        Debug.Log("\n[SaveSystemTest] === Pruebas finalizadas ===");
    }
}
