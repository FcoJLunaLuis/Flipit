using UnityEngine;
using Flipit.Dialogue;
using Flipit.NPC;

/// <summary>
/// Bridges the Collector NPC dialogue to the CollectorNPCUIController.
/// After the collector's dialogue completes naturally, opens the collector menu
/// (sell/trade interface). When the menu closes, restores player control.
/// 
/// Identifies the collector NPC by GameObject name containing "Coleccionista" or "Collector".
/// 
/// Lives in Assembly-CSharp (Integration folder).
/// </summary>
public class CollectorDialogueBridge : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to CollectorNPCUIController")]
    [SerializeField] private CollectorNPCUIController _collectorUI;

    [Tooltip("Root GameObject of the Collector UI Canvas (contains CollectorNPCUIController)")]
    [SerializeField] private GameObject _collectorCanvasGO;

    private bool _monitoring = true;
    private bool _collectorDialogueActive;
    private bool _menuOpen;
    private DialogueState _previousState = DialogueState.Idle;
    private GameObject _playerGO;

    private void Start()
    {
        // Auto-find the collector canvas GO
        if (_collectorCanvasGO == null)
        {
            _collectorUI = FindAnyObjectByType<CollectorNPCUIController>(FindObjectsInactive.Include);
            if (_collectorUI != null)
            {
                _collectorCanvasGO = _collectorUI.gameObject;
            }
        }
        else
        {
            // If canvas assigned but controller not, get it from the canvas
            if (_collectorUI == null)
                _collectorUI = _collectorCanvasGO.GetComponent<CollectorNPCUIController>();
        }

        // Cache player reference
        _playerGO = GameObject.FindWithTag("Player");

        // Ensure canvas starts disabled
        if (_collectorCanvasGO != null)
        {
            _collectorCanvasGO.SetActive(false);
        }

        // Subscribe to collector menu close event
        if (_collectorUI != null)
        {
            _collectorUI.OnMenuClosed += HandleMenuClosed;
        }
        else
        {
            Debug.LogWarning("[CollectorDialogueBridge] CollectorNPCUIController not found in scene. Collector menu will not work.");
        }
    }

    private void OnDestroy()
    {
        if (_collectorUI != null)
        {
            _collectorUI.OnMenuClosed -= HandleMenuClosed;
        }
    }

    private void Update()
    {
        if (!_monitoring || _menuOpen) return;
        if (Dialogue_Manager.Instance == null) return;

        var currentState = Dialogue_Manager.Instance.CurrentState;

        // Detect dialogue starting with the collector NPC
        if (!_collectorDialogueActive && _previousState == DialogueState.Idle && currentState != DialogueState.Idle)
        {
            TryDetectCollectorDialogue();
        }

        // Detect dialogue ending (transition to Idle)
        if (_collectorDialogueActive && currentState == DialogueState.Idle && _previousState != DialogueState.Idle)
        {
            OnCollectorDialogueFinished();
        }

        _previousState = currentState;
    }

    /// <summary>
    /// Checks if the current dialogue target is the collector NPC.
    /// </summary>
    private void TryDetectCollectorDialogue()
    {
        var interactor = FindAnyObjectByType<Player_Interactor>();
        if (interactor == null || interactor.CurrentTarget == null) return;

        string npcName = interactor.CurrentTarget.gameObject.name;

        if (IsCollectorNPC(npcName))
        {
            _collectorDialogueActive = true;
            Debug.Log($"[CollectorDialogueBridge] Detected collector dialogue: {npcName}");
        }
    }

    /// <summary>
    /// Called when the collector's dialogue finishes naturally.
    /// Opens the collector UI menu.
    /// </summary>
    private void OnCollectorDialogueFinished()
    {
        _collectorDialogueActive = false;

        // Only open menu if dialogue completed naturally (all lines shown)
        int currentLineIndex = Dialogue_Manager.Instance.CurrentLineIndex;
        var interactor = FindAnyObjectByType<Player_Interactor>();
        DialogueData dialogueData = interactor?.CurrentTarget?.DialogueData;
        int totalLines = dialogueData != null ? dialogueData.Lines.Count : 0;

        // If cancelled early, don't open collector menu
        if (totalLines > 0 && currentLineIndex < totalLines)
        {
            Debug.Log("[CollectorDialogueBridge] Collector dialogue cancelled, not opening menu.");
            return;
        }

        OpenCollectorMenu();
    }

    /// <summary>
    /// Opens the collector NPC UI menu.
    /// </summary>
    private void OpenCollectorMenu()
    {
        if (_collectorUI == null)
        {
            Debug.LogWarning("[CollectorDialogueBridge] CollectorNPCUIController not assigned!");
            return;
        }

        if (_collectorUI.CurrentState != NPCUIState.Closed)
        {
            Debug.LogWarning("[CollectorDialogueBridge] Collector menu already open.");
            return;
        }

        _menuOpen = true;

        // Activate the collector canvas
        if (_collectorCanvasGO != null)
        {
            _collectorCanvasGO.SetActive(true);
        }

        // Open the menu
        _collectorUI.OpenMenu();

        // Lock player movement
        if (_playerGO != null)
        {
            _playerGO.SendMessage("LockMovement", SendMessageOptions.DontRequireReceiver);
        }

        Debug.Log("[CollectorDialogueBridge] Collector menu opened.");
    }

    /// <summary>
    /// Called when the collector menu is closed.
    /// Restores player control.
    /// </summary>
    private void HandleMenuClosed()
    {
        if (!_menuOpen) return;

        _menuOpen = false;

        // Deactivate the collector canvas
        if (_collectorCanvasGO != null)
        {
            _collectorCanvasGO.SetActive(false);
        }

        // Unlock player movement
        if (_playerGO != null)
        {
            _playerGO.SendMessage("UnlockMovement", SendMessageOptions.DontRequireReceiver);
        }

        // Restore player camera (in case it changed)
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.ActivarCamara(CameraManager.CameraType.Player);
        }

        Debug.Log("[CollectorDialogueBridge] Collector menu closed, returning to exploration.");
    }

    /// <summary>
    /// Determines if a GameObject name belongs to the collector NPC.
    /// City_Generator creates it as "NPC_Coleccionista".
    /// </summary>
    private bool IsCollectorNPC(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;

        return name.Contains("Coleccionista") ||
               name.Contains("Collector");
    }
}
