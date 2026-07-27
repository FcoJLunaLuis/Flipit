using System.Collections.Generic;
using Flipit.Core;
using Flipit.NPC;

/// <summary>
/// Adapter that implements IChipCatalog using real FichaTemplate assets.
/// Bridges the NPC system (string chipId, ChipRarity) with the Album system (int templateId, Rareza).
/// ChipId format: templateId.ToString() (e.g., "1", "2", "15")
/// </summary>
public class AlbumChipCatalog : IChipCatalog
{
    private readonly Dictionary<string, FichaTemplate> templatesByChipId = new Dictionary<string, FichaTemplate>();
    private readonly Dictionary<ChipRarity, List<string>> chipsByRarity = new Dictionary<ChipRarity, List<string>>();
    private readonly List<string> allChipIds = new List<string>();

    public AlbumChipCatalog(FichaTemplate[] templates)
    {
        chipsByRarity[ChipRarity.Common] = new List<string>();
        chipsByRarity[ChipRarity.Rare] = new List<string>();
        chipsByRarity[ChipRarity.UltraRare] = new List<string>();

        if (templates == null) return;

        foreach (var template in templates)
        {
            if (template == null) continue;

            string chipId = template.id.ToString();
            templatesByChipId[chipId] = template;
            allChipIds.Add(chipId);

            ChipRarity rarity = ShopAlbumBridge.MapRarityReverse(template.rareza);
            chipsByRarity[rarity].Add(chipId);
        }
    }

    public IReadOnlyList<string> GetAllChipIds()
    {
        return allChipIds;
    }

    public ChipRarity GetChipRarity(string chipId)
    {
        if (templatesByChipId.TryGetValue(chipId, out FichaTemplate template))
        {
            return ShopAlbumBridge.MapRarityReverse(template.rareza);
        }

        return ChipRarity.Common;
    }

    public IReadOnlyList<string> GetChipsByRarity(ChipRarity rarity)
    {
        if (chipsByRarity.TryGetValue(rarity, out List<string> list))
            return list;

        return new List<string>();
    }

    public bool ChipExists(string chipId)
    {
        return templatesByChipId.ContainsKey(chipId);
    }

    /// <summary>
    /// Gets the FichaTemplate for a chipId. Useful for other integration points.
    /// </summary>
    public FichaTemplate GetTemplate(string chipId)
    {
        if (templatesByChipId.TryGetValue(chipId, out FichaTemplate template))
            return template;

        return null;
    }
}
