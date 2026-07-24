using System.Collections.Generic;
using NUnit.Framework;
using Flipit.Core;

namespace Flipit.NPC.Tests
{
    [TestFixture]
    public class MockChipCatalogTests
    {
        private MockChipCatalog catalog;

        [SetUp]
        public void SetUp()
        {
            catalog = new MockChipCatalog();
        }

        [Test]
        public void GetAllChipIds_ReturnsAllChips()
        {
            var allChips = catalog.GetAllChipIds();

            // Default mock has 8 common + 5 rare + 3 ultra = 16
            Assert.AreEqual(16, allChips.Count);
        }

        [Test]
        public void GetChipsByRarity_Common_ReturnsCorrectCount()
        {
            var commons = catalog.GetChipsByRarity(ChipRarity.Common);

            Assert.AreEqual(8, commons.Count);
        }

        [Test]
        public void GetChipsByRarity_Rare_ReturnsCorrectCount()
        {
            var rares = catalog.GetChipsByRarity(ChipRarity.Rare);

            Assert.AreEqual(5, rares.Count);
        }

        [Test]
        public void GetChipsByRarity_UltraRare_ReturnsCorrectCount()
        {
            var ultras = catalog.GetChipsByRarity(ChipRarity.UltraRare);

            Assert.AreEqual(3, ultras.Count);
        }

        [Test]
        public void GetChipRarity_KnownChip_ReturnsCorrectRarity()
        {
            Assert.AreEqual(ChipRarity.Common, catalog.GetChipRarity("common_aguila"));
            Assert.AreEqual(ChipRarity.Rare, catalog.GetChipRarity("rare_dragon"));
            Assert.AreEqual(ChipRarity.UltraRare, catalog.GetChipRarity("ultra_cosmico"));
        }

        [Test]
        public void GetChipRarity_UnknownChip_ReturnsCommonDefault()
        {
            Assert.AreEqual(ChipRarity.Common, catalog.GetChipRarity("nonexistent"));
        }

        [Test]
        public void ChipExists_KnownChip_ReturnsTrue()
        {
            Assert.IsTrue(catalog.ChipExists("common_aguila"));
            Assert.IsTrue(catalog.ChipExists("rare_dragon"));
            Assert.IsTrue(catalog.ChipExists("ultra_cosmico"));
        }

        [Test]
        public void ChipExists_UnknownChip_ReturnsFalse()
        {
            Assert.IsFalse(catalog.ChipExists("nonexistent"));
        }

        [Test]
        public void CustomCatalog_WorksWithProvidedData()
        {
            var customChips = new Dictionary<string, ChipRarity>
            {
                { "test_a", ChipRarity.Common },
                { "test_b", ChipRarity.Rare }
            };

            var customCatalog = new MockChipCatalog(customChips);

            Assert.AreEqual(2, customCatalog.GetAllChipIds().Count);
            Assert.IsTrue(customCatalog.ChipExists("test_a"));
            Assert.IsTrue(customCatalog.ChipExists("test_b"));
            Assert.AreEqual(ChipRarity.Common, customCatalog.GetChipRarity("test_a"));
            Assert.AreEqual(ChipRarity.Rare, customCatalog.GetChipRarity("test_b"));
        }

        [Test]
        public void GetChipsByRarity_AllChipsInCorrectCategory()
        {
            var commons = catalog.GetChipsByRarity(ChipRarity.Common);
            var rares = catalog.GetChipsByRarity(ChipRarity.Rare);
            var ultras = catalog.GetChipsByRarity(ChipRarity.UltraRare);

            foreach (string chipId in commons)
                Assert.AreEqual(ChipRarity.Common, catalog.GetChipRarity(chipId));

            foreach (string chipId in rares)
                Assert.AreEqual(ChipRarity.Rare, catalog.GetChipRarity(chipId));

            foreach (string chipId in ultras)
                Assert.AreEqual(ChipRarity.UltraRare, catalog.GetChipRarity(chipId));
        }
    }
}
