using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Flipit.NPC.Tests
{
    [TestFixture]
    public class MockChipInventoryTests
    {
        private MockChipInventory inventory;

        [SetUp]
        public void SetUp()
        {
            inventory = new MockChipInventory(new Dictionary<string, int>
            {
                { "common_aguila", 5 },
                { "common_sol", 3 },
                { "rare_dragon", 2 },
                { "ultra_cosmico", 1 }
            });
        }

        [Test]
        public void GetOwnedChipIds_ReturnsAllOwnedChips()
        {
            var owned = inventory.GetOwnedChipIds();

            Assert.AreEqual(4, owned.Count);
            Assert.IsTrue(owned.Contains("common_aguila"));
            Assert.IsTrue(owned.Contains("common_sol"));
            Assert.IsTrue(owned.Contains("rare_dragon"));
            Assert.IsTrue(owned.Contains("ultra_cosmico"));
        }

        [Test]
        public void GetChipCount_ExistingChip_ReturnsCorrectCount()
        {
            Assert.AreEqual(5, inventory.GetChipCount("common_aguila"));
            Assert.AreEqual(3, inventory.GetChipCount("common_sol"));
            Assert.AreEqual(2, inventory.GetChipCount("rare_dragon"));
            Assert.AreEqual(1, inventory.GetChipCount("ultra_cosmico"));
        }

        [Test]
        public void GetChipCount_NonExistingChip_ReturnsZero()
        {
            Assert.AreEqual(0, inventory.GetChipCount("nonexistent"));
        }

        [Test]
        public void HasChip_OwnedChip_ReturnsTrue()
        {
            Assert.IsTrue(inventory.HasChip("common_aguila"));
        }

        [Test]
        public void HasChip_NotOwnedChip_ReturnsFalse()
        {
            Assert.IsFalse(inventory.HasChip("nonexistent"));
        }

        [Test]
        public void AddChips_NewChip_AddsToInventory()
        {
            inventory.AddChips("new_chip", 3);

            Assert.AreEqual(3, inventory.GetChipCount("new_chip"));
            Assert.IsTrue(inventory.HasChip("new_chip"));
        }

        [Test]
        public void AddChips_ExistingChip_IncreasesCount()
        {
            inventory.AddChips("common_aguila", 2);

            Assert.AreEqual(7, inventory.GetChipCount("common_aguila"));
        }

        [Test]
        public void AddChips_ZeroAmount_DoesNothing()
        {
            inventory.AddChips("new_chip", 0);

            Assert.IsFalse(inventory.HasChip("new_chip"));
        }

        [Test]
        public void AddChips_NegativeAmount_DoesNothing()
        {
            inventory.AddChips("new_chip", -5);

            Assert.IsFalse(inventory.HasChip("new_chip"));
        }

        [Test]
        public void RemoveChips_HasEnough_ReturnsTrueAndReduces()
        {
            bool result = inventory.RemoveChips("common_aguila", 3);

            Assert.IsTrue(result);
            Assert.AreEqual(2, inventory.GetChipCount("common_aguila"));
        }

        [Test]
        public void RemoveChips_ExactAmount_RemovesFromInventory()
        {
            bool result = inventory.RemoveChips("ultra_cosmico", 1);

            Assert.IsTrue(result);
            Assert.AreEqual(0, inventory.GetChipCount("ultra_cosmico"));
            Assert.IsFalse(inventory.HasChip("ultra_cosmico"));
        }

        [Test]
        public void RemoveChips_NotEnough_ReturnsFalseAndKeepsOriginal()
        {
            bool result = inventory.RemoveChips("common_sol", 10);

            Assert.IsFalse(result);
            Assert.AreEqual(3, inventory.GetChipCount("common_sol"));
        }

        [Test]
        public void RemoveChips_NonExistingChip_ReturnsFalse()
        {
            bool result = inventory.RemoveChips("nonexistent", 1);

            Assert.IsFalse(result);
        }

        [Test]
        public void RemoveChips_ZeroAmount_ReturnsFalse()
        {
            bool result = inventory.RemoveChips("common_aguila", 0);

            Assert.IsFalse(result);
            Assert.AreEqual(5, inventory.GetChipCount("common_aguila"));
        }

        [Test]
        public void RemoveChips_NegativeAmount_ReturnsFalse()
        {
            bool result = inventory.RemoveChips("common_aguila", -1);

            Assert.IsFalse(result);
            Assert.AreEqual(5, inventory.GetChipCount("common_aguila"));
        }

        [Test]
        public void EmptyInventory_GetOwnedChipIds_ReturnsEmpty()
        {
            var emptyInventory = new MockChipInventory();

            Assert.AreEqual(0, emptyInventory.GetOwnedChipIds().Count);
        }

        [Test]
        public void Constructor_IgnoresZeroQuantities()
        {
            var inv = new MockChipInventory(new Dictionary<string, int>
            {
                { "chip_a", 0 },
                { "chip_b", 3 }
            });

            Assert.IsFalse(inv.HasChip("chip_a"));
            Assert.IsTrue(inv.HasChip("chip_b"));
        }
    }
}
