using UnityEngine;
using Flipit.NPC;

/// <summary>
/// Bridges the NPC UI system with the Pause system.
/// Blocks pause when NPC menu is open, unblocks when closed.
/// Place on any GameObject in the scene.
/// Auto-finds references at runtime if not assigned via Inspector.
/// </summary>
public class NPCPauseBridge : MonoBehaviour
{
    [SerializeField] private CollectorNPCUIController npcUIController;
    
    [Header("Referencias")]
    [Tooltip("Referencia dentro del Pause system, pause manager, llenar referencia despues de agregar Pause System")]
    [SerializeField] private PauseManager pauseManager;

    private bool _subscribed;

    private void Start()
    {
        // Auto-find references if not assigned via inspector
        if (npcUIController == null)
            npcUIController = FindAnyObjectByType<CollectorNPCUIController>(FindObjectsInactive.Include);

        if (pauseManager == null)
            pauseManager = FindAnyObjectByType<PauseManager>();

        // Subscribe if we found the controller (might have been null in OnEnable)
        if (npcUIController != null && !_subscribed)
        {
            npcUIController.OnMenuOpened += HandleNPCOpened;
            npcUIController.OnMenuClosed += HandleNPCClosed;
            _subscribed = true;
        }

        if (npcUIController == null)
            Debug.LogWarning("[NPCPauseBridge] CollectorNPCUIController not found. Pause blocking for collector won't work.");
        if (pauseManager == null)
            Debug.LogWarning("[NPCPauseBridge] PauseManager not found. Pause blocking for collector won't work.");
    }

    private void OnEnable()
    {
        if (npcUIController != null && !_subscribed)
        {
            npcUIController.OnMenuOpened += HandleNPCOpened;
            npcUIController.OnMenuClosed += HandleNPCClosed;
            _subscribed = true;
        }
    }

    private void OnDisable()
    {
        if (npcUIController != null && _subscribed)
        {
            npcUIController.OnMenuOpened -= HandleNPCOpened;
            npcUIController.OnMenuClosed -= HandleNPCClosed;
            _subscribed = false;
        }
    }

    private void HandleNPCOpened()
    {
        if (pauseManager != null)
        {
            pauseManager.BlockPause();
        }
    }

    private void HandleNPCClosed()
    {
        if (pauseManager != null)
        {
            pauseManager.UnblockPause();
        }
    }
}
