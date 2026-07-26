using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Flipit.Dialogue;

/// <summary>
/// Editor utility that creates a complete dialogue system demo scene with:
/// - Ground plane
/// - Player with movement, PlayerInput, and Player_Interactor
/// - 4 NPCs with unique DialogueData assets
/// - Full Dialogue UI Canvas with TextMeshPro
/// - Dialogue_Manager wired to all references
/// - Camera set for top-down 2D view
///
/// Usage: Unity menu → Flipit → Build Dialogue Demo Scene
/// </summary>
public static class DialogueSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/DialogueDemoScene.unity";
    private const string DialogueDataFolder = "Assets/DialogueData";

    [MenuItem("Flipit/Build Dialogue Demo Scene")]
    public static void BuildScene()
    {
        // Create a new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Ensure folders exist
        EnsureFolder(DialogueDataFolder);

        // ─── Create Dialogue Data Assets ─────────────────────────────────────
        var npc1Data = CreateDialogueData("NPC_Tienda", "Vendedor", new List<DialogueLine>
        {
            new DialogueLine("Hola! Bienvenido a mi tienda.", null),
            new DialogueLine("Quieres comprar chicharrines?", null)
        });

        var npc2Data = CreateDialogueData("NPC_Flipazos", "Compadre", new List<DialogueLine>
        {
            new DialogueLine("Que quiere compa?", null),
            new DialogueLine("Unos flipazos?", new List<DialogueOption>
            {
                new DialogueOption("Si, dame unos!", "", 2),
                new DialogueOption("No gracias, estoy bien.", "", 3)
            }),
            new DialogueLine("Orale! Aqui tienes tus flipazos. Provecho!", null),
            new DialogueLine("Va pues, cuando quieras regresas compa.", null)
        });

        var npc3Data = CreateDialogueData("NPC_Guardia1", "Guardia", new List<DialogueLine>
        {
            new DialogueLine("Bienvenido a la ciudad, viajero.", null),
            new DialogueLine("Esperamos que disfrutes tu estancia aqui.", null)
        });

        var npc4Data = CreateDialogueData("NPC_Guardia2", "Anciano", new List<DialogueLine>
        {
            new DialogueLine("Ah, un rostro nuevo por aqui!", null),
            new DialogueLine("Esta ciudad es tranquila, te va a gustar.", null),
            new DialogueLine("Si necesitas algo, no dudes en preguntar.", null)
        });

        // ─── Setup Camera (2D top-down) ──────────────────────────────────────
        var mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(0, 0, -10);
            mainCam.orthographic = true;
            mainCam.orthographicSize = 7;
            mainCam.backgroundColor = new Color(0.15f, 0.15f, 0.2f);
        }

        // ─── Ground Plane (sprite-based) ─────────────────────────────────────
        var ground = new GameObject("Ground");
        var groundSR = ground.AddComponent<SpriteRenderer>();
        groundSR.sprite = CreateSquareSprite();
        groundSR.color = new Color(0.3f, 0.5f, 0.3f);
        ground.transform.localScale = new Vector3(20f, 20f, 1f);
        ground.transform.position = Vector3.zero;

        // ─── Player ──────────────────────────────────────────────────────────
        var player = new GameObject("Player");
        player.transform.position = Vector3.zero;

        var playerSR = player.AddComponent<SpriteRenderer>();
        playerSR.sprite = CreateSquareSprite();
        playerSR.color = Color.cyan;
        player.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

        var playerRb = player.AddComponent<Rigidbody2D>();
        playerRb.gravityScale = 0;
        playerRb.freezeRotation = true;

        var playerCollider = player.AddComponent<BoxCollider2D>();
        playerCollider.size = Vector2.one;

        player.AddComponent<TopDownPlayerMovement>();

        var playerInteractor = player.AddComponent<Player_Interactor>();
        // Set interaction radius via reflection since property uses Mathf.Max
        var radiusField = typeof(Player_Interactor).GetField("interactionRadius",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        radiusField.SetValue(playerInteractor, 2.5f);

        // Add PlayerInput component
        var playerInput = player.AddComponent<PlayerInput>();
        var inputActionsAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
        if (inputActionsAsset != null)
        {
            playerInput.actions = inputActionsAsset;
            playerInput.defaultActionMap = "Player";
            playerInput.notificationBehavior = PlayerNotifications.SendMessages;
        }
        else
        {
            Debug.LogWarning("[DialogueSceneBuilder] Could not find InputSystem_Actions.inputactions at expected path.");
        }

        // ─── NPCs ────────────────────────────────────────────────────────────
        CreateNPC("NPC_Tienda", new Vector3(-4, 3, 0), Color.yellow, npc1Data);
        CreateNPC("NPC_Flipazos", new Vector3(4, 3, 0), Color.magenta, npc2Data);
        CreateNPC("NPC_Guardia1", new Vector3(-4, -3, 0), new Color(0.6f, 0.4f, 0.2f), npc3Data);
        CreateNPC("NPC_Guardia2", new Vector3(4, -3, 0), new Color(0.8f, 0.6f, 0.3f), npc4Data);

        // ─── Dialogue Manager ────────────────────────────────────────────────
        var managerGO = new GameObject("Dialogue_Manager");
        var dialogueManager = managerGO.AddComponent<Dialogue_Manager>();
        var typewriterEffect = managerGO.AddComponent<Typewriter_Effect>();

        // ─── Dialogue UI Canvas ──────────────────────────────────────────────
        var canvasGO = CreateDialogueCanvas(out var dialogueUI);

        // ─── Wire Dialogue_Manager references ────────────────────────────────
        var dmType = typeof(Dialogue_Manager);
        var bindFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        var uiField = dmType.GetField("dialogueUIComponent", bindFlags);
        uiField.SetValue(dialogueManager, dialogueUI);

        var twField = dmType.GetField("typewriterEffect", bindFlags);
        twField.SetValue(dialogueManager, typewriterEffect);

        var piField = dmType.GetField("playerInput", bindFlags);
        piField.SetValue(dialogueManager, playerInput);

        // ─── Interaction Prompt (floating text above nearest NPC) ─────────────
        // Add a simple helper script for visual feedback
        var promptGO = new GameObject("InteractionPrompt");
        promptGO.transform.SetParent(player.transform);
        promptGO.transform.localPosition = new Vector3(0, 1.2f, 0);
        var promptText = promptGO.AddComponent<TextMesh>();
        promptText.text = "[E] Hablar";
        promptText.fontSize = 24;
        promptText.characterSize = 0.15f;
        promptText.anchor = TextAnchor.MiddleCenter;
        promptText.alignment = TextAlignment.Center;
        promptText.color = Color.white;
        var promptHelper = promptGO.AddComponent<InteractionPromptHelper>();

        // ─── Save Scene ──────────────────────────────────────────────────────
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[DialogueSceneBuilder] Demo scene created at: {ScenePath}");
        Debug.Log("[DialogueSceneBuilder] Controls: WASD to move, E to interact, Enter/Space to advance/skip, Escape to cancel, W/S to navigate choices.");
    }

    // ─── Helper Methods ──────────────────────────────────────────────────────

    private static DialogueData CreateDialogueData(string assetName, string speaker, List<DialogueLine> lines)
    {
        string path = $"{DialogueDataFolder}/{assetName}.asset";

        var data = ScriptableObject.CreateInstance<DialogueData>();

        // Use reflection to set private fields
        var type = typeof(DialogueData);
        var bindFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        var speakerField = type.GetField("speakerName", bindFlags);
        speakerField.SetValue(data, speaker);

        var linesField = type.GetField("lines", bindFlags);
        linesField.SetValue(data, lines);

        AssetDatabase.CreateAsset(data, path);
        return data;
    }

    private static GameObject CreateNPC(string name, Vector3 position, Color color, DialogueData data)
    {
        var npc = new GameObject(name);
        npc.transform.position = position;

        var sr = npc.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = color;
        npc.transform.localScale = new Vector3(1f, 1.5f, 1f);

        var collider = npc.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;

        var interactable = npc.AddComponent<NPC_Interactable>();

        // Set dialogue data via reflection
        var field = typeof(NPC_Interactable).GetField("dialogueData",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.SetValue(interactable, data);

        // Add a label above the NPC
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(npc.transform);
        labelGO.transform.localPosition = new Vector3(0, 0.9f, 0);
        var label = labelGO.AddComponent<TextMesh>();
        label.text = name.Replace("NPC_", "");
        label.fontSize = 20;
        label.characterSize = 0.12f;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.color = Color.white;

        return npc;
    }

    private static GameObject CreateDialogueCanvas(out Dialogue_UI dialogueUI)
    {
        // Canvas
        var canvasGO = new GameObject("DialogueCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Dialogue Panel (bottom of screen)
        var panelGO = CreateUIPanel(canvasGO.transform, "DialoguePanel",
            new Vector2(0, 0), new Vector2(0.05f, 0), new Vector2(0.95f, 0.3f));
        var panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

        // Speaker Name
        var speakerGO = CreateUIElement(panelGO.transform, "SpeakerName",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0.4f, 1),
            new Vector2(20, -10), new Vector2(300, -10));
        var speakerText = speakerGO.AddComponent<TextMeshProUGUI>();
        speakerText.text = "";
        speakerText.fontSize = 28;
        speakerText.fontStyle = FontStyles.Bold;
        speakerText.color = Color.yellow;

        // Dialogue Text
        var dialogueTextGO = CreateUIElement(panelGO.transform, "DialogueText",
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1),
            new Vector2(20, 15), new Vector2(-20, -45));
        var dialogueText = dialogueTextGO.AddComponent<TextMeshProUGUI>();
        dialogueText.text = "";
        dialogueText.fontSize = 22;
        dialogueText.color = Color.white;
        dialogueText.maxVisibleCharacters = 0;

        // Advance Indicator (arrow at bottom right)
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

        // Options Panel (above dialogue panel)
        var optionsPanelGO = CreateUIPanel(canvasGO.transform, "OptionsPanel",
            new Vector2(0, 0.3f), new Vector2(0.3f, 0.32f), new Vector2(0.7f, 0.6f));

        // Create 4 option buttons
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

            // Highlight background
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

            // Label
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(btnGO.transform, false);
            var labelRT = labelGO.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(10, 0);
            labelRT.offsetMax = new Vector2(-10, 0);
            var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
            labelTMP.text = "";
            labelTMP.fontSize = 20;
            labelTMP.color = Color.white;
            labelTMP.alignment = TextAlignmentOptions.MidlineLeft;

            // Add DialogueOptionButton component
            var optionBtn = btnGO.AddComponent<DialogueOptionButton>();
            var btnType = typeof(DialogueOptionButton);
            var bindFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            btnType.GetField("labelText", bindFlags).SetValue(optionBtn, labelTMP);
            btnType.GetField("highlightImage", bindFlags).SetValue(optionBtn, highlightImage);

            optionButtons[i] = optionBtn;
            btnGO.SetActive(false);
        }

        // Add Dialogue_UI component to the canvas
        dialogueUI = canvasGO.AddComponent<Dialogue_UI>();
        var duiType = typeof(Dialogue_UI);
        var duiFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        duiType.GetField("dialogueCanvas", duiFlags).SetValue(dialogueUI, canvas);
        duiType.GetField("speakerNameText", duiFlags).SetValue(dialogueUI, speakerText);
        duiType.GetField("dialogueText", duiFlags).SetValue(dialogueUI, dialogueText);
        duiType.GetField("advanceIndicator", duiFlags).SetValue(dialogueUI, advanceGO);
        duiType.GetField("typingIndicator", duiFlags).SetValue(dialogueUI, typingGO);
        duiType.GetField("optionButtons", duiFlags).SetValue(dialogueUI, optionButtons);

        // Start with canvas hidden
        canvas.enabled = false;

        return canvasGO;
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

    private static Sprite CreateSquareSprite()
    {
        // Use Unity's built-in white texture to create a simple square sprite
        var tex = Texture2D.whiteTexture;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f), 100f);
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
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}
