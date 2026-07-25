using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Flipit.Core;
using Random = System.Random;

namespace Flipit.NPC.Tests
{
    [TestFixture]
    public class TradeGeneratorTests
    {
        private TradeGenerator generator;
        private CollectorNPCConfig config;
        private MockChipCatalog catalog;
        private MockChipInventory inventory;

        [SetUp]
        public void SetUp()
        {
            generator = new TradeGenerator(new Random(42));
            config = ScriptableObject.CreateInstance<CollectorNPCConfig>();
            SetupConfig(tradeOfferCount: 5, minReqTypes: 1, maxReqTypes: 3);

            catalog = new MockChipCatalog();

            // Player owns some chips
            inventory = new MockChipInventory(new Dictionary<string, int>
            {
                { "common_aguila", 5 },
                { "common_sol", 3 },
                { "common_luna", 4 },
                { "common_estrella", 2 },
                { "rare_dragon", 3 },
                { "rare_fenix", 2 }
            });
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(config);
        }

        [Test]
        public void GenerateOffers_ReturnsCorrectCount()
        {
            var offers = generator.GenerateOffers(config, catalog, inventory);

            Assert.AreEqual(5, offers.Length);
        }

        [Test]
        public void GenerateOffers_AtLeastOneUnownedChip()
        {
            // Run multiple times with different seeds
            for (int seed = 0; seed < 50; seed++)
            {
                var gen = new TradeGenerator(new Random(seed));
                var offers = gen.GenerateOffers(config, catalog, inventory);

                bool hasUnowned = false;
                for (int i = 0; i < offers.Length; i++)
                {
                    if (!inventory.HasChip(offers[i].OfferedChipId))
                    {
                        hasUnowned = true;
                        break;
                    }
                }

                Assert.IsTrue(hasUnowned, $"Seed {seed}: Should have at least one unowned chip offer");
            }
        }

        [Test]
        public void GenerateOffers_NoDuplicateOffers()
        {
            for (int seed = 0; seed < 50; seed++)
            {
                var gen = new TradeGenerator(new Random(seed));
                var offers = gen.GenerateOffers(config, catalog, inventory);

                var chipIds = new HashSet<string>();
                foreach (var offer in offers)
                {
                    Assert.IsFalse(chipIds.Contains(offer.OfferedChipId),
                        $"Seed {seed}: Duplicate offer for {offer.OfferedChipId}");
                    chipIds.Add(offer.OfferedChipId);
                }
            }
        }

        [Test]
        public void GenerateOffers_RequirementsWithinPlayerOwnership()
        {
            for (int seed = 0; seed < 50; seed++)
            {
                var gen = new TradeGenerator(new Random(seed));
                var offers = gen.GenerateOffers(config, catalog, inventory);

                foreach (var offer in offers)
                {
                    foreach (var req in offer.Requirements)
                    {
                        int playerOwns = inventory.GetChipCount(req.RequiredChipId);
                        Assert.GreaterOrEqual(playerOwns, req.RequiredAmount,
                            $"Seed {seed}: Requires {req.RequiredAmount} of {req.RequiredChipId} but player only has {playerOwns}");
                        Assert.GreaterOrEqual(req.RequiredAmount, 1,
                            $"Seed {seed}: Requirement amount should be at least 1");
                    }
                }
            }
        }

        [Test]
        public void GenerateOffers_RequirementCountWithinConfig()
        {
            for (int seed = 0; seed < 50; seed++)
            {
                var gen = new TradeGenerator(new Random(seed));
                var offers = gen.GenerateOffers(config, catalog, inventory);

                foreach (var offer in offers)
                {
                    Assert.GreaterOrEqual(offer.Requirements.Length, 1,
                        $"Seed {seed}: Should have at least 1 requirement");
                    Assert.LessOrEqual(offer.Requirements.Length, 3,
                        $"Seed {seed}: Should have at most 3 requirements");
                }
            }
        }

        [Test]
        public void GenerateOffers_OfferedChipNotInRequirements()
        {
            for (int seed = 0; seed < 50; seed++)
            {
                var gen = new TradeGenerator(new Random(seed));
                var offers = gen.GenerateOffers(config, catalog, inventory);

                foreach (var offer in offers)
                {
                    foreach (var req in offer.Requirements)
                    {
                        Assert.AreNotEqual(offer.OfferedChipId, req.RequiredChipId,
                            $"Seed {seed}: Offered chip should not be in its own requirements");
                    }
                }
            }
        }

        [Test]
        public void GenerateOffers_OfferedChipRarityMatchesCatalog()
        {
            var offers = generator.GenerateOffers(config, catalog, inventory);

            foreach (var offer in offers)
            {
                ChipRarity expectedRarity = catalog.GetChipRarity(offer.OfferedChipId);
                Assert.AreEqual(expectedRarity, offer.OfferedRarity);
            }
        }

        [Test]
        public void GenerateOffers_NullConfig_ReturnsEmpty()
        {
            var offers = generator.GenerateOffers(null, catalog, inventory);

            Assert.AreEqual(0, offers.Length);
        }

        [Test]
        public void GenerateOffers_NullCatalog_ReturnsEmpty()
        {
            var offers = generator.GenerateOffers(config, null, inventory);

            Assert.AreEqual(0, offers.Length);
        }

        [Test]
        public void GenerateOffers_NullInventory_ReturnsEmpty()
        {
            var offers = generator.GenerateOffers(config, catalog, null);

            Assert.AreEqual(0, offers.Length);
        }

        [Test]
        public void GenerateOffers_EmptyInventory_ReturnsEmpty()
        {
            var emptyInventory = new MockChipInventory();

            var offers = generator.GenerateOffers(config, catalog, emptyInventory);

            Assert.AreEqual(0, offers.Length);
        }

        [Test]
        public void GenerateOffers_SmallInventory_GeneratesFewerOffersGracefully()
        {
            // Player only has 1 chip type — can only create offers that require that chip
            var smallInventory = new MockChipInventory(new Dictionary<string, int>
            {
                { "common_aguila", 5 }
            });

            var offers = generator.GenerateOffers(config, catalog, smallInventory);

            // Should still generate offers (possibly fewer than 5 if not enough variety)
            Assert.GreaterOrEqual(offers.Length, 1);

            // All requirements should be for common_aguila
            foreach (var offer in offers)
            {
                foreach (var req in offer.Requirements)
                {
                    Assert.AreEqual("common_aguila", req.RequiredChipId);
                }
            }
        }

        [Test]
        public void GenerateOffers_AllChipsOwned_StillGeneratesOffers()
        {
            // Player owns all chips — no unowned chips exist
            var fullInventory = new MockChipInventory(new Dictionary<string, int>
            {
                { "common_aguila", 5 },
                { "common_sol", 3 },
                { "common_luna", 4 },
                { "common_estrella", 2 },
                { "common_nube", 2 },
                { "common_flor", 2 },
                { "common_rio", 2 },
                { "common_monte", 2 },
                { "rare_dragon", 3 },
                { "rare_fenix", 2 },
                { "rare_kraken", 2 },
                { "rare_quetzal", 2 },
                { "rare_jaguar", 2 },
                { "ultra_cosmico", 1 },
                { "ultra_dimensional", 1 },
                { "ultra_celestial", 1 }
            });

            var offers = generator.GenerateOffers(config, catalog, fullInventory);

            // Should still work even without unowned chips
            Assert.Greater(offers.Length, 0);
        }

        [Test]
        public void TradeOffer_CanPlayerAfford_WhenHasAllRequirements_ReturnsTrue()
        {
            var offer = new TradeOffer("ultra_cosmico", ChipRarity.UltraRare, new TradeRequirement[]
            {
                new TradeRequirement("common_aguila", 2),
                new TradeRequirement("rare_dragon", 1)
            });

            Assert.IsTrue(offer.CanPlayerAfford(inventory));
        }

        [Test]
        public void TradeOffer_CanPlayerAfford_WhenMissingChips_ReturnsFalse()
        {
            var offer = new TradeOffer("ultra_cosmico", ChipRarity.UltraRare, new TradeRequirement[]
            {
                new TradeRequirement("common_aguila", 100), // Player only has 5
                new TradeRequirement("rare_dragon", 1)
            });

            Assert.IsFalse(offer.CanPlayerAfford(inventory));
        }

        [Test]
        public void TradeOffer_CanPlayerAfford_WhenChipNotOwned_ReturnsFalse()
        {
            var offer = new TradeOffer("ultra_cosmico", ChipRarity.UltraRare, new TradeRequirement[]
            {
                new TradeRequirement("nonexistent", 1)
            });

            Assert.IsFalse(offer.CanPlayerAfford(inventory));
        }

        [Test]
        public void TradeOffer_CanPlayerAfford_NullInventory_ReturnsFalse()
        {
            var offer = new TradeOffer("ultra_cosmico", ChipRarity.UltraRare, new TradeRequirement[]
            {
                new TradeRequirement("common_aguila", 1)
            });

            Assert.IsFalse(offer.CanPlayerAfford(null));
        }

        [Test]
        public void GenerateOffers_DeterministicWithSameSeed()
        {
            var gen1 = new TradeGenerator(new Random(99));
            var gen2 = new TradeGenerator(new Random(99));

            var offers1 = gen1.GenerateOffers(config, catalog, inventory);
            var offers2 = gen2.GenerateOffers(config, catalog, inventory);

            Assert.AreEqual(offers1.Length, offers2.Length);
            for (int i = 0; i < offers1.Length; i++)
            {
                Assert.AreEqual(offers1[i].OfferedChipId, offers2[i].OfferedChipId);
                Assert.AreEqual(offers1[i].Requirements.Length, offers2[i].Requirements.Length);
            }
        }

        private void SetupConfig(int tradeOfferCount, int minReqTypes, int maxReqTypes)
        {
            var so = new SerializedObject(config);

            so.FindProperty("tradeOfferCount").intValue = tradeOfferCount;
            so.FindProperty("minTradeRequirementTypes").intValue = minReqTypes;
            so.FindProperty("maxTradeRequirementTypes").intValue = maxReqTypes;

            // Also setup sell prices (needed for full config)
            var sellPrices = so.FindProperty("sellPriceRanges");
            sellPrices.arraySize = 3;

            var common = sellPrices.GetArrayElementAtIndex(0);
            common.FindPropertyRelative("rarity").enumValueIndex = (int)ChipRarity.Common;
            common.FindPropertyRelative("minPrice").intValue = 5;
            common.FindPropertyRelative("maxPrice").intValue = 15;

            var rare = sellPrices.GetArrayElementAtIndex(1);
            rare.FindPropertyRelative("rarity").enumValueIndex = (int)ChipRarity.Rare;
            rare.FindPropertyRelative("minPrice").intValue = 25;
            rare.FindPropertyRelative("maxPrice").intValue = 75;

            var ultraRare = sellPrices.GetArrayElementAtIndex(2);
            ultraRare.FindPropertyRelative("rarity").enumValueIndex = (int)ChipRarity.UltraRare;
            ultraRare.FindPropertyRelative("minPrice").intValue = 100;
            ultraRare.FindPropertyRelative("maxPrice").intValue = 300;

            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
