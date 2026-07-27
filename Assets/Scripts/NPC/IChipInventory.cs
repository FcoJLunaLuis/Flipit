using System.Collections.Generic;

namespace Flipit.NPC
{
    /// <summary>
    /// Abstraction for the player's chip inventory.
    /// Will be replaced by the real album system after merge.
    /// </summary>
    public interface IChipInventory
    {
        /// <summary>
        /// Returns all chip IDs the player owns (with quantity > 0).
        /// </summary>
        IReadOnlyList<string> GetOwnedChipIds();

        /// <summary>
        /// Returns how many of a specific chip the player owns.
        /// </summary>
        int GetChipCount(string chipId);

        /// <summary>
        /// Removes a number of chips from the player's inventory.
        /// Returns true if successful, false if not enough chips.
        /// </summary>
        bool RemoveChips(string chipId, int amount);

        /// <summary>
        /// Adds chips to the player's inventory.
        /// </summary>
        void AddChips(string chipId, int amount);

        /// <summary>
        /// Returns true if the player owns at least one of this chip.
        /// </summary>
        bool HasChip(string chipId);
    }
}
