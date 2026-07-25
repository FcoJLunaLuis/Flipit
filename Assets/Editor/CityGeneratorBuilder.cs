using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Flipit.CityTerrain;
using Flipit.Dialogue;

/// <summary>
/// Editor script exposing "Flipit/Generate City" menu item.
/// Creates/opens CityExplorationScene, finds or creates City_Generator,
/// executes generation with confirmation dialogs between stages.
/// </summary>
public static class CityGeneratorBuilder
{
    private const string ScenePath = "Assets/Scenes/CityExplorationScene.unity";
    private const string MenuPath = "Flipit/Generate City";

    [MenuItem("Flipit/Generate City (No Dialogs)")]
    public static void GenerateCityNoDialogs()
    {
        // Step 1: Open or create the CityExplorationScene
        Scene scene;
        if (System.IO.File.Exists(ScenePath))
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }
        else
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }
            scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        // Step 2: Find or create City_Generator GameObject
        City_Generator generator = UnityEngine.Object.FindAnyObjectByType<City_Generator>();
        if (generator == null)
        {
            GameObject generatorGO = new GameObject("City_Generator");
            generator = generatorGO.AddComponent<City_Generator>();
        }

        // Step 3: Ensure a City_Config is assigned
        EnsureCityConfig(generator);

        // Step 4: Prepare and run all stages without dialogs
        if (!generator.PrepareGeneration())
        {
            Debug.LogError("[CityGeneratorBuilder] Preparation failed. Check Console.");
            return;
        }

        try
        {
            if (!generator.ExecuteStage1_CoreLayout())
            { Debug.LogError("[CityGeneratorBuilder] Stage 1 failed."); SaveScene(scene); return; }
            Debug.Log($"[CityGeneratorBuilder] Stage 1 done: {generator.GetLastStageSummary()}");

            if (!generator.ExecuteStage2_UrbanProps())
            { Debug.LogError("[CityGeneratorBuilder] Stage 2 failed."); SaveScene(scene); return; }
            Debug.Log($"[CityGeneratorBuilder] Stage 2 done: {generator.GetLastStageSummary()}");

            if (!generator.ExecuteStage3_NPCPlacement())
            { Debug.LogError("[CityGeneratorBuilder] Stage 3 failed."); SaveScene(scene); return; }
            Debug.Log($"[CityGeneratorBuilder] Stage 3 done: {generator.GetLastStageSummary()}");

            if (!generator.ExecuteStage4_Transparency())
            { Debug.LogError("[CityGeneratorBuilder] Stage 4 failed."); SaveScene(scene); return; }
            Debug.Log($"[CityGeneratorBuilder] Stage 4 done: {generator.GetLastStageSummary()}");

            if (!generator.ExecuteStage5_Variation())
            { Debug.LogError("[CityGeneratorBuilder] Stage 5 failed."); SaveScene(scene); return; }
            Debug.Log($"[CityGeneratorBuilder] Stage 5 done: {generator.GetLastStageSummary()}");

            Debug.Log("[CityGeneratorBuilder] ALL 5 STAGES COMPLETE! Press Play to explore.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CityGeneratorBuilder] Error: {ex.Message}\n{ex.StackTrace}");
        }

        // --- Create Dialogue System ---
        CreateDialogueSystem();

        // --- Create Special Buildings ---
        CreateSpecialBuildings();

        // Disable the default Main Camera if present
        var mainCam = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCam != null && mainCam.name == "Main Camera")
        {
            mainCam.SetActive(false);
            Debug.Log("[CityGeneratorBuilder] Disabled default Main Camera.");
        }

        SaveScene(scene);
    }

    [MenuItem(MenuPath)]
    public static void GenerateCity()
    {
        // Step 1: Open or create the CityExplorationScene
        Scene scene;
        if (System.IO.File.Exists(ScenePath))
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }
        else
        {
            // Ensure Scenes folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        // Step 2: Find or create City_Generator GameObject
        City_Generator generator = UnityEngine.Object.FindAnyObjectByType<City_Generator>();
        if (generator == null)
        {
            GameObject generatorGO = new GameObject("City_Generator");
            generator = generatorGO.AddComponent<City_Generator>();
        }

        // Step 3: Ensure a City_Config is assigned
        EnsureCityConfig(generator);

        // Step 4: Prepare generation (validate, seed, clear)
        if (!generator.PrepareGeneration())
        {
            EditorUtility.DisplayDialog(
                "Generation Failed",
                "City generation preparation failed. Check the Console for details.",
                "OK");
            return;
        }

        // Step 5: Execute stages with confirmation dialogs between each
        try
        {
            // Stage 1: Core Layout
            if (!ExecuteStageWithErrorHandling(generator, 1))
            {
                SaveScene(scene);
                return;
            }

            if (!ShowStageConfirmation(1, generator.GetLastStageSummary()))
            {
                Debug.Log("[CityGeneratorBuilder] Generation cancelled by user after Stage 1.");
                SaveScene(scene);
                return;
            }

            // Stage 2: Urban Props
            if (!ExecuteStageWithErrorHandling(generator, 2))
            {
                SaveScene(scene);
                return;
            }

            if (!ShowStageConfirmation(2, generator.GetLastStageSummary()))
            {
                Debug.Log("[CityGeneratorBuilder] Generation cancelled by user after Stage 2.");
                SaveScene(scene);
                return;
            }

            // Stage 3: NPC Placement
            if (!ExecuteStageWithErrorHandling(generator, 3))
            {
                SaveScene(scene);
                return;
            }

            if (!ShowStageConfirmation(3, generator.GetLastStageSummary()))
            {
                Debug.Log("[CityGeneratorBuilder] Generation cancelled by user after Stage 3.");
                SaveScene(scene);
                return;
            }

            // Stage 4: Building Transparency
            if (!ExecuteStageWithErrorHandling(generator, 4))
            {
                SaveScene(scene);
                return;
            }

            if (!ShowStageConfirmation(4, generator.GetLastStageSummary()))
            {
                Debug.Log("[CityGeneratorBuilder] Generation cancelled by user after Stage 4.");
                SaveScene(scene);
                return;
            }

            // Stage 5: Decorative Variation
            if (!ExecuteStageWithErrorHandling(generator, 5))
            {
                SaveScene(scene);
                return;
            }

            // Stage 5 complete — show completion dialog (no "proceed" option)
            EditorUtility.DisplayDialog(
                "Generation Complete",
                $"All 5 stages completed successfully.\n\nStage 5 Summary: {generator.GetLastStageSummary()}",
                "OK");

            Debug.Log("[CityGeneratorBuilder] City generation complete. All 5 stages finished.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CityGeneratorBuilder] Unexpected error during generation: {ex.Message}\n{ex.StackTrace}");
            EditorUtility.DisplayDialog(
                "Generation Error",
                $"An unexpected error occurred:\n\n{ex.Message}\n\nPreviously completed stages have been preserved.",
                "OK");
        }

        // Save scene after completion or cancellation
        SaveScene(scene);
    }

    /// <summary>
    /// Shows a confirmation dialog after a stage completes.
    /// Returns true if the user clicks "Continue", false if "Cancel".
    /// </summary>
    private static bool ShowStageConfirmation(int stage, string summary)
    {
        return EditorUtility.DisplayDialog(
            $"Stage {stage} Complete",
            $"Stage {stage} generation completed.\n\n{summary}\n\nProceed to Stage {stage + 1}?",
            "Continue",
            "Cancel");
    }

    /// <summary>
    /// Executes a single generation stage with error handling.
    /// Returns true if the stage succeeded, false if it failed (shows error dialog).
    /// </summary>
    private static bool ExecuteStageWithErrorHandling(City_Generator generator, int stage)
    {
        bool success;

        try
        {
            switch (stage)
            {
                case 1:
                    success = generator.ExecuteStage1_CoreLayout();
                    break;
                case 2:
                    success = generator.ExecuteStage2_UrbanProps();
                    break;
                case 3:
                    success = generator.ExecuteStage3_NPCPlacement();
                    break;
                case 4:
                    success = generator.ExecuteStage4_Transparency();
                    break;
                case 5:
                    success = generator.ExecuteStage5_Variation();
                    break;
                default:
                    Debug.LogError($"[CityGeneratorBuilder] Invalid stage number: {stage}");
                    return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CityGeneratorBuilder] Stage {stage} threw an exception: {ex.Message}\n{ex.StackTrace}");
            EditorUtility.DisplayDialog(
                $"Stage {stage} Error",
                $"Stage {stage} encountered an error:\n\n{ex.Message}\n\nPreviously completed stages have been preserved.",
                "OK");
            return false;
        }

        if (!success)
        {
            EditorUtility.DisplayDialog(
                $"Stage {stage} Failed",
                $"Stage {stage} generation failed. Check the Console for details.\n\nPreviously completed stages have been preserved.",
                "OK");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Ensures the City_Generator has a City_Config assigned.
    /// If none exists in the project, creates one with default values.
    /// </summary>
    private static void EnsureCityConfig(City_Generator generator)
    {
        // Check if config is already assigned via serialized field
        var serializedObj = new SerializedObject(generator);
        var configProp = serializedObj.FindProperty("_config");

        if (configProp.objectReferenceValue != null)
            return;

        // Try to find a City_Config asset in the project
        string[] guids = AssetDatabase.FindAssets("t:City_Config");
        City_Config config = null;

        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            config = AssetDatabase.LoadAssetAtPath<City_Config>(path);
        }

        // If no config exists, create one with sensible defaults
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<City_Config>();

            // Set default color palette via serialized object (needs min 5 colors)
            string configFolder = "Assets/Scripts/CityTerrain";
            if (!AssetDatabase.IsValidFolder(configFolder))
            {
                configFolder = "Assets";
            }
            string configPath = configFolder + "/DefaultCityConfig.asset";
            AssetDatabase.CreateAsset(config, configPath);

            // Now set the color palette (requires SerializedObject since field is private)
            var configSO = new SerializedObject(config);
            var paletteProp = configSO.FindProperty("_buildingColorPalette");
            paletteProp.arraySize = 6;
            paletteProp.GetArrayElementAtIndex(0).colorValue = new Color(0.7f, 0.7f, 0.8f, 1f); // light blue-gray
            paletteProp.GetArrayElementAtIndex(1).colorValue = new Color(0.85f, 0.75f, 0.65f, 1f); // tan
            paletteProp.GetArrayElementAtIndex(2).colorValue = new Color(0.6f, 0.6f, 0.6f, 1f); // gray
            paletteProp.GetArrayElementAtIndex(3).colorValue = new Color(0.8f, 0.85f, 0.75f, 1f); // light olive
            paletteProp.GetArrayElementAtIndex(4).colorValue = new Color(0.75f, 0.65f, 0.6f, 1f); // dusty rose
            paletteProp.GetArrayElementAtIndex(5).colorValue = new Color(0.65f, 0.75f, 0.8f, 1f); // steel blue
            configSO.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.SaveAssets();
            Debug.Log($"[CityGeneratorBuilder] Created default City_Config at: {configPath}");
        }

        configProp.objectReferenceValue = config;
        serializedObj.ApplyModifiedPropertiesWithoutUndo();
        Debug.Log($"[CityGeneratorBuilder] Assigned City_Config to City_Generator.");
    }

    /// <summary>
    /// Saves the scene at ScenePath.
    /// </summary>
    private static void SaveScene(Scene scene)
    {
        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log($"[CityGeneratorBuilder] Scene saved to: {ScenePath}");
    }

    /// <summary>
    /// Creates the Dialogue Manager and Dialogue Canvas UI hierarchy.
    /// Called after all 5 generation stages complete.
    /// </summary>
    private static void CreateDialogueSystem()
    {
        // Remove any existing dialogue system
        var existingManager = UnityEngine.Object.FindAnyObjectByType<Dialogue_Manager>();
        if (existingManager != null)
            DestroyImmediate(existingManager.gameObject);

        var existingCanvas = GameObject.Find("DialogueCanvas");
        if (existingCanvas != null)
            DestroyImmediate(existingCanvas);

        // Create Dialogue_Manager as standalone object
        var dialogueManagerGO = new GameObject("Dialogue_Manager");
        var dialogueManager = dialogueManagerGO.AddComponent<Dialogue_Manager>();

        // Create DialogueCanvas (Screen Space Overlay)
        var canvasGO = new GameObject("DialogueCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // DialoguePanel - positioned at bottom of screen with dark background
        var panelGO = new GameObject("DialoguePanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.02f, 0.01f);
        panelRect.anchorMax = new Vector2(0.98f, 0.45f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        var panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

        // SpeakerName - top of panel
        var speakerGO = new GameObject("SpeakerName");
        speakerGO.transform.SetParent(panelGO.transform, false);
        var speakerRect = speakerGO.AddComponent<RectTransform>();
        speakerRect.anchorMin = new Vector2(0.02f, 0.75f);
        speakerRect.anchorMax = new Vector2(0.5f, 0.98f);
        speakerRect.offsetMin = Vector2.zero;
        speakerRect.offsetMax = Vector2.zero;
        var speakerTMP = speakerGO.AddComponent<TextMeshProUGUI>();
        speakerTMP.fontSize = 48;
        speakerTMP.fontStyle = FontStyles.Bold;
        speakerTMP.color = Color.yellow;

        // DialogueText - middle of panel
        var dialogueTextGO = new GameObject("DialogueText");
        dialogueTextGO.transform.SetParent(panelGO.transform, false);
        var dialogueTextRect = dialogueTextGO.AddComponent<RectTransform>();
        dialogueTextRect.anchorMin = new Vector2(0.02f, 0.15f);
        dialogueTextRect.anchorMax = new Vector2(0.98f, 0.72f);
        dialogueTextRect.offsetMin = Vector2.zero;
        dialogueTextRect.offsetMax = Vector2.zero;
        var dialogueTextTMP = dialogueTextGO.AddComponent<TextMeshProUGUI>();
        dialogueTextTMP.fontSize = 42;
        dialogueTextTMP.color = Color.white;

        // AdvanceIndicator
        var advanceGO = new GameObject("AdvanceIndicator");
        advanceGO.transform.SetParent(panelGO.transform, false);
        advanceGO.AddComponent<RectTransform>();
        advanceGO.SetActive(false);

        // TypingIndicator
        var typingGO = new GameObject("TypingIndicator");
        typingGO.transform.SetParent(panelGO.transform, false);
        typingGO.AddComponent<RectTransform>();
        typingGO.SetActive(false);

        // OptionsPanel - bottom of panel
        var optionsPanelGO = new GameObject("OptionsPanel");
        optionsPanelGO.transform.SetParent(panelGO.transform, false);
        var optionsRect = optionsPanelGO.AddComponent<RectTransform>();
        optionsRect.anchorMin = new Vector2(0.02f, 0.02f);
        optionsRect.anchorMax = new Vector2(0.98f, 0.14f);
        optionsRect.offsetMin = Vector2.zero;
        optionsRect.offsetMax = Vector2.zero;

        // 2 Option Buttons (Si / No)
        var optionButtons = new DialogueOptionButton[2];
        for (int i = 0; i < 2; i++)
        {
            var btnGO = new GameObject($"OptionButton_{i}");
            btnGO.transform.SetParent(optionsPanelGO.transform, false);
            var btnRect = btnGO.AddComponent<RectTransform>();
            float xMin = i * 0.5f + 0.02f;
            float xMax = (i + 1) * 0.5f - 0.02f;
            btnRect.anchorMin = new Vector2(xMin, 0.1f);
            btnRect.anchorMax = new Vector2(xMax, 0.9f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            var highlightImg = btnGO.AddComponent<Image>();
            highlightImg.color = new Color(0.3f, 0.6f, 1f, 0.5f);
            highlightImg.enabled = false;

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(btnGO.transform, false);
            var labelRect = labelGO.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
            labelTMP.fontSize = 40;
            labelTMP.color = Color.white;
            labelTMP.alignment = TextAlignmentOptions.Center;

            var optBtn = btnGO.AddComponent<DialogueOptionButton>();
            SetPrivateFieldStatic(optBtn, "labelText", (TMP_Text)labelTMP);
            SetPrivateFieldStatic(optBtn, "highlightImage", highlightImg);

            optionButtons[i] = optBtn;
            btnGO.SetActive(false);
        }

        // Add Dialogue_UI and Typewriter_Effect to canvas
        var dialogueUI = canvasGO.AddComponent<Dialogue_UI>();
        SetPrivateFieldStatic(dialogueUI, "dialogueCanvas", canvas);
        SetPrivateFieldStatic(dialogueUI, "speakerNameText", (TMP_Text)speakerTMP);
        SetPrivateFieldStatic(dialogueUI, "dialogueText", (TMP_Text)dialogueTextTMP);
        SetPrivateFieldStatic(dialogueUI, "advanceIndicator", advanceGO);
        SetPrivateFieldStatic(dialogueUI, "typingIndicator", typingGO);
        SetPrivateFieldStatic(dialogueUI, "optionButtons", optionButtons);

        var typewriter = canvasGO.AddComponent<Typewriter_Effect>();

        // Canvas starts disabled - Dialogue_UI.Show() enables it
        canvas.enabled = false;

        // Configure Dialogue_Manager
        var playerInput = UnityEngine.Object.FindAnyObjectByType<PlayerInput>();
        SetPrivateFieldStatic(dialogueManager, "dialogueUIComponent", (MonoBehaviour)dialogueUI);
        SetPrivateFieldStatic(dialogueManager, "typewriterEffect", typewriter);
        SetPrivateFieldStatic(dialogueManager, "playerInput", playerInput);

        Debug.Log("[CityGeneratorBuilder] Dialogue system created successfully.");
    }

    /// <summary>
    /// Helper to set private/serialized fields via reflection (static context version).
    /// </summary>
    private static void SetPrivateFieldStatic(object target, string fieldName, object value)
    {
        if (target == null) return;

        var type = target.GetType();
        System.Reflection.FieldInfo field = null;

        while (type != null && field == null)
        {
            field = type.GetField(fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);
            type = type.BaseType;
        }

        if (field != null)
        {
            field.SetValue(target, value);
        }
        else
        {
            Debug.LogWarning($"[CityGeneratorBuilder] Could not find field '{fieldName}' on {target.GetType().Name}");
        }
    }

    private static void CreateSpecialBuildings()
    {
        float cellSize = 4f; // matches City_Config
        var buildingsParent = GameObject.Find("City_Generator")?.transform.Find("Buildings");
        var npcsParent = GameObject.Find("City_Generator")?.transform.Find("NPCs");
        if (buildingsParent == null || npcsParent == null)
        {
            Debug.LogError("[CityGeneratorBuilder] Cannot create special buildings: missing parents.");
            return;
        }

        // Define special buildings: (name, position row/col, height, color, label, labelColor, npcName, npcColor, npcDialoguePath, npcOffsetRow)
        CreateSpecialBuilding(buildingsParent, npcsParent, cellSize,
            "Building_Coleccion", 6, 6, 8f, new Color(0.6f, 0.2f, 0.8f),
            "COLECCIÓN", new Color(0.8f, 0.4f, 1f),
            "NPC_Coleccionista", new Color(0.7f, 0.3f, 0.9f), "Assets/DialogueData/NPC_Coleccionista.asset", -1);

        CreateSpecialBuilding(buildingsParent, npcsParent, cellSize,
            "Building_Tiendita", 14, 22, 6f, new Color(1f, 0.6f, 0.1f),
            "TIENDITA", new Color(1f, 0.7f, 0.2f),
            "NPC_Tendero", new Color(1f, 0.7f, 0.2f), "Assets/DialogueData/NPC_Tendero.asset", -1);

        CreateSpecialBuilding(buildingsParent, npcsParent, cellSize,
            "Building_Casa", 18, 18, 5f, new Color(0.2f, 0.7f, 0.3f),
            "CASA", new Color(0.3f, 0.9f, 0.4f),
            null, default, null, 0); // No NPC for Casa

        Debug.Log("[CityGeneratorBuilder] Special buildings created (Colección, Tiendita, Casa).");
    }

    private static void CreateSpecialBuilding(Transform buildingsParent, Transform npcsParent, float cellSize,
        string buildingName, int row, int col, float height, Color buildingColor,
        string labelText, Color labelColor,
        string npcName, Color npcColor, string dialoguePath, int npcRowOffset)
    {
        float worldX = col * cellSize;
        float worldZ = row * cellSize;

        // Remove any existing building at this exact position
        var toRemove = new System.Collections.Generic.List<GameObject>();
        for (int i = 0; i < buildingsParent.childCount; i++)
        {
            var child = buildingsParent.GetChild(i);
            if (child.name == buildingName) continue; // don't remove self if somehow already exists
            float dx = Mathf.Abs(child.position.x - worldX);
            float dz = Mathf.Abs(child.position.z - worldZ);
            if (dx < cellSize * 0.5f && dz < cellSize * 0.5f)
                toRemove.Add(child.gameObject);
        }
        foreach (var go in toRemove)
            DestroyImmediate(go);

        // Create the building
        var building = GameObject.CreatePrimitive(PrimitiveType.Cube);
        building.name = buildingName;
        building.transform.position = new Vector3(worldX, height * 0.5f, worldZ);
        building.transform.localScale = new Vector3(cellSize, height, cellSize);
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = buildingColor;
        building.GetComponent<MeshRenderer>().sharedMaterial = mat;
        building.transform.SetParent(buildingsParent);

        // Building label (on top)
        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(building.transform);
        lblGO.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        lblGO.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
        var cvs = lblGO.AddComponent<Canvas>();
        cvs.renderMode = RenderMode.WorldSpace;
        lblGO.GetComponent<RectTransform>().sizeDelta = new Vector2(300f, 60f);
        var tGO = new GameObject("Text");
        tGO.transform.SetParent(lblGO.transform, false);
        var tr = tGO.AddComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;
        var tmp = tGO.AddComponent<TextMeshProUGUI>();
        tmp.text = labelText;
        tmp.fontSize = 36;
        tmp.color = labelColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
        lblGO.AddComponent<Flipit.CityTerrain.Billboard>();

        // Create NPC if specified
        if (!string.IsNullOrEmpty(npcName))
        {
            float npcWorldZ = (row + npcRowOffset) * cellSize;
            var npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            npc.name = npcName;
            npc.transform.position = new Vector3(worldX, 1f, npcWorldZ);
            var npcMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            npcMat.color = npcColor;
            npc.GetComponent<MeshRenderer>().sharedMaterial = npcMat;
            npc.transform.SetParent(npcsParent);

            // Add NPC_Interactable with dialogue
            var interactable = npc.AddComponent<Flipit.Dialogue.NPC_Interactable>();
            if (!string.IsNullOrEmpty(dialoguePath))
            {
                var dialogue = AssetDatabase.LoadAssetAtPath<Flipit.Dialogue.DialogueData>(dialoguePath);
                if (dialogue != null)
                    SetPrivateFieldStatic(interactable, "dialogueData", dialogue);
            }

            // Silhouette
            npc.AddComponent<Flipit.CityTerrain.CharacterSilhouette>();

            // NPC label
            var npcLblGO = new GameObject("Label");
            npcLblGO.transform.SetParent(npc.transform);
            npcLblGO.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            npcLblGO.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            var npcCvs = npcLblGO.AddComponent<Canvas>();
            npcCvs.renderMode = RenderMode.WorldSpace;
            npcLblGO.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 50f);
            var npcTGO = new GameObject("Text");
            npcTGO.transform.SetParent(npcLblGO.transform, false);
            var npcTR = npcTGO.AddComponent<RectTransform>();
            npcTR.anchorMin = Vector2.zero; npcTR.anchorMax = Vector2.one;
            npcTR.offsetMin = Vector2.zero; npcTR.offsetMax = Vector2.zero;
            var npcTMP = npcTGO.AddComponent<TextMeshProUGUI>();
            npcTMP.text = npcName.Replace("NPC_", "");
            npcTMP.fontSize = 20;
            npcTMP.color = npcColor;
            npcTMP.alignment = TextAlignmentOptions.Center;
            npcTMP.fontStyle = FontStyles.Bold;
            npcTMP.enableWordWrapping = false;
            npcTMP.overflowMode = TextOverflowModes.Overflow;
            npcLblGO.AddComponent<Flipit.CityTerrain.Billboard>();
        }
    }

    private static void DestroyImmediate(UnityEngine.Object obj)
    {
        UnityEngine.Object.DestroyImmediate(obj);
    }
}
