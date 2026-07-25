using System.Collections.Generic;
using Flipit.Core;

namespace Flipit.NPC
{
    /// <summary>
    /// Abstraction for the chip catalog (all chips that exist in the game).
    /// Will be replaced by the real chip database after merge.
    /// </summary>
    public interface IChipCatalog
    {
        /// <summary>
        /// Returns all chip IDs in the game.
        /// </summary>
        IReadOnlyList<string> GetAllChipIds();

        /// <summary>
        /// Returns the rarity of a specific chip.
        /// </summary>
        ChipRarity GetChipRarity(string chipId);

        /// <summary>
        /// Returns all chip IDs of a specific rarity.
        /// </summary>
        IReadOnlyList<string> GetChipsByRarity(ChipRarity rarity);

        /// <summary>
        /// Returns true if the chip exists in the catalog.
        /// </summary>
        bool ChipExists(string chipId);
    }
}
