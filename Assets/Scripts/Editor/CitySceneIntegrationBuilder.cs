using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Editor script that instantiates all system prefabs into the CityExplorationScene
/// and wires up references. This connects Combat, Shop, Collector, Pause, Album,
/// CoinFlip, and Camera systems so the full game loop works in a single scene.
/// 
/// Menu: Flipit → Integrate Systems in City
/// </summary>
public static class CitySceneIntegrationBuilder
{
    private const string MenuPath = "Flipit/Integrate Systems in City";

    // Prefab paths (InstancarCombate folder)
    private const string CombatSystemPrefab = "Assets/Prefabs/InstancarCombate/--- COMBAT SYSTEM ---.prefab";
    private const string CoinFlipPrefab = "Assets/Prefabs/InstancarCombate/CoinFliperManager.prefab";
    private const string PauseSystemPrefab = "Assets/Prefabs/InstancarCombate/--- PAUSE SYSTEM ---.prefab";
    private const string AlbumCanvasPrefab = "Assets/Prefabs/InstancarCombate/AlbumCanvas.prefab";
    private const string AlbumManagerPrefab = "Assets/Prefabs/InstancarCombate/AlbumManager.prefab";
    private const string CameraManagerPrefab = "Assets/Prefabs/InstancarCombate/CameraManager.prefab";
    private const string EventSystemPrefab = "Assets/Prefabs/InstancarCombate/EventSystem.prefab";

    // Prefab paths (SceneRequired folder)
    private const string GameStateManagerPrefab = "Assets/Prefabs/SceneRequired/GameStateManager.prefab";
    private const string NPCPauseBridgePrefab = "Assets/Prefabs/SceneRequired/NPCPauseBridge.prefab";
    private const string ShopAlbumBridgePrefab = "Assets/Prefabs/SceneRequired/ShopAlbumBridge.prefab";
    private const string CollectorSystemPrefab = "Assets/Prefabs/SceneRequired/NPC_CollectorSystem.prefab";
    private const string NPCCanvasPrefab = "Assets/Prefabs/SceneRequired/NPC_Canvas.prefab";
    private const string ShopSystemPrefab = "Assets/Prefabs/ShopSystem.prefab";

    [MenuItem(MenuPath)]
    public static void IntegrateSystems()
    {
        Debug.Log("[CitySceneIntegration] === Starting systems integration ===");

        int instantiated = 0;
        int skipped = 0;

        // --- Core Singletons ---
        InstantiatePrefabIfMissing<GameStateManager>(GameStateManagerPrefab, ref instantiated, ref skipped);
        InstantiatePrefabIfMissing<CameraManager>(CameraManagerPrefab, ref instantiated, ref skipped);

        // --- Combat System at offset ---
        var combatGO = InstantiatePrefabIfMissing<CombatManager>(CombatSystemPrefab, ref instantiated, ref skipped);
        if (combatGO != null)
        {
            combatGO.transform.position = new Vector3(200f, 0f, 200f);
            Debug.Log("[CitySceneIntegration] Combat System moved to offset (200, 0, 200).");
        }

        // --- CoinFlip ---
        InstantiatePrefabIfMissing<CoinFlipManager>(CoinFlipPrefab, ref instantiated, ref skipped);

        // --- Pause System ---
        InstantiatePrefabIfMissingByName("--- PAUSE SYSTEM ---", PauseSystemPrefab, ref instantiated, ref skipped);

        // --- Album ---
        InstantiatePrefabIfMissing<AlbumManager>(AlbumManagerPrefab, ref instantiated, ref skipped);
        InstantiatePrefabIfMissingByName("AlbumCanvas", AlbumCanvasPrefab, ref instantiated, ref skipped);

        // --- Shop (starts disabled — VendorShopBridge activates on vendor dialogue) ---
        var shopGO = InstantiatePrefabIfMissingByName("ShopSystem", ShopSystemPrefab, ref instantiated, ref skipped);
        if (shopGO != null)
        {
            shopGO.SetActive(false);
            Debug.Log("[CitySceneIntegration] ShopSystem set to inactive (activated by VendorShopBridge).");
        }
        else
        {
            // If it already existed, ensure it's disabled
            var existingShop = GameObject.Find("ShopSystem");
            if (existingShop != null)
                existingShop.SetActive(false);
        }
        InstantiatePrefabIfMissingByName("ShopAlbumBridge", ShopAlbumBridgePrefab, ref instantiated, ref skipped);

        // --- Collector ---
        InstantiatePrefabIfMissingByName("NPC_CollectorSystem", CollectorSystemPrefab, ref instantiated, ref skipped);

        // --- Collector UI Canvas (starts disabled — CollectorDialogueBridge activates on dialogue) ---
        var npcCanvasGO = InstantiatePrefabIfMissingByName("NPC_Canvas", NPCCanvasPrefab, ref instantiated, ref skipped);
        if (npcCanvasGO != null)
        {
            npcCanvasGO.SetActive(false);
            Debug.Log("[CitySceneIntegration] NPC_Canvas set to inactive (activated by CollectorDialogueBridge).");
        }
        else
        {
            // If it already existed, ensure it's disabled
            var existingCanvas = GameObject.Find("NPC_Canvas");
            if (existingCanvas == null)
            {
                // Try finding inactive
                var allControllers = Resources.FindObjectsOfTypeAll<Flipit.NPC.CollectorNPCUIController>();
                foreach (var ctrl in allControllers)
                {
                    if (ctrl.gameObject.scene.isLoaded)
                    {
                        ctrl.gameObject.SetActive(false);
                        break;
                    }
                }
            }
            else
            {
                existingCanvas.SetActive(false);
            }
        }

        InstantiatePrefabIfMissingByName("NPCPauseBridge", NPCPauseBridgePrefab, ref instantiated, ref skipped);

        // --- EventSystem (only if none exists) ---
        var existingEventSystem = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (existingEventSystem == null)
        {
            InstantiatePrefabIfMissingByName("EventSystem", EventSystemPrefab, ref instantiated, ref skipped);
        }
        else
        {
            skipped++;
            Debug.Log("[CitySceneIntegration] EventSystem already exists, skipping.");
        }

        // --- Wire Player Camera ---
        SetupPlayerCamera();

        // --- Wire Combat Camera ---
        SetupCombatCamera();

        // --- Disable default Main Camera ---
        DisableDefaultMainCamera();

        // --- Disable Combat_System if present (we use CombatManager directly) ---
        DisableCombatSystemSceneLoader();

        // --- Create Integration Bridges GO ---
        CreateIntegrationBridges();

        // --- Save ---
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log($"[CitySceneIntegration] === Complete! Instantiated: {instantiated}, Skipped (already exist): {skipped} ===");
    }

    /// <summary>
    /// Adds PlayerCameraRegister to the Isometric_Camera if not already present.
    /// </summary>
    private static void SetupPlayerCamera()
    {
        var isoCam = Object.FindAnyObjectByType<Flipit.CityTerrain.Isometric_Camera>();
        if (isoCam == null)
        {
            Debug.LogWarning("[CitySceneIntegration] Isometric_Camera not found. Generate city first.");
            return;
        }

        if (isoCam.GetComponent<PlayerCameraRegister>() == null)
        {
            isoCam.gameObject.AddComponent<PlayerCameraRegister>();
            Debug.Log("[CitySceneIntegration] Added PlayerCameraRegister to Isometric_Camera.");
        }

        // Ensure camera has MainCamera tag
        isoCam.gameObject.tag = "MainCamera";
    }

    /// <summary>
    /// Finds the CombatCamera inside the Combat System hierarchy and adds CombatCameraRegister.
    /// </summary>
    private static void SetupCombatCamera()
    {
        // Look for a Camera component inside the Combat System hierarchy (not the player camera)
        var combatManager = Object.FindAnyObjectByType<CombatManager>();
        if (combatManager == null)
        {
            Debug.LogWarning("[CitySceneIntegration] CombatManager not found. Cannot setup combat camera.");
            return;
        }

        // Find cameras under the combat system hierarchy
        var cameras = combatManager.GetComponentsInChildren<Camera>(true);
        foreach (var cam in cameras)
        {
            if (cam.GetComponent<CombatCameraRegister>() == null)
            {
                cam.gameObject.AddComponent<CombatCameraRegister>();
                cam.gameObject.tag = "MainCamera";
                Debug.Log($"[CitySceneIntegration] Added CombatCameraRegister to '{cam.gameObject.name}'.");
                break; // Only the first combat camera
            }
        }
    }

    /// <summary>
    /// Disables the default "Main Camera" created by Unity scene template.
    /// </summary>
    private static void DisableDefaultMainCamera()
    {
        var mainCam = GameObject.Find("Main Camera");
        if (mainCam != null && mainCam.GetComponent<Flipit.CityTerrain.Isometric_Camera>() == null)
        {
            mainCam.SetActive(false);
            Debug.Log("[CitySceneIntegration] Disabled default 'Main Camera'.");
        }
    }

    /// <summary>
    /// Disables Combat_System component to prevent scene-loading combat flow.
    /// We use CombatManager for same-scene combat instead.
    /// </summary>
    private static void DisableCombatSystemSceneLoader()
    {
        var combatSystem = Object.FindAnyObjectByType<Flipit.Combat.Combat_System>();
        if (combatSystem != null)
        {
            combatSystem.enabled = false;
            Debug.Log("[CitySceneIntegration] Disabled Combat_System (using same-scene combat via CombatManager).");
        }

        var dialogueHandler = Object.FindAnyObjectByType<Flipit.Combat.CombatDialogue_Handler>();
        if (dialogueHandler != null)
        {
            dialogueHandler.enabled = false;
            Debug.Log("[CitySceneIntegration] Disabled CombatDialogue_Handler (using ChallengerCombatBridge instead).");
        }
    }

    /// <summary>
    /// Creates the Integration Bridges parent GO with all bridge components.
    /// </summary>
    private static void CreateIntegrationBridges()
    {
        const string bridgesName = "--- INTEGRATION BRIDGES ---";
        var existing = GameObject.Find(bridgesName);
        if (existing != null)
        {
            Debug.Log("[CitySceneIntegration] Integration Bridges already exist, skipping.");
            return;
        }

        var bridgesGO = new GameObject(bridgesName);

        // Add bridge components
        bridgesGO.AddComponent<BetSelectionBridge>();
        bridgesGO.AddComponent<ChallengerCombatBridge>();
        bridgesGO.AddComponent<VendorShopBridge>();
        bridgesGO.AddComponent<CollectorDialogueBridge>();
        bridgesGO.AddComponent<CombatResultBridge>();

        Debug.Log("[CitySceneIntegration] Created Integration Bridges container.");
    }

    // ─── Helper Methods ───────────────────────────────────────────────────────

    private static GameObject InstantiatePrefabIfMissing<T>(string prefabPath, ref int instantiated, ref int skipped) where T : Component
    {
        var existing = Object.FindAnyObjectByType<T>();
        if (existing != null)
        {
            skipped++;
            Debug.Log($"[CitySceneIntegration] {typeof(T).Name} already exists, skipping.");
            return null;
        }

        return InstantiatePrefab(prefabPath, ref instantiated);
    }

    private static GameObject InstantiatePrefabIfMissingByName(string goName, string prefabPath, ref int instantiated, ref int skipped)
    {
        var existing = GameObject.Find(goName);
        if (existing != null)
        {
            skipped++;
            Debug.Log($"[CitySceneIntegration] '{goName}' already exists, skipping.");
            return null;
        }

        return InstantiatePrefab(prefabPath, ref instantiated);
    }

    private static GameObject InstantiatePrefab(string prefabPath, ref int instantiated)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogWarning($"[CitySceneIntegration] Prefab not found at: {prefabPath}");
            return null;
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Undo.RegisterCreatedObjectUndo(instance, $"Instantiate {prefab.name}");
        instantiated++;
        Debug.Log($"[CitySceneIntegration] Instantiated: {prefab.name}");
        return instance;
    }
}
