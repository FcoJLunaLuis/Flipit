using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Flipit.Combat;
using Flipit.Dialogue;

public static class CombatSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/CombatScene.unity";
    private const string DialogueDataFolder = "Assets/DialogueData";
    private const string PrefabsFolder = "Assets/Prefabs";

    [MenuItem("Flipit/Build Combat Scene")]
    public static void BuildScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        EnsureFolder(DialogueDataFolder);
        EnsureFolder(PrefabsFolder);

        // Camera
        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0, 0, -10);
            cam.orthographic = true;
            cam.orthographicSize = 12;
            cam.backgroundColor = new Color(0.12f, 0.12f, 0.18f);
        }

        // Ground
        var ground = new GameObject("Ground");
        var gsr = ground.AddComponent<SpriteRenderer>();
        gsr.sprite = LoadSprite("Assets/Sprites/Ground.png") ?? MakeSprite();
        gsr.color = Color.white;
        gsr.sortingOrder = -10;
        ground.transform.localScale = new Vector3(15f, 12f, 1f);

        // Walls
        MakeWall("Wall_Top", new Vector3(0,12,0), new Vector2(30,1));
        MakeWall("Wall_Bottom", new Vector3(0,-12,0), new Vector2(30,1));
        MakeWall("Wall_Left", new Vector3(-15,0,0), new Vector2(1,24));
        MakeWall("Wall_Right", new Vector3(15,0,0), new Vector2(1,24));

        // Player
        var player = MakePlayer();
        var playerInput = player.GetComponent<PlayerInput>();

        // Dialogue System
        var dialogueCanvas = MakeDialogueCanvas(out var dialogueUI);
        var dmGO = new GameObject("Dialogue_Manager");
        var dm = dmGO.AddComponent<Dialogue_Manager>();
        var tw = dmGO.AddComponent<Typewriter_Effect>();
        WireDM(dm, tw, dialogueUI, playerInput);

        // Combat System
        var csGO = new GameObject("Combat_System");
        var cs = csGO.AddComponent<Combat_System>();
        csGO.AddComponent<CombatDialogue_Handler>();

        // Transition overlay
        MakeTransitionCanvas(out var transitionUI);

        // Forced panel
        MakeForcedPanel(dialogueCanvas.transform, out var forcedPanel, out var forcedBtn);

        // Forced dialogue data
        var forcedData = MakeForcedDialogueData();

        // Wire combat system
        var csFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(Combat_System).GetField("_transitionUI", csFlags)?.SetValue(cs, transitionUI);
        typeof(Combat_System).GetField("_playerInput", csFlags)?.SetValue(cs, playerInput);
        typeof(Combat_System).GetField("_forcedEncounterPanel", csFlags)?.SetValue(cs, forcedPanel);
        typeof(Combat_System).GetField("_forcedAcceptButton", csFlags)?.SetValue(cs, forcedBtn);
        typeof(Combat_System).GetField("_forcedEncounterDialogueData", csFlags)?.SetValue(cs, forcedData);

        // NPCs
        var d1 = MakeCombatData("CombatNPC_Retador", "Retador");
        var d2 = MakeCombatData("CombatNPC_Luchador", "Luchador");
        var d4 = MakeCombatData("CombatNPC_Novato", "Novato");

        MakeNPC("FlipCombat_NPC_1", new Vector3(-8,6,0), Color.red, d1, "CombatScene", "Retador");
        MakeNPC("FlipCombat_NPC_2", new Vector3(8,6,0), Color.blue, d2, "CombatScene", "Luchador");
        MakeNPC("FlipCombat_NPC_4", new Vector3(8,-6,0), new Color(1f,0.5f,0f), d4, "CombatScene", "Novato");

        // Campeón: Trigger Zone Encounter (invisible, appears on approach)
        MakeTriggerEncounter("Campeon_Encounter", new Vector3(-8,-6,0),
            new Color(0.8f,0.2f,0.8f), forcedData, "CombatScene", "Campeón");

        // Spawn Points
        var sp1 = new GameObject("SpawnPoint_1"); sp1.transform.position = new Vector3(-5,0,0);
        var sp2 = new GameObject("SpawnPoint_2"); sp2.transform.position = new Vector3(5,0,0);
        var sp3 = new GameObject("SpawnPoint_3"); sp3.transform.position = new Vector3(0,6,0);

        // Interaction prompt
        var prompt = new GameObject("InteractionPrompt");
        prompt.transform.SetParent(player.transform);
        prompt.transform.localPosition = new Vector3(0,1.2f,0);
        var pt = prompt.AddComponent<TextMesh>();
        pt.text = "[E] Retar"; pt.fontSize = 24; pt.characterSize = 0.15f;
        pt.anchor = TextAnchor.MiddleCenter; pt.color = Color.white;
        prompt.AddComponent<InteractionPromptHelper>();

        // Save
        EnsureFolder("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        AddToBuildSettings(ScenePath);
        Debug.Log("[CombatSceneBuilder] Done! WASD move, E interact.");
    }

    static GameObject MakePlayer()
    {
        var p = new GameObject("Player"); p.tag = "Player";
        p.transform.position = new Vector3(0,-2,0);
        var sr = p.AddComponent<SpriteRenderer>(); sr.sprite = LoadSprite("Assets/Sprites/Player.png"); sr.color = Color.white; sr.sortingOrder = 5;
        p.transform.localScale = new Vector3(1.5f,1.5f,1f);
        var rb = p.AddComponent<Rigidbody2D>(); rb.gravityScale = 0; rb.freezeRotation = true;
        p.AddComponent<BoxCollider2D>().size = Vector2.one;
        p.AddComponent<TopDownPlayerMovement>();
        var interactor = p.AddComponent<Player_Interactor>();
        typeof(Player_Interactor).GetField("interactionRadius",
            System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)?.SetValue(interactor, 2.5f);
        var pi = p.AddComponent<PlayerInput>();
        var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
        if (asset != null) { pi.actions = asset; pi.defaultActionMap = "Player"; pi.notificationBehavior = PlayerNotifications.SendMessages; }
        return p;
    }

    static void MakeNPC(string name, Vector3 pos, Color col, DialogueData data, string scene, string display)
    {
        var n = new GameObject(name); n.transform.position = pos;
        var sprite = LoadSprite($"Assets/Sprites/NPC_{display}.png") ?? MakeSprite();
        var sr = n.AddComponent<SpriteRenderer>(); sr.sprite = sprite; sr.color = Color.white; sr.sortingOrder = 3;
        n.transform.localScale = new Vector3(1.5f,1.5f,1f);
        var c = n.AddComponent<BoxCollider2D>(); c.size = Vector2.one; c.isTrigger = false;
        var npc = n.AddComponent<FlipCombat_NPC>();
        var f = System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
        typeof(NPC_Interactable).GetField("dialogueData", f)?.SetValue(npc, data);
        typeof(FlipCombat_NPC).GetField("_combatSceneName", f)?.SetValue(npc, scene);
        typeof(FlipCombat_NPC).GetField("_npcDisplayName", f)?.SetValue(npc, display);
        var lbl = new GameObject("Label"); lbl.transform.SetParent(n.transform); lbl.transform.localPosition = new Vector3(0,1f,0);
        var t = lbl.AddComponent<TextMesh>(); t.text = display; t.fontSize = 24; t.characterSize = 0.12f;
        t.anchor = TextAnchor.MiddleCenter; t.color = Color.white;
    }

    static void MakeTriggerEncounter(string name, Vector3 pos, Color col, DialogueData data, string scene, string display)
    {
        var n = new GameObject(name); n.transform.position = pos;
        var sprite = LoadSprite("Assets/Sprites/NPC_Campeon.png") ?? MakeSprite();
        var sr = n.AddComponent<SpriteRenderer>(); sr.sprite = sprite; sr.color = Color.white; sr.sortingOrder = 3;
        n.transform.localScale = new Vector3(1.5f,1.5f,1f);
        var enc = n.AddComponent<TriggerZone_Encounter>();
        var f = System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
        typeof(TriggerZone_Encounter).GetField("_combatSceneName", f)?.SetValue(enc, scene);
        typeof(TriggerZone_Encounter).GetField("_npcDisplayName", f)?.SetValue(enc, display);
        typeof(TriggerZone_Encounter).GetField("_forcedDialogueData", f)?.SetValue(enc, data);
        typeof(TriggerZone_Encounter).GetField("_detectionRadius", f)?.SetValue(enc, 3f);
        var lbl = new GameObject("Label"); lbl.transform.SetParent(n.transform); lbl.transform.localPosition = new Vector3(0,1f,0);
        var t = lbl.AddComponent<TextMesh>(); t.text = display; t.fontSize = 24; t.characterSize = 0.12f;
        t.anchor = TextAnchor.MiddleCenter; t.color = Color.white;
    }

    static DialogueData MakeCombatData(string asset, string speaker)
    {
        string path = $"{DialogueDataFolder}/{asset}.asset";
        var ex = AssetDatabase.LoadAssetAtPath<DialogueData>(path);
        if (ex != null) return ex;
        var d = ScriptableObject.CreateInstance<DialogueData>();
        var lines = new List<DialogueLine> {
            new DialogueLine("¡Te reto a una partida de Flipit!", new List<DialogueOption> {
                new DialogueOption("¡Acepto!", "", 1), new DialogueOption("No, gracias.", "", 2) }),
            new DialogueLine("[COMBAT_ACCEPT]", null),
            new DialogueLine("Hasta luego.", null) };
        var f = System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
        typeof(DialogueData).GetField("speakerName", f)?.SetValue(d, speaker);
        typeof(DialogueData).GetField("lines", f)?.SetValue(d, lines);
        AssetDatabase.CreateAsset(d, path); return d;
    }

    static DialogueData MakeForcedDialogueData()
    {
        string path = $"{DialogueDataFolder}/ForcedEncounter_Dialogue.asset";
        var ex = AssetDatabase.LoadAssetAtPath<DialogueData>(path);
        if (ex != null) return ex;
        var d = ScriptableObject.CreateInstance<DialogueData>();
        var lines = new List<DialogueLine> {
            new DialogueLine("¡No puedes escapar! ¡Te reto a una partida de Flipit!", new List<DialogueOption> {
                new DialogueOption("¡Acepto!", "", 1) }),
            new DialogueLine("[COMBAT_ACCEPT]", null) };
        var f = System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
        typeof(DialogueData).GetField("speakerName", f)?.SetValue(d, "???");
        typeof(DialogueData).GetField("lines", f)?.SetValue(d, lines);
        AssetDatabase.CreateAsset(d, path); return d;
    }

    static void MakeWall(string name, Vector3 pos, Vector2 size)
    {
        var w = new GameObject(name); w.transform.position = pos;
        w.AddComponent<BoxCollider2D>().size = size;
        var sr = w.AddComponent<SpriteRenderer>(); sr.sprite = MakeSprite();
        sr.color = new Color(0.4f,0.3f,0.2f,0.8f); sr.sortingOrder = 1;
        w.transform.localScale = new Vector3(size.x,size.y,1f);
    }

    static GameObject MakeDialogueCanvas(out Dialogue_UI dialogueUI)
    {
        var c = new GameObject("DialogueCanvas");
        var canvas = c.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 100;
        c.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        c.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920,1080);
        c.AddComponent<GraphicRaycaster>();
        var panel = MakeRT(c.transform,"DialoguePanel",new Vector2(0.05f,0.02f),new Vector2(0.95f,0.25f));
        panel.AddComponent<Image>().color = new Color(0.1f,0.1f,0.15f,0.9f);
        var speakerGO = MakeRT(panel.transform,"SpeakerName",new Vector2(0.02f,0.75f),new Vector2(0.4f,0.98f));
        var speakerTxt = speakerGO.AddComponent<TextMeshProUGUI>(); speakerTxt.fontSize=24; speakerTxt.fontStyle=FontStyles.Bold; speakerTxt.color=Color.yellow;
        var textGO = MakeRT(panel.transform,"DialogueText",new Vector2(0.02f,0.05f),new Vector2(0.95f,0.72f));
        var dTxt = textGO.AddComponent<TextMeshProUGUI>(); dTxt.fontSize=20; dTxt.color=Color.white; dTxt.maxVisibleCharacters=0;
        var advGO = MakeRT(panel.transform,"AdvanceIndicator",new Vector2(0.93f,0.02f),new Vector2(0.98f,0.15f));
        advGO.AddComponent<TextMeshProUGUI>().text="▼"; advGO.SetActive(false);
        var typGO = MakeRT(panel.transform,"TypingIndicator",new Vector2(0.93f,0.02f),new Vector2(0.98f,0.15f));
        typGO.AddComponent<TextMeshProUGUI>().text="..."; typGO.SetActive(false);
        var optPanel = MakeRT(c.transform,"OptionsPanel",new Vector2(0.25f,0.27f),new Vector2(0.75f,0.48f));
        var btns = new DialogueOptionButton[4];
        for (int i = 0; i < 4; i++)
        {
            var b = new GameObject($"Opt_{i}"); b.transform.SetParent(optPanel.transform, false);
            var rt = b.AddComponent<RectTransform>();
            float btnHeight = 0.20f;
            float gap = 0.05f;
            float top = 1f - i * (btnHeight + gap);
            rt.anchorMin = new Vector2(0.0f, top - btnHeight);
            rt.anchorMax = new Vector2(1.0f, top);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            // Button background - solid dark
            var btnBg = b.AddComponent<Image>();
            btnBg.color = new Color(0.08f, 0.08f, 0.14f, 1f);
            // Highlight overlay (shown when selected)
            var hlGO = new GameObject("HL"); hlGO.transform.SetParent(b.transform, false);
            var hlRT = hlGO.AddComponent<RectTransform>();
            hlRT.anchorMin = Vector2.zero; hlRT.anchorMax = Vector2.one;
            hlRT.offsetMin = Vector2.zero; hlRT.offsetMax = Vector2.zero;
            var hlImg = hlGO.AddComponent<Image>(); hlImg.color = new Color(0.1f,0.5f,1f,0.5f); hlImg.enabled = false;
            // Label text
            var lGO = new GameObject("Lbl"); lGO.transform.SetParent(b.transform, false);
            var lRT = lGO.AddComponent<RectTransform>();
            lRT.anchorMin = new Vector2(0.05f, 0f); lRT.anchorMax = new Vector2(0.95f, 1f);
            lRT.offsetMin = Vector2.zero; lRT.offsetMax = Vector2.zero;
            var lTxt = lGO.AddComponent<TextMeshProUGUI>(); lTxt.fontSize=22; lTxt.color=Color.white;
            lTxt.alignment = TextAlignmentOptions.MidlineLeft;
            var ob = b.AddComponent<DialogueOptionButton>();
            var bf = System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
            typeof(DialogueOptionButton).GetField("labelText",bf)?.SetValue(ob, lTxt);
            typeof(DialogueOptionButton).GetField("highlightImage",bf)?.SetValue(ob, hlImg);
            btns[i] = ob; b.SetActive(false);
        }
        dialogueUI = c.AddComponent<Dialogue_UI>();
        var df = System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
        typeof(Dialogue_UI).GetField("dialogueCanvas",df)?.SetValue(dialogueUI, canvas);
        typeof(Dialogue_UI).GetField("speakerNameText",df)?.SetValue(dialogueUI, speakerTxt);
        typeof(Dialogue_UI).GetField("dialogueText",df)?.SetValue(dialogueUI, dTxt);
        typeof(Dialogue_UI).GetField("advanceIndicator",df)?.SetValue(dialogueUI, advGO);
        typeof(Dialogue_UI).GetField("typingIndicator",df)?.SetValue(dialogueUI, typGO);
        typeof(Dialogue_UI).GetField("optionButtons",df)?.SetValue(dialogueUI, btns);
        canvas.enabled = false;
        return c;
    }

    static void MakeTransitionCanvas(out Combat_Transition_UI ui)
    {
        var c = new GameObject("TransitionCanvas");
        var canvas = c.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 999;
        c.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        c.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920,1080);
        c.AddComponent<GraphicRaycaster>();
        var bg = MakeRT(c.transform,"BG",Vector2.zero,Vector2.one);
        bg.AddComponent<Image>().color = new Color(0,0,0,0.9f);
        var txt = MakeRT(c.transform,"Text",new Vector2(0.1f,0.3f),new Vector2(0.9f,0.7f));
        var tmp = txt.AddComponent<TextMeshProUGUI>(); tmp.text="RETO ACEPTADO"; tmp.fontSize=72;
        tmp.fontStyle=FontStyles.Bold; tmp.color=Color.yellow; tmp.alignment=TextAlignmentOptions.Center;
        ui = c.AddComponent<Combat_Transition_UI>();
        var f = System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
        typeof(Combat_Transition_UI).GetField("_overlayCanvas",f)?.SetValue(ui, canvas);
        typeof(Combat_Transition_UI).GetField("_challengeText",f)?.SetValue(ui, tmp);
        canvas.enabled = false;
    }

    static void MakeForcedPanel(Transform parent, out GameObject panel, out Button btn)
    {
        panel = new GameObject("ForcedPanel"); panel.transform.SetParent(parent, false);
        var rt = panel.AddComponent<RectTransform>(); rt.anchorMin=new Vector2(0.25f,0.4f); rt.anchorMax=new Vector2(0.75f,0.65f);
        panel.AddComponent<Image>().color = new Color(0.15f,0.05f,0.05f,0.95f);
        var tGO = MakeRT(panel.transform,"Title",new Vector2(0,0.5f),Vector2.one);
        var tTxt = tGO.AddComponent<TextMeshProUGUI>(); tTxt.text="¡ENCUENTRO!"; tTxt.fontSize=32; tTxt.color=Color.red; tTxt.alignment=TextAlignmentOptions.Center;
        var bGO = new GameObject("Btn"); bGO.transform.SetParent(panel.transform, false);
        var brt = bGO.AddComponent<RectTransform>(); brt.anchorMin=new Vector2(0.25f,0.05f); brt.anchorMax=new Vector2(0.75f,0.45f);
        var bImg = bGO.AddComponent<Image>(); bImg.color = new Color(0.8f,0.2f,0.2f);
        btn = bGO.AddComponent<Button>(); btn.targetGraphic = bImg;
        var blGO = MakeRT(bGO.transform,"Lbl",Vector2.zero,Vector2.one);
        var bl = blGO.AddComponent<TextMeshProUGUI>(); bl.text="¡Aceptar!"; bl.fontSize=24; bl.color=Color.white; bl.alignment=TextAlignmentOptions.Center;
        panel.SetActive(false);
    }

    static void WireDM(Dialogue_Manager dm, Typewriter_Effect tw, Dialogue_UI ui, PlayerInput pi)
    {
        var f = System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
        typeof(Dialogue_Manager).GetField("dialogueUIComponent",f)?.SetValue(dm, ui);
        typeof(Dialogue_Manager).GetField("typewriterEffect",f)?.SetValue(dm, tw);
        typeof(Dialogue_Manager).GetField("playerInput",f)?.SetValue(dm, pi);
    }

    static GameObject MakeRT(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        var g = new GameObject(name); g.transform.SetParent(parent, false);
        var rt = g.AddComponent<RectTransform>(); rt.anchorMin=anchorMin; rt.anchorMax=anchorMax;
        rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero;
        return g;
    }

    static Sprite MakeSprite()
    {
        var t = Texture2D.whiteTexture;
        return Sprite.Create(t, new Rect(0,0,t.width,t.height), new Vector2(0.5f,0.5f), 100f);
    }

    static Sprite LoadSprite(string assetPath)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite != null) return sprite;
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (tex != null) return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 64f);
        return null;
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
