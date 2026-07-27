using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using Newtonsoft.Json;

/// <summary>
/// Puente entre el sistema de Combate y el Álbum del jugador.
/// Gestiona validación pre-combate, remoción temporal de fichas,
/// transferencia post-combate y recuperación ante crash.
/// Opera como componente observador externo al CombatManager,
/// suscribiéndose a sus eventos sin ser referenciado directamente.
/// </summary>
public class CombatAlbumBridge : MonoBehaviour
{
    // === Eventos ===

    /// <summary>
    /// Se dispara cuando el álbum ha sido actualizado post-combate.
    /// Los suscriptores reciben el CombatData con los resultados del combate.
    /// </summary>
    public Action<CombatData> OnAlbumActualizado;

    // === Configuración ===

    [Header("Configuración")]
    [SerializeField] private CombatConfig _combatConfig;

    [Header("UI Feedback")]
    [SerializeField] private GameObject _mensajeValidacionPanel;
    [SerializeField] private TextMeshProUGUI _mensajeValidacionTexto;

    // === Estado Interno ===

    private AlbumChipInventory _chipInventory;
    private AlbumChipCatalog _chipCatalog;
    private List<FichaEnJuegoEntry> _fichasEnJuego;
    private bool _apuestaConfirmada;

    private const string CRASH_RECOVERY_FILE = "fichas_en_juego.json";

    // === Lifecycle ===

    private void Awake()
    {
        IntentarRecuperacionCrash();
    }

    private void OnEnable()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombateIniciado += OnCombateIniciado;
            CombatManager.Instance.OnCombateTerminado += OnCombateTerminado;
            CombatManager.Instance.OnFaseCambiada += OnFaseCambiada;
        }
    }

    private void OnDisable()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombateIniciado -= OnCombateIniciado;
            CombatManager.Instance.OnCombateTerminado -= OnCombateTerminado;
            CombatManager.Instance.OnFaseCambiada -= OnFaseCambiada;
        }
    }

    // === API Pública ===

    /// <summary>
    /// Valida si el jugador tiene fichas suficientes para iniciar combate.
    /// Requiere al menos (minFichasApuesta + 1) fichas no rotas.
    /// Retorna true si puede combatir, false si no.
    /// </summary>
    public bool ValidarPreCombate()
    {
        if (AlbumManager.Instance == null)
        {
            Debug.LogError("[CombatAlbumBridge] AlbumManager.Instance no encontrado.");
            return false;
        }

        AlbumData albumData = AlbumManager.Instance.ObtenerAlbumData();
        List<AlbumEntry> todasLasFichas = albumData.ObtenerTodasLasFichas();

        if (todasLasFichas == null)
        {
            Debug.LogError("[CombatAlbumBridge] ObtenerTodasLasFichas retornó null.");
            return false;
        }

        int count = ContarFichasNoRotas();

        if (count == 0)
        {
            MostrarMensajeValidacion("No posees fichas en condiciones de combate.");
            return false;
        }

        if (count < _combatConfig.minFichasApuesta + 1)
        {
            if (count == _combatConfig.minFichasApuesta)
            {
                MostrarMensajeValidacion("Necesitas al menos una ficha adicional no rota para usar como lanzadora.");
                return false;
            }
            else
            {
                MostrarMensajeValidacion($"Se requieren al menos {_combatConfig.minFichasApuesta + 1} fichas no rotas para combatir.");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Obtiene las fichas disponibles del álbum para la fase de selección.
    /// Excluye fichas rotas y fichas con cantidad 0.
    /// </summary>
    public List<FichaData> ObtenerFichasDisponiblesParaApuesta()
    {
        if (AlbumManager.Instance == null)
        {
            return new List<FichaData>();
        }

        AlbumData albumData = AlbumManager.Instance.ObtenerAlbumData();
        if (albumData == null)
        {
            return new List<FichaData>();
        }

        List<AlbumEntry> todasLasFichas = albumData.ObtenerTodasLasFichas();
        if (todasLasFichas == null)
        {
            return new List<FichaData>();
        }

        List<FichaData> fichasDisponibles = new List<FichaData>();
        foreach (AlbumEntry entry in todasLasFichas)
        {
            if (entry.ficha.estaRoto == false && entry.cantidad >= 1)
            {
                fichasDisponibles.Add(entry.ficha);
            }
        }

        return fichasDisponibles;
    }

    /// <summary>
    /// Confirma la apuesta: remueve fichas del álbum y persiste a disco.
    /// Implementa rollback transaccional: si alguna remoción falla,
    /// restaura todas las fichas previamente removidas en la misma operación.
    /// Retorna true si la operación fue exitosa.
    /// </summary>
    public bool ConfirmarApuesta(List<FichaData> fichasApostadas, FichaData fichaLanzadora)
    {
        if (fichasApostadas == null || fichasApostadas.Count == 0 || fichaLanzadora == null)
        {
            Debug.LogError("[CombatAlbumBridge] ConfirmarApuesta: fichasApostadas es null/vacía o fichaLanzadora es null.");
            return false;
        }

        if (AlbumManager.Instance == null)
        {
            Debug.LogError("[CombatAlbumBridge] ConfirmarApuesta: AlbumManager.Instance no encontrado.");
            return false;
        }

        // Initialize chip inventory if needed
        if (_chipInventory == null)
        {
            var albumData = AlbumManager.Instance.ObtenerAlbumData();
            _chipInventory = new AlbumChipInventory(albumData, _chipCatalog);
        }

        var fichasRemovidas = new List<(string chipId, int cantidad)>();

        try
        {
            // Remove each bet chip
            foreach (var ficha in fichasApostadas)
            {
                string chipId = ficha.templateId.ToString();
                if (!_chipInventory.RemoveChips(chipId, 1))
                {
                    // Rollback: restore all previously removed chips
                    foreach (var removida in fichasRemovidas)
                    {
                        _chipInventory.AddChips(removida.chipId, removida.cantidad);
                    }
                    Debug.LogError($"[CombatAlbumBridge] Fallo al remover ficha {chipId}. Rollback ejecutado.");
                    return false;
                }
                fichasRemovidas.Add((chipId, 1));
            }

            // Remove lanzadora
            string lanzadoraId = fichaLanzadora.templateId.ToString();
            if (!_chipInventory.RemoveChips(lanzadoraId, 1))
            {
                // Rollback: restore all previously removed chips
                foreach (var removida in fichasRemovidas)
                {
                    _chipInventory.AddChips(removida.chipId, removida.cantidad);
                }
                Debug.LogError($"[CombatAlbumBridge] Fallo al remover lanzadora {lanzadoraId}. Rollback ejecutado.");
                return false;
            }

            // Build fichas en juego list
            _fichasEnJuego = new List<FichaEnJuegoEntry>();
            foreach (var ficha in fichasApostadas)
            {
                _fichasEnJuego.Add(new FichaEnJuegoEntry
                {
                    templateId = ficha.templateId,
                    cantidad = 1,
                    esLanzadora = false,
                    fichaData = ficha
                });
            }
            _fichasEnJuego.Add(new FichaEnJuegoEntry
            {
                templateId = fichaLanzadora.templateId,
                cantidad = 1,
                esLanzadora = true,
                fichaData = fichaLanzadora
            });

            // Persist to disk for crash recovery
            PersistirFichasEnJuego();

            _apuestaConfirmada = true;
            Debug.Log($"[CombatAlbumBridge] Apuesta confirmada: {fichasApostadas.Count} fichas + lanzadora removidas del álbum.");
            return true;
        }
        catch (Exception ex)
        {
            // Rollback on unexpected exception
            foreach (var removida in fichasRemovidas)
            {
                _chipInventory.AddChips(removida.chipId, removida.cantidad);
            }
            Debug.LogError($"[CombatAlbumBridge] Excepción durante confirmación: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Procesa el resultado del combate: transfiere fichas ganadas,
    /// mantiene perdidas removidas, restaura lanzadora, guarda.
    /// </summary>
    public void ProcesarResultadoCombate(CombatData combatData)
    {
        if (combatData == null)
        {
            Debug.LogWarning("[CombatAlbumBridge] CombatData nulo, no se puede procesar resultado.");
            return;
        }

        if (AlbumManager.Instance == null)
        {
            Debug.LogError("[CombatAlbumBridge] AlbumManager.Instance no encontrado para procesar resultado.");
            return;
        }

        // 1. Agregar fichas ganadas al álbum (Requirement 7.1)
        if (combatData.FichasGanadasJugador != null)
        {
            foreach (var ficha in combatData.FichasGanadasJugador)
            {
                AlbumManager.Instance.AgregarFicha(ficha);
            }
            Debug.Log($"[CombatAlbumBridge] {combatData.FichasGanadasJugador.Count} fichas ganadas agregadas al álbum.");
        }
        else
        {
            Debug.LogWarning("[CombatAlbumBridge] FichasGanadasJugador es null. Omitiendo operación.");
        }

        // 2. Manejar fichas perdidas (Requirement 8.1, 8.2, 8.3)
        if (combatData.FichasGanadasNPC != null && combatData.FichasApostadasJugador != null)
        {
            var apostadasIds = new HashSet<int>(combatData.FichasApostadasJugador.ConvertAll(f => f.templateId));
            foreach (var fichaGanadaNPC in combatData.FichasGanadasNPC)
            {
                if (apostadasIds.Contains(fichaGanadaNPC.templateId))
                {
                    // Ficha perdida por el jugador — remover de fichasEnJuego para que no se restaure
                    if (_fichasEnJuego != null)
                    {
                        _fichasEnJuego.RemoveAll(e => e.templateId == fichaGanadaNPC.templateId && !e.esLanzadora);
                    }
                }
            }
            Debug.Log($"[CombatAlbumBridge] Fichas perdidas procesadas. NPC ganó {combatData.FichasGanadasNPC.Count} fichas total.");
        }

        // 3. Restaurar fichas ganadas propias del jugador — remover de _fichasEnJuego (Requirement 7.2)
        if (combatData.FichasGanadasJugador != null && _fichasEnJuego != null)
        {
            var ganadasIds = new HashSet<int>(combatData.FichasGanadasJugador.ConvertAll(f => f.templateId));
            _fichasEnJuego.RemoveAll(e => ganadasIds.Contains(e.templateId) && !e.esLanzadora);
        }

        // 4. Restaurar ficha lanzadora (Requirement 7.3)
        if (combatData.FichaLanzadoraJugador != null)
        {
            AlbumManager.Instance.AgregarFicha(combatData.FichaLanzadoraJugador);
        }
        else
        {
            Debug.LogWarning("[CombatAlbumBridge] FichaLanzadoraJugador es null. Omitiendo restauración de lanzadora.");
        }

        // 5. Limpiar crash recovery (Requirement 3.6)
        EliminarArchivoCrashRecovery();
        _fichasEnJuego = null;
        _apuestaConfirmada = false;

        // 6. Disparar guardado (Requirement 9.1)
        StartCoroutine(GuardarConReintentos());

        // 7. Disparar evento (Requirement 7.4)
        OnAlbumActualizado?.Invoke(combatData);
    }

    // === Crash Recovery ===

    private void IntentarRecuperacionCrash()
    {
        string path = Path.Combine(Application.persistentDataPath, CRASH_RECOVERY_FILE);

        if (!File.Exists(path))
            return;

        string json = File.ReadAllText(path);

        try
        {
            var data = JsonConvert.DeserializeObject<FichasEnJuegoData>(json);

            if (data == null || data.fichas == null)
            {
                Debug.LogWarning("[CombatAlbumBridge] Archivo de crash recovery vacío o corrupto. Eliminando.");
                File.Delete(path);
                return;
            }

            foreach (var entry in data.fichas)
            {
                if (entry.fichaData != null && AlbumManager.Instance != null)
                {
                    for (int i = 0; i < entry.cantidad; i++)
                    {
                        AlbumManager.Instance.AgregarFicha(entry.fichaData);
                    }
                }
            }

            Debug.Log($"[CombatAlbumBridge] Crash recovery: {data.fichas.Count} fichas restauradas al álbum.");
            File.Delete(path);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[CombatAlbumBridge] Archivo de crash recovery corrupto: {ex.Message}. Eliminando.");
            try
            {
                File.Delete(path);
            }
            catch
            {
                // Avoid secondary failures
            }
        }
    }

    private void PersistirFichasEnJuego()
    {
        try
        {
            var data = new FichasEnJuegoData();
            data.timestampConfirmacion = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            data.fichas = _fichasEnJuego;

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            string path = Path.Combine(Application.persistentDataPath, CRASH_RECOVERY_FILE);
            File.WriteAllText(path, json);

            Debug.Log($"[CombatAlbumBridge] Fichas en juego persistidas: {_fichasEnJuego.Count} entradas en {path}");
        }
        catch (IOException ex)
        {
            Debug.LogError($"[CombatAlbumBridge] Error al persistir fichas en juego: {ex.Message}");
        }
    }

    private void EliminarArchivoCrashRecovery()
    {
        string path = Path.Combine(Application.persistentDataPath, CRASH_RECOVERY_FILE);

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("[CombatAlbumBridge] Archivo de crash recovery eliminado.");
        }
    }

    // === Guardado con Reintentos ===

    /// <summary>
    /// Intenta guardar el estado del álbum con hasta 3 reintentos.
    /// Si todos fallan, el estado queda en memoria para el próximo guardado.
    /// </summary>
    private IEnumerator GuardarConReintentos(int maxReintentos = 3, float intervalo = 1f)
    {
        if (AlbumManager.Instance == null)
        {
            Debug.LogError("[CombatAlbumBridge] AlbumManager.Instance no disponible para guardar.");
            yield break;
        }

        for (int intento = 1; intento <= maxReintentos; intento++)
        {
            SaveData saveData = new SaveData();
            saveData.album = AlbumSaveData.CrearDesdeAlbum(AlbumManager.Instance.ObtenerAlbumData());

            if (SaveSystem.Guardar(saveData))
            {
                int fichasUnicas = AlbumManager.Instance.ObtenerAlbumData().ObtenerTotalFichasUnicas();
                Debug.Log($"[AlbumManager] Guardado post-combate exitoso (intento {intento}). Fichas únicas persistidas: {fichasUnicas}");
                yield break;
            }

            Debug.LogWarning($"[AlbumManager] Guardado post-combate fallido (intento {intento}/{maxReintentos}). Reintentando en {intervalo}s...");
            yield return new WaitForSeconds(intervalo);
        }

        Debug.LogWarning("[AlbumManager] Todos los reintentos de guardado fallaron. Estado en memoria pendiente de persistencia.");
    }

    // === Handlers de Eventos ===

    private void OnCombateIniciado()
    {
        try
        {
            _apuestaConfirmada = false;
            _fichasEnJuego = null;
            OcultarMensajeValidacion();
            Debug.Log("[CombatAlbumBridge] Combate iniciado — estado interno reseteado.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CombatAlbumBridge] Error en OnCombateIniciado: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void OnCombateTerminado(CombatData data)
    {
        try
        {
            ProcesarResultadoCombate(data);
            Debug.Log("[CombatAlbumBridge] Combate terminado — resultados procesados.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CombatAlbumBridge] Error en OnCombateTerminado: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void OnFaseCambiada(CombatData.CombatPhase fase)
    {
        try
        {
            if (fase == CombatData.CombatPhase.BetSelection)
            {
                var fichasDisponibles = ObtenerFichasDisponiblesParaApuesta();
                Debug.Log($"[CombatAlbumBridge] Fase BetSelection — {fichasDisponibles.Count} fichas disponibles para apostar.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CombatAlbumBridge] Error en OnFaseCambiada: {ex.Message}\n{ex.StackTrace}");
        }
    }

    // === Helpers ===

    private int ContarFichasNoRotas()
    {
        AlbumData albumData = AlbumManager.Instance?.ObtenerAlbumData();
        if (albumData == null)
        {
            return 0;
        }

        List<AlbumEntry> todasLasFichas = albumData.ObtenerTodasLasFichas();
        if (todasLasFichas == null)
        {
            return 0;
        }

        int count = 0;
        foreach (AlbumEntry entry in todasLasFichas)
        {
            if (entry.ficha.estaRoto == false && entry.cantidad >= 1)
            {
                count++;
            }
        }

        return count;
    }

    private void MostrarMensajeValidacion(string mensaje)
    {
        if (_mensajeValidacionPanel != null)
        {
            _mensajeValidacionPanel.SetActive(true);
        }

        if (_mensajeValidacionTexto != null)
        {
            _mensajeValidacionTexto.text = mensaje;
        }

        Debug.LogWarning($"[CombatAlbumBridge] Validación: {mensaje}");
    }

    private void OcultarMensajeValidacion()
    {
        if (_mensajeValidacionPanel != null)
        {
            _mensajeValidacionPanel.SetActive(false);
        }
    }
}
