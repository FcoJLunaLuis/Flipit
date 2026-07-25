using System.Collections.Generic;
using UnityEngine;
using Flipit.Core;

/// <summary>
/// Bridge that connects the Shop system with the Album system.
/// When chips are obtained from bag purchases, this component translates
/// ChipResult (with ChipRarity) into FichaTemplate and adds them to the Album.
/// 
/// Place on a GameObject in the scene alongside ShopManager and AlbumManager.
/// </summary>
public class ShopAlbumBridge : MonoBehaviour
{
    [Header("References")]
    [Tooltip("All available FichaTemplates in the game, grouped internally by rarity.")]
    [SerializeField] private FichaTemplate[] allFichaTemplates;

    // Grouped templates by rarity for quick random selection
    private List<FichaTemplate> commonTemplates = new List<FichaTemplate>();
    private List<FichaTemplate> rareTemplates = new List<FichaTemplate>();
    private List<FichaTemplate> ultraRareTemplates = new List<FichaTemplate>();

    private System.Random random = new System.Random();

    private void Awake()
    {
        GroupTemplatesByRarity();
    }

    /// <summary>
    /// Processes purchase results from the shop and adds chips to the album.
    /// Call this after a successful purchase from ShopManager.TryPurchase().
    /// </summary>
    /// <param name="purchaseResult">The result from ShopManager.TryPurchase()</param>
    public void ProcessPurchaseResult(PurchaseResult purchaseResult)
    {
        if (purchaseResult == null || !purchaseResult.Success || purchaseResult.Chips == null)
            return;

        foreach (var chipResult in purchaseResult.Chips)
        {
            FichaTemplate template = SelectRandomTemplate(chipResult.Rarity);
            if (template != null)
            {
                AddToAlbum(template);
            }
            else
            {
                Debug.LogWarning($"[ShopAlbumBridge] No template found for rarity: {chipResult.Rarity}");
            }
        }
    }

    /// <summary>
    /// Processes a single ChipResult and adds it to the album.
    /// </summary>
    public void ProcessChipResult(ChipResult chipResult)
    {
        if (chipResult == null) return;

        FichaTemplate template = SelectRandomTemplate(chipResult.Rarity);
        if (template != null)
        {
            AddToAlbum(template);
        }
    }

    /// <summary>
    /// Selects a random FichaTemplate matching the given ChipRarity.
    /// </summary>
    public FichaTemplate SelectRandomTemplate(ChipRarity rarity)
    {
        List<FichaTemplate> pool = GetPoolForRarity(rarity);

        if (pool == null || pool.Count == 0)
            return null;

        int index = random.Next(pool.Count);
        return pool[index];
    }

    /// <summary>
    /// Maps ChipRarity (Flipit.Core) to Rareza (Album system).
    /// </summary>
    public static Rareza MapRarity(ChipRarity chipRarity)
    {
        switch (chipRarity)
        {
            case ChipRarity.Common: return Rareza.Comun;
            case ChipRarity.Rare: return Rareza.Raro;
            case ChipRarity.UltraRare: return Rareza.UltraRaro;
            default: return Rareza.Comun;
        }
    }

    /// <summary>
    /// Maps Rareza (Album system) to ChipRarity (Flipit.Core).
    /// </summary>
    public static ChipRarity MapRarityReverse(Rareza rareza)
    {
        switch (rareza)
        {
            case Rareza.Comun: return ChipRarity.Common;
            case Rareza.Raro: return ChipRarity.Rare;
            case Rareza.UltraRaro: return ChipRarity.UltraRare;
            default: return ChipRarity.Common;
        }
    }

    /// <summary>
    /// Gets all templates (for use by other systems like NPC catalog).
    /// </summary>
    public FichaTemplate[] GetAllTemplates()
    {
        return allFichaTemplates;
    }

    private void AddToAlbum(FichaTemplate template)
    {
        if (AlbumManager.Instance != null)
        {
            AlbumManager.Instance.AgregarFicha(template);
        }
        else
        {
            Debug.LogWarning("[ShopAlbumBridge] AlbumManager.Instance not found. Chip not added to album.");
        }
    }

    private List<FichaTemplate> GetPoolForRarity(ChipRarity rarity)
    {
        switch (rarity)
        {
            case ChipRarity.Common: return commonTemplates;
            case ChipRarity.Rare: return rareTemplates;
            case ChipRarity.UltraRare: return ultraRareTemplates;
            default: return commonTemplates;
        }
    }

    private void GroupTemplatesByRarity()
    {
        commonTemplates.Clear();
        rareTemplates.Clear();
        ultraRareTemplates.Clear();

        if (allFichaTemplates == null) return;

        foreach (var template in allFichaTemplates)
        {
            if (template == null) continue;

            switch (template.rareza)
            {
                case Rareza.Comun:
                    commonTemplates.Add(template);
                    break;
                case Rareza.Raro:
                    rareTemplates.Add(template);
                    break;
                case Rareza.UltraRaro:
                    ultraRareTemplates.Add(template);
                    break;
            }
        }

        Debug.Log($"[ShopAlbumBridge] Templates grouped: {commonTemplates.Count} Common, {rareTemplates.Count} Rare, {ultraRareTemplates.Count} UltraRare");
    }

    // For testing
    public void InjectTemplates(FichaTemplate[] templates, System.Random rng = null)
    {
        allFichaTemplates = templates;
        if (rng != null) random = rng;
        GroupTemplatesByRarity();
    }
}
