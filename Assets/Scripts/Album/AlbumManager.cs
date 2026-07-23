using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Coordinador principal del álbum. Conecta:
/// - AlbumData (lógica de colección)
/// - AlbumUI (interfaz visual)
/// - AlbumInputHandler (input del jugador)
/// - SaveSystem (persistencia)
/// 
/// Expone métodos públicos para que otros sistemas interactúen con el álbum.
/// </summary>
public class AlbumManager : MonoBehaviour
{
    [Header("Referencias")]
    public AlbumUI albumUI;
    public InputActionAsset inputActions;

    [Header("Templates disponibles (para debug/test)")]
    public FichaTemplate[] fichasDisponibles;

    // Datos internos
    private AlbumData albumData;
    private AlbumInputHandler inputHandler;
    private int paginaActual = 0;
    private bool albumAbierto = false;

    // Input del Player para desactivar cuando el álbum está abierto
    private InputActionMap playerActionMap;

    public static AlbumManager Instance { get; private set; }

    void Awake()
    {
        // Singleton simple
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Configurar input handler
        inputHandler = gameObject.AddComponent<AlbumInputHandler>();
        inputHandler.inputActions = inputActions;
    }

    void Start()
    {
        // Cargar datos guardados
        CargarDatos();

        // Obtener Player action map para desactivarlo cuando se abre el álbum
        if (inputActions != null)
        {
            playerActionMap = inputActions.FindActionMap("Player");
        }

        // Suscribirse a eventos de input
        inputHandler.OnNavigate += HandleNavigate;
        inputHandler.OnPageNext += HandlePageNext;
        inputHandler.OnPagePrevious += HandlePagePrevious;
        inputHandler.OnClose += HandleClose;
        inputHandler.OnSelect += HandleSelect;

        // Suscribirse a botones de UI
        if (albumUI != null)
        {
            if (albumUI.botonCerrar != null)
                albumUI.botonCerrar.onClick.AddListener(CerrarAlbum);
            if (albumUI.botonSiguiente != null)
                albumUI.botonSiguiente.onClick.AddListener(PaginaSiguiente);
            if (albumUI.botonAnterior != null)
                albumUI.botonAnterior.onClick.AddListener(PaginaAnterior);
        }

        // Asegurarse de que el álbum empiece cerrado
        if (albumUI != null)
            albumUI.Ocultar();

        Debug.Log($"[AlbumManager] Iniciado. Fichas en álbum: {albumData.ObtenerTotalFichasUnicas()} únicas, {albumData.ObtenerTotalFichas()} total.");
    }

    // ===== API PÚBLICA =====

    /// <summary>
    /// Abre el álbum. Activa la UI y el input del álbum, desactiva el input del jugador.
    /// </summary>
    public void AbrirAlbum()
    {
        if (albumAbierto) return;

        albumAbierto = true;
        paginaActual = 0;

        // Desactivar input del jugador
        playerActionMap?.Disable();

        // Activar input del álbum
        inputHandler.Activar();

        // Mostrar UI
        if (albumUI != null)
        {
            albumUI.Mostrar();
            ActualizarUI();
        }

        // Pausar el juego
        Time.timeScale = 0f;

        Debug.Log("[AlbumManager] Álbum abierto.");
    }

    /// <summary>
    /// Cierra el álbum. Desactiva la UI y el input del álbum, reactiva el input del jugador.
    /// </summary>
    public void CerrarAlbum()
    {
        if (!albumAbierto) return;

        albumAbierto = false;

        // Desactivar input del álbum
        inputHandler.Desactivar();

        // Ocultar UI
        if (albumUI != null)
            albumUI.Ocultar();

        // Reactivar input del jugador
        playerActionMap?.Enable();

        // Reanudar el juego
        Time.timeScale = 1f;

        // Guardar datos al cerrar
        GuardarDatos();

        Debug.Log("[AlbumManager] Álbum cerrado. Datos guardados.");
    }

    /// <summary>
    /// Agrega una ficha al álbum usando un template.
    /// </summary>
    public void AgregarFicha(FichaTemplate template)
    {
        if (template == null) return;

        albumData.AgregarFicha(template);
        Debug.Log($"[AlbumManager] Ficha '{template.nombre}' agregada al álbum. Total: x{albumData.ObtenerCantidad(template.id)}");

        // Si el álbum está abierto, refrescar la vista
        if (albumAbierto)
        {
            ActualizarUI();
        }
    }

    /// <summary>
    /// Agrega una ficha al álbum usando FichaData directamente.
    /// </summary>
    public void AgregarFicha(FichaData fichaData)
    {
        if (fichaData == null) return;

        albumData.AgregarFicha(fichaData);
        Debug.Log($"[AlbumManager] Ficha '{fichaData.nombre}' agregada al álbum. Total: x{albumData.ObtenerCantidad(fichaData.templateId)}");

        if (albumAbierto)
        {
            ActualizarUI();
        }
    }

    /// <summary>
    /// Verifica si el álbum está abierto.
    /// </summary>
    public bool EstaAbierto()
    {
        return albumAbierto;
    }

    /// <summary>
    /// Obtiene los datos del álbum (para lectura externa).
    /// </summary>
    public AlbumData ObtenerAlbumData()
    {
        return albumData;
    }

    // ===== HANDLERS DE INPUT =====

    private void HandleNavigate(int direction)
    {
        if (!albumAbierto || albumUI == null) return;
        albumUI.MoverSeleccion(direction);
    }

    private void HandlePageNext()
    {
        if (!albumAbierto) return;
        PaginaSiguiente();
    }

    private void HandlePagePrevious()
    {
        if (!albumAbierto) return;
        PaginaAnterior();
    }

    private void HandleClose()
    {
        CerrarAlbum();
    }

    private void HandleSelect()
    {
        // Por ahora, seleccionar muestra los detalles (ya se hace automáticamente con la navegación).
        // En el futuro podría abrir un submenú de la ficha.
        if (!albumAbierto || albumUI == null) return;

        var entrada = albumUI.ObtenerEntradaSeleccionada();
        if (entrada != null)
        {
            Debug.Log($"[AlbumManager] Ficha seleccionada: {entrada.ficha.nombre} (x{entrada.cantidad})");
        }
    }

    // ===== PAGINACIÓN =====

    private void PaginaSiguiente()
    {
        int totalPaginas = albumData.ObtenerTotalPaginas();
        if (paginaActual < totalPaginas - 1)
        {
            paginaActual++;
            ActualizarUI();
            // Al cambiar de página, selección va al primer slot
            albumUI?.SetIndiceSeleccionado(0);
        }
    }

    private void PaginaAnterior()
    {
        if (paginaActual > 0)
        {
            paginaActual--;
            ActualizarUI();
            // Al cambiar de página, selección va al primer slot
            albumUI?.SetIndiceSeleccionado(0);
        }
    }

    // ===== UI =====

    private void ActualizarUI()
    {
        if (albumUI == null) return;

        List<AlbumEntry> entradas = albumData.ObtenerFichasPaginadas(paginaActual);
        int totalPaginas = albumData.ObtenerTotalPaginas();

        albumUI.ActualizarLista(entradas, paginaActual, totalPaginas);
    }

    // ===== PERSISTENCIA =====

    private void CargarDatos()
    {
        SaveData saveData = SaveSystem.Cargar();

        if (saveData != null && saveData.album != null)
        {
            albumData = saveData.album.RestaurarAlbum();
            Debug.Log($"[AlbumManager] Datos cargados: {albumData.ObtenerTotalFichasUnicas()} fichas únicas.");
        }
        else
        {
            albumData = new AlbumData();
            Debug.Log("[AlbumManager] No hay datos guardados. Álbum vacío creado.");
        }
    }

    private void GuardarDatos()
    {
        SaveData saveData = new SaveData();
        saveData.album = AlbumSaveData.CrearDesdeAlbum(albumData);
        SaveSystem.Guardar(saveData);
    }

    void OnDestroy()
    {
        // Desuscribirse de eventos
        if (inputHandler != null)
        {
            inputHandler.OnNavigate -= HandleNavigate;
            inputHandler.OnPageNext -= HandlePageNext;
            inputHandler.OnPagePrevious -= HandlePagePrevious;
            inputHandler.OnClose -= HandleClose;
            inputHandler.OnSelect -= HandleSelect;
        }

        // Asegurarse de que el tiempo vuelva a la normalidad
        Time.timeScale = 1f;
    }

    void OnApplicationQuit()
    {
        // Guardar al salir si el álbum tiene datos
        if (albumData != null && albumData.ObtenerTotalFichas() > 0)
        {
            GuardarDatos();
        }
    }
}
