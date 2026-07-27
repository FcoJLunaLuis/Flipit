using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script de test que simula el flujo completo de apuesta-album.
/// Al iniciar: llena el album con fichas de prueba, muestra la UI de selección,
/// permite al jugador apostar, genera la apuesta del NPC, y construye la torre.
/// Colocar en la escena BettingAlbumIntegrationTestScene.
/// </summary>
public class BettingIntegrationTestRunner : MonoBehaviour
{
    [Header("Fichas de Prueba")]
    [SerializeField] private FichaTemplate[] _fichasTest;
    [SerializeField] private int _cantidadPorFicha = 3;

    [Header("NPC Pool")]
    [SerializeField] private FichaTemplate[] _fichasNPC;

    [Header("UI de Selección de Apuesta")]
    [SerializeField] private GameObject _betSelectionPanel;
    [SerializeField] private Transform _contenedorFichas;
    [SerializeField] private GameObject _fichaSlotPrefab;
    [SerializeField] private TextMeshProUGUI _contadorTexto;
    [SerializeField] private TextMeshProUGUI _lanzadoraTexto;
    [SerializeField] private TextMeshProUGUI _estadoTexto;
    [SerializeField] private Button _botonConfirmar;
    [SerializeField] private Button _botonIniciar;
    [SerializeField] private Button _botonVoltear;

    [Header("Torre Visual")]
    [SerializeField] private Transform _contenedorTorre;
    [SerializeField] private GameObject _fichaTorrePrefab;

    [Header("Configuración")]
    [SerializeField] private int _maxApuesta = 5;
    [SerializeField] private int _minApuesta = 1;
    [SerializeField] private float _espaciadoVertical = 0.12f;

    private AlbumData _albumData;
    private BetSelectionLogic _betLogic;
    private NPCBetGenerator _npcBetGenerator;
    private List<FichaData> _fichasNPCPool;
    private List<GameObject> _slotsInstanciados = new List<GameObject>();
    private List<GameObject> _torreInstanciada = new List<GameObject>();
    private List<TowerSlot> _torreActual;
    private bool _turnoJugador = true;
    private int _rondaActual = 0;
    private List<FichaData> _fichasGanadasJugador = new List<FichaData>();
    private List<FichaData> _fichasGanadasNPC = new List<FichaData>();

    // UI dinámica
    private GameObject _panelResultado;
    private TextMeshProUGUI _textoResultado;
    private Button _botonCerrarResultado;
    private GameObject _panelAlbum;
    private TextMeshProUGUI _textoAlbum;
    private Button _botonCerrarAlbum;
    private Button _botonVerAlbum;

    private void Start()
    {
        // Esperar un frame para que AlbumManager.Instance se inicialice
        StartCoroutine(InicializarConDelay());
    }

    private System.Collections.IEnumerator InicializarConDelay()
    {
        yield return null; // esperar un frame

        InicializarAlbum();
        InicializarNPCPool();
        _betLogic = new BetSelectionLogic(_maxApuesta, _minApuesta);
        _npcBetGenerator = new NPCBetGenerator(_maxApuesta);

        // Buscar botones si no están asignados
        if (_botonIniciar == null)
        {
            var go = GameObject.Find("BotonIniciar");
            if (go != null) _botonIniciar = go.GetComponent<Button>();
        }
        if (_botonConfirmar == null)
        {
            var go = GameObject.Find("BotonConfirmar");
            if (go != null) _botonConfirmar = go.GetComponent<Button>();
        }
        if (_betSelectionPanel == null)
        {
            _betSelectionPanel = GameObject.Find("BetSelectionPanel");
        }
        if (_contenedorFichas == null)
        {
            var go = GameObject.Find("FichasContainer");
            if (go != null) _contenedorFichas = go.transform;
        }
        if (_fichaSlotPrefab == null)
        {
            var go = GameObject.Find("FichaSlotPrefab");
            if (go != null) _fichaSlotPrefab = go;
        }
        if (_contadorTexto == null)
        {
            var go = GameObject.Find("ContadorTexto");
            if (go != null) _contadorTexto = go.GetComponent<TextMeshProUGUI>();
        }
        if (_lanzadoraTexto == null)
        {
            var go = GameObject.Find("LanzadoraTexto");
            if (go != null) _lanzadoraTexto = go.GetComponent<TextMeshProUGUI>();
        }
        if (_estadoTexto == null)
        {
            var go = GameObject.Find("EstadoTexto");
            if (go != null) _estadoTexto = go.GetComponent<TextMeshProUGUI>();
        }

        if (_botonIniciar != null)
        {
            _botonIniciar.onClick.AddListener(MostrarSeleccion);
            Debug.Log("[BettingTestRunner] Boton Iniciar conectado.");
        }
        else
        {
            Debug.LogError("[BettingTestRunner] BotonIniciar NO encontrado!");
        }

        if (_botonConfirmar != null)
            _botonConfirmar.onClick.AddListener(ConfirmarApuesta);

        // Botón voltear - crear dinámicamente
        if (_botonVoltear == null)
        {
            var canvasGO = GameObject.Find("UICanvas");
            if (canvasGO != null)
            {
                var btnGO = new GameObject("BotonVoltear");
                btnGO.transform.SetParent(canvasGO.transform, false);
                var rt = btnGO.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.35f, 0.12f);
                rt.anchorMax = new Vector2(0.65f, 0.2f);
                rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                btnGO.AddComponent<Image>().color = new Color(0.9f, 0.5f, 0.0f);
                _botonVoltear = btnGO.AddComponent<Button>();
                var txtGO = new GameObject("Text"); txtGO.transform.SetParent(btnGO.transform, false);
                var trt = txtGO.AddComponent<RectTransform>();
                trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
                var tmp = txtGO.AddComponent<TextMeshProUGUI>();
                tmp.text = "VOLTEAR FICHA"; tmp.fontSize = 20; tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white;
                btnGO.SetActive(false);
            }
        }

        if (_botonVoltear != null)
            _botonVoltear.onClick.AddListener(RealizarVolteoRonda);

        if (_betSelectionPanel != null)
            _betSelectionPanel.SetActive(false);

        // Crear panel de resultado
        CrearPanelResultado();
        // Crear panel de álbum
        CrearPanelAlbum();
        // Crear botón Ver Álbum
        CrearBotonVerAlbum();

        ActualizarEstado($"Listo! Album: {_albumData.ObtenerTotalFichas()} fichas. Presiona INICIAR COMBATE.");
    }

    private void InicializarAlbum()
    {
        _albumData = new AlbumData();

        // Intentar obtener fichas de _fichasTest (asignadas via Inspector/Builder)
        FichaTemplate[] templates = _fichasTest;

        // Fallback: usar fichasDisponibles del AlbumManager
        if ((templates == null || templates.Length == 0) && AlbumManager.Instance != null)
        {
            templates = AlbumManager.Instance.fichasDisponibles;
        }

        // Fallback final: crear fichas en código directamente
        if (templates == null || templates.Length == 0)
        {
            Debug.Log("[BettingTestRunner] No se encontraron FichaTemplates. Creando fichas de prueba en codigo.");
            CrearFichasEnCodigo();
            return;
        }

        foreach (var template in templates)
        {
            if (template == null) continue;
            for (int i = 0; i < _cantidadPorFicha; i++)
            {
                _albumData.AgregarFicha(template);
                if (AlbumManager.Instance != null)
                    AlbumManager.Instance.AgregarFicha(template);
            }
        }

        Debug.Log($"[BettingTestRunner] Album inicializado: {_albumData.ObtenerTotalFichasUnicas()} tipos, {_albumData.ObtenerTotalFichas()} total.");
    }

    private void CrearFichasEnCodigo()
    {
        string[] nombres = { "Piedra", "Cristal", "Obsidiana", "Madera", "Fuego", "Agua" };
        Rareza[] rarezas = { Rareza.Comun, Rareza.Comun, Rareza.Raro, Rareza.Comun, Rareza.UltraRaro, Rareza.Raro };

        for (int i = 0; i < nombres.Length; i++)
        {
            var ficha = new FichaData
            {
                templateId = i + 1,
                nombre = nombres[i],
                rareza = rarezas[i],
                rango = 1,
                experienciaDeRango = 0f,
                estaRoto = false,
                desgaste = 0f,
                perk = "",
                peso = 1.0f + (i * 0.2f),
                suerte = 1.0f + (i * 0.1f)
            };

            for (int j = 0; j < _cantidadPorFicha; j++)
            {
                _albumData.AgregarFicha(ficha);
                if (AlbumManager.Instance != null)
                    AlbumManager.Instance.AgregarFicha(ficha);
            }
        }

        Debug.Log($"[BettingTestRunner] Fichas creadas en codigo: {_albumData.ObtenerTotalFichasUnicas()} tipos, {_albumData.ObtenerTotalFichas()} total.");
    }

    private void InicializarNPCPool()
    {
        _fichasNPCPool = new List<FichaData>();
        var templates = _fichasNPC != null && _fichasNPC.Length > 0 ? _fichasNPC : _fichasTest;

        if (templates != null && templates.Length > 0)
        {
            foreach (var t in templates)
            {
                if (t == null) continue;
                _fichasNPCPool.Add(FichaData.CrearDesdePlantilla(t));
                _fichasNPCPool.Add(FichaData.CrearDesdePlantilla(t));
            }
        }
        else
        {
            // Crear NPC pool en código
            string[] nombres = { "NPC_Roca", "NPC_Hielo", "NPC_Viento", "NPC_Trueno", "NPC_Sombra", "NPC_Luz" };
            for (int i = 0; i < nombres.Length; i++)
            {
                _fichasNPCPool.Add(new FichaData
                {
                    templateId = 100 + i,
                    nombre = nombres[i],
                    rareza = (Rareza)(i % 3),
                    rango = 1, estaRoto = false, peso = 1.0f, suerte = 1.0f
                });
            }
        }
        Debug.Log($"[BettingTestRunner] NPC pool: {_fichasNPCPool.Count} fichas.");
    }

    private void MostrarSeleccion()
    {
        _betLogic.Limpiar();
        LimpiarTorre();

        if (_betSelectionPanel != null)
            _betSelectionPanel.SetActive(true);

        if (_botonIniciar != null)
            _botonIniciar.gameObject.SetActive(false);

        GenerarSlots();
        ActualizarUI();
        ActualizarEstado("Selecciona fichas para apostar (click) y una lanzadora (click derecho)");
    }

    private void GenerarSlots()
    {
        LimpiarSlots();

        if (_contenedorFichas == null)
        {
            Debug.LogError("[BettingTestRunner] _contenedorFichas es null!");
            return;
        }

        var entradas = _albumData.ObtenerTodasLasFichas();
        Debug.Log($"[BettingTestRunner] GenerarSlots: {entradas.Count} entradas en album.");

        foreach (var entrada in entradas)
        {
            if (entrada.ficha.estaRoto || entrada.cantidad <= 0) continue;

            GameObject slotGO;

            if (_fichaSlotPrefab != null)
            {
                slotGO = Object.Instantiate(_fichaSlotPrefab, _contenedorFichas);
                slotGO.SetActive(true); // Importante: el prefab está inactivo
            }
            else
            {
                // Crear slot directamente si no hay prefab
                slotGO = new GameObject($"Slot_{entrada.ficha.nombre}");
                slotGO.transform.SetParent(_contenedorFichas, false);
                var rt = slotGO.AddComponent<RectTransform>();
                var le = slotGO.AddComponent<LayoutElement>();
                le.preferredHeight = 45;
                le.minHeight = 45;
                slotGO.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 0.8f);

                var txtGO = new GameObject("Text");
                txtGO.transform.SetParent(slotGO.transform, false);
                var txtRT = txtGO.AddComponent<RectTransform>();
                txtRT.anchorMin = new Vector2(0.05f, 0);
                txtRT.anchorMax = new Vector2(0.95f, 1);
                txtRT.offsetMin = Vector2.zero;
                txtRT.offsetMax = Vector2.zero;
                var tmp = txtGO.AddComponent<TextMeshProUGUI>();
                tmp.fontSize = 20;
                tmp.color = Color.white;
                tmp.alignment = TextAlignmentOptions.MidlineLeft;
            }

            // Configurar texto
            var textos = slotGO.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (textos.Length > 0)
                textos[0].text = $"{entrada.ficha.nombre} (x{entrada.cantidad}) [{entrada.ficha.rareza}]";

            // Botón click izquierdo
            var btn = slotGO.GetComponent<Button>();
            if (btn == null) btn = slotGO.AddComponent<Button>();

            var fichaCaptura = entrada.ficha;
            btn.onClick.AddListener(() => OnFichaClick(fichaCaptura));

            // Click derecho para lanzadora
            var trigger = slotGO.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (trigger == null) trigger = slotGO.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            var rightClick = new UnityEngine.EventSystems.EventTrigger.Entry();
            rightClick.eventID = UnityEngine.EventSystems.EventTriggerType.PointerClick;
            rightClick.callback.AddListener((data) =>
            {
                var pointerData = (UnityEngine.EventSystems.PointerEventData)data;
                if (pointerData.button == UnityEngine.EventSystems.PointerEventData.InputButton.Right)
                    OnFichaRightClick(fichaCaptura);
            });
            trigger.triggers.Add(rightClick);

            _slotsInstanciados.Add(slotGO);
        }

        Debug.Log($"[BettingTestRunner] {_slotsInstanciados.Count} slots generados.");
    }

    private void OnFichaClick(FichaData ficha)
    {
        if (_betLogic.EstaSeleccionada(ficha.templateId))
            _betLogic.DeseleccionarFicha(ficha);
        else
            _betLogic.SeleccionarFicha(ficha);
        ActualizarUI();
    }

    private void OnFichaRightClick(FichaData ficha)
    {
        _betLogic.SeleccionarFichaLanzadora(ficha);
        ActualizarUI();
    }

    private void ConfirmarApuesta()
    {
        string error;
        if (!_betLogic.ValidarSeleccion(out error))
        {
            ActualizarEstado($"Error: {error}");
            return;
        }

        // Remover fichas del album temporalmente
        foreach (var ficha in _betLogic.FichasSeleccionadas)
            _albumData.RemoverFicha(ficha.templateId);
        _albumData.RemoverFicha(_betLogic.FichaLanzadora.templateId);

        // Generar apuesta NPC
        var fichasNPC = _npcBetGenerator.GenerarApuesta(_fichasNPCPool, _betLogic.CantidadSeleccionada);
        var lanzadoraNPC = _npcBetGenerator.SeleccionarFichaLanzadora(_fichasNPCPool, fichasNPC);

        // Construir torre
        var torre = TowerBuilder.ConstruirTorre(_betLogic.FichasSeleccionadas, fichasNPC);
        _torreActual = torre;
        _turnoJugador = true;
        _rondaActual = 1;

        // Ocultar panel de selección
        if (_betSelectionPanel != null)
            _betSelectionPanel.SetActive(false);

        // Mostrar torre visual
        ConstruirTorreVisual(torre);

        // Construir resumen
        string resumenJugador = "TUS FICHAS: ";
        foreach (var f in _betLogic.FichasSeleccionadas)
            resumenJugador += f.nombre + ", ";

        string resumenNPC = "NPC APUESTA: ";
        foreach (var f in fichasNPC)
            resumenNPC += f.nombre + ", ";

        string msg = $"¡TORRE CONSTRUIDA!\n{resumenJugador}\n{resumenNPC}\nLanzadora: {_betLogic.FichaLanzadora.nombre} | Torre: {torre.Count} fichas";
        ActualizarEstado(msg);

        // Mostrar botón de voltear
        if (_botonVoltear != null)
            _botonVoltear.gameObject.SetActive(false); // Se mostrará después del conteo

        // Ocultar botón iniciar
        if (_botonIniciar != null)
            _botonIniciar.gameObject.SetActive(false);

        // Iniciar conteo regresivo antes del volteo
        StartCoroutine(ConteoRegresivo());

        Debug.Log($"[BettingTestRunner] {msg}");
    }

    private System.Collections.IEnumerator ConteoRegresivo()
    {
        ActualizarEstado("3...");
        yield return new WaitForSeconds(1f);
        ActualizarEstado("2...");
        yield return new WaitForSeconds(1f);
        ActualizarEstado("1...");
        yield return new WaitForSeconds(1f);
        ActualizarEstado("¡VOLTEO!");
        yield return new WaitForSeconds(0.5f);

        // Ahora iniciar el volteo automático por rondas
        StartCoroutine(VolteoAutomatico());
    }

    private System.Collections.IEnumerator VolteoAutomatico()
    {
        while (true)
        {
            var sinVoltear = _torreActual.FindAll(s => !s.EstaVolteada);
            if (sinVoltear.Count == 0)
            {
                TerminarVolteo();
                yield break;
            }

            RealizarVolteoRonda();

            sinVoltear = _torreActual.FindAll(s => !s.EstaVolteada);
            if (sinVoltear.Count == 0)
            {
                TerminarVolteo();
                yield break;
            }

            yield return new WaitForSeconds(1.5f);
        }
    }

    private void ConstruirTorreVisual(List<TowerSlot> torre)
    {
        LimpiarTorre();
        if (_contenedorTorre == null) return;

        foreach (var slot in torre)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.transform.SetParent(_contenedorTorre);
            float posY = slot.PosicionEnTorre.y * _espaciadoVertical;
            go.transform.localPosition = new Vector3(0, posY, 0);
            go.transform.localScale = new Vector3(0.8f, 0.04f, 0.8f);

            bool esJugador = slot.Dueno == TowerSlot.SlotOwner.Jugador;
            var color = esJugador
                ? new Color(0.2f, 0.6f, 1f)
                : new Color(1f, 0.3f, 0.2f);

            var rend = go.GetComponent<Renderer>();
            if (rend != null)
                rend.material.color = color;

            // Etiqueta 3D con nombre de la ficha - desplazada a la derecha
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform);
            labelGO.transform.localPosition = new Vector3(12f, 0f, 0);
            var tm = labelGO.AddComponent<TextMesh>();
            string duenoTxt = esJugador ? "[TU]" : "[NPC]";
            tm.text = $"{duenoTxt} {slot.Ficha.nombre}";
            tm.fontSize = 28;
            tm.characterSize = 0.06f;
            tm.anchor = TextAnchor.MiddleLeft;
            tm.color = esJugador ? Color.cyan : new Color(1f, 0.7f, 0.5f);
            tm.alignment = TextAlignment.Left;

            go.name = $"Ficha_{slot.Ficha.nombre}_{slot.Dueno}";
            _torreInstanciada.Add(go);
        }

        Debug.Log($"[BettingTestRunner] Torre visual: {_torreInstanciada.Count} fichas apiladas.");

        // Acercar cámara a la torre para verla mejor
        var cam = Camera.main;
        if (cam != null)
        {
            float alturaMedia = (_torreInstanciada.Count * _espaciadoVertical) / 2f;
            cam.transform.position = new Vector3(0, alturaMedia, -3f);
            cam.orthographicSize = 1.5f;
        }

        // Ocultar el letrero NPC Retador y el Info para no tapar la torre
        var npcLabel = GameObject.Find("FlipCombat_NPC_Test");
        if (npcLabel != null) npcLabel.SetActive(false);
        var infoLabel = GameObject.Find("Info");
        if (infoLabel != null) infoLabel.SetActive(false);
    }

    private void RealizarVolteoRonda()
    {
        if (_torreActual == null || _torreActual.Count == 0) return;

        // Buscar fichas no volteadas
        var sinVoltear = _torreActual.FindAll(s => !s.EstaVolteada);
        if (sinVoltear.Count == 0) return;

        // Simular volteo: quien lanza voltea 1-3 fichas aleatorias
        int cantidadVolteo = Mathf.Min(Random.Range(1, 4), sinVoltear.Count);
        string quienLanza = _turnoJugador ? "JUGADOR" : "NPC";
        string fichasVolteadasStr = "";

        for (int i = 0; i < cantidadVolteo; i++)
        {
            int idx = Random.Range(0, sinVoltear.Count);
            var slot = sinVoltear[idx];
            slot.Voltear();
            sinVoltear.RemoveAt(idx);

            fichasVolteadasStr += $"{slot.Ficha.nombre}({slot.Dueno}), ";

            // Actualizar visual - oscurecer la ficha volteada
            int torreIdx = _torreActual.IndexOf(slot);
            if (torreIdx >= 0 && torreIdx < _torreInstanciada.Count)
            {
                var go = _torreInstanciada[torreIdx];
                var rend = go.GetComponent<Renderer>();
                if (rend != null)
                    rend.material.color = _turnoJugador ? new Color(0.0f, 1f, 0.3f) : new Color(0.8f, 0.0f, 0.8f);

                // Rotar la ficha para indicar que fue volteada
                go.transform.localRotation = Quaternion.Euler(0, 0, 90f);

                // Actualizar etiqueta
                var tm = go.GetComponentInChildren<TextMesh>();
                if (tm != null)
                    tm.text = $"[{quienLanza} GANA] {slot.Ficha.nombre}";
            }
        }

        _rondaActual++;
        string estado = $"Ronda {_rondaActual - 1}: {quienLanza} volteó {cantidadVolteo} fichas: {fichasVolteadasStr}";

        var restantes = _torreActual.FindAll(s => !s.EstaVolteada);

        // Cambiar turno
        _turnoJugador = !_turnoJugador;
        string siguiente = _turnoJugador ? "JUGADOR" : "NPC";
        ActualizarEstado($"Ronda {_rondaActual - 1}: {quienLanza} volteó {cantidadVolteo} fichas\nRestantes: {restantes.Count} | Siguiente: {siguiente}");
    }

    private void TerminarVolteo()
    {
        _fichasGanadasJugador.Clear();
        _fichasGanadasNPC.Clear();

        // Determinar qué fichas ganó cada quien basado en el color del renderer
        for (int i = 0; i < _torreInstanciada.Count && i < _torreActual.Count; i++)
        {
            var go = _torreInstanciada[i];
            var slot = _torreActual[i];
            var rend = go.GetComponent<Renderer>();
            if (rend == null) continue;

            Color c = rend.material.color;
            if (Mathf.Approximately(c.r, 0f) && Mathf.Approximately(c.g, 1f) && c.b < 0.5f)
                _fichasGanadasJugador.Add(slot.Ficha);
            else
                _fichasGanadasNPC.Add(slot.Ficha);
        }

        // Agregar fichas ganadas al álbum del jugador
        foreach (var ficha in _fichasGanadasJugador)
            _albumData.AgregarFicha(ficha);

        // Restaurar la ficha lanzadora al álbum
        if (_betLogic.FichaLanzadora != null)
            _albumData.AgregarFicha(_betLogic.FichaLanzadora);

        string resultado = _fichasGanadasJugador.Count > _fichasGanadasNPC.Count ? "¡GANASTE!" :
                           _fichasGanadasNPC.Count > _fichasGanadasJugador.Count ? "PERDISTE" : "EMPATE";
        ActualizarEstado($"COMBATE TERMINADO - {resultado}");

        // Ocultar botón voltear
        if (_botonVoltear != null)
            _botonVoltear.gameObject.SetActive(false);

        // Mostrar diálogo de resultado
        MostrarResultadoCombate();

        // Mostrar botón reiniciar
        if (_botonIniciar != null)
        {
            _botonIniciar.gameObject.SetActive(true);
            var btnRT = _botonIniciar.GetComponent<RectTransform>();
            if (btnRT != null)
            {
                btnRT.anchorMin = new Vector2(0.35f, 0.02f);
                btnRT.anchorMax = new Vector2(0.65f, 0.09f);
            }
            var textos = _botonIniciar.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (textos.Length > 0) textos[0].text = "APOSTAR DE NUEVO";
        }

        Debug.Log($"[BettingTestRunner] Combate terminado. Jugador ganó: {_fichasGanadasJugador.Count}, NPC ganó: {_fichasGanadasNPC.Count}");
    }

    private void CrearPanelResultado()
    {
        var canvasGO = GameObject.Find("UICanvas");
        if (canvasGO == null) return;

        _panelResultado = new GameObject("PanelResultado");
        _panelResultado.transform.SetParent(canvasGO.transform, false);
        var rt = _panelResultado.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.15f, 0.15f); rt.anchorMax = new Vector2(0.85f, 0.85f);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        _panelResultado.AddComponent<Image>().color = new Color(0.05f, 0.1f, 0.15f, 0.97f);

        // Título
        var tituloGO = new GameObject("Titulo"); tituloGO.transform.SetParent(_panelResultado.transform, false);
        var trt = tituloGO.AddComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.05f, 0.85f); trt.anchorMax = new Vector2(0.95f, 0.98f);
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
        var tTmp = tituloGO.AddComponent<TextMeshProUGUI>();
        tTmp.text = "RESULTADO DEL COMBATE"; tTmp.fontSize = 28; tTmp.color = Color.yellow;
        tTmp.alignment = TextAlignmentOptions.Center; tTmp.fontStyle = FontStyles.Bold;

        // Texto resultados
        var textoGO = new GameObject("TextoResultado"); textoGO.transform.SetParent(_panelResultado.transform, false);
        var txrt = textoGO.AddComponent<RectTransform>();
        txrt.anchorMin = new Vector2(0.05f, 0.2f); txrt.anchorMax = new Vector2(0.95f, 0.83f);
        txrt.offsetMin = Vector2.zero; txrt.offsetMax = Vector2.zero;
        _textoResultado = textoGO.AddComponent<TextMeshProUGUI>();
        _textoResultado.fontSize = 20; _textoResultado.color = Color.white;
        _textoResultado.alignment = TextAlignmentOptions.TopLeft;

        // Botón cerrar
        var btnGO = new GameObject("BotonCerrar"); btnGO.transform.SetParent(_panelResultado.transform, false);
        var brt = btnGO.AddComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.3f, 0.03f); brt.anchorMax = new Vector2(0.7f, 0.15f);
        brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;
        btnGO.AddComponent<Image>().color = new Color(0.2f, 0.6f, 0.3f);
        _botonCerrarResultado = btnGO.AddComponent<Button>();
        var btGO = new GameObject("Text"); btGO.transform.SetParent(btnGO.transform, false);
        var btrt = btGO.AddComponent<RectTransform>(); btrt.anchorMin = Vector2.zero; btrt.anchorMax = Vector2.one; btrt.offsetMin = Vector2.zero; btrt.offsetMax = Vector2.zero;
        btGO.AddComponent<TextMeshProUGUI>().text = "CERRAR"; btGO.GetComponent<TextMeshProUGUI>().fontSize = 20; btGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        _botonCerrarResultado.onClick.AddListener(() => _panelResultado.SetActive(false));
        _panelResultado.SetActive(false);
    }

    private void CrearPanelAlbum()
    {
        var canvasGO = GameObject.Find("UICanvas");
        if (canvasGO == null) return;

        _panelAlbum = new GameObject("PanelAlbum");
        _panelAlbum.transform.SetParent(canvasGO.transform, false);
        var rt = _panelAlbum.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.1f, 0.1f); rt.anchorMax = new Vector2(0.9f, 0.9f);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        _panelAlbum.AddComponent<Image>().color = new Color(0.08f, 0.05f, 0.15f, 0.97f);

        // Título
        var tituloGO = new GameObject("Titulo"); tituloGO.transform.SetParent(_panelAlbum.transform, false);
        var trt = tituloGO.AddComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.05f, 0.88f); trt.anchorMax = new Vector2(0.95f, 0.98f);
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
        var tTmp = tituloGO.AddComponent<TextMeshProUGUI>();
        tTmp.text = "MI ÁLBUM DE FLIPITS"; tTmp.fontSize = 28; tTmp.color = Color.cyan;
        tTmp.alignment = TextAlignmentOptions.Center; tTmp.fontStyle = FontStyles.Bold;

        // Texto álbum
        var textoGO = new GameObject("TextoAlbum"); textoGO.transform.SetParent(_panelAlbum.transform, false);
        var txrt = textoGO.AddComponent<RectTransform>();
        txrt.anchorMin = new Vector2(0.05f, 0.15f); txrt.anchorMax = new Vector2(0.95f, 0.86f);
        txrt.offsetMin = Vector2.zero; txrt.offsetMax = Vector2.zero;
        _textoAlbum = textoGO.AddComponent<TextMeshProUGUI>();
        _textoAlbum.fontSize = 20; _textoAlbum.color = Color.white;
        _textoAlbum.alignment = TextAlignmentOptions.TopLeft;

        // Botón cerrar
        var btnGO = new GameObject("BotonCerrarAlbum"); btnGO.transform.SetParent(_panelAlbum.transform, false);
        var brt = btnGO.AddComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.3f, 0.03f); brt.anchorMax = new Vector2(0.7f, 0.12f);
        brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;
        btnGO.AddComponent<Image>().color = new Color(0.6f, 0.2f, 0.2f);
        _botonCerrarAlbum = btnGO.AddComponent<Button>();
        var btGO = new GameObject("Text"); btGO.transform.SetParent(btnGO.transform, false);
        var btrt = btGO.AddComponent<RectTransform>(); btrt.anchorMin = Vector2.zero; btrt.anchorMax = Vector2.one; btrt.offsetMin = Vector2.zero; btrt.offsetMax = Vector2.zero;
        btGO.AddComponent<TextMeshProUGUI>().text = "CERRAR"; btGO.GetComponent<TextMeshProUGUI>().fontSize = 20; btGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        _botonCerrarAlbum.onClick.AddListener(() => _panelAlbum.SetActive(false));
        _panelAlbum.SetActive(false);
    }

    private void CrearBotonVerAlbum()
    {
        var canvasGO = GameObject.Find("UICanvas");
        if (canvasGO == null) return;

        var btnGO = new GameObject("BotonVerAlbum");
        btnGO.transform.SetParent(canvasGO.transform, false);
        var rt = btnGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.02f, 0.9f); rt.anchorMax = new Vector2(0.18f, 0.98f);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        btnGO.AddComponent<Image>().color = new Color(0.3f, 0.2f, 0.7f);
        _botonVerAlbum = btnGO.AddComponent<Button>();
        var txtGO = new GameObject("Text"); txtGO.transform.SetParent(btnGO.transform, false);
        var trt = txtGO.AddComponent<RectTransform>(); trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
        txtGO.AddComponent<TextMeshProUGUI>().text = "VER ÁLBUM"; txtGO.GetComponent<TextMeshProUGUI>().fontSize = 18; txtGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center; txtGO.GetComponent<TextMeshProUGUI>().color = Color.white;

        _botonVerAlbum.onClick.AddListener(MostrarAlbum);
    }

    private void MostrarAlbum()
    {
        if (_panelAlbum == null) return;
        // Cerrar panel de resultado si está abierto
        if (_panelResultado != null) _panelResultado.SetActive(false);

        _panelAlbum.SetActive(true);

        string contenido = $"<b>Total: {_albumData.ObtenerTotalFichas()} fichas ({_albumData.ObtenerTotalFichasUnicas()} tipos)</b>\n\n";
        var entradas = _albumData.ObtenerTodasLasFichas();
        foreach (var entrada in entradas)
        {
            string rarColor = entrada.ficha.rareza == Rareza.UltraRaro ? "<color=yellow>" :
                              entrada.ficha.rareza == Rareza.Raro ? "<color=#5588FF>" : "<color=white>";
            contenido += $"{rarColor}● {entrada.ficha.nombre}</color> x{entrada.cantidad} [{entrada.ficha.rareza}]\n";
        }

        if (entradas.Count == 0)
            contenido += "<color=red>Álbum vacío</color>";

        _textoAlbum.text = contenido;
    }

    private void MostrarResultadoCombate()
    {
        if (_panelResultado == null) return;
        // Cerrar panel de álbum si está abierto
        if (_panelAlbum != null) _panelAlbum.SetActive(false);

        _panelResultado.SetActive(true);

        string resultado = _fichasGanadasJugador.Count > _fichasGanadasNPC.Count ? "<color=green>¡VICTORIA!</color>" :
                           _fichasGanadasNPC.Count > _fichasGanadasJugador.Count ? "<color=red>DERROTA</color>" : "<color=yellow>EMPATE</color>";

        string contenido = $"<size=24>{resultado}</size>\n\n";
        contenido += $"<b>Fichas que GANASTE ({_fichasGanadasJugador.Count}):</b>\n";
        foreach (var f in _fichasGanadasJugador)
            contenido += $"  <color=green>+ {f.nombre} [{f.rareza}]</color>  → Agregada al álbum\n";

        contenido += $"\n<b>Fichas que PERDISTE ({_fichasGanadasNPC.Count}):</b>\n";
        foreach (var f in _fichasGanadasNPC)
            contenido += $"  <color=red>- {f.nombre} [{f.rareza}]</color>  → Removida del álbum\n";

        contenido += $"\n──────────────────────────────";
        contenido += $"\n<b>ÁLBUM ACTUALIZADO:</b> {_albumData.ObtenerTotalFichas()} fichas ({_albumData.ObtenerTotalFichasUnicas()} tipos)";

        _textoResultado.text = contenido;
    }

    private void ActualizarUI()
    {
        if (_contadorTexto != null)
            _contadorTexto.text = $"Apostadas: {_betLogic.CantidadSeleccionada}/{_maxApuesta}";

        if (_lanzadoraTexto != null)
        {
            _lanzadoraTexto.text = _betLogic.FichaLanzadora != null
                ? $"Lanzadora: {_betLogic.FichaLanzadora.nombre}"
                : "Lanzadora: (click derecho)";
        }

        if (_botonConfirmar != null)
            _botonConfirmar.interactable = _betLogic.ApuestaCompleta;

        // Actualizar colores de slots
        for (int i = 0; i < _slotsInstanciados.Count; i++)
        {
            var slot = _slotsInstanciados[i];
            if (slot == null) continue;
            var img = slot.GetComponent<Image>();
            if (img == null) continue;

            var entradas = _albumData.ObtenerTodasLasFichas();
            if (i >= entradas.Count) continue;
            var ficha = entradas[i].ficha;

            if (_betLogic.EsLanzadora(ficha.templateId))
                img.color = new Color(0.2f, 0.8f, 0.9f, 0.9f);
            else if (_betLogic.EstaSeleccionada(ficha.templateId))
                img.color = new Color(1f, 0.9f, 0.2f, 0.9f);
            else
                img.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        }
    }

    private void ActualizarEstado(string mensaje)
    {
        if (_estadoTexto != null)
            _estadoTexto.text = mensaje;
        Debug.Log($"[BettingTestRunner] {mensaje}");
    }

    private void LimpiarSlots()
    {
        foreach (var go in _slotsInstanciados)
            if (go != null) Destroy(go);
        _slotsInstanciados.Clear();
    }

    private void LimpiarTorre()
    {
        foreach (var go in _torreInstanciada)
            if (go != null) Destroy(go);
        _torreInstanciada.Clear();
    }
}
