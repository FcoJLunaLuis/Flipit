using System;
using UnityEngine;

namespace Flipit.Core
{
    [Serializable]
    public class Wallet
    {
        private const int ConversionRate = 100;

        [SerializeField] private int sheintavos;
        [SerializeField] private int pejecoins;
        [SerializeField] private int ajolopesos;

        public int Sheintavos => sheintavos;
        public int Pejecoins => pejecoins;
        public int Ajolopesos => ajolopesos;

        public void Add(int addSheintavos, int addPejecoins, int addAjolopesos)
        {
            sheintavos += addSheintavos;
            pejecoins += addPejecoins;
            ajolopesos += addAjolopesos;
            Normalize();
        }

        public bool CanAfford(int costSheintavos, int costPejecoins, int costAjolopesos)
        {
            long totalOwned = GetTotalInSheintavos();
            long totalCost = (long)costSheintavos
                           + (long)costPejecoins * ConversionRate
                           + (long)costAjolopesos * ConversionRate * ConversionRate;
            return totalOwned >= totalCost;
        }

        public bool TrySpend(int costSheintavos, int costPejecoins, int costAjolopesos)
        {
            if (!CanAfford(costSheintavos, costPejecoins, costAjolopesos))
                return false;

            long totalOwned = GetTotalInSheintavos();
            long totalCost = (long)costSheintavos
                           + (long)costPejecoins * ConversionRate
                           + (long)costAjolopesos * ConversionRate * ConversionRate;

            long remaining = totalOwned - totalCost;

            ajolopesos = (int)(remaining / (ConversionRate * ConversionRate));
            remaining %= (ConversionRate * ConversionRate);

            pejecoins = (int)(remaining / ConversionRate);
            remaining %= ConversionRate;

            sheintavos = (int)remaining;

            return true;
        }

        public long GetTotalInSheintavos()
        {
            return (long)sheintavos
                 + (long)pejecoins * ConversionRate
                 + (long)ajolopesos * ConversionRate * ConversionRate;
        }

        public void GiveWeeklyAllowance()
        {
            Add(0, 25, 0);
        }

        public void Reset()
        {
            sheintavos = 0;
            pejecoins = 0;
            ajolopesos = 0;
        }

        private void Normalize()
        {
            if (sheintavos >= ConversionRate)
            {
                pejecoins += sheintavos / ConversionRate;
                sheintavos %= ConversionRate;
            }

            if (pejecoins >= ConversionRate)
            {
                ajolopesos += pejecoins / ConversionRate;
                pejecoins %= ConversionRate;
            }
        }
    }
}