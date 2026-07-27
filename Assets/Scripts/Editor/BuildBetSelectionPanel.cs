using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

/// <summary>
/// Editor script que construye el panel BetSelectionUI dentro del CombatCanvas.
/// Menú: Flipit → Build BetSelection Panel
/// Crea la jerarquía UI completa con 3 zonas y asigna todas las referencias.
/// </summary>
public class BuildBetSelectionPanel
{
    [MenuItem("Flipit/Build BetSelection Panel")]
    public static void Build()
    {
        // Buscar CombatCanvas en la escena
        var combatCanvas = GameObject.Find("--- COMBAT SYSTEM ---/CombatCanvas");
        if (combatCanvas == null)
        {
            Debug.LogError("[BuildBetSelectionPanel] No se encontró '--- COMBAT SYSTEM ---/CombatCanvas' en la escena.");
            return;
        }

        // Verificar si ya existe
        var existente = combatCanvas.transform.Find("BetSelectionPanel");
        if (existente != null)
        {
            Debug.LogError("[BuildBetSelectionPanel] BetSelectionPanel ya existe. Elimínalo antes de reconstruir.");
            return;
        }

        // Buscar o crear el slot prefab
        GameObject slotPrefab = FindOrCreateSlotPrefab();

        // === ROOT PANEL (fullscreen overlay) ===
        var panel = CreateUIObject("BetSelectionPanel", combatCanvas.transform);
        var panelRect = panel.GetComponent<RectTransform>();
        StretchFull(panelRect);
        var panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.85f);

        // === TÍTULO ===
        var titulo = CreateTextObject("TituloText", panel.transform, "SELECCIÓN DE APUESTA", 28);
        var tituloRect = titulo.GetComponent<RectTransform>();
        tituloRect.anchorMin = new Vector2(0f, 0.9f);
        tituloRect.anchorMax = new Vector2(1f, 1f);
        tituloRect.offsetMin = Vector2.zero;
        tituloRect.offsetMax = Vector2.zero;

        // === ZONA CENTRAL (contenedor de los dos paneles) ===
        var zonaCentral = CreateUIObject("ZonaCentral", panel.transform);
        var zcRect = zonaCentral.GetComponent<RectTransform>();
        zcRect.anchorMin = new Vector2(0.02f, 0.3f);
        zcRect.anchorMax = new Vector2(0.98f, 0.88f);
        zcRect.offsetMin = Vector2.zero;
        zcRect.offsetMax = Vector2.zero;
        var zcLayout = zonaCentral.AddComponent<HorizontalLayoutGroup>();
        zcLayout.spacing = 20f;
        zcLayout.childForceExpandWidth = true;
        zcLayout.childForceExpandHeight = true;
        zcLayout.padding = new RectOffset(10, 10, 10, 10);

        // === PANEL IZQUIERDO - Fichas Apostadas ===
        var panelIzq = CreateUIObject("PanelApostadas", zonaCentral.transform);
        var pIzqImage = panelIzq.AddComponent<Image>();
        pIzqImage.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);
        var pIzqLayout = panelIzq.AddComponent<VerticalLayoutGroup>();
        pIzqLayout.spacing = 5f;
        pIzqLayout.padding = new RectOffset(10, 10, 10, 10);
        pIzqLayout.childForceExpandWidth = true;
        pIzqLayout.childForceExpandHeight = false;
        pIzqLayout.childAlignment = TextAnchor.UpperCenter;
        var pIzqElement = panelIzq.AddComponent<LayoutElement>();
        pIzqElement.flexibleWidth = 0.4f;

        // Título panel izquierdo
        var tituloIzq = CreateTextObject("TituloApostadas", panelIzq.transform, "FICHAS APOSTADAS", 18);
        var tituloIzqLE = tituloIzq.AddComponent<LayoutElement>();
        tituloIzqLE.preferredHeight = 30f;

        // Contador
        var contadorGO = CreateTextObject("ContadorText", panelIzq.transform, "Apostadas: 0/5", 14);
        var contadorLE = contadorGO.AddComponent<LayoutElement>();
        contadorLE.preferredHeight = 25f;

        // Contenedor de fichas apostadas
        var contenedorApostadas = CreateUIObject("ContenedorApostadas", panelIzq.transform);
        var caLayout = contenedorApostadas.AddComponent<GridLayoutGroup>();
        caLayout.cellSize = new Vector2(80f, 35f);
        caLayout.spacing = new Vector2(5f, 5f);
        caLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        caLayout.constraintCount = 2;
        caLayout.childAlignment = TextAnchor.UpperCenter;
        var caElement = contenedorApostadas.AddComponent<LayoutElement>();
        caElement.flexibleHeight = 1f;

        // === PANEL DERECHO - Fichas Disponibles ===
        var panelDer = CreateUIObject("PanelDisponibles", zonaCentral.transform);
        var pDerImage = panelDer.AddComponent<Image>();
        pDerImage.color = new Color(0.1f, 0.12f, 0.18f, 0.9f);
        var pDerLayout = panelDer.AddComponent<VerticalLayoutGroup>();
        pDerLayout.spacing = 5f;
        pDerLayout.padding = new RectOffset(10, 10, 10, 10);
        pDerLayout.childForceExpandWidth = true;
        pDerLayout.childForceExpandHeight = false;
        pDerLayout.childAlignment = TextAnchor.UpperCenter;
        var pDerElement = panelDer.AddComponent<LayoutElement>();
        pDerElement.flexibleWidth = 0.6f;

        // Título panel derecho
        var tituloDer = CreateTextObject("TituloDisponibles", panelDer.transform, "TUS FICHAS", 18);
        var tituloDerLE = tituloDer.AddComponent<LayoutElement>();
        tituloDerLE.preferredHeight = 30f;

        // Contenedor de fichas disponibles (grid 2x5)
        var contenedorDisponibles = CreateUIObject("ContenedorDisponibles", panelDer.transform);
        var cdLayout = contenedorDisponibles.AddComponent<GridLayoutGroup>();
        cdLayout.cellSize = new Vector2(90f, 35f);
        cdLayout.spacing = new Vector2(5f, 5f);
        cdLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        cdLayout.constraintCount = 5;
        cdLayout.childAlignment = TextAnchor.UpperCenter;
        var cdElement = contenedorDisponibles.AddComponent<LayoutElement>();
        cdElement.flexibleHeight = 1f;

        // Paginación
        var paginacion = CreateUIObject("Paginacion", panelDer.transform);
        var pagLayout = paginacion.AddComponent<HorizontalLayoutGroup>();
        pagLayout.spacing = 10f;
        pagLayout.childAlignment = TextAnchor.MiddleCenter;
        pagLayout.childForceExpandWidth = false;
        pagLayout.childForceExpandHeight = false;
        var pagLE = paginacion.AddComponent<LayoutElement>();
        pagLE.preferredHeight = 35f;

        var btnAnterior = CreateButtonObject("BtnAnterior", paginacion.transform, "<", 30, 30);
        var paginaTexto = CreateTextObject("PaginaText", paginacion.transform, "1/1", 14);
        var pagTextoLE = paginaTexto.AddComponent<LayoutElement>();
        pagTextoLE.preferredWidth = 50f;
        var btnSiguiente = CreateButtonObject("BtnSiguiente", paginacion.transform, ">", 30, 30);

        // === ZONA INFERIOR - Lanzadora ===
        var zonaInferior = CreateUIObject("ZonaLanzadora", panel.transform);
        var ziRect = zonaInferior.GetComponent<RectTransform>();
        ziRect.anchorMin = new Vector2(0.02f, 0.08f);
        ziRect.anchorMax = new Vector2(0.98f, 0.28f);
        ziRect.offsetMin = Vector2.zero;
        ziRect.offsetMax = Vector2.zero;
        var ziImage = zonaInferior.AddComponent<Image>();
        ziImage.color = new Color(0.12f, 0.18f, 0.12f, 0.9f);
        var ziLayout = zonaInferior.AddComponent<VerticalLayoutGroup>();
        ziLayout.spacing = 5f;
        ziLayout.padding = new RectOffset(10, 10, 5, 5);
        ziLayout.childForceExpandWidth = true;
        ziLayout.childForceExpandHeight = false;
        ziLayout.childAlignment = TextAnchor.UpperCenter;

        // Texto lanzadora
        var lanzadoraTexto = CreateTextObject("LanzadoraText", zonaInferior.transform, "Selecciona tu ficha lanzadora:", 16);
        var lanzTextLE = lanzadoraTexto.AddComponent<LayoutElement>();
        lanzTextLE.preferredHeight = 25f;

        // Contenedor lanzadora (horizontal)
        var contenedorLanzadora = CreateUIObject("ContenedorLanzadora", zonaInferior.transform);
        var clLayout = contenedorLanzadora.AddComponent<GridLayoutGroup>();
        clLayout.cellSize = new Vector2(90f, 35f);
        clLayout.spacing = new Vector2(5f, 5f);
        clLayout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        clLayout.constraintCount = 1;
        clLayout.childAlignment = TextAnchor.MiddleCenter;
        var clElement = contenedorLanzadora.AddComponent<LayoutElement>();
        clElement.flexibleHeight = 1f;

        // === BOTÓN LISTO ===
        var botonListo = CreateButtonObject("BotonListo", panel.transform, "LISTO", 200, 50);
        var blRect = botonListo.GetComponent<RectTransform>();
        blRect.anchorMin = new Vector2(0.5f, 0.01f);
        blRect.anchorMax = new Vector2(0.5f, 0.01f);
        blRect.pivot = new Vector2(0.5f, 0f);
        blRect.anchoredPosition = new Vector2(0f, 10f);
        blRect.sizeDelta = new Vector2(200f, 50f);
        // Style the button
        var blImage = botonListo.GetComponent<Image>();
        blImage.color = new Color(0.2f, 0.6f, 0.2f, 1f);

        // === AGREGAR COMPONENT BetSelectionUI ===
        var betUI = panel.AddComponent<BetSelectionUI>();
        var so = new SerializedObject(betUI);
        so.FindProperty("_panelSeleccion").objectReferenceValue = panel;
        so.FindProperty("_contenedorDisponibles").objectReferenceValue = contenedorDisponibles.transform;
        so.FindProperty("_slotPrefab").objectReferenceValue = slotPrefab;
        so.FindProperty("_paginaTexto").objectReferenceValue = paginaTexto.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_botonPaginaSiguiente").objectReferenceValue = btnSiguiente.GetComponent<Button>();
        so.FindProperty("_botonPaginaAnterior").objectReferenceValue = btnAnterior.GetComponent<Button>();
        so.FindProperty("_contenedorApostadas").objectReferenceValue = contenedorApostadas.transform;
        so.FindProperty("_contenedorLanzadora").objectReferenceValue = contenedorLanzadora.transform;
        so.FindProperty("_lanzadoraTexto").objectReferenceValue = lanzadoraTexto.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_botonListo").objectReferenceValue = botonListo.GetComponent<Button>();
        so.FindProperty("_contadorTexto").objectReferenceValue = contadorGO.GetComponent<TextMeshProUGUI>();
        so.ApplyModifiedProperties();

        // Desactivar por defecto
        panel.SetActive(false);

        // Marcar escena como modificada
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[BuildBetSelectionPanel] Panel de selección de apuesta creado exitosamente en CombatCanvas.");
    }

    // === HELPERS ===

    private static GameObject FindOrCreateSlotPrefab()
    {
        // Buscar el prefab existente
        string[] guids = AssetDatabase.FindAssets("BetSlotPrefab t:Prefab");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        // Crear uno si no existe
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");

        var slotGO = new GameObject("BetSlotPrefab");
        var slotRect = slotGO.AddComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(90f, 35f);

        // Fondo
        var fondoImage = slotGO.AddComponent<Image>();
        fondoImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Botón
        var button = slotGO.AddComponent<Button>();
        button.targetGraphic = fondoImage;

        // Texto nombre
        var textoGO = new GameObject("NombreText");
        textoGO.transform.SetParent(slotGO.transform, false);
        var textoRect = textoGO.AddComponent<RectTransform>();
        StretchFull(textoRect);
        textoRect.offsetMin = new Vector2(5f, 2f);
        textoRect.offsetMax = new Vector2(-5f, -2f);
        var tmp = textoGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "Ficha";
        tmp.fontSize = 12f;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = Color.white;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Ellipsis;

        // BetSlotUI component
        var betSlot = slotGO.AddComponent<BetSlotUI>();
        var slotSO = new SerializedObject(betSlot);
        slotSO.FindProperty("_fondoSlot").objectReferenceValue = fondoImage;
        slotSO.FindProperty("_nombreTexto").objectReferenceValue = tmp;
        slotSO.FindProperty("_boton").objectReferenceValue = button;
        slotSO.ApplyModifiedProperties();

        // Guardar como prefab
        string prefabPath = "Assets/Prefabs/UI/BetSlotPrefab.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(slotGO, prefabPath);
        Object.DestroyImmediate(slotGO);

        Debug.Log($"[BuildBetSelectionPanel] BetSlotPrefab creado en {prefabPath}");
        return prefab;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private static GameObject CreateTextObject(string name, Transform parent, string text, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return go;
    }

    private static GameObject CreateButtonObject(string name, Transform parent, string text, float width, float height)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);
        var image = go.AddComponent<Image>();
        image.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        var button = go.AddComponent<Button>();
        button.targetGraphic = image;
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = width;
        le.preferredHeight = height;

        // Texto del botón
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var textRect = textGO.AddComponent<RectTransform>();
        StretchFull(textRect);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 14;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return go;
    }

    private static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
