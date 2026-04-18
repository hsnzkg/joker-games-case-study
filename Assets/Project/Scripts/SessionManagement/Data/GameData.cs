using System;

namespace Project.Scripts.SessionManagement.Data
{
    [Serializable]
    public sealed class GameData
    {
        public int? CurrencyAmount;

        public GameData(int currencyAmount)
        {
            CurrencyAmount = currencyAmount;
        }

        public int GetCurrencyAmountOrDefault(int defaultValue)
        {
            if (!CurrencyAmount.HasValue || CurrencyAmount.Value < 0)
            {
                return defaultValue;
            }

            return CurrencyAmount.Value;
        }
    }
}
