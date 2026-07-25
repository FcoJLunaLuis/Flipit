using NUnit.Framework;
using UnityEngine;
using Flipit.Core;
using Flipit.Shop;
using Random = System.Random;

namespace Flipit.Core.Tests
{
    [TestFixture]
    public class ShopManagerTests
    {
        private GameObject shopObj;
        private ShopManager shopManager;
        private WalletData wallet;
        private PityConfig pityConfig;
        private BagConfig greenBag;
        private BagConfig redBag;
        private BagConfig whiteBag;

        [SetUp]
        public void SetUp()
        {
            shopObj = new GameObject("ShopManager");
            shopManager = shopObj.AddComponent<ShopManager>();

            wallet = ScriptableObject.CreateInstance<WalletData>();
            pityConfig = ScriptableObject.CreateInstance<PityConfig>();

            greenBag = ScriptableObject.CreateInstance<BagConfig>();
            redBag = ScriptableObject.CreateInstance<BagConfig>();
            whiteBag = ScriptableObject.CreateInstance<BagConfig>();

            SetupBagConfig(greenBag, BagTier.Green, 1, 2, 0, 5, 0);
            SetupBagConfig(redBag, BagTier.Red, 2, 3, 0, 15, 0);
            SetupBagConfig(whiteBag, BagTier.White, 3, 5, 0, 50, 0);

            var configs = new BagConfig[] { greenBag, redBag, whiteBag };
            shopManager.InjectDependencies(configs, wallet, pityConfig, new BagGenerator(new Random(42)));
            shopManager.PreGenerateBags();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(shopObj);
            Object.DestroyImmediate(wallet);
            Object.DestroyImmediate(pityConfig);
            Object.DestroyImmediate(greenBag);
            Object.DestroyImmediate(redBag);
            Object.DestroyImmediate(whiteBag);
        }

        [Test]
        public void TryPurchase_WithEnoughMoney_Succeeds()
        {
            wallet.Add(0, 25, 0);

            var result = shopManager.TryPurchase(0, 0); // Green bag, 5 pejecoins

            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Chips);
            Assert.Greater(result.Chips.Length, 0);
        }

        [Test]
        public void TryPurchase_DeductsMoney()
        {
            wallet.Add(0, 25, 0);

            shopManager.TryPurchase(0, 0); // Green bag costs 5 pejecoins

            Assert.AreEqual(20, wallet.Pejecoins);
        }

        [Test]
        public void TryPurchase_NotEnoughMoney_Fails()
        {
            wallet.Add(0, 3, 0); // Only 3 pejecoins, green costs 5

            var result = shopManager.TryPurchase(0, 0);

            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.FailReason);
        }

        [Test]
        public void TryPurchase_NotEnoughMoney_DoesNotDeduct()
        {
            wallet.Add(0, 3, 0);

            shopManager.TryPurchase(0, 0);

            Assert.AreEqual(3, wallet.Pejecoins);
        }

        [Test]
        public void TryPurchase_InvalidTierIndex_Fails()
        {
            wallet.Add(0, 100, 0);

            var result = shopManager.TryPurchase(99, 0);

            Assert.IsFalse(result.Success);
        }

        [Test]
        public void TryPurchase_InvalidBagIndex_Fails()
        {
            wallet.Add(0, 100, 0);

            var result = shopManager.TryPurchase(0, 99);

            Assert.IsFalse(result.Success);
        }

        [Test]
        public void TryPurchase_RegeneratesBagAfterPurchase()
        {
            wallet.Add(0, 100, 0);

            // Buy same slot twice - should get different pre-generated bags
            var result1 = shopManager.TryPurchase(0, 0);
            var result2 = shopManager.TryPurchase(0, 0);

            Assert.IsTrue(result1.Success);
            Assert.IsTrue(result2.Success);
            // Both should have valid chips (regenerated after first purchase)
            Assert.IsNotNull(result2.Chips);
            Assert.Greater(result2.Chips.Length, 0);
        }

        [Test]
        public void PreGenerateBags_Creates15Bags()
        {
            // 3 tiers * 5 bags = 15 total
            // Verify all slots are purchasable
            wallet.Add(0, 0, 10); // Lots of money

            int successCount = 0;
            for (int tier = 0; tier < 3; tier++)
            {
                for (int bag = 0; bag < 5; bag++)
                {
                    var result = shopManager.TryPurchase(tier, bag);
                    if (result.Success) successCount++;
                }
            }

            Assert.AreEqual(15, successCount);
        }

        [Test]
        public void GetBagConfig_ValidIndex_ReturnsConfig()
        {
            var config = shopManager.GetBagConfig(0);

            Assert.IsNotNull(config);
            Assert.AreEqual(BagTier.Green, config.Tier);
        }

        [Test]
        public void GetBagConfig_InvalidIndex_ReturnsNull()
        {
            var config = shopManager.GetBagConfig(99);

            Assert.IsNull(config);
        }

        [Test]
        public void CanAffordBag_WithMoney_ReturnsTrue()
        {
            wallet.Add(0, 25, 0);

            Assert.IsTrue(shopManager.CanAffordBag(0)); // Green, 5 pejecoins
        }

        [Test]
        public void CanAffordBag_WithoutMoney_ReturnsFalse()
        {
            Assert.IsFalse(shopManager.CanAffordBag(0)); // No money
        }

        [Test]
        public void TierCount_ReturnsCorrectNumber()
        {
            Assert.AreEqual(3, shopManager.TierCount);
        }

        private void SetupBagConfig(BagConfig config, BagTier tier, int min, int max,
            int costS, int costP, int costA)
        {
            var so = new UnityEditor.SerializedObject(config);
            so.FindProperty("bagName").stringValue = tier.ToString() + " Bag";
            so.FindProperty("tier").enumValueIndex = (int)tier;
            so.FindProperty("minChips").intValue = min;
            so.FindProperty("maxChips").intValue = max;
            so.FindProperty("costSheintavos").intValue = costS;
            so.FindProperty("costPejecoins").intValue = costP;
            so.FindProperty("costAjolopesos").intValue = costA;

            var rarityArray = so.FindProperty("rarityWeights");
            rarityArray.arraySize = 3;

            var common = rarityArray.GetArrayElementAtIndex(0);
            common.FindPropertyRelative("rarity").enumValueIndex = (int)ChipRarity.Common;
            common.FindPropertyRelative("baseProbability").floatValue = 0.9f;
            common.FindPropertyRelative("pityBonusPerMiss").floatValue = 0f;
            common.FindPropertyRelative("maxProbability").floatValue = 1f;

            var rare = rarityArray.GetArrayElementAtIndex(1);
            rare.FindPropertyRelative("rarity").enumValueIndex = (int)ChipRarity.Rare;
            rare.FindPropertyRelative("baseProbability").floatValue = 0.4f;
            rare.FindPropertyRelative("pityBonusPerMiss").floatValue = 0.05f;
            rare.FindPropertyRelative("maxProbability").floatValue = 0.8f;

            var ultraRare = rarityArray.GetArrayElementAtIndex(2);
            ultraRare.FindPropertyRelative("rarity").enumValueIndex = (int)ChipRarity.UltraRare;
            ultraRare.FindPropertyRelative("baseProbability").floatValue = 0.01f;
            ultraRare.FindPropertyRelative("pityBonusPerMiss").floatValue = 0.02f;
            ultraRare.FindPropertyRelative("maxProbability").floatValue = 0.5f;

            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}