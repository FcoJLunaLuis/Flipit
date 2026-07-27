using System.Collections.Generic;
using UnityEngine;
using Flipit.Dialogue;
using Flipit.Combat;

/// <summary>
/// Bridges FlipCombat_NPC dialogue flow to same-scene CombatManager.
/// Monitors Dialogue_Manager state: when a challenger's dialogue transitions to Idle,
/// checks if dialogue completed naturally (accepted) or was cancelled (rejected).
/// 
/// Detection logic:
/// - CurrentLineIndex >= Lines.Count → player accepted (went through all lines)
/// - CurrentLineIndex < Lines.Count → player rejected or cancelled early
/// 
/// Replaces CombatDialogue_Handler + Combat_System for the city scene.
/// Lives in Assembly-CSharp (Integration folder).
/// </summary>
public class ChallengerCombatBridge : MonoBehaviour
{
    [Header("NPC Ficha Generation")]
    [Tooltip("Number of fichas the NPC will bet")]
    [SerializeField] private int _npcFichaCount = 3;

    [Tooltip("Available templates for NPC fichas (optional — uses mock if empty)")]
    [SerializeField] private FichaTemplate[] _npcFichaTemplates;

    private FlipCombat_NPC _activeChallenger;
    private DialogueData _activeDialogueData;
    private bool _monitoring;
    private DialogueState _previousState = DialogueState.Idle;

    private void Update()
    {
        if (!_monitoring) return;
        if (Dialogue_Manager.Instance == null) return;

        var currentState = Dialogue_Manager.Instance.CurrentState;

        // Detect dialogue starting with a challenger
        if (_activeChallenger == null && _previousState == DialogueState.Idle && currentState != DialogueState.Idle)
        {
            TryDetectChallengerDialogue();
        }

        // Detect dialogue ending (transition to Idle) while tracking a challenger
        if (_activeChallenger != null && currentState == DialogueState.Idle && _previousState != DialogueState.Idle)
        {
            OnChallengerDialogueFinished();
        }

        _previousState = currentState;
    }

    /// <summary>
    /// Detects when a dialogue starts with a FlipCombat_NPC as the target.
    /// </summary>
    private void TryDetectChallengerDialogue()
    {
        var interactor = FindAnyObjectByType<Player_Interactor>();
        if (interactor == null || interactor.CurrentTarget == null) return;

        var challenger = interactor.CurrentTarget as FlipCombat_NPC;
        if (challenger == null) return;

        // Start monitoring this challenger's dialogue
        _activeChallenger = challenger;
        _activeDialogueData = challenger.DialogueData;

        Debug.Log($"[ChallengerCombatBridge] Detected challenger dialogue: {challenger.NpcDisplayName}");
    }

    /// <summary>
    /// Called when the dialogue transitions to Idle while a challenger is active.
    /// Determines if the player accepted or rejected based on CurrentLineIndex.
    /// </summary>
    private void OnChallengerDialogueFinished()
    {
        var challenger = _activeChallenger;
        var dialogueData = _activeDialogueData;
        CleanupState();

        if (dialogueData == null)
        {
            Debug.LogWarning("[ChallengerCombatBridge] DialogueData was null, cannot determine outcome.");
            return;
        }

        int currentLineIndex = Dialogue_Manager.Instance.CurrentLineIndex;
        int totalLines = dialogueData.Lines.Count;

        // If CurrentLineIndex >= Lines.Count, the player went through all lines (accepted)
        if (currentLineIndex >= totalLines)
        {
            OnCombatAccepted(challenger);
        }
        else
        {
            OnCombatRejected(challenger);
        }
    }

    /// <summary>
    /// Player accepted combat. Start same-scene combat via CombatManager.
    /// </summary>
    private void OnCombatAccepted(FlipCombat_NPC challenger)
    {
        Debug.Log($"[ChallengerCombatBridge] Combat accepted against {challenger.NpcDisplayName}! Starting same-scene combat.");

        // Generate NPC fichas
        var fichasNPC = GenerateNPCFichas(challenger);

        // Start combat via CombatManager
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.IniciarCombate(fichasNPC);
        }
        else
        {
            Debug.LogError("[ChallengerCombatBridge] CombatManager.Instance is null! Cannot start combat.");
        }
    }

    /// <summary>
    /// Player rejected combat. No action needed — dialogue already closed.
    /// </summary>
    private void OnCombatRejected(FlipCombat_NPC challenger)
    {
        Debug.Log($"[ChallengerCombatBridge] Combat rejected against {challenger.NpcDisplayName}.");
    }

    /// <summary>
    /// Generates a list of FichaData for the NPC to bet.
    /// Uses templates if available, otherwise creates mock fichas.
    /// </summary>
    private List<FichaData> GenerateNPCFichas(FlipCombat_NPC challenger)
    {
        var fichas = new List<FichaData>();
        int count = Mathf.Max(1, _npcFichaCount);

        if (_npcFichaTemplates != null && _npcFichaTemplates.Length > 0)
        {
            // Generate from templates
            for (int i = 0; i < count; i++)
            {
                var template = _npcFichaTemplates[i % _npcFichaTemplates.Length];
                fichas.Add(FichaData.CrearDesdePlantilla(template));
            }
        }
        else
        {
            // Generate mock fichas
            string displayName = challenger != null ? challenger.NpcDisplayName : "NPC";
            string[] nombres = {
                $"Ficha de {displayName} 1",
                $"Ficha de {displayName} 2",
                $"Ficha de {displayName} 3",
                $"Ficha de {displayName} 4",
                $"Ficha de {displayName} 5"
            };

            for (int i = 0; i < count && i < nombres.Length; i++)
            {
                fichas.Add(new FichaData
                {
                    templateId = 500 + i,
                    nombre = nombres[i],
                    rareza = (Rareza)(i % 3),
                    rango = 1,
                    experienciaDeRango = 0,
                    estaRoto = false,
                    desgaste = Random.Range(0f, 15f),
                    perk = "Ninguno",
                    peso = Random.Range(8f, 18f),
                    suerte = Random.Range(0.3f, 0.7f)
                });
            }
        }

        Debug.Log($"[ChallengerCombatBridge] Generated {fichas.Count} NPC fichas.");
        return fichas;
    }

    private void CleanupState()
    {
        _activeChallenger = null;
        _activeDialogueData = null;
    }

    private void OnEnable()
    {
        _monitoring = true;
    }

    private void OnDisable()
    {
        _monitoring = false;
        CleanupState();
    }
}
