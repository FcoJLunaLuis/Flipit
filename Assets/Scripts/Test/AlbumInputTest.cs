using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Script de prueba para verificar el AlbumInputHandler.
/// Al iniciar, activa el input del álbum y muestra en consola cada acción detectada.
/// Presiona las teclas para ver los eventos en consola.
/// </summary>
public class AlbumInputTest : MonoBehaviour
{
    [Header("Referencia al InputActionAsset")]
    public InputActionAsset inputActions;

    private AlbumInputHandler inputHandler;

    void Start()
    {
        Debug.Log("[AlbumInputTest] === Iniciando prueba de input ===");
        Debug.Log("[AlbumInputTest] Controles:");
        Debug.Log("  W/S o Flechas o DPad: Navegar arriba/abajo");
        Debug.Log("  Q/E o LB/RB: Página anterior/siguiente");
        Debug.Log("  ESC o B: Cerrar");
        Debug.Log("  Enter o A: Seleccionar");

        inputHandler = gameObject.AddComponent<AlbumInputHandler>();
        inputHandler.inputActions = inputActions;

        // Verificar que el Action Map existe
        if (inputActions != null)
        {
            var albumMap = inputActions.FindActionMap("Album");
            if (albumMap != null)
            {
                Debug.Log($"[AlbumInputTest] Action Map 'Album' encontrado con {albumMap.actions.Count} acciones.");
                foreach (var action in albumMap.actions)
                {
                    Debug.Log($"  - {action.name} ({action.type}) con {action.bindings.Count} bindings");
                }
            }
            else
            {
                Debug.LogError("[AlbumInputTest] No se encontró el Action Map 'Album'!");
            }
        }

        // Suscribirse y activar el input
        inputHandler.OnNavigate += HandleNavigate;
        inputHandler.OnPageNext += HandlePageNext;
        inputHandler.OnPagePrevious += HandlePagePrevious;
        inputHandler.OnClose += HandleClose;
        inputHandler.OnSelect += HandleSelect;
        inputHandler.Activar();
    }

    void OnEnable() { }

    private void ActivarInput() { }

    private void HandleNavigate(int direction)
    {
        string dir = direction > 0 ? "ARRIBA" : "ABAJO";
        Debug.Log($"[AlbumInputTest] Navegar: {dir}");
    }

    private void HandlePageNext()
    {
        Debug.Log("[AlbumInputTest] Página SIGUIENTE (E / RB)");
    }

    private void HandlePagePrevious()
    {
        Debug.Log("[AlbumInputTest] Página ANTERIOR (Q / LB)");
    }

    private void HandleClose()
    {
        Debug.Log("[AlbumInputTest] CERRAR álbum (ESC / B)");
        inputHandler.Desactivar();
        Debug.Log("[AlbumInputTest] Input desactivado. Las teclas ya no deberían responder.");
    }

    private void HandleSelect()
    {
        Debug.Log("[AlbumInputTest] SELECCIONAR ficha (Enter / A)");
    }

    void OnDisable()
    {
        if (inputHandler != null)
        {
            inputHandler.OnNavigate -= HandleNavigate;
            inputHandler.OnPageNext -= HandlePageNext;
            inputHandler.OnPagePrevious -= HandlePagePrevious;
            inputHandler.OnClose -= HandleClose;
            inputHandler.OnSelect -= HandleSelect;
            inputHandler.Desactivar();
        }
    }
}
