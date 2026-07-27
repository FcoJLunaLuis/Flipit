using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

/// <summary>
/// Editor utility to build the SummaryPanel UI hierarchy in the CombatCanvas.
/// Run from menu: Tools/Combat/Build Summary Panel
/// </summary>
public static class BuildSummaryPanel
{
    [MenuItem("Tools/Combat/Build Summary Panel")]
    public static void Build()
    {
        // Find CombatCanvas in scene
        var combatCanvas = GameObject.Find("CombatCanvas");
        if (combatCanvas == null)
        {
            Debug.LogError("[BuildSummaryPanel] No se encontró 'CombatCanvas' en la escena.");
            return;
        }

        var canvasRect = combatCanvas.GetComponent<RectTransform>();

        // Create SummaryPanel (full screen dark overlay)
        var summaryPanel = CreateUIObject("SummaryPanel", canvasRect);
        var panelRect = summaryPanel.GetComponent<RectTransform>();
        StretchFull(panelRect);
        var panelImage = summaryPanel.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.05f, 0.1f, 0.92f);

        // Add VerticalLayoutGroup
        var vlg = summaryPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(60, 60, 40, 40);
        vlg.spacing = 12f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Add ContentSizeFitter to panel for vertical content
        var panelFitter = summaryPanel.AddComponent<ContentSizeFitter>();
        panelFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        // === TITULO ===
        var tituloObj = CreateTMPObject("TituloText", panelRect, "¡Victoria!", 36f, TextAlignmentOptions.Center, Color.white);
        AddLayoutElement(tituloObj, 50f);

        // === LANZADORA ===
        var lanzadoraObj = CreateTMPObject("LanzadoraText", panelRect, "Lanzadora: ---", 20f, TextAlignmentOptions.Center, new Color(0.8f, 0.9f, 1f));
        AddLayoutElement(lanzadoraObj, 30f);

        // === STATS PANEL (Horizontal) ===
        var statsPanel = CreateUIObject("StatsPanel", panelRect);
        var statsHlg = statsPanel.AddComponent<HorizontalLayoutGroup>();
        statsHlg.spacing = 40f;
        statsHlg.childAlignment = TextAnchor.MiddleCenter;
        statsHlg.childControlWidth = true;
        statsHlg.childControlHeight = true;
        statsHlg.childForceExpandWidth = true;
        statsHlg.childForceExpandHeight = false;
        AddLayoutElement(statsPanel, 30f);

        var xpObj = CreateTMPObject("XPText", statsPanel.GetComponent<RectTransform>(), "XP ganada: +0", 20f, TextAlignmentOptions.Center, new Color(0.4f, 1f, 0.4f));
        var desgasteObj = CreateTMPObject("DesgasteText", statsPanel.GetComponent<RectTransform>(), "Desgaste: +0", 20f, TextAlignmentOptions.Center, new Color(1f, 0.6f, 0.3f));

        // === SEPARATOR ===
        AddSeparator(panelRect);

        // === TITULO GANADAS ===
        var tituloGanadasObj = CreateTMPObject("TituloGanadasText", panelRect, "Fichas Ganadas: 0", 22f, TextAlignmentOptions.Left, new Color(0.3f, 1f, 0.5f));
        AddLayoutElement(tituloGanadasObj, 30f);

        // === CONTENEDOR GANADAS ===
        var contenedorGanadas = CreateUIObject("ContenedorGanadas", panelRect);
        var ganadasVlg = contenedorGanadas.AddComponent<VerticalLayoutGroup>();
        ganadasVlg.spacing = 4f;
        ganadasVlg.childControlWidth = true;
        ganadasVlg.childControlHeight = false;
        ganadasVlg.childForceExpandWidth = true;
        ganadasVlg.childForceExpandHeight = false;
        var ganadasFitter = contenedorGanadas.AddComponent<ContentSizeFitter>();
        ganadasFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        AddLayoutElement(contenedorGanadas, -1f, true);

        // === SEPARATOR ===
        AddSeparator(panelRect);

        // === TITULO PERDIDAS ===
        var tituloPerdidasObj = CreateTMPObject("TituloPerdidasText", panelRect, "Fichas Perdidas: 0", 22f, TextAlignmentOptions.Left, new Color(1f, 0.4f, 0.4f));
        AddLayoutElement(tituloPerdidasObj, 30f);

        // === CONTENEDOR PERDIDAS ===
        var contenedorPerdidas = CreateUIObject("ContenedorPerdidas", panelRect);
        var perdidasVlg = contenedorPerdidas.AddComponent<VerticalLayoutGroup>();
        perdidasVlg.spacing = 4f;
        perdidasVlg.childControlWidth = true;
        perdidasVlg.childControlHeight = false;
        perdidasVlg.childForceExpandWidth = true;
        perdidasVlg.childForceExpandHeight = false;
        var perdidasFitter = contenedorPerdidas.AddComponent<ContentSizeFitter>();
        perdidasFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        AddLayoutElement(contenedorPerdidas, -1f, true);

        // === SPACER ===
        var spacer = CreateUIObject("Spacer", panelRect);
        var spacerLE = spacer.AddComponent<LayoutElement>();
        spacerLE.flexibleHeight = 1f;

        // === BOTON CONTINUAR ===
        var botonObj = CreateUIObject("BotonContinuar", panelRect);
        var botonImage = botonObj.AddComponent<Image>();
        botonImage.color = new Color(0.2f, 0.6f, 0.3f, 1f);
        botonImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        botonImage.type = Image.Type.Sliced;
        var boton = botonObj.AddComponent<Button>();
        boton.targetGraphic = botonImage;
        var botonColors = boton.colors;
        botonColors.highlightedColor = new Color(0.3f, 0.8f, 0.4f, 1f);
        botonColors.pressedColor = new Color(0.1f, 0.4f, 0.2f, 1f);
        boton.colors = botonColors;
        AddLayoutElement(botonObj, 50f);

        var botonTextoObj = CreateTMPObject("Text", botonObj.GetComponent<RectTransform>(), "Continuar", 22f, TextAlignmentOptions.Center, Color.white);
        var botonTextoRect = botonTextoObj.GetComponent<RectTransform>();
        StretchFull(botonTextoRect);

        // === ADD CombatSummaryUI COMPONENT ===
        var summaryUI = summaryPanel.AddComponent<CombatSummaryUI>();

        // Wire serialized references
        var so = new SerializedObject(summaryUI);
        so.FindProperty("_panelResumen").objectReferenceValue = summaryPanel;
        so.FindProperty("_tituloTexto").objectReferenceValue = tituloObj.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_xpTexto").objectReferenceValue = xpObj.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_desgasteTexto").objectReferenceValue = desgasteObj.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_lanzadoraTexto").objectReferenceValue = lanzadoraObj.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_tituloGanadasTexto").objectReferenceValue = tituloGanadasObj.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_contenedorFichasGanadas").objectReferenceValue = contenedorGanadas.transform;
        so.FindProperty("_tituloPerdidasTexto").objectReferenceValue = tituloPerdidasObj.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_contenedorFichasPerdidas").objectReferenceValue = contenedorPerdidas.transform;
        so.FindProperty("_botonContinuar").objectReferenceValue = boton;

        // Load FichaItemUI prefab
        var fichaItemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/FichaItemUI.prefab");
        if (fichaItemPrefab != null)
            so.FindProperty("_fichaItemPrefab").objectReferenceValue = fichaItemPrefab;
        else
            Debug.LogWarning("[BuildSummaryPanel] No se encontró Assets/Prefabs/UI/FichaItemUI.prefab");

        so.ApplyModifiedPropertiesWithoutUndo();

        // Start inactive
        summaryPanel.SetActive(false);

        // === WIRE CombatTestRunner REFERENCE ===
        WireCombatTestRunner(summaryUI);

        EditorUtility.SetDirty(combatCanvas);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[BuildSummaryPanel] SummaryPanel creado y conectado exitosamente.");
    }

    private static void WireCombatTestRunner(CombatSummaryUI summaryUI)
    {
        // Find CombatTestRunner in scene
        var testRunner = Object.FindFirstObjectByType<CombatTestRunner>();
        if (testRunner != null)
        {
            var trSO = new SerializedObject(testRunner);
            var prop = trSO.FindProperty("_summaryUI");
            if (prop != null)
            {
                prop.objectReferenceValue = summaryUI;
                trSO.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("[BuildSummaryPanel] CombatTestRunner._summaryUI conectado.");
            }
        }
        else
        {
            Debug.LogWarning("[BuildSummaryPanel] CombatTestRunner no encontrado en la escena. Conectar _summaryUI manualmente.");
        }
    }

    private static GameObject CreateUIObject(string name, RectTransform parent)
    {
        var obj = new GameObject(name);
        var rect = obj.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        return obj;
    }

    private static GameObject CreateTMPObject(string name, RectTransform parent, string text, float fontSize, TextAlignmentOptions alignment, Color color)
    {
        var obj = new GameObject(name);
        var rect = obj.AddComponent<RectTransform>();
        rect.SetParent(parent, false);

        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = color;
        tmp.enableWordWrapping = true;
        tmp.richText = true;

        return obj;
    }

    private static void AddLayoutElement(GameObject obj, float preferredHeight, bool flexHeight = false)
    {
        var le = obj.AddComponent<LayoutElement>();
        if (preferredHeight > 0)
            le.preferredHeight = preferredHeight;
        if (flexHeight)
            le.flexibleHeight = 0f;
    }

    private static void AddSeparator(RectTransform parent)
    {
        var sep = CreateUIObject("Separator", parent);
        var sepImage = sep.AddComponent<Image>();
        sepImage.color = new Color(1f, 1f, 1f, 0.2f);
        var sepLE = sep.AddComponent<LayoutElement>();
        sepLE.preferredHeight = 2f;
    }

    private static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
