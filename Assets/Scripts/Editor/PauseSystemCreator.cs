using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Reflection;

/// <summary>
/// Editor script que crea toda la jerarquía del Sistema de Pausa en la escena.
/// Uso: Menú Unity → Tools → Crear Sistema de Pausa
/// Crea la UI fuera de Play Mode para que las referencias persistan.
/// </summary>
public class PauseSystemCreator : Editor
{
    [MenuItem("Tools/Crear Sistema de Pausa")]
    public static void CreatePauseSystem()
    {
        // --- Root Object ---
        var root = new GameObject("--- PAUSE SYSTEM ---");
        Undo.RegisterCreatedObjectUndo(root, "Crear Sistema de Pausa");

        // --- Managers ---
        var gameStateObj = new GameObject("GameStateManager");
        gameStateObj.transform.SetParent(root.transform);
        gameStateObj.AddComponent<GameStateManager>();

        var playerDataObj = new GameObject("MockPlayerDataProvider");
        playerDataObj.transform.SetParent(root.transform);
        playerDataObj.AddComponent<MockPlayerDataProvider>();

        var inputHandlerObj = new GameObject("PauseInputHandler");
        inputHandlerObj.transform.SetParent(root.transform);
        var inputHandler = inputHandlerObj.AddComponent<PauseInputHandler>();

        var pauseManagerObj = new GameObject("PauseManager");
        pauseManagerObj.transform.SetParent(root.transform);
        var pauseManager = pauseManagerObj.AddComponent<PauseManager>();

        var debugObj = new GameObject("DebugStateToggle");
        debugObj.transform.SetParent(root.transform);
        debugObj.AddComponent<DebugStateToggle>();

        // --- Canvas ---
        var canvasObj = new GameObject("PauseCanvas");
        canvasObj.transform.SetParent(root.transform);
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // =============================================
        // COMBAT PAUSE OVERLAY
        // =============================================
        var combatOverlay = CreatePanel(canvasObj.transform, "CombatPauseOverlay");

        var combatBg = new GameObject("Background");
        combatBg.transform.SetParent(combatOverlay.transform, false);
        var combatBgRect = combatBg.AddComponent<RectTransform>();
        StretchFull(combatBgRect);
        var combatBgImage = combatBg.AddComponent<Image>();
        combatBgImage.color = new Color(0, 0, 0, 0.7f);

        var combatTextObj = new GameObject("PauseText");
        combatTextObj.transform.SetParent(combatOverlay.transform, false);
        var combatTextRect = combatTextObj.AddComponent<RectTransform>();
        combatTextRect.anchoredPosition = Vector2.zero;
        combatTextRect.sizeDelta = new Vector2(600, 120);
        var combatTMP = combatTextObj.AddComponent<TextMeshProUGUI>();
        combatTMP.text = "Pausa";
        combatTMP.fontSize = 72;
        combatTMP.alignment = TextAlignmentOptions.Center;
        combatTMP.color = Color.white;

        // Agregar componente y asignar referencias via SerializedObject
        var combatOverlayComp = combatOverlay.AddComponent<CombatPauseOverlay>();
        var soCombat = new SerializedObject(combatOverlayComp);
        soCombat.FindProperty("backgroundImage").objectReferenceValue = combatBgImage;
        soCombat.FindProperty("pauseText").objectReferenceValue = combatTMP;
        soCombat.ApplyModifiedProperties();
        combatOverlay.SetActive(false);

        // =============================================
        // OPTIONS PANEL
        // =============================================
        var optionsPanel = CreatePanel(canvasObj.transform, "OptionsPanel");
        var optionsBg = optionsPanel.AddComponent<Image>();
        optionsBg.color = new Color(0, 0, 0, 0.6f);

        var optContainer = CreateContainer(optionsPanel.transform, new Vector2(450, 550));

        CreateTMPText(optContainer.transform, "Título", "Opciones", 32);
        var musicSlider = CreateLabeledSlider(optContainer.transform, "MusicVolume", "Volumen Música");
        var sfxSlider = CreateLabeledSlider(optContainer.transform, "SFXVolume", "Volumen Efectos");
        var brightnessSlider = CreateLabeledSlider(optContainer.transform, "Brightness", "Brillo");

        // Dropdown resolución
        var dropdownObj = CreateDropdown(optContainer.transform, "ResolutionDropdown");
        var resDropdown = dropdownObj.GetComponent<TMP_Dropdown>();

        // Toggle pantalla completa
        var toggleObj = CreateToggle(optContainer.transform, "FullscreenToggle", "Pantalla Completa");
        var fullscreenToggle = toggleObj.GetComponent<Toggle>();

        var optBackBtn = CreateUIButton(optContainer.transform, "Btn_Back", "Atrás");

        // Agregar OptionsPanel componente y asignar
        var optPanelComp = optionsPanel.AddComponent<OptionsPanel>();
        var soOpt = new SerializedObject(optPanelComp);
        soOpt.FindProperty("musicVolumeSlider").objectReferenceValue = musicSlider;
        soOpt.FindProperty("sfxVolumeSlider").objectReferenceValue = sfxSlider;
        soOpt.FindProperty("brightnessSlider").objectReferenceValue = brightnessSlider;
        soOpt.FindProperty("resolutionDropdown").objectReferenceValue = resDropdown;
        soOpt.FindProperty("fullscreenToggle").objectReferenceValue = fullscreenToggle;
        soOpt.FindProperty("backButton").objectReferenceValue = optBackBtn;
        soOpt.FindProperty("firstSelected").objectReferenceValue = musicSlider.gameObject;
        // pauseMenuPanel se asigna después de crearlo
        soOpt.ApplyModifiedProperties();
        optionsPanel.SetActive(false);

        // =============================================
        // CONFIRMATION DIALOG
        // =============================================
        var confirmDialog = CreatePanel(canvasObj.transform, "ConfirmationDialog");
        var confirmBg = confirmDialog.AddComponent<Image>();
        confirmBg.color = new Color(0, 0, 0, 0.7f);

        var confirmContainer = CreateContainer(confirmDialog.transform, new Vector2(500, 200));
        var confirmContainerImg = confirmContainer.AddComponent<Image>();
        confirmContainerImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        var msgText = CreateTMPText(confirmContainer.transform, "MessageText", "¿Estás seguro?", 22);

        // Fila de botones
        var btnRow = new GameObject("ButtonsRow");
        btnRow.transform.SetParent(confirmContainer.transform, false);
        var btnRowRect = btnRow.AddComponent<RectTransform>();
        btnRowRect.sizeDelta = new Vector2(0, 50);
        var rowLayout = btnRow.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 40;
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = true;
        rowLayout.childForceExpandHeight = true;

        var confirmBtn = CreateUIButton(btnRow.transform, "Btn_Confirm", "Sí");
        var cancelBtn = CreateUIButton(btnRow.transform, "Btn_Cancel", "No");

        // Agregar ConfirmationDialog componente y asignar
        var confirmComp = confirmDialog.AddComponent<ConfirmationDialog>();
        var soConfirm = new SerializedObject(confirmComp);
        soConfirm.FindProperty("messageText").objectReferenceValue = msgText;
        soConfirm.FindProperty("confirmButton").objectReferenceValue = confirmBtn;
        soConfirm.FindProperty("cancelButton").objectReferenceValue = cancelBtn;
        // confirmButtonText y cancelButtonText opcionales
        var confirmBtnText = confirmBtn.GetComponentInChildren<TextMeshProUGUI>();
        var cancelBtnText = cancelBtn.GetComponentInChildren<TextMeshProUGUI>();
        soConfirm.FindProperty("confirmButtonText").objectReferenceValue = confirmBtnText;
        soConfirm.FindProperty("cancelButtonText").objectReferenceValue = cancelBtnText;
        soConfirm.ApplyModifiedProperties();
        confirmDialog.SetActive(false);

        // =============================================
        // PAUSE MENU PANEL (Exploración)
        // =============================================
        var pauseMenuPanel = CreatePanel(canvasObj.transform, "PauseMenuPanel");
        var pauseMenuBg = pauseMenuPanel.AddComponent<Image>();
        pauseMenuBg.color = new Color(0, 0, 0, 0.5f);

        var menuContainer = CreateContainer(pauseMenuPanel.transform, new Vector2(400, 550));

        var nameText = CreateTMPText(menuContainer.transform, "PlayerNameText", "Jugador", 28);
        var currencyText = CreateTMPText(menuContainer.transform, "CurrencyText", "$0", 22);

        var btnAlbum = CreateUIButton(menuContainer.transform, "Btn_AlbumFichas", "Álbum de Fichas");
        var btnOptions = CreateUIButton(menuContainer.transform, "Btn_Opciones", "Opciones");
        var btnSave = CreateUIButton(menuContainer.transform, "Btn_GuardarSalir", "Guardar y Salir");
        var btnReturn = CreateUIButton(menuContainer.transform, "Btn_VolverMenu", "Volver al Menú Principal");

        var feedbackText = CreateTMPText(menuContainer.transform, "FeedbackText", "", 18);
        feedbackText.color = Color.yellow;
        feedbackText.gameObject.SetActive(false);

        // Agregar PauseMenuUI y asignar referencias
        var menuUIComp = pauseMenuPanel.AddComponent<PauseMenuUI>();
        var soMenu = new SerializedObject(menuUIComp);
        soMenu.FindProperty("playerNameText").objectReferenceValue = nameText;
        soMenu.FindProperty("playerCurrencyText").objectReferenceValue = currencyText;
        soMenu.FindProperty("albumFichasButton").objectReferenceValue = btnAlbum;
        soMenu.FindProperty("optionsButton").objectReferenceValue = btnOptions;
        soMenu.FindProperty("saveAndQuitButton").objectReferenceValue = btnSave;
        soMenu.FindProperty("returnToMainMenuButton").objectReferenceValue = btnReturn;
        soMenu.FindProperty("optionsPanel").objectReferenceValue = optionsPanel;
        soMenu.FindProperty("confirmationPanel").objectReferenceValue = confirmDialog;
        soMenu.FindProperty("pauseManager").objectReferenceValue = pauseManager;
        soMenu.FindProperty("firstSelected").objectReferenceValue = btnAlbum.gameObject;
        soMenu.ApplyModifiedProperties();

        // Agregar SaveAndQuitButton
        var saveComp = pauseMenuPanel.AddComponent<SaveAndQuitButton>();
        var soSave = new SerializedObject(saveComp);
        soSave.FindProperty("saveAndQuitButton").objectReferenceValue = btnSave;
        soSave.FindProperty("feedbackText").objectReferenceValue = feedbackText;
        soSave.ApplyModifiedProperties();

        // Agregar AlbumFichasButton
        var albumComp = pauseMenuPanel.AddComponent<AlbumFichasButton>();
        var soAlbum = new SerializedObject(albumComp);
        soAlbum.FindProperty("albumButton").objectReferenceValue = btnAlbum;
        soAlbum.FindProperty("pauseMenuPanel").objectReferenceValue = pauseMenuPanel;
        soAlbum.FindProperty("feedbackText").objectReferenceValue = feedbackText;
        soAlbum.ApplyModifiedProperties();

        pauseMenuPanel.SetActive(false);

        // =============================================
        // ASIGNAR REFERENCIA DE OptionsPanel -> pauseMenuPanel
        // =============================================
        soOpt = new SerializedObject(optPanelComp);
        soOpt.FindProperty("pauseMenuPanel").objectReferenceValue = pauseMenuPanel;
        soOpt.ApplyModifiedProperties();

        // =============================================
        // ASIGNAR REFERENCIAS EN PAUSE MANAGER
        // =============================================
        var soPM = new SerializedObject(pauseManager);
        soPM.FindProperty("inputHandler").objectReferenceValue = inputHandler;
        soPM.FindProperty("pauseMenuPanel").objectReferenceValue = pauseMenuPanel;
        soPM.FindProperty("combatPauseOverlay").objectReferenceValue = combatOverlay;
        soPM.ApplyModifiedProperties();

        // =============================================
        // EventSystem (necesario para UI)
        // =============================================
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            var uiModule = eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // Buscar el Input Action Asset del proyecto para asignarlo al módulo de UI.
            // Esto permite que el equipo configure Navigate/Submit/Cancel desde el asset.
            var guids = AssetDatabase.FindAssets("InputSystem_Actions t:InputActionAsset");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var actionsAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(path);
                if (actionsAsset != null)
                {
                    var soUI = new SerializedObject(uiModule);
                    soUI.FindProperty("m_ActionsAsset").objectReferenceValue = actionsAsset;
                    soUI.ApplyModifiedProperties();
                    Debug.Log($"  Input Action Asset asignado: {path}");
                }
            }
            else
            {
                Debug.LogWarning("[PauseSystemCreator] No se encontró InputSystem_Actions. Asignar manualmente el Input Action Asset al InputSystemUIInputModule.");
            }
        }

        // Seleccionar el root en la jerarquía
        Selection.activeGameObject = root;

        Debug.Log("✓ Sistema de Pausa creado correctamente.");
        Debug.Log("  Presiona F1 para alternar entre Exploration/Combat.");
        Debug.Log("  Presiona Escape para abrir/cerrar pausa.");
        Debug.Log("  Guarda la escena (Ctrl+S) para persistir los cambios.");
    }

    // =============================================
    // HELPER METHODS
    // =============================================

    private static GameObject CreatePanel(Transform parent, string name)
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        var rect = panel.AddComponent<RectTransform>();
        StretchFull(rect);
        return panel;
    }

    private static GameObject CreateContainer(Transform parent, Vector2 size)
    {
        var container = new GameObject("Container");
        container.transform.SetParent(parent, false);
        var rect = container.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
        var layout = container.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 15;
        layout.padding = new RectOffset(20, 20, 20, 20);
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        return container;
    }

    private static TextMeshProUGUI CreateTMPText(Transform parent, string name, string text, int fontSize)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, fontSize + 20);
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return tmp;
    }

    private static Button CreateUIButton(Transform parent, string name, string label)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 45);

        var image = obj.AddComponent<Image>();
        image.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        var button = obj.AddComponent<Button>();
        var colors = button.colors;
        colors.highlightedColor = new Color(0.45f, 0.45f, 0.65f, 1f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.4f, 1f);
        colors.selectedColor = new Color(0.45f, 0.45f, 0.65f, 1f);
        button.colors = colors;

        // Navegación automática para soporte de teclado/gamepad
        var nav = button.navigation;
        nav.mode = Navigation.Mode.Automatic;
        button.navigation = nav;

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        StretchFull(textRect);
        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return button;
    }

    private static Slider CreateLabeledSlider(Transform parent, string name, string label)
    {
        var container = new GameObject(name);
        container.transform.SetParent(parent, false);
        var rect = container.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 40);
        var hLayout = container.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 10;
        hLayout.childControlWidth = true;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = true;
        hLayout.childForceExpandHeight = true;

        // Label
        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(container.transform, false);
        var labelRect = labelObj.AddComponent<RectTransform>();
        var labelLayout = labelObj.AddComponent<LayoutElement>();
        labelLayout.preferredWidth = 150;
        var labelTmp = labelObj.AddComponent<TextMeshProUGUI>();
        labelTmp.text = label;
        labelTmp.fontSize = 16;
        labelTmp.alignment = TextAlignmentOptions.Left;
        labelTmp.color = Color.white;

        // Slider
        var sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(container.transform, false);
        var sliderRect = sliderObj.AddComponent<RectTransform>();

        // Background
        var bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        var bgRect = bgObj.AddComponent<RectTransform>();
        StretchFull(bgRect);
        var bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        // Fill Area
        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        var fillAreaRect = fillArea.AddComponent<RectTransform>();
        StretchFull(fillAreaRect);
        fillAreaRect.offsetMin = new Vector2(5, 5);
        fillAreaRect.offsetMax = new Vector2(-5, -5);

        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillRect = fill.AddComponent<RectTransform>();
        StretchFull(fillRect);
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.4f, 0.6f, 0.8f, 1f);

        var slider = sliderObj.AddComponent<Slider>();
        slider.fillRect = fillRect;
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 80;

        // Navegación automática para soporte de teclado/gamepad
        var nav = slider.navigation;
        nav.mode = Navigation.Mode.Automatic;
        slider.navigation = nav;

        return slider;
    }

    private static GameObject CreateDropdown(Transform parent, string name)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 40);

        var image = obj.AddComponent<Image>();
        image.color = new Color(0.25f, 0.25f, 0.25f, 1f);

        // Label
        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(obj.transform, false);
        var labelRect = labelObj.AddComponent<RectTransform>();
        StretchFull(labelRect);
        labelRect.offsetMin = new Vector2(10, 0);
        labelRect.offsetMax = new Vector2(-30, 0);
        var labelTmp = labelObj.AddComponent<TextMeshProUGUI>();
        labelTmp.text = "1920x1080";
        labelTmp.fontSize = 16;
        labelTmp.alignment = TextAlignmentOptions.Left;
        labelTmp.color = Color.white;

        // Template (mínimo para que TMP_Dropdown funcione)
        var template = new GameObject("Template");
        template.transform.SetParent(obj.transform, false);
        var templateRect = template.AddComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0, 0);
        templateRect.anchorMax = new Vector2(1, 0);
        templateRect.pivot = new Vector2(0.5f, 1f);
        templateRect.sizeDelta = new Vector2(0, 150);
        template.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
        template.AddComponent<ScrollRect>();

        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(template.transform, false);
        var vpRect = viewport.AddComponent<RectTransform>();
        StretchFull(vpRect);
        viewport.AddComponent<Image>();
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        var content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        var contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(0, 30);

        var item = new GameObject("Item");
        item.transform.SetParent(content.transform, false);
        var itemRect = item.AddComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 0.5f);
        itemRect.anchorMax = new Vector2(1, 0.5f);
        itemRect.sizeDelta = new Vector2(0, 30);
        var itemToggle = item.AddComponent<Toggle>();

        var itemLabel = new GameObject("Item Label");
        itemLabel.transform.SetParent(item.transform, false);
        var itemLabelRect = itemLabel.AddComponent<RectTransform>();
        StretchFull(itemLabelRect);
        var itemLabelTmp = itemLabel.AddComponent<TextMeshProUGUI>();
        itemLabelTmp.fontSize = 14;
        itemLabelTmp.color = Color.white;

        var scrollRect = template.GetComponent<ScrollRect>();
        scrollRect.content = contentRect;
        scrollRect.viewport = vpRect;

        var dropdown = obj.AddComponent<TMP_Dropdown>();
        dropdown.template = templateRect;
        dropdown.captionText = labelTmp;
        dropdown.itemText = itemLabelTmp;
        dropdown.ClearOptions();
        dropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "1920x1080", "1280x720", "1600x900", "2560x1440"
        });

        // Navegación automática para soporte de teclado/gamepad
        var nav = dropdown.navigation;
        nav.mode = Navigation.Mode.Automatic;
        dropdown.navigation = nav;

        template.SetActive(false);
        return obj;
    }

    private static GameObject CreateToggle(Transform parent, string name, string label)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 35);
        var hLayout = obj.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 10;
        hLayout.childControlWidth = false;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = false;
        hLayout.childForceExpandHeight = true;
        hLayout.childAlignment = TextAnchor.MiddleLeft;

        // Checkbox background
        var bgObj = new GameObject("Background");
        bgObj.transform.SetParent(obj.transform, false);
        var bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(25, 25);
        var bgLayout = bgObj.AddComponent<LayoutElement>();
        bgLayout.preferredWidth = 25;
        bgLayout.preferredHeight = 25;
        var bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.25f, 0.25f, 0.25f, 1f);

        // Checkmark
        var checkObj = new GameObject("Checkmark");
        checkObj.transform.SetParent(bgObj.transform, false);
        var checkRect = checkObj.AddComponent<RectTransform>();
        StretchFull(checkRect);
        checkRect.offsetMin = new Vector2(3, 3);
        checkRect.offsetMax = new Vector2(-3, -3);
        var checkImg = checkObj.AddComponent<Image>();
        checkImg.color = new Color(0.4f, 0.8f, 0.4f, 1f);

        // Label
        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(obj.transform, false);
        var labelRect = labelObj.AddComponent<RectTransform>();
        var labelLayout = labelObj.AddComponent<LayoutElement>();
        labelLayout.preferredWidth = 200;
        var labelTmp = labelObj.AddComponent<TextMeshProUGUI>();
        labelTmp.text = label;
        labelTmp.fontSize = 16;
        labelTmp.alignment = TextAlignmentOptions.Left;
        labelTmp.color = Color.white;

        var toggle = obj.AddComponent<Toggle>();
        toggle.targetGraphic = bgImg;
        toggle.graphic = checkImg;
        toggle.isOn = true;

        // Navegación automática para soporte de teclado/gamepad
        var nav = toggle.navigation;
        nav.mode = Navigation.Mode.Automatic;
        toggle.navigation = nav;

        return obj;
    }

    private static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
