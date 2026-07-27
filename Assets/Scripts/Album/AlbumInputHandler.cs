using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Maneja el input del álbum usando el Action Map "Album" del Input System.
/// Se activa cuando el álbum se abre y se desactiva cuando se cierra.
/// Expone eventos para que otros sistemas reaccionen al input.
/// </summary>
public class AlbumInputHandler : MonoBehaviour
{
    /// <summary>
    /// Se dispara cuando el jugador navega arriba (1) o abajo (-1).
    /// </summary>
    public event Action<int> OnNavigate;

    /// <summary>
    /// Se dispara cuando el jugador quiere ir a la siguiente página.
    /// </summary>
    public event Action OnPageNext;

    /// <summary>
    /// Se dispara cuando el jugador quiere ir a la página anterior.
    /// </summary>
    public event Action OnPagePrevious;

    /// <summary>
    /// Se dispara cuando el jugador quiere cerrar el álbum.
    /// </summary>
    public event Action OnClose;

    /// <summary>
    /// Se dispara cuando el jugador selecciona/confirma una ficha.
    /// </summary>
    public event Action OnSelect;

    [Header("Asignar el InputActionAsset (opcional, se busca automáticamente si está vacío)")]
    public InputActionAsset inputActions;

    private InputActionMap albumActionMap;

    private InputAction navigateAction;
    private InputAction pageNextAction;
    private InputAction pagePreviousAction;
    private InputAction closeAction;
    private InputAction selectAction;

    // Cooldown para navegación para evitar inputs demasiado rápidos
    private float navigationCooldown = 0.2f;
    private float lastNavigationTime;

void Awake()
    {
        if (inputActions == null)
        {
            // Buscar el InputActionAsset si no fue asignado externamente
            inputActions = GetComponent<PlayerInput>()?.actions;
        }

        if (inputActions == null)
        {
            var allAssets = Resources.FindObjectsOfTypeAll<InputActionAsset>();
            if (allAssets.Length > 0)
            {
                inputActions = allAssets[0];
            }
        }

        SetupActions();
    }

    private void SetupActions()
    {
        if (inputActions == null)
        {
            Debug.LogError("[AlbumInputHandler] No se encontró InputActionAsset.");
            return;
        }

        albumActionMap = inputActions.FindActionMap("Album");

        if (albumActionMap == null)
        {
            Debug.LogWarning("[AlbumInputHandler] No se encontró el Action Map 'Album'. La navegación por teclado del álbum no estará disponible.");
            return;
        }

        navigateAction = albumActionMap.FindAction("Navigate");
        pageNextAction = albumActionMap.FindAction("PageNext");
        pagePreviousAction = albumActionMap.FindAction("PagePrevious");
        closeAction = albumActionMap.FindAction("Close");
        selectAction = albumActionMap.FindAction("Select");
    }

    /// <summary>
    /// Activa el input del álbum. Llamar cuando se abre el álbum.
    /// </summary>
    public void Activar()
    {
        // Si no se configuró en Awake (por ejemplo, si se asignó inputActions después), configurar ahora
        if (albumActionMap == null)
        {
            SetupActions();
        }

        if (albumActionMap == null) return;

        albumActionMap.Enable();

        if (navigateAction != null) navigateAction.performed += HandleNavigate;
        if (pageNextAction != null) pageNextAction.performed += HandlePageNext;
        if (pagePreviousAction != null) pagePreviousAction.performed += HandlePagePrevious;
        if (closeAction != null) closeAction.performed += HandleClose;
        if (selectAction != null) selectAction.performed += HandleSelect;

        Debug.Log("[AlbumInputHandler] Input del álbum activado.");
    }

    /// <summary>
    /// Desactiva el input del álbum. Llamar cuando se cierra el álbum.
    /// </summary>
    public void Desactivar()
    {
        if (albumActionMap == null) return;

        if (navigateAction != null) navigateAction.performed -= HandleNavigate;
        if (pageNextAction != null) pageNextAction.performed -= HandlePageNext;
        if (pagePreviousAction != null) pagePreviousAction.performed -= HandlePagePrevious;
        if (closeAction != null) closeAction.performed -= HandleClose;
        if (selectAction != null) selectAction.performed -= HandleSelect;

        albumActionMap.Disable();

        Debug.Log("[AlbumInputHandler] Input del álbum desactivado.");
    }

    private void HandleNavigate(InputAction.CallbackContext context)
    {
        // Cooldown para evitar navegación demasiado rápida
        if (Time.unscaledTime - lastNavigationTime < navigationCooldown) return;
        lastNavigationTime = Time.unscaledTime;

        float value = context.ReadValue<float>();
        int direction = value > 0 ? 1 : -1; // 1 = arriba, -1 = abajo
        OnNavigate?.Invoke(direction);
    }

    private void HandlePageNext(InputAction.CallbackContext context)
    {
        OnPageNext?.Invoke();
    }

    private void HandlePagePrevious(InputAction.CallbackContext context)
    {
        OnPagePrevious?.Invoke();
    }

    private void HandleClose(InputAction.CallbackContext context)
    {
        OnClose?.Invoke();
    }

    private void HandleSelect(InputAction.CallbackContext context)
    {
        OnSelect?.Invoke();
    }

    void OnDestroy()
    {
        Desactivar();
    }
}
