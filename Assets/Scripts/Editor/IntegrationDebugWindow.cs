using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Editor window with debug buttons to test integration flows.
/// Menu: Flipit → Integration Debug
/// 
/// Provides buttons to force-trigger each system:
/// - Force combat with mock NPC fichas
/// - Force open shop
/// - Force open collector menu
/// - Check system status
/// </summary>
public class IntegrationDebugWindow : EditorWindow
{
    private Vector2 _scrollPos;

    [MenuItem("Flipit/Integration Debug")]
    public static void ShowWindow()
    {
        var window = GetWindow<IntegrationDebugWindow>("Integration Debug");
        window.minSize = new Vector2(350, 500);
    }

    private void OnGUI()
    {
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        GUILayout.Label("=== Integration Debug ===", EditorStyles.boldLabel);
        GUILayout.Space(5);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to use runtime debug buttons.", MessageType.Info);
            GUILayout.Space(10);

            DrawEditorSection();
        }
        else
        {
            DrawRuntimeSection();
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawEditorSection()
    {
        GUILayout.Label("Editor Actions", EditorStyles.boldLabel);
        GUILayout.Space(5);

        if (GUILayout.Button("Generate City (Full Integration)", GUILayout.Height(30)))
        {
            GenerateCityWithIntegration.IntegrateOnly();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Integrate Systems Only (existing scene)", GUILayout.Height(25)))
        {
            CitySceneIntegrationBuilder.IntegrateSystems();
        }

        GUILayout.Space(15);
        GUILayout.Label("Prefab Status Check", EditorStyles.boldLabel);
        DrawPrefabStatus();
    }

    private void DrawRuntimeSection()
    {
        // --- System Status ---
        GUILayout.Label("System Status", EditorStyles.boldLabel);
        DrawSystemStatus();

        GUILayout.Space(15);

        // --- Combat ---
        GUILayout.Label("Combat", EditorStyles.boldLabel);
        if (GUILayout.Button("Force Start Combat (mock fichas)", GUILayout.Height(25)))
        {
            ForceCombat();
        }

        if (GUILayout.Button("Force Exit Combat", GUILayout.Height(25)))
        {
            ForceExitCombat();
        }

        GUILayout.Space(10);

        // --- Shop ---
        GUILayout.Label("Shop", EditorStyles.boldLabel);
        if (GUILayout.Button("Force Open Shop", GUILayout.Height(25)))
        {
            ForceOpenShop();
        }

        if (GUILayout.Button("Force Close Shop", GUILayout.Height(25)))
        {
            ForceCloseShop();
        }

        GUILayout.Space(10);

        // --- Collector ---
        GUILayout.Label("Collector", EditorStyles.boldLabel);
        if (GUILayout.Button("Force Open Collector Menu", GUILayout.Height(25)))
        {
            ForceOpenCollector();
        }

        GUILayout.Space(10);

        // --- Camera ---
        GUILayout.Label("Camera", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Player Cam"))
            CameraManager.Instance?.ActivarCamara(CameraManager.CameraType.Player);
        if (GUILayout.Button("Combat Cam"))
            CameraManager.Instance?.ActivarCamara(CameraManager.CameraType.Combat);
        if (GUILayout.Button("CoinFlip Cam"))
            CameraManager.Instance?.ActivarCamara(CameraManager.CameraType.CoinFlip);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // --- Album ---
        GUILayout.Label("Album", EditorStyles.boldLabel);
        if (AlbumManager.Instance != null)
        {
            var data = AlbumManager.Instance.ObtenerAlbumData();
            EditorGUILayout.LabelField("Fichas únicas:", data?.ObtenerTotalFichasUnicas().ToString() ?? "N/A");
            EditorGUILayout.LabelField("Fichas totales:", data?.ObtenerTotalFichas().ToString() ?? "N/A");
        }

        if (GUILayout.Button("Add 3 Random Mock Fichas to Album"))
        {
            AddMockFichasToAlbum();
        }

        GUILayout.Space(10);

        // --- GameState ---
        GUILayout.Label("GameState", EditorStyles.boldLabel);
        if (GameStateManager.Instance != null)
        {
            EditorGUILayout.LabelField("Current State:", GameStateManager.Instance.CurrentState.ToString());
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Set Exploration"))
                GameStateManager.Instance.SetExploration();
            if (GUILayout.Button("Set Combat"))
                GameStateManager.Instance.SetCombat();
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawSystemStatus()
    {
        DrawStatusRow("GameStateManager", GameStateManager.Instance != null);
        DrawStatusRow("CameraManager", CameraManager.Instance != null);
        DrawStatusRow("CombatManager", CombatManager.Instance != null);
        DrawStatusRow("CoinFlipManager", CoinFlipManager.Instance != null);
        DrawStatusRow("AlbumManager", AlbumManager.Instance != null);

        var dialogueMgr = Flipit.Dialogue.Dialogue_Manager.Instance;
        DrawStatusRow("Dialogue_Manager", dialogueMgr != null);

        var pauseMgr = Object.FindAnyObjectByType<PauseManager>();
        DrawStatusRow("PauseManager", pauseMgr != null);

        var challengerBridge = Object.FindAnyObjectByType<ChallengerCombatBridge>();
        DrawStatusRow("ChallengerCombatBridge", challengerBridge != null);

        var vendorBridge = Object.FindAnyObjectByType<VendorShopBridge>();
        DrawStatusRow("VendorShopBridge", vendorBridge != null);

        var collectorBridge = Object.FindAnyObjectByType<CollectorDialogueBridge>();
        DrawStatusRow("CollectorDialogueBridge", collectorBridge != null);

        var combatResultBridge = Object.FindAnyObjectByType<CombatResultBridge>();
        DrawStatusRow("CombatResultBridge", combatResultBridge != null);

        var betBridge = Object.FindAnyObjectByType<BetSelectionBridge>();
        DrawStatusRow("BetSelectionBridge", betBridge != null);
    }

    private void DrawStatusRow(string name, bool exists)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(name, GUILayout.Width(200));

        var prevColor = GUI.color;
        GUI.color = exists ? Color.green : Color.red;
        EditorGUILayout.LabelField(exists ? "OK" : "MISSING", EditorStyles.boldLabel, GUILayout.Width(80));
        GUI.color = prevColor;

        EditorGUILayout.EndHorizontal();
    }

    private void DrawPrefabStatus()
    {
        string[] prefabs = {
            "Assets/Prefabs/InstancarCombate/--- COMBAT SYSTEM ---.prefab",
            "Assets/Prefabs/InstancarCombate/CoinFliperManager.prefab",
            "Assets/Prefabs/InstancarCombate/--- PAUSE SYSTEM ---.prefab",
            "Assets/Prefabs/InstancarCombate/AlbumManager.prefab",
            "Assets/Prefabs/InstancarCombate/AlbumCanvas.prefab",
            "Assets/Prefabs/InstancarCombate/CameraManager.prefab",
            "Assets/Prefabs/SceneRequired/GameStateManager.prefab",
            "Assets/Prefabs/SceneRequired/ShopAlbumBridge.prefab",
            "Assets/Prefabs/SceneRequired/NPC_CollectorSystem.prefab",
            "Assets/Prefabs/ShopSystem.prefab"
        };

        foreach (var path in prefabs)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            DrawStatusRow(name, asset != null);
        }
    }

    // ─── Force Actions ───────────────────────────────────────────────────────

    private void ForceCombat()
    {
        if (CombatManager.Instance == null)
        {
            Debug.LogError("[IntegrationDebug] CombatManager.Instance is null!");
            return;
        }

        var fichas = new List<FichaData>();
        string[] nombres = { "Debug Ficha A", "Debug Ficha B", "Debug Ficha C" };
        for (int i = 0; i < 3; i++)
        {
            fichas.Add(new FichaData
            {
                templateId = 900 + i,
                nombre = nombres[i],
                rareza = (Rareza)(i % 3),
                rango = 1,
                experienciaDeRango = 0,
                estaRoto = false,
                desgaste = Random.Range(0f, 10f),
                perk = "Debug",
                peso = Random.Range(10f, 15f),
                suerte = 0.5f
            });
        }

        CombatManager.Instance.IniciarCombate(fichas);
        Debug.Log("[IntegrationDebug] Combat started with 3 debug fichas.");
    }

    private void ForceExitCombat()
    {
        if (CombatManager.Instance == null) return;
        CombatManager.Instance.SalirDelCombate();
        Debug.Log("[IntegrationDebug] Combat force-exited.");
    }

    private void ForceOpenShop()
    {
        var shopGO = GameObject.Find("ShopSystem");
        if (shopGO == null)
        {
            Debug.LogError("[IntegrationDebug] ShopSystem GO not found in scene!");
            return;
        }

        var shopManager = shopGO.GetComponent<Flipit.Shop.ShopManager>();
        if (shopManager != null)
            shopManager.InitializeShop();

        shopGO.SetActive(true);
        Debug.Log("[IntegrationDebug] Shop forced open.");
    }

    private void ForceCloseShop()
    {
        var shopGO = GameObject.Find("ShopSystem");
        if (shopGO != null)
        {
            shopGO.SetActive(false);
            Debug.Log("[IntegrationDebug] Shop forced closed.");
        }

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetExploration();
    }

    private void ForceOpenCollector()
    {
        var collectorUI = Object.FindAnyObjectByType<Flipit.NPC.CollectorNPCUIController>();
        if (collectorUI == null)
        {
            Debug.LogError("[IntegrationDebug] CollectorNPCUIController not found!");
            return;
        }

        collectorUI.OpenMenu();
        Debug.Log("[IntegrationDebug] Collector menu forced open.");
    }

    private void AddMockFichasToAlbum()
    {
        if (AlbumManager.Instance == null)
        {
            Debug.LogError("[IntegrationDebug] AlbumManager.Instance is null!");
            return;
        }

        string[] nombres = { "Dragon Dorado", "Fenix Rojo", "Lobo Plateado" };
        for (int i = 0; i < 3; i++)
        {
            var ficha = new FichaData
            {
                templateId = 800 + i + Random.Range(0, 100),
                nombre = nombres[i],
                rareza = (Rareza)(i % 3),
                rango = 1,
                experienciaDeRango = 0,
                estaRoto = false,
                desgaste = 0,
                perk = "Ninguno",
                peso = Random.Range(8f, 18f),
                suerte = Random.Range(0.3f, 0.8f)
            };
            AlbumManager.Instance.AgregarFicha(ficha);
        }

        Debug.Log("[IntegrationDebug] Added 3 mock fichas to album.");
    }

    private void OnInspectorUpdate()
    {
        // Repaint during play mode to keep status updated
        if (Application.isPlaying)
            Repaint();
    }
}
