using NUnit.Framework;
using UnityEngine;

namespace Flipit.Core.Tests
{
    [TestFixture]
    public class WalletDataTests
    {
        private WalletData walletData;

        [SetUp]
        public void SetUp()
        {
            walletData = ScriptableObject.CreateInstance<WalletData>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(walletData);
        }

        [Test]
        public void Add_DelegatesToWallet()
        {
            walletData.Add(150, 0, 0);

            Assert.AreEqual(50, walletData.Sheintavos);
            Assert.AreEqual(1, walletData.Pejecoins);
        }

        [Test]
        public void TrySpend_DelegatesToWallet()
        {
            walletData.Add(0, 50, 0);

            bool result = walletData.TrySpend(0, 25, 0);

            Assert.IsTrue(result);
            Assert.AreEqual(25, walletData.Pejecoins);
        }

        [Test]
        public void TrySpend_NotEnough_ReturnsFalse()
        {
            walletData.Add(0, 10, 0);

            bool result = walletData.TrySpend(0, 25, 0);

            Assert.IsFalse(result);
            Assert.AreEqual(10, walletData.Pejecoins);
        }

        [Test]
        public void CanAfford_DelegatesToWallet()
        {
            walletData.Add(0, 25, 0);

            Assert.IsTrue(walletData.CanAfford(0, 25, 0));
            Assert.IsFalse(walletData.CanAfford(0, 0, 1));
        }

        [Test]
        public void GiveWeeklyAllowance_DelegatesToWallet()
        {
            walletData.GiveWeeklyAllowance();

            Assert.AreEqual(25, walletData.Pejecoins);
        }

        [Test]
        public void GetTotalInSheintavos_DelegatesToWallet()
        {
            walletData.Add(50, 25, 0);

            Assert.AreEqual(2550L, walletData.GetTotalInSheintavos());
        }

        [Test]
        public void Reset_DelegatesToWallet()
        {
            walletData.Add(50, 25, 3);
            walletData.Reset();

            Assert.AreEqual(0, walletData.Sheintavos);
            Assert.AreEqual(0, walletData.Pejecoins);
            Assert.AreEqual(0, walletData.Ajolopesos);
        }

        [Test]
        public void InitializeNewGame_ResetsAndGivesAllowance()
        {
            walletData.Add(99, 99, 5);
            walletData.InitializeNewGame();

            Assert.AreEqual(0, walletData.Sheintavos);
            Assert.AreEqual(25, walletData.Pejecoins);
            Assert.AreEqual(0, walletData.Ajolopesos);
        }
    }
}