using System.Collections.Generic;
using Flipit.NPC;

/// <summary>
/// Adapter that implements IChipInventory using the real AlbumData.
/// Bridges the NPC system (string chipId) with the Album system (int templateId).
/// ChipId format: templateId.ToString() (e.g., "1", "2", "15")
/// </summary>
public class AlbumChipInventory : IChipInventory
{
    private readonly AlbumData albumData;
    private readonly AlbumChipCatalog catalog;

    public AlbumChipInventory(AlbumData albumData, AlbumChipCatalog catalog)
    {
        this.albumData = albumData;
        this.catalog = catalog;
    }

    public IReadOnlyList<string> GetOwnedChipIds()
    {
        var result = new List<string>();

        if (albumData == null) return result;

        var entries = albumData.ObtenerTodasLasFichas();
        foreach (var entry in entries)
        {
            if (entry.cantidad > 0)
            {
                result.Add(entry.ficha.templateId.ToString());
            }
        }

        return result;
    }

    public int GetChipCount(string chipId)
    {
        if (albumData == null || !TryParseChipId(chipId, out int templateId))
            return 0;

        return albumData.ObtenerCantidad(templateId);
    }

    public bool RemoveChips(string chipId, int amount)
    {
        if (albumData == null || amount <= 0)
            return false;

        if (!TryParseChipId(chipId, out int templateId))
            return false;

        return albumData.RemoverFichas(templateId, amount);
    }

    public void AddChips(string chipId, int amount)
    {
        if (albumData == null || amount <= 0)
            return;

        if (!TryParseChipId(chipId, out int templateId))
            return;

        // Try to get existing entry first
        var entry = albumData.ObtenerEntrada(templateId);
        if (entry != null)
        {
            for (int i = 0; i < amount; i++)
            {
                albumData.AgregarFicha(entry.ficha);
            }
            return;
        }

        // If entry doesn't exist, use catalog to get template
        if (catalog != null)
        {
            FichaTemplate template = catalog.GetTemplate(chipId);
            if (template != null)
            {
                albumData.AgregarFichas(template, amount);
            }
        }
    }

    public bool HasChip(string chipId)
    {
        if (albumData == null || !TryParseChipId(chipId, out int templateId))
            return false;

        return albumData.ContieneFicha(templateId);
    }

    private bool TryParseChipId(string chipId, out int templateId)
    {
        return int.TryParse(chipId, out templateId);
    }
}
