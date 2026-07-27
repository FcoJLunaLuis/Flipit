using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public static class BettingAlbumIntegrationSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/BettingAlbumIntegrationTestScene.unity";
    private const string FichasFolder = "Assets/ScriptableObjects/Fichas";
    private const string DialogueDataFolder = "Assets/DialogueData";

    [MenuItem("Flipit/Build Betting Album Integration Test Scene")]
    public static void BuildScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        EnsureFolder("Assets/Scenes");
        EnsureFolder(FichasFolder);
        EnsureFolder(DialogueDataFolder);

        // Camera
        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0, 0, -10);
            cam.orthographic = true;
            cam.orthographicSize = 12;
            cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
        }

        // Ground
        var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground"; ground.tag = "Ground";
        ground.transform.position = new Vector3(0, -1f, 0);
        ground.transform.localScale = new Vector3(15f, 0.1f, 15f);

        // Load FichaTemplates
        var testFichas = LoadTestFichas();

        // AlbumManager
        var albumGO = new GameObject("AlbumManager");
        var albumManager = albumGO.AddComponent<AlbumManager>();
        albumManager.fichasDisponibles = testFichas;

        // CombatManager
        var combatGO = new GameObject("CombatManager");
        var combatManager = combatGO.AddComponent<CombatManager>();
        var cmFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var config = AssetDatabase.LoadAssetAtPath<CombatConfig>("Assets/ScriptableObjects/CombatConfig.asset");
        if (config != null)
            typeof(CombatManager).GetField("_config", cmFlags)?.SetValue(combatManager, config);

        // Validation UI
        var canvas = new GameObject("UICanvas");
        var cv = canvas.AddComponent<Canvas>(); cv.renderMode = RenderMode.ScreenSpaceOverlay; cv.sortingOrder = 100;
        var sc = canvas.AddComponent<CanvasScaler>(); sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution = new Vector2(1920,1080);
        canvas.AddComponent<GraphicRaycaster>();

        var panel = new GameObject("ValidationPanel"); panel.transform.SetParent(canvas.transform, false);
        var prt = panel.AddComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.2f, 0.35f); prt.anchorMax = new Vector2(0.8f, 0.55f);
        prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;
        panel.AddComponent<Image>().color = new Color(0.15f, 0.05f, 0.05f, 0.92f);

        var textGO = new GameObject("ValidationText"); textGO.transform.SetParent(panel.transform, false);
        var trt = textGO.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
        var valText = textGO.AddComponent<TextMeshProUGUI>();
        valText.fontSize = 22; valText.color = Color.white; valText.alignment = TextAlignmentOptions.Center;
        panel.SetActive(false);

        // CombatAlbumBridge
        var bridgeGO = new GameObject("CombatAlbumBridge");
        var bridge = bridgeGO.AddComponent<CombatAlbumBridge>();
        var bf = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        if (config != null) typeof(CombatAlbumBridge).GetField("_combatConfig", bf)?.SetValue(bridge, config);
        typeof(CombatAlbumBridge).GetField("_mensajeValidacionPanel", bf)?.SetValue(bridge, panel);
        typeof(CombatAlbumBridge).GetField("_mensajeValidacionTexto", bf)?.SetValue(bridge, valText);

        // Player
        var player = new GameObject("Player"); player.tag = "Player";
        player.transform.position = new Vector3(0, -2, 0);
        player.AddComponent<SpriteRenderer>().color = Color.green;
        player.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        var rb = player.AddComponent<Rigidbody2D>(); rb.gravityScale = 0; rb.freezeRotation = true;
        player.AddComponent<BoxCollider2D>().size = Vector2.one;

        var pi = player.AddComponent<PlayerInput>();
        var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
        if (inputAsset != null) { pi.actions = inputAsset; pi.defaultActionMap = "Player"; }

        // NPC label
        var npcGO = new GameObject("FlipCombat_NPC_Test");
        npcGO.transform.position = new Vector3(5, 0, 0);
        npcGO.AddComponent<SpriteRenderer>().color = Color.red;
        npcGO.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        npcGO.AddComponent<BoxCollider2D>().size = Vector2.one;
        var lbl = new GameObject("Label"); lbl.transform.SetParent(npcGO.transform);
        lbl.transform.localPosition = new Vector3(0, 1f, 0);
        var tm = lbl.AddComponent<TextMesh>(); tm.text = "NPC Retador"; tm.fontSize = 48;
        tm.characterSize = 0.4f; tm.anchor = TextAnchor.MiddleCenter; tm.color = Color.white;

        // Info
        var info = new GameObject("Info");
        info.transform.position = new Vector3(0, 5f, 0);
        var infoTM = info.AddComponent<TextMesh>();
        infoTM.text = "Album-Combate Integration Test\nPresiona Play y usa el boton INICIAR COMBATE";
        infoTM.fontSize = 36; infoTM.characterSize = 0.35f;
        infoTM.anchor = TextAnchor.MiddleCenter; infoTM.color = Color.yellow;

        // === BettingIntegrationTestRunner con UI funcional ===
        var testRunnerGO = new GameObject("BettingTestRunner");
        var testRunner = testRunnerGO.AddComponent<BettingIntegrationTestRunner>();
        var trFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        // Asignar fichas de test
        typeof(BettingIntegrationTestRunner).GetField("_fichasTest", trFlags)?.SetValue(testRunner, testFichas);
        typeof(BettingIntegrationTestRunner).GetField("_fichasNPC", trFlags)?.SetValue(testRunner, testFichas);

        // Crear UI de apuesta
        var betPanel = new GameObject("BetSelectionPanel"); betPanel.transform.SetParent(canvas.transform, false);
        var bprt = betPanel.AddComponent<RectTransform>();
        bprt.anchorMin = new Vector2(0.05f, 0.05f); bprt.anchorMax = new Vector2(0.95f, 0.85f);
        bprt.offsetMin = Vector2.zero; bprt.offsetMax = Vector2.zero;
        betPanel.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

        // Contenedor de fichas (scroll area)
        var fichasContainer = new GameObject("FichasContainer"); fichasContainer.transform.SetParent(betPanel.transform, false);
        var fcrt = fichasContainer.AddComponent<RectTransform>();
        fcrt.anchorMin = new Vector2(0.02f, 0.15f); fcrt.anchorMax = new Vector2(0.7f, 0.9f);
        fcrt.offsetMin = Vector2.zero; fcrt.offsetMax = Vector2.zero;
        var vlg = fichasContainer.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 5; vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
        vlg.childControlHeight = true; vlg.childControlWidth = true;
        var csf = fichasContainer.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Ficha slot prefab (creamos uno temporal como hijos de la escena)
        var slotPrefab = new GameObject("FichaSlotPrefab");
        var sprt = slotPrefab.AddComponent<RectTransform>();
        var sple = slotPrefab.AddComponent<LayoutElement>(); sple.preferredHeight = 40;
        slotPrefab.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        var slotTextGO = new GameObject("Text"); slotTextGO.transform.SetParent(slotPrefab.transform, false);
        var strt = slotTextGO.AddComponent<RectTransform>();
        strt.anchorMin = new Vector2(0.05f, 0); strt.anchorMax = new Vector2(0.95f, 1); strt.offsetMin = Vector2.zero; strt.offsetMax = Vector2.zero;
        var stTxt = slotTextGO.AddComponent<TextMeshProUGUI>(); stTxt.fontSize = 18; stTxt.color = Color.white; stTxt.alignment = TextAlignmentOptions.MidlineLeft;
        slotPrefab.SetActive(false);

        // Contador texto
        var contGO = new GameObject("ContadorTexto"); contGO.transform.SetParent(betPanel.transform, false);
        var cntrt = contGO.AddComponent<RectTransform>();
        cntrt.anchorMin = new Vector2(0.72f, 0.7f); cntrt.anchorMax = new Vector2(0.98f, 0.85f); cntrt.offsetMin = Vector2.zero; cntrt.offsetMax = Vector2.zero;
        var contTxt = contGO.AddComponent<TextMeshProUGUI>(); contTxt.fontSize = 18; contTxt.color = Color.yellow; contTxt.text = "Apostadas: 0/5";

        // Lanzadora texto
        var lanzGO = new GameObject("LanzadoraTexto"); lanzGO.transform.SetParent(betPanel.transform, false);
        var lnzrt = lanzGO.AddComponent<RectTransform>();
        lnzrt.anchorMin = new Vector2(0.72f, 0.55f); lnzrt.anchorMax = new Vector2(0.98f, 0.7f); lnzrt.offsetMin = Vector2.zero; lnzrt.offsetMax = Vector2.zero;
        var lanzTxt = lanzGO.AddComponent<TextMeshProUGUI>(); lanzTxt.fontSize = 16; lanzTxt.color = Color.cyan; lanzTxt.text = "Lanzadora: (click derecho)";

        // Estado texto
        var estGO = new GameObject("EstadoTexto"); estGO.transform.SetParent(betPanel.transform, false);
        var estrt = estGO.AddComponent<RectTransform>();
        estrt.anchorMin = new Vector2(0.02f, 0.9f); estrt.anchorMax = new Vector2(0.98f, 0.98f); estrt.offsetMin = Vector2.zero; estrt.offsetMax = Vector2.zero;
        var estTxt = estGO.AddComponent<TextMeshProUGUI>(); estTxt.fontSize = 20; estTxt.color = Color.white; estTxt.alignment = TextAlignmentOptions.Center;

        // Botón confirmar
        var btnConfGO = new GameObject("BotonConfirmar"); btnConfGO.transform.SetParent(betPanel.transform, false);
        var bcrt = btnConfGO.AddComponent<RectTransform>();
        bcrt.anchorMin = new Vector2(0.72f, 0.2f); bcrt.anchorMax = new Vector2(0.98f, 0.35f); bcrt.offsetMin = Vector2.zero; bcrt.offsetMax = Vector2.zero;
        btnConfGO.AddComponent<Image>().color = new Color(0.2f, 0.7f, 0.3f);
        var btnConf = btnConfGO.AddComponent<Button>();
        var bcTxtGO = new GameObject("Text"); bcTxtGO.transform.SetParent(btnConfGO.transform, false);
        var bctrt = bcTxtGO.AddComponent<RectTransform>(); bctrt.anchorMin = Vector2.zero; bctrt.anchorMax = Vector2.one; bctrt.offsetMin = Vector2.zero; bctrt.offsetMax = Vector2.zero;
        bcTxtGO.AddComponent<TextMeshProUGUI>().text = "CONFIRMAR"; bcTxtGO.GetComponent<TextMeshProUGUI>().fontSize = 16; bcTxtGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Botón iniciar (fuera del panel de apuesta)
        var btnIniGO = new GameObject("BotonIniciar"); btnIniGO.transform.SetParent(canvas.transform, false);
        var birt = btnIniGO.AddComponent<RectTransform>();
        birt.anchorMin = new Vector2(0.35f, 0.05f); birt.anchorMax = new Vector2(0.65f, 0.12f); birt.offsetMin = Vector2.zero; birt.offsetMax = Vector2.zero;
        btnIniGO.AddComponent<Image>().color = new Color(0.8f, 0.3f, 0.1f);
        var btnIni = btnIniGO.AddComponent<Button>();
        var biTxtGO = new GameObject("Text"); biTxtGO.transform.SetParent(btnIniGO.transform, false);
        var bitrt = biTxtGO.AddComponent<RectTransform>(); bitrt.anchorMin = Vector2.zero; bitrt.anchorMax = Vector2.one; bitrt.offsetMin = Vector2.zero; bitrt.offsetMax = Vector2.zero;
        biTxtGO.AddComponent<TextMeshProUGUI>().text = "INICIAR COMBATE"; biTxtGO.GetComponent<TextMeshProUGUI>().fontSize = 22; biTxtGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Torre contenedor (3D)
        var torreGO = new GameObject("TorreContenedor");
        torreGO.transform.position = new Vector3(0, 0, 0);

        // EventSystem
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // Wiring TestRunner
        typeof(BettingIntegrationTestRunner).GetField("_betSelectionPanel", trFlags)?.SetValue(testRunner, betPanel);
        typeof(BettingIntegrationTestRunner).GetField("_contenedorFichas", trFlags)?.SetValue(testRunner, fichasContainer.transform);
        typeof(BettingIntegrationTestRunner).GetField("_fichaSlotPrefab", trFlags)?.SetValue(testRunner, slotPrefab);
        typeof(BettingIntegrationTestRunner).GetField("_contadorTexto", trFlags)?.SetValue(testRunner, contTxt);
        typeof(BettingIntegrationTestRunner).GetField("_lanzadoraTexto", trFlags)?.SetValue(testRunner, lanzTxt);
        typeof(BettingIntegrationTestRunner).GetField("_estadoTexto", trFlags)?.SetValue(testRunner, estTxt);
        typeof(BettingIntegrationTestRunner).GetField("_botonConfirmar", trFlags)?.SetValue(testRunner, btnConf);
        typeof(BettingIntegrationTestRunner).GetField("_botonIniciar", trFlags)?.SetValue(testRunner, btnIni);
        typeof(BettingIntegrationTestRunner).GetField("_contenedorTorre", trFlags)?.SetValue(testRunner, torreGO.transform);

        betPanel.SetActive(false);

        // Save
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        AddToBuildSettings(ScenePath);
        Debug.Log("[BettingAlbumIntegrationSceneBuilder] Done! WASD move, E interact.");
    }

    static FichaTemplate[] LoadTestFichas()
    {
        var fichas = new List<FichaTemplate>();
        string[] paths = {
            $"{FichasFolder}/Ficha_001_Piedra.asset",
            $"{FichasFolder}/Ficha_002_Cristal.asset",
            $"{FichasFolder}/Ficha_003_Obsidiana.asset",
            $"{FichasFolder}/Ficha_004_Madera.asset"
        };
        foreach (var p in paths)
        {
            var f = AssetDatabase.LoadAssetAtPath<FichaTemplate>(p);
            if (f != null) fichas.Add(f);
        }
        return fichas.ToArray();
    }

    static void AddToBuildSettings(string path)
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in scenes) if (s.path == path) return;
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        var parts = path.Split('/'); string cur = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = cur + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(cur, parts[i]);
            cur = next;
        }
    }
}
