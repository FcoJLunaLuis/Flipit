using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Flipit.Combat;

/// <summary>
/// Listens for the "level_complete" event (fired by House_Trigger when player arrives home).
/// Shows a thank-you screen with a button to return to the title scene.
/// Builds its own UI Canvas programmatically — no prefab needed.
/// Place on any active GameObject in the CityExplorationScene.
/// </summary>
public class DemoEndHandler : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string _thankYouMessage = "¡Gracias por jugar!\n\nFin de la Demo";
    [SerializeField] private float _fadeInDuration = 1f;
    [SerializeField] private float _delayBeforeButton = 5f;
    [SerializeField] private string _titleSceneName = "TitleScene";

    private bool _triggered;

    private void OnEnable()
    {
        Combat_Event_Bus.OnCombatEventRaised += HandleEvent;
    }

    private void OnDisable()
    {
        Combat_Event_Bus.OnCombatEventRaised -= HandleEvent;
    }

    private void HandleEvent(string eventId)
    {
        if (eventId != "level_complete") return;
        if (_triggered) return;
        _triggered = true;

        StartCoroutine(DemoEndSequence());
    }

    private IEnumerator DemoEndSequence()
    {
        // Lock player movement
        var player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player != null)
            player.SendMessage("LockMovement", SendMessageOptions.DontRequireReceiver);

        // Build UI
        var canvasGO = new GameObject("DemoEndCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Black background panel
        var bgGO = CreatePanel(canvasGO.transform, "Background", Vector2.zero, Vector2.one);
        var bgImage = bgGO.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0f); // starts transparent
        bgImage.raycastTarget = true;

        // Thank you text (hidden initially)
        var textGO = CreatePanel(canvasGO.transform, "ThankYouText",
            new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.75f));
        var textTMP = textGO.AddComponent<TextMeshProUGUI>();
        textTMP.text = _thankYouMessage;
        textTMP.fontSize = 42;
        textTMP.color = new Color(1f, 1f, 1f, 0f); // starts transparent
        textTMP.alignment = TextAlignmentOptions.Center;
        textTMP.raycastTarget = false;

        // Button (hidden initially)
        var btnGO = CreatePanel(canvasGO.transform, "BtnReturnTitle",
            new Vector2(0.35f, 0.15f), new Vector2(0.65f, 0.27f));
        var btnImage = btnGO.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.4f, 0.6f, 0f); // starts transparent
        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnImage;
        btn.interactable = false;

        var lblGO = CreatePanel(btnGO.transform, "Label", Vector2.zero, Vector2.one);
        var lblTMP = lblGO.AddComponent<TextMeshProUGUI>();
        lblTMP.text = "Volver al Titulo";
        lblTMP.fontSize = 24;
        lblTMP.color = new Color(1f, 1f, 1f, 0f); // starts transparent
        lblTMP.alignment = TextAlignmentOptions.Center;
        lblTMP.raycastTarget = false;

        // === Fade in black background ===
        float elapsed = 0f;
        while (elapsed < _fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _fadeInDuration);
            bgImage.color = new Color(0f, 0f, 0f, t);
            yield return null;
        }
        bgImage.color = Color.black;

        // === Show thank you text (fade in) ===
        elapsed = 0f;
        float textFadeDuration = 1f;
        while (elapsed < textFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / textFadeDuration);
            textTMP.color = new Color(1f, 1f, 1f, t);
            yield return null;
        }
        textTMP.color = Color.white;

        // === Wait before showing button ===
        yield return new WaitForSeconds(_delayBeforeButton);

        // === Show button (fade in) ===
        elapsed = 0f;
        float btnFadeDuration = 0.5f;
        while (elapsed < btnFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / btnFadeDuration);
            btnImage.color = new Color(0.2f, 0.4f, 0.6f, t);
            lblTMP.color = new Color(1f, 1f, 1f, t);
            yield return null;
        }
        btnImage.color = new Color(0.2f, 0.4f, 0.6f, 1f);
        lblTMP.color = Color.white;

        // Enable button and wait for click
        btn.interactable = true;
        bool clicked = false;
        btn.onClick.AddListener(() => clicked = true);

        while (!clicked)
            yield return null;

        // Load title scene
        SceneManager.LoadScene(_titleSceneName);
    }

    private GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return go;
    }
}
