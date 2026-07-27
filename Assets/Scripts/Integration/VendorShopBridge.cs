using UnityEngine;
using Flipit.Dialogue;
using Flipit.Shop;
using Flipit.Core;

/// <summary>
/// Bridges Vendor NPCs to the Shop UI system.
/// After a vendor's dialogue completes naturally, opens the shop interface.
/// When the shop is exited, restores player control.
/// 
/// Identifies vendors by GameObject name containing "Vendor" (City_Generator names them Vendor_0, Vendor_1, etc.)
/// or by name containing "Tendero".
/// 
/// Lives in Assembly-CSharp (Integration folder).
/// </summary>
public class VendorShopBridge : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Root GameObject of the Shop system (contains ShopManager, ShopUIController)")]
    [SerializeField] private GameObject _shopSystemGO;

    [Tooltip("Reference to ShopUIController for exit event subscription")]
    [SerializeField] private ShopUIController _shopUIController;

    [Tooltip("Reference to ShopManager for initialization")]
    [SerializeField] private ShopManager _shopManager;

    [Tooltip("Reference to ShopAlbumBridge for processing purchases")]
    [SerializeField] private ShopAlbumBridge _shopAlbumBridge;

    private bool _monitoring = true;
    private bool _vendorDialogueActive;
    private bool _shopOpen;
    private DialogueState _previousState = DialogueState.Idle;
    private GameObject _playerGO;
    private PauseManager _pauseManager;

    private void Start()
    {
        // Auto-find references if not assigned via inspector
        if (_shopSystemGO == null)
        {
            _shopSystemGO = GameObject.Find("ShopSystem");
            // Also check inactive objects
            if (_shopSystemGO == null)
            {
                var allShops = Resources.FindObjectsOfTypeAll<Flipit.Shop.ShopManager>();
                foreach (var sm in allShops)
                {
                    if (sm.gameObject.scene.isLoaded)
                    {
                        _shopSystemGO = sm.gameObject;
                        break;
                    }
                }
            }
        }

        if (_shopSystemGO != null)
        {
            if (_shopUIController == null)
                _shopUIController = _shopSystemGO.GetComponentInChildren<ShopUIController>(true);
            if (_shopManager == null)
                _shopManager = _shopSystemGO.GetComponentInChildren<ShopManager>(true);
        }

        if (_shopAlbumBridge == null)
            _shopAlbumBridge = FindAnyObjectByType<ShopAlbumBridge>();

        // Cache player and pause manager references
        _playerGO = GameObject.FindWithTag("Player");
        _pauseManager = FindAnyObjectByType<PauseManager>();

        // Ensure shop starts disabled
        if (_shopSystemGO != null)
        {
            _shopSystemGO.SetActive(false);
        }

        // Subscribe to shop exit event
        if (_shopUIController != null)
        {
            _shopUIController.OnExitShop += HandleShopExit;
            _shopUIController.OnResultsShown += HandlePurchaseResult;
        }
    }

    private void OnDestroy()
    {
        if (_shopUIController != null)
        {
            _shopUIController.OnExitShop -= HandleShopExit;
            _shopUIController.OnResultsShown -= HandlePurchaseResult;
        }
    }

    private void Update()
    {
        if (!_monitoring || _shopOpen) return;
        if (Dialogue_Manager.Instance == null) return;

        var currentState = Dialogue_Manager.Instance.CurrentState;

        // Detect dialogue starting with a vendor
        if (!_vendorDialogueActive && _previousState == DialogueState.Idle && currentState != DialogueState.Idle)
        {
            TryDetectVendorDialogue();
        }

        // Detect dialogue ending (transition to Idle)
        if (_vendorDialogueActive && currentState == DialogueState.Idle && _previousState != DialogueState.Idle)
        {
            OnVendorDialogueFinished();
        }

        _previousState = currentState;
    }

    /// <summary>
    /// Checks if the current dialogue target is a vendor NPC.
    /// </summary>
    private void TryDetectVendorDialogue()
    {
        var interactor = FindAnyObjectByType<Player_Interactor>();
        if (interactor == null || interactor.CurrentTarget == null) return;

        string npcName = interactor.CurrentTarget.gameObject.name;

        if (IsVendorNPC(npcName))
        {
            _vendorDialogueActive = true;
            Debug.Log($"[VendorShopBridge] Detected vendor dialogue: {npcName}");
        }
    }

    /// <summary>
    /// Called when the vendor's dialogue finishes naturally.
    /// Opens the shop UI.
    /// </summary>
    private void OnVendorDialogueFinished()
    {
        _vendorDialogueActive = false;

        // Only open shop if dialogue completed naturally (all lines shown)
        int currentLineIndex = Dialogue_Manager.Instance.CurrentLineIndex;
        var interactor = FindAnyObjectByType<Player_Interactor>();
        DialogueData dialogueData = interactor?.CurrentTarget?.DialogueData;
        int totalLines = dialogueData != null ? dialogueData.Lines.Count : 0;

        // If cancelled early, don't open shop
        if (totalLines > 0 && currentLineIndex < totalLines)
        {
            Debug.Log("[VendorShopBridge] Vendor dialogue cancelled, not opening shop.");
            return;
        }

        OpenShop();
    }

    /// <summary>
    /// Activates the shop system and blocks player movement.
    /// </summary>
    private void OpenShop()
    {
        if (_shopSystemGO == null)
        {
            Debug.LogWarning("[VendorShopBridge] ShopSystem GO not assigned!");
            return;
        }

        _shopOpen = true;

        // Initialize shop if needed
        if (_shopManager != null)
        {
            _shopManager.InitializeShop();
        }

        // Activate shop GO (triggers OnEnable on ShopUIController)
        _shopSystemGO.SetActive(true);

        // Block player movement via SendMessage
        if (_playerGO != null)
        {
            _playerGO.SendMessage("LockMovement", SendMessageOptions.DontRequireReceiver);
        }

        // Block pause while shop is open
        if (_pauseManager != null)
        {
            _pauseManager.BlockPause();
        }

        // Set game state to Combat (for PauseManager UI display)
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetCombat();
        }

        Debug.Log("[VendorShopBridge] Shop opened.");
    }

    /// <summary>
    /// Called when the player exits the shop.
    /// </summary>
    private void HandleShopExit()
    {
        if (!_shopOpen) return;

        _shopOpen = false;

        // Deactivate shop GO
        if (_shopSystemGO != null)
        {
            _shopSystemGO.SetActive(false);
        }

        // Unlock player movement
        if (_playerGO != null)
        {
            _playerGO.SendMessage("UnlockMovement", SendMessageOptions.DontRequireReceiver);
        }

        // Unblock pause
        if (_pauseManager != null)
        {
            _pauseManager.UnblockPause();
        }

        // Restore player state
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetExploration();
        }

        // Restore player camera
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.ActivarCamara(CameraManager.CameraType.Player);
        }

        Debug.Log("[VendorShopBridge] Shop closed, returning to exploration.");
    }

    /// <summary>
    /// Called when a purchase result is shown. Processes it through ShopAlbumBridge.
    /// </summary>
    private void HandlePurchaseResult(PurchaseResult result)
    {
        if (_shopAlbumBridge != null && result != null && result.Success)
        {
            _shopAlbumBridge.ProcessPurchaseResult(result);
            Debug.Log($"[VendorShopBridge] Purchase processed: {result.Chips.Length} chips added to album.");
        }
    }

    /// <summary>
    /// Determines if a GameObject name belongs to a vendor NPC.
    /// City_Generator names them "Vendor_0", "Vendor_1", etc.
    /// Special vendor is named "NPC_Tendero".
    /// </summary>
    private bool IsVendorNPC(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;

        return name.StartsWith("Vendor") ||
               name.Contains("Tendero") ||
               name.Contains("Vendedor");
    }
}
