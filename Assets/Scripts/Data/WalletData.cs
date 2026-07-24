using UnityEngine;

namespace Flipit.Core
{
    [CreateAssetMenu(fileName = "WalletData", menuName = "Flipit/Currency/Wallet Data")]
    public class WalletData : ScriptableObject
    {
        [SerializeField] private Wallet wallet = new Wallet();

        public int Sheintavos => wallet.Sheintavos;
        public int Pejecoins => wallet.Pejecoins;
        public int Ajolopesos => wallet.Ajolopesos;

        public void Add(int sheintavos, int pejecoins, int ajolopesos)
        {
            wallet.Add(sheintavos, pejecoins, ajolopesos);
        }

        public bool CanAfford(int sheintavos, int pejecoins, int ajolopesos)
        {
            return wallet.CanAfford(sheintavos, pejecoins, ajolopesos);
        }

        public bool TrySpend(int sheintavos, int pejecoins, int ajolopesos)
        {
            return wallet.TrySpend(sheintavos, pejecoins, ajolopesos);
        }

        public long GetTotalInSheintavos()
        {
            return wallet.GetTotalInSheintavos();
        }

        public void GiveWeeklyAllowance()
        {
            wallet.GiveWeeklyAllowance();
        }

        public void Reset()
        {
            wallet.Reset();
        }

        public void InitializeNewGame()
        {
            Reset();
            GiveWeeklyAllowance();
        }
    }
}