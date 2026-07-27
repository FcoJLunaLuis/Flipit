using NUnit.Framework;

namespace Flipit.Core.Tests
{
    [TestFixture]
    public class WalletTests
    {
        private Wallet wallet;

        [SetUp]
        public void SetUp()
        {
            wallet = new Wallet();
        }

        [Test]
        public void Add_BasicSheintavos_IncreasesBalance()
        {
            wallet.Add(50, 0, 0);

            Assert.AreEqual(50, wallet.Sheintavos);
            Assert.AreEqual(0, wallet.Pejecoins);
            Assert.AreEqual(0, wallet.Ajolopesos);
        }

        [Test]
        public void Add_Over99Sheintavos_NormalizesToPejecoins()
        {
            wallet.Add(150, 0, 0);

            Assert.AreEqual(50, wallet.Sheintavos);
            Assert.AreEqual(1, wallet.Pejecoins);
            Assert.AreEqual(0, wallet.Ajolopesos);
        }

        [Test]
        public void Add_Over99Pejecoins_NormalizesToAjolopesos()
        {
            wallet.Add(0, 150, 0);

            Assert.AreEqual(0, wallet.Sheintavos);
            Assert.AreEqual(50, wallet.Pejecoins);
            Assert.AreEqual(1, wallet.Ajolopesos);
        }

        [Test]
        public void Add_CascadeNormalization_SheintavosToAjolopesos()
        {
            wallet.Add(10000, 0, 0);

            Assert.AreEqual(0, wallet.Sheintavos);
            Assert.AreEqual(0, wallet.Pejecoins);
            Assert.AreEqual(1, wallet.Ajolopesos);
        }

        [Test]
        public void Add_MultipleCalls_Accumulates()
        {
            wallet.Add(50, 0, 0);
            wallet.Add(60, 0, 0);

            Assert.AreEqual(10, wallet.Sheintavos);
            Assert.AreEqual(1, wallet.Pejecoins);
        }

        [Test]
        public void Add_AllDenominations_NormalizesCorrectly()
        {
            wallet.Add(250, 150, 3);

            // 250 sheintavos = 2 pejecoins + 50 sheintavos
            // 150 + 2 = 152 pejecoins = 1 ajolopeso + 52 pejecoins
            // 3 + 1 = 4 ajolopesos
            Assert.AreEqual(50, wallet.Sheintavos);
            Assert.AreEqual(52, wallet.Pejecoins);
            Assert.AreEqual(4, wallet.Ajolopesos);
        }

        [Test]
        public void CanAfford_HasExactAmount_ReturnsTrue()
        {
            wallet.Add(0, 25, 0);

            Assert.IsTrue(wallet.CanAfford(0, 25, 0));
        }

        [Test]
        public void CanAfford_HasMoreThanEnough_ReturnsTrue()
        {
            wallet.Add(0, 50, 0);

            Assert.IsTrue(wallet.CanAfford(0, 25, 0));
        }

        [Test]
        public void CanAfford_NotEnough_ReturnsFalse()
        {
            wallet.Add(0, 10, 0);

            Assert.IsFalse(wallet.CanAfford(0, 25, 0));
        }

        [Test]
        public void CanAfford_CrossDenomination_ReturnsTrue()
        {
            wallet.Add(0, 0, 1); // 1 ajolopeso = 10000 sheintavos

            Assert.IsTrue(wallet.CanAfford(0, 50, 0)); // 50 pejecoins = 5000 sheintavos
        }

        [Test]
        public void CanAfford_EmptyWallet_ReturnsFalse()
        {
            Assert.IsFalse(wallet.CanAfford(1, 0, 0));
        }

        [Test]
        public void CanAfford_ZeroCost_ReturnsTrue()
        {
            Assert.IsTrue(wallet.CanAfford(0, 0, 0));
        }

        [Test]
        public void TrySpend_HasEnough_ReturnsTrueAndDeducts()
        {
            wallet.Add(0, 50, 0);

            bool result = wallet.TrySpend(0, 25, 0);

            Assert.IsTrue(result);
            Assert.AreEqual(0, wallet.Sheintavos);
            Assert.AreEqual(25, wallet.Pejecoins);
            Assert.AreEqual(0, wallet.Ajolopesos);
        }

        [Test]
        public void TrySpend_NotEnough_ReturnsFalseAndKeepsBalance()
        {
            wallet.Add(0, 10, 0);

            bool result = wallet.TrySpend(0, 25, 0);

            Assert.IsFalse(result);
            Assert.AreEqual(0, wallet.Sheintavos);
            Assert.AreEqual(10, wallet.Pejecoins);
            Assert.AreEqual(0, wallet.Ajolopesos);
        }

        [Test]
        public void TrySpend_DecomposesHigherDenomination()
        {
            wallet.Add(0, 0, 1); // 1 ajolopeso

            bool result = wallet.TrySpend(50, 0, 0); // spend 50 sheintavos

            Assert.IsTrue(result);
            // 10000 - 50 = 9950 sheintavos = 0 ajolopesos, 99 pejecoins, 50 sheintavos
            Assert.AreEqual(50, wallet.Sheintavos);
            Assert.AreEqual(99, wallet.Pejecoins);
            Assert.AreEqual(0, wallet.Ajolopesos);
        }

        [Test]
        public void TrySpend_ExactAmount_LeavesZero()
        {
            wallet.Add(0, 25, 0);

            bool result = wallet.TrySpend(0, 25, 0);

            Assert.IsTrue(result);
            Assert.AreEqual(0, wallet.Sheintavos);
            Assert.AreEqual(0, wallet.Pejecoins);
            Assert.AreEqual(0, wallet.Ajolopesos);
        }

        [Test]
        public void TrySpend_MixedDenominationCost()
        {
            wallet.Add(50, 30, 2);

            bool result = wallet.TrySpend(25, 15, 1);

            Assert.IsTrue(result);
            // Total owned: 50 + 3000 + 20000 = 23050
            // Total cost: 25 + 1500 + 10000 = 11525
            // Remaining: 23050 - 11525 = 11525
            // 11525 / 10000 = 1 ajolopeso, remainder 1525
            // 1525 / 100 = 15 pejecoins, remainder 25
            Assert.AreEqual(25, wallet.Sheintavos);
            Assert.AreEqual(15, wallet.Pejecoins);
            Assert.AreEqual(1, wallet.Ajolopesos);
        }

        [Test]
        public void GiveWeeklyAllowance_Adds25Pejecoins()
        {
            wallet.GiveWeeklyAllowance();

            Assert.AreEqual(0, wallet.Sheintavos);
            Assert.AreEqual(25, wallet.Pejecoins);
            Assert.AreEqual(0, wallet.Ajolopesos);
        }

        [Test]
        public void GiveWeeklyAllowance_AccumulatesOverMultipleWeeks()
        {
            wallet.GiveWeeklyAllowance();
            wallet.GiveWeeklyAllowance();
            wallet.GiveWeeklyAllowance();
            wallet.GiveWeeklyAllowance();

            // 4 * 25 = 100 pejecoins = 1 ajolopeso
            Assert.AreEqual(0, wallet.Sheintavos);
            Assert.AreEqual(0, wallet.Pejecoins);
            Assert.AreEqual(1, wallet.Ajolopesos);
        }

        [Test]
        public void GetTotalInSheintavos_CalculatesCorrectly()
        {
            wallet.Add(50, 25, 2);

            // 50 + 25*100 + 2*10000 = 50 + 2500 + 20000 = 22550
            Assert.AreEqual(22550L, wallet.GetTotalInSheintavos());
        }

        [Test]
        public void GetTotalInSheintavos_EmptyWallet_ReturnsZero()
        {
            Assert.AreEqual(0L, wallet.GetTotalInSheintavos());
        }

        [Test]
        public void Reset_ClearsAllDenominations()
        {
            wallet.Add(50, 25, 3);
            wallet.Reset();

            Assert.AreEqual(0, wallet.Sheintavos);
            Assert.AreEqual(0, wallet.Pejecoins);
            Assert.AreEqual(0, wallet.Ajolopesos);
        }

        [Test]
        public void NeverNegative_TrySpendMoreThanOwned_BalanceUnchanged()
        {
            wallet.Add(0, 10, 0);
            int originalPejecoins = wallet.Pejecoins;

            wallet.TrySpend(0, 0, 1); // can't afford 1 ajolopeso

            Assert.AreEqual(0, wallet.Sheintavos);
            Assert.AreEqual(originalPejecoins, wallet.Pejecoins);
            Assert.AreEqual(0, wallet.Ajolopesos);
        }
    }
}