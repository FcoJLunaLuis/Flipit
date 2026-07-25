using UnityEngine;
using Flipit.NPC;

/// <summary>
/// Bridges the NPC UI system with the Pause system.
/// Blocks pause when NPC menu is open, unblocks when closed.
/// Place on any GameObject in the scene.
/// </summary>
public class NPCPauseBridge : MonoBehaviour
{
    [SerializeField] private CollectorNPCUIController npcUIController;
    
    [Header("Referencias")]
    [Tooltip("Referencia dentro del Pause system, pause manager, llenar referencia despues de agregar Pause System")]
    [SerializeField] private PauseManager pauseManager;
    

    private void OnEnable()
    {
        if (npcUIController != null)
        {
            npcUIController.OnMenuOpened += HandleNPCOpened;
            npcUIController.OnMenuClosed += HandleNPCClosed;
        }
    }

    private void OnDisable()
    {
        if (npcUIController != null)
        {
            npcUIController.OnMenuOpened -= HandleNPCOpened;
            npcUIController.OnMenuClosed -= HandleNPCClosed;
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
