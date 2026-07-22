using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Flipit.Combat;
using Flipit.Dialogue;

/// <summary>
/// Editor utility that creates a fully functional CombatScene for testing.
/// Reuses the same patterns as DialogueSceneBuilder: sprite-based ground,
/// orthographic camera, Player with full components, Dialogue system, and
/// all Combat system managers wired together.
///
/// Usage: Unity menu → Flipit → Build Combat Scene
/// </summary>
public static class CombatSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/CombatScene.unity";
    private const string DialogueDataFolder = "Assets/DialogueData";
    private const string PrefabsFolder = "Assets/Prefabs";
    private const string EncounterConfigPath = "Assets/DialogueData/DefaultEncounterConfig.asset";
    private const string EncounterNPCPrefabPath = "Assets/Prefabs/RandomEncounter_NPC.prefab";

    [MenuItem("Flipit/Build Combat Scene")]
    public static void BuildScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        EnsureFolder(DialogueDataFolder);
        EnsureFolder(PrefabsFolder);

        // ─── Camera (same as DialogueDemoScene) ──────────────────────────────
        var mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(0, 0, -10);
            mainCam.orthographic = true;
            mainCam.orthographicSize = 12;
            mainCam.backgroundColor = new Color(0.12f, 0.12f, 0.18f);
        }

        // ─── Ground (same sprite approach as DialogueDemoScene) ──────────────
        var ground = new GameObject("Ground");
        var groundSR = ground.AddComponent<SpriteRenderer>();
        groundSR.sprite = CreateSquareSprite();
        groundSR.color = new Color(0.25f, 0.4f, 0.25f);
        ground.transform.localScale = new Vector3(30f, 24f, 1f);
        ground.transform.position = Vector3.zero;
        groundSR.sortingOrder = -10;

        // ─── City Boundaries (4 walls) ───────────────────────────────────────
        CreateBoundaryWall("Wall_Top", new Vector3(0, 12, 0), new Vector2(30, 1));
        CreateBoundaryWall("Wall_Bottom", new Vector3(0, -12, 0), new Vector2(30, 1));
        CreateBoundaryWall("Wall_Left", new Vector3(-15, 0, 0), new Vector2(1, 24));
        CreateBoundaryWall("Wall_Right", new Vector3(15, 0, 0), new Vector2(1, 24));

        // ─── Player (full setup with all components) ─────────────────────────
        var player = CreatePlayer();
        var playerInput = player.GetComponent<PlayerInput>();

        // ─── Dialogue System (required for combat flow) ──────────────────────
        var dialogueCanvas = CreateDialogueCanvas(out var dialogueUI);
        var managerGO = new GameObject("Dialogue_Manager");
        var dialogueManager = managerGO.AddComponent<Dialogue_Manager>();
        var typewriterEffect = managerGO.AddComponent<Typewriter_Effect>();
        WireDialogueManager(dialogueManager, typewriterEffect, dialogueUI, playerInput);

        // ─── Combat System Managers ──────────────────────────────────────────
        var combatSystemGO = new GameObject("Combat_System");
        var combatSystem = combatSystemGO.AddComponent<Combat_System>();
        var combatHandler = combatSystemGO.AddComponent<CombatDialogue_Handler>();

        // ─── Combat Transition UI (overlay) ──────────────────────────────────
        var transitionCanvas = CreateTransitionCanvas(out var transitionUI);

        // ─── Forced Encounter Panel (UI) ─────────────────────────────────────
        GameObject forcedPanelGO;
        Button forcedButton;
        CreateForcedEncounterPanel(dialogueCanvas.transform,
            out forcedPanelGO, out forcedButton);

        // Wire Combat_System references
        var forcedDialogueData = CreateForcedEncounterDialogueData();
        WireCombatSystem(combatSystem, transitionUI, playerInput, forcedPanelGO, forcedButton, forcedDialogueData);

        // ─── Create DialogueData for combat NPCs ─────────────────────────────
        var npc1Data = CreateCombatDialogueData("CombatNPC_Retador1", "Retador");
        var npc2Data = CreateCombatDialogueData("CombatNPC_Retador2", "Luchador");
        var npc4Data = CreateCombatDialogueData("CombatNPC_Retador4", "Novato");

        // ─── FlipCombat_NPCs (3 NPCs voluntarios) ────────────────────────────
        CreateFlipCombatNPC("FlipCombat_NPC_1", new Vector3(-8, 6, 0), Color.red,
            npc1Data, "CombatScene", "Retador");
        CreateFlipCombatNPC("FlipCombat_NPC_2", new Vector3(8, 6, 0), Color.blue,
            npc2Data, "CombatScene", "Luchador");
        CreateFlipCombatNPC("FlipCombat_NPC_4", new Vector3(8, -6, 0),
            new Color(1f, 0.5f, 0f), npc4Data, "CombatScene", "Novato");

        // ─── Campeón: Encuentro por zona (invisible, aparece cuando te acercas) ─
        CreateTriggerZoneEncounter("Campeon_Encounter", new Vector3(-8, -6, 0),
            new Color(0.8f, 0.2f, 0.8f), forcedDialogueData, "CombatScene", "Campeón");

        // ─── Spawn Points (3 points, >= 5 unit separation) ───────────────────
        var sp1 = CreateSpawnPoint("SpawnPoint_1", new Vector3(-5, 0, 0));
        var sp2 = CreateSpawnPoint("SpawnPoint_2", new Vector3(5, 0, 0));
        var sp3 = CreateSpawnPoint("SpawnPoint_3", new Vector3(0, 6, 0));

        // ─── Random Encounter Manager ────────────────────────────────────────
        var encounterConfig = CreateOrLoadEncounterConfig();
        var encounterPrefab = CreateOrLoadEncounterNPCPrefab();
        CreateEncounterManager(encounterConfig, encounterPrefab,
            new Transform[] { sp1.transform, sp2.transform, sp3.transform });

        // ─── Interaction Prompt (same as DialogueDemoScene) ───────────────────
        var promptGO = new GameObject("InteractionPrompt");
        promptGO.transform.SetParent(player.transform);
        promptGO.transform.localPosition = new Vector3(0, 1.2f, 0);
        var promptText = promptGO.AddComponent<TextMesh>();
        promptText.text = "[E] Retar";
        promptText.fontSize = 24;
        promptText.characterSize = 0.15f;
        promptText.anchor = TextAnchor.MiddleCenter;
        promptText.alignment = TextAlignment.Center;
        promptText.color = Color.white;
        promptGO.AddComponent<InteractionPromptHelper>();

        // ─── Save Scene ──────────────────────────────────────────────────────
        EnsureFolder("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        AddSceneToBuildSettings(ScenePath);

        Debug.Log($"[CombatSceneBuilder] Combat scene created at: {ScenePath}");
        Debug.Log("[CombatSceneBuilder] Scene is fully playable: WASD to move, E to interact with NPCs.");
        Debug.Log("[CombatSceneBuilder] 4 FlipCombat_NPCs, 3 spawn points, Random Encounter Manager active.");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PLAYER CREATION (same as DialogueDemoScene)
    // ═══════════════════════════════════════════════════════════════════════════

    private static GameObject CreatePlayer()
    {
        var player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0, -2, 0);

        var sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = Color.cyan;
        sr.sortingOrder = 5;
        player.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        var col = player.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;

        player.AddComponent<TopDownPlayerMovement>();

        var interactor = player.AddComponent<Player_Interactor>();
        var radiusField = typeof(Player_Interactor).GetField("interactionRadius",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (radiusField != null) radiusField.SetValue(interactor, 2.5f);

        var playerInput = player.AddComponent<PlayerInput>();
        var inputActionsAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
            "Assets/InputSystem_Actions.inputactions");
        if (inputActionsAsset != null)
        {
            playerInput.actions = inputActionsAsset;
            playerInput.defaultActionMap = "Player";
            playerInput.notificationBehavior = PlayerNotifications.SendMessages;
        }
        else
        {
            Debug.LogWarning("[CombatSceneBuilder] InputSystem_Actions.inputactions not found.");
        }

        return player;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DIALOGUE CANVAS (exact copy from DialogueSceneBuilder)
    // ═══════════════════════════════════════════════════════════════════════════

    private static GameObject CreateDialogueCanvas(out Dialogue_UI dialogueUI)
    {
        var canvasGO = new GameObject("DialogueCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Dialogue Panel
        var panelGO = CreateUIPanel(canvasGO.transform, "DialoguePanel",
            new Vector2(0, 0), new Vector2(0.05f, 0), new Vector2(0.95f, 0.3f));
        var panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

        // Speaker Name
        var speakerGO = CreateUIElement(panelGO.transform, "SpeakerName",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0.4f, 1),
            new Vector2(20, -10), new Vector2(300, -10));
        var speakerText = speakerGO.AddComponent<TextMeshProUGUI>();
        speakerText.fontSize = 28;
        speakerText.fontStyle = FontStyles.Bold;
        speakerText.color = Color.yellow;

        // Dialogue Text
        var dialogueTextGO = CreateUIElement(panelGO.transform, "DialogueText",
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1),
            new Vector2(20, 15), new Vector2(-20, -45));
        var dialogueText = dialogueTextGO.AddComponent<TextMeshProUGUI>();
        dialogueText.fontSize = 22;
        dialogueText.color = Color.white;
        dialogueText.maxVisibleCharacters = 0;

        // Advance Indicator
        var advanceGO = CreateUIElement(panelGO.transform, "AdvanceIndicator",
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0),
            new Vector2(-60, 10), new Vector2(-10, 40));
        var advanceText = advanceGO.AddComponent<TextMeshProUGUI>();
        advanceText.text = "▼";
        advanceText.fontSize = 24;
        advanceText.color = Color.white;
        advanceText.alignment = TextAlignmentOptions.Center;
        advanceGO.SetActive(false);

        // Typing Indicator
        var typingGO = CreateUIElement(panelGO.transform, "TypingIndicator",
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0),
            new Vector2(-60, 10), new Vector2(-10, 40));
        var typingText = typingGO.AddComponent<TextMeshProUGUI>();
        typingText.text = "...";
        typingText.fontSize = 24;
        typingText.color = new Color(1, 1, 1, 0.5f);
        typingText.alignment = TextAlignmentOptions.Center;
        typingGO.SetActive(false);

        // Options Panel
        var optionsPanelGO = CreateUIPanel(canvasGO.transform, "OptionsPanel",
            new Vector2(0, 0.3f), new Vector2(0.3f, 0.32f), new Vector2(0.7f, 0.6f));

        // Option Buttons (4)
        var optionButtons = new DialogueOptionButton[4];
        for (int i = 0; i < 4; i++)
        {
            var btnGO = new GameObject($"OptionButton_{i}");
            btnGO.transform.SetParent(optionsPanelGO.transform, false);
            var btnRT = btnGO.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0, 1f - (i + 1) * 0.25f);
            btnRT.anchorMax = new Vector2(1, 1f - i * 0.25f);
            btnRT.offsetMin = new Vector2(5, 3);
            btnRT.offsetMax = new Vector2(-5, -3);

            var highlightGO = new GameObject("Highlight");
            highlightGO.transform.SetParent(btnGO.transform, false);
            var highlightRT = highlightGO.AddComponent<RectTransform>();
            highlightRT.anchorMin = Vector2.zero;
            highlightRT.anchorMax = Vector2.one;
            highlightRT.offsetMin = Vector2.zero;
            highlightRT.offsetMax = Vector2.zero;
            var highlightImage = highlightGO.AddComponent<Image>();
            highlightImage.color = new Color(0.3f, 0.5f, 0.8f, 0.6f);
            highlightImage.enabled = false;

            var labelGO2 = new GameObject("Label");
            labelGO2.transform.SetParent(btnGO.transform, false);
            var labelRT = labelGO2.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(10, 0);
            labelRT.offsetMax = new Vector2(-10, 0);
            var labelTMP = labelGO2.AddComponent<TextMeshProUGUI>();
            labelTMP.fontSize = 20;
            labelTMP.color = Color.white;
            labelTMP.alignment = TextAlignmentOptions.MidlineLeft;

            var optionBtn = btnGO.AddComponent<DialogueOptionButton>();
            var btnType = typeof(DialogueOptionButton);
            var btnFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            btnType.GetField("labelText", btnFlags)?.SetValue(optionBtn, labelTMP);
            btnType.GetField("highlightImage", btnFlags)?.SetValue(optionBtn, highlightImage);

            optionButtons[i] = optionBtn;
            btnGO.SetActive(false);
        }

        // Add Dialogue_UI component
        dialogueUI = canvasGO.AddComponent<Dialogue_UI>();
        var duiType = typeof(Dialogue_UI);
        var duiFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        duiType.GetField("dialogueCanvas", duiFlags)?.SetValue(dialogueUI, canvas);
        duiType.GetField("speakerNameText", duiFlags)?.SetValue(dialogueUI, speakerText);
        duiType.GetField("dialogueText", duiFlags)?.SetValue(dialogueUI, dialogueText);
        duiType.GetField("advanceIndicator", duiFlags)?.SetValue(dialogueUI, advanceGO);
        duiType.GetField("typingIndicator", duiFlags)?.SetValue(dialogueUI, typingGO);
        duiType.GetField("optionButtons", duiFlags)?.SetValue(dialogueUI, optionButtons);

        canvas.enabled = false;
        return canvasGO;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TRANSITION UI CANVAS (fullscreen overlay for "RETO ACEPTADO")
    // ═══════════════════════════════════════════════════════════════════════════

    private static GameObject CreateTransitionCanvas(out Combat_Transition_UI transitionUI)
    {
        var canvasGO = new GameObject("TransitionCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);

        // Background overlay
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bgRT = bgGO.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        var bgImage = bgGO.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.85f);

        // Challenge text
        var textGO = new GameObject("ChallengeText");
        textGO.transform.SetParent(canvasGO.transform, false);
        var textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0.1f, 0.3f);
        textRT.anchorMax = new Vector2(0.9f, 0.7f);
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        var challengeText = textGO.AddComponent<TextMeshProUGUI>();
        challengeText.text = "RETO ACEPTADO";
        challengeText.fontSize = 96;
        challengeText.fontStyle = FontStyles.Bold;
        challengeText.color = Color.yellow;
        challengeText.alignment = TextAlignmentOptions.Center;
        challengeText.textWrappingMode = TextWrappingModes.NoWrap;

        // Add Combat_Transition_UI component
        transitionUI = canvasGO.AddComponent<Combat_Transition_UI>();
        var ctuType = typeof(Combat_Transition_UI);
        var ctuFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        ctuType.GetField("_overlayCanvas", ctuFlags)?.SetValue(transitionUI, canvas);
        ctuType.GetField("_challengeText", ctuFlags)?.SetValue(transitionUI, challengeText);

        canvas.enabled = false;
        return canvasGO;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FORCED ENCOUNTER PANEL (single "Aceptar combate" button)
    // ═══════════════════════════════════════════════════════════════════════════

    private static void CreateForcedEncounterPanel(Transform canvasParent,
        out GameObject panelGO, out Button acceptButton)
    {
        panelGO = new GameObject("ForcedEncounterPanel");
        panelGO.transform.SetParent(canvasParent, false);
        var panelRT = panelGO.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.25f, 0.4f);
        panelRT.anchorMax = new Vector2(0.75f, 0.65f);
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        var panelBg = panelGO.AddComponent<Image>();
        panelBg.color = new Color(0.15f, 0.05f, 0.05f, 0.95f);

        // Title text
        var titleGO = new GameObject("Title");
        titleGO.transform.SetParent(panelGO.transform, false);
        var titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 0.5f);
        titleRT.anchorMax = new Vector2(1, 1);
        titleRT.offsetMin = new Vector2(10, 5);
        titleRT.offsetMax = new Vector2(-10, -5);
        var titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text = "¡ENCUENTRO ALEATORIO!";
        titleText.fontSize = 32;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = Color.red;
        titleText.alignment = TextAlignmentOptions.Center;

        // Accept button
        var btnGO = new GameObject("AcceptButton");
        btnGO.transform.SetParent(panelGO.transform, false);
        var btnRT = btnGO.AddComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.25f, 0.05f);
        btnRT.anchorMax = new Vector2(0.75f, 0.45f);
        btnRT.offsetMin = Vector2.zero;
        btnRT.offsetMax = Vector2.zero;

        var btnImage = btnGO.AddComponent<Image>();
        btnImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        acceptButton = btnGO.AddComponent<Button>();
        acceptButton.targetGraphic = btnImage;

        var btnLabelGO = new GameObject("ButtonLabel");
        btnLabelGO.transform.SetParent(btnGO.transform, false);
        var btnLabelRT = btnLabelGO.AddComponent<RectTransform>();
        btnLabelRT.anchorMin = Vector2.zero;
        btnLabelRT.anchorMax = Vector2.one;
        btnLabelRT.offsetMin = Vector2.zero;
        btnLabelRT.offsetMax = Vector2.zero;
        var btnLabel = btnLabelGO.AddComponent<TextMeshProUGUI>();
        btnLabel.text = "¡Aceptar combate!";
        btnLabel.fontSize = 24;
        btnLabel.fontStyle = FontStyles.Bold;
        btnLabel.color = Color.white;
        btnLabel.alignment = TextAlignmentOptions.Center;

        panelGO.SetActive(false);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // WIRING HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    private static void WireDialogueManager(Dialogue_Manager dm, Typewriter_Effect tw,
        Dialogue_UI dialogueUI, PlayerInput playerInput)
    {
        var dmType = typeof(Dialogue_Manager);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        dmType.GetField("dialogueUIComponent", flags)?.SetValue(dm, dialogueUI);
        dmType.GetField("typewriterEffect", flags)?.SetValue(dm, tw);
        dmType.GetField("playerInput", flags)?.SetValue(dm, playerInput);
    }

    private static void WireCombatSystem(Combat_System cs, Combat_Transition_UI transitionUI,
        PlayerInput playerInput, GameObject forcedPanel, Button forcedButton,
        DialogueData forcedDialogueData)
    {
        var csType = typeof(Combat_System);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        csType.GetField("_transitionUI", flags)?.SetValue(cs, transitionUI);
        csType.GetField("_playerInput", flags)?.SetValue(cs, playerInput);
        csType.GetField("_forcedEncounterPanel", flags)?.SetValue(cs, forcedPanel);
        csType.GetField("_forcedAcceptButton", flags)?.SetValue(cs, forcedButton);
        csType.GetField("_forcedEncounterDialogueData", flags)?.SetValue(cs, forcedDialogueData);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NPC CREATION
    // ═══════════════════════════════════════════════════════════════════════════

    private static void CreateFlipCombatNPC(string name, Vector3 position, Color color,
        DialogueData dialogueData, string combatSceneName, string displayName)
    {
        var npc = new GameObject(name);
        npc.transform.position = position;

        var sr = npc.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = color;
        sr.sortingOrder = 3;
        npc.transform.localScale = new Vector3(1f, 1.5f, 1f);

        var collider = npc.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
        collider.isTrigger = true;

        var combatNPC = npc.AddComponent<FlipCombat_NPC>();
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        typeof(NPC_Interactable).GetField("dialogueData", flags)?.SetValue(combatNPC, dialogueData);
        typeof(FlipCombat_NPC).GetField("_combatSceneName", flags)?.SetValue(combatNPC, combatSceneName);
        typeof(FlipCombat_NPC).GetField("_npcDisplayName", flags)?.SetValue(combatNPC, displayName);

        // Label
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(npc.transform);
        labelGO.transform.localPosition = new Vector3(0, 1.0f, 0);
        var label = labelGO.AddComponent<TextMesh>();
        label.text = displayName;
        label.fontSize = 24;
        label.characterSize = 0.12f;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.color = Color.white;
    }

    private static GameObject CreateSpawnPoint(string name, Vector3 position)
    {
        var sp = new GameObject(name);
        sp.transform.position = position;
        return sp;
    }

    private static void CreateTriggerZoneEncounter(string name, Vector3 position, Color color,
        DialogueData forcedDialogueData, string combatSceneName, string displayName)
    {
        var npc = new GameObject(name);
        npc.transform.position = position;

        // SpriteRenderer — the script will hide it on Start()
        var sr = npc.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = color;
        sr.sortingOrder = 3;
        npc.transform.localScale = new Vector3(1f, 1.5f, 1f);

        // The TriggerZone_Encounter component (uses distance check, no collider needed)
        var encounter = npc.AddComponent<TriggerZone_Encounter>();
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(TriggerZone_Encounter).GetField("_combatSceneName", flags)?.SetValue(encounter, combatSceneName);
        typeof(TriggerZone_Encounter).GetField("_npcDisplayName", flags)?.SetValue(encounter, displayName);
        typeof(TriggerZone_Encounter).GetField("_forcedDialogueData", flags)?.SetValue(encounter, forcedDialogueData);
        typeof(TriggerZone_Encounter).GetField("_detectionRadius", flags)?.SetValue(encounter, 3f);

        // Label
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(npc.transform);
        labelGO.transform.localPosition = new Vector3(0, 1.0f, 0);
        var label = labelGO.AddComponent<TextMesh>();
        label.text = displayName;
        label.fontSize = 24;
        label.characterSize = 0.12f;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.color = Color.white;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ENCOUNTER MANAGER & PREFAB
    // ═══════════════════════════════════════════════════════════════════════════

    private static EncounterConfig CreateOrLoadEncounterConfig()
    {
        var existing = AssetDatabase.LoadAssetAtPath<EncounterConfig>(EncounterConfigPath);
        if (existing != null) return existing;

        var config = ScriptableObject.CreateInstance<EncounterConfig>();
        config.CheckInterval = 8f;
        config.MinTimeBetweenEncounters = 20f;
        config.EncounterProbability = 0.3f;
        config.SpawnRadius = 12f;
        config.SpawnDistanceFromPlayer = 3f;

        AssetDatabase.CreateAsset(config, EncounterConfigPath);
        AssetDatabase.SaveAssets();
        return config;
    }

    private static RandomEncounter_NPC CreateOrLoadEncounterNPCPrefab()
    {
        var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EncounterNPCPrefabPath);
        if (existingPrefab != null)
        {
            var comp = existingPrefab.GetComponent<RandomEncounter_NPC>();
            if (comp != null) return comp;
        }

        var tempGO = new GameObject("RandomEncounter_NPC");
        var sr = tempGO.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = new Color(1f, 0.2f, 0.2f);
        sr.sortingOrder = 4;
        tempGO.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

        var col = tempGO.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        col.isTrigger = true;

        var encounterNPC = tempGO.AddComponent<RandomEncounter_NPC>();
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(RandomEncounter_NPC).GetField("_combatSceneName", flags)?
            .SetValue(encounterNPC, "CombatScene");

        var prefab = PrefabUtility.SaveAsPrefabAsset(tempGO, EncounterNPCPrefabPath);
        Object.DestroyImmediate(tempGO);

        return prefab.GetComponent<RandomEncounter_NPC>();
    }

    private static void CreateEncounterManager(EncounterConfig config,
        RandomEncounter_NPC prefab, Transform[] spawnPoints)
    {
        var go = new GameObject("Random_Encounter_Manager");
        var manager = go.AddComponent<Random_Encounter_Manager>();
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        typeof(Random_Encounter_Manager).GetField("_encounterConfig", flags)?
            .SetValue(manager, config);
        typeof(Random_Encounter_Manager).GetField("_encounterPrefab", flags)?
            .SetValue(manager, prefab);
        typeof(Random_Encounter_Manager).GetField("_spawnPoints", flags)?
            .SetValue(manager, spawnPoints);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DIALOGUE DATA FOR COMBAT NPCs
    // ═══════════════════════════════════════════════════════════════════════════

    private static DialogueData CreateCombatDialogueData(string assetName, string speaker)
    {
        string path = $"{DialogueDataFolder}/{assetName}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<DialogueData>(path);
        if (existing != null) return existing;

        var data = ScriptableObject.CreateInstance<DialogueData>();
        var lines = new List<DialogueLine>
        {
            // Line 0: Challenge with options
            new DialogueLine("¡Te reto a una partida de Flipit!", new List<DialogueOption>
            {
                new DialogueOption("¡Acepto!", "", 1),
                new DialogueOption("No, gracias.", "", 2)
            }),
            // Line 1: Accept marker (intercepted by CombatDialogue_Handler)
            new DialogueLine("[COMBAT_ACCEPT]", null),
            // Line 2: Reject message shown to player
            new DialogueLine("Hasta luego.", null)
        };

        var type = typeof(DialogueData);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        type.GetField("speakerName", flags)?.SetValue(data, speaker);
        type.GetField("lines", flags)?.SetValue(data, lines);

        AssetDatabase.CreateAsset(data, path);
        return data;
    }

    private static DialogueData CreateForcedEncounterDialogueData()
    {
        string path = $"{DialogueDataFolder}/ForcedEncounter_Dialogue.asset";
        var existing = AssetDatabase.LoadAssetAtPath<DialogueData>(path);
        if (existing != null) return existing;

        var data = ScriptableObject.CreateInstance<DialogueData>();
        var lines = new List<DialogueLine>
        {
            new DialogueLine("¡No puedes escapar! ¡Te reto a una partida de Flipit!", new List<DialogueOption>
            {
                new DialogueOption("¡Acepto!", "", 1)
            }),
            new DialogueLine("[COMBAT_ACCEPT]", null)
        };

        var type = typeof(DialogueData);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        type.GetField("speakerName", flags)?.SetValue(data, "???");
        type.GetField("lines", flags)?.SetValue(data, lines);

        AssetDatabase.CreateAsset(data, path);
        return data;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // UTILITY HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    private static void CreateBoundaryWall(string name, Vector3 position, Vector2 size)
    {
        var wall = new GameObject(name);
        wall.transform.position = position;
        var collider = wall.AddComponent<BoxCollider2D>();
        collider.size = size;

        var sr = wall.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = new Color(0.4f, 0.3f, 0.2f, 0.8f);
        sr.sortingOrder = 1;
        wall.transform.localScale = new Vector3(size.x, size.y, 1f);
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in scenes)
            if (s.path == scenePath) return;
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static Sprite CreateSquareSprite()
    {
        var tex = Texture2D.whiteTexture;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f), 100f);
    }

    private static GameObject CreateUIPanel(Transform parent, string name,
        Vector2 pivot, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = pivot;
        return go;
    }

    private static GameObject CreateUIElement(Transform parent, string name,
        Vector2 pivot, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        rt.pivot = pivot;
        return go;
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
