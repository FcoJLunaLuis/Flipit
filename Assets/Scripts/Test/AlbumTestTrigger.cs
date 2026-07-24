using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Objeto de prueba para interactuar con el álbum.
/// - Presiona TAB o Start para abrir/cerrar el álbum.
/// - Presiona F1-F6 para agregar fichas de prueba al álbum.
/// </summary>
public class AlbumTestTrigger : MonoBehaviour
{
    [Header("Templates de prueba para agregar")]
    public FichaTemplate[] fichasDeTest;

    [Header("Tecla para abrir/cerrar el álbum")]
    public Key teclaAlbum = Key.Tab;

    void Update()
    {
        // Abrir/cerrar álbum con TAB
        if (Keyboard.current != null && Keyboard.current[teclaAlbum].wasPressedThisFrame)
        {
            if (AlbumManager.Instance != null)
            {
                if (AlbumManager.Instance.EstaAbierto())
                    AlbumManager.Instance.CerrarAlbum();
                else
                    AlbumManager.Instance.AbrirAlbum();
            }
        }

        // Agregar fichas con F1-F6 (solo cuando el álbum está cerrado)
        if (AlbumManager.Instance != null && !AlbumManager.Instance.EstaAbierto())
        {
            if (Keyboard.current != null && fichasDeTest != null)
            {
                if (Keyboard.current[Key.F1].wasPressedThisFrame && fichasDeTest.Length > 0)
                    AgregarFichaTest(0);
                if (Keyboard.current[Key.F2].wasPressedThisFrame && fichasDeTest.Length > 1)
                    AgregarFichaTest(1);
                if (Keyboard.current[Key.F3].wasPressedThisFrame && fichasDeTest.Length > 2)
                    AgregarFichaTest(2);
                if (Keyboard.current[Key.F4].wasPressedThisFrame && fichasDeTest.Length > 3)
                    AgregarFichaTest(3);
                if (Keyboard.current[Key.F5].wasPressedThisFrame && fichasDeTest.Length > 4)
                    AgregarFichaTest(4);
                if (Keyboard.current[Key.F6].wasPressedThisFrame && fichasDeTest.Length > 5)
                    AgregarFichaTest(5);
            }
        }
    }

    private void AgregarFichaTest(int index)
    {
        var template = fichasDeTest[index];
        AlbumManager.Instance.AgregarFicha(template);
        Debug.Log($"[TestTrigger] Ficha '{template.nombre}' agregada con F{index + 1}.");
    }

    [Header("Precargar fichas al iniciar")]
    public bool precargarFichas = true;

    void Start()
    {
        Debug.Log("[AlbumTestTrigger] === Controles de prueba ===");
        Debug.Log("  TAB: Abrir/cerrar álbum");
        Debug.Log("  F1-F6: Agregar fichas de prueba");
        Debug.Log("  Dentro del álbum:");
        Debug.Log("    W/S: Navegar fichas");
        Debug.Log("    Q/E: Cambiar página");
        Debug.Log("    ESC: Cerrar álbum");
        Debug.Log("    Enter: Seleccionar ficha");
    }

    void OnEnable()
    {
        // Precargar fichas después de que todo se inicialice
        if (precargarFichas)
        {
            Invoke(nameof(IntentarPrecarga), 0.1f);
        }
    }

    private void IntentarPrecarga()
    {
        if (AlbumManager.Instance != null && fichasDeTest != null)
        {
            if (AlbumManager.Instance.ObtenerAlbumData().ObtenerTotalFichas() == 0)
            {
                PrecargarFichas();
            }
        }
    }

    private void PrecargarFichas()
    {
        Debug.Log($"[AlbumTestTrigger] Precargando {fichasDeTest.Length} fichas al álbum...");
        foreach (var template in fichasDeTest)
        {
            if (template != null)
            {
                AlbumManager.Instance.AgregarFicha(template);
            }
        }
        Debug.Log($"[AlbumTestTrigger] Precarga completa. Total: {AlbumManager.Instance.ObtenerAlbumData().ObtenerTotalFichasUnicas()} fichas únicas.");
    }
}
