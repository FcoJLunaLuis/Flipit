using System.Collections.Generic;

namespace Flipit.NPC
{
    /// <summary>
    /// Mock implementation of IChipInventory for testing.
    /// Stores chip quantities in a dictionary.
    /// Replace with real album inventory after merge.
    /// </summary>
    public class MockChipInventory : IChipInventory
    {
        private readonly Dictionary<string, int> ownedChips = new Dictionary<string, int>();

        public MockChipInventory() { }

        /// <summary>
        /// Constructor with initial chip data for testing.
        /// </summary>
        public MockChipInventory(Dictionary<string, int> initialChips)
        {
            foreach (var kvp in initialChips)
            {
                if (kvp.Value > 0)
                    ownedChips[kvp.Key] = kvp.Value;
            }
        }

        public IReadOnlyList<string> GetOwnedChipIds()
        {
            var owned = new List<string>();
            foreach (var kvp in ownedChips)
            {
                if (kvp.Value > 0)
                    owned.Add(kvp.Key);
            }
            return owned;
        }

        public int GetChipCount(string chipId)
        {
            if (ownedChips.TryGetValue(chipId, out int count))
                return count;

            return 0;
        }

        public bool RemoveChips(string chipId, int amount)
        {
            if (amount <= 0)
                return false;

            if (!ownedChips.TryGetValue(chipId, out int current))
                return false;

            if (current < amount)
                return false;

            current -= amount;
            if (current <= 0)
                ownedChips.Remove(chipId);
            else
                ownedChips[chipId] = current;

            return true;
        }

        public void AddChips(string chipId, int amount)
        {
            if (amount <= 0)
                return;

            if (ownedChips.TryGetValue(chipId, out int current))
                ownedChips[chipId] = current + amount;
            else
                ownedChips[chipId] = amount;
        }

        public bool HasChip(string chipId)
        {
            return ownedChips.TryGetValue(chipId, out int count) && count > 0;
        }
    }
}
