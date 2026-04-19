using System;

namespace Project.Scripts.SessionManagement.Data
{
    [Serializable]
    public sealed class GameData
    {
        public int? CurrencyAmount;
        public StatisticsData Statistics;

        public GameData()
        {
            Statistics = new StatisticsData();
        }

        public GameData(int currencyAmount)
            : this(currencyAmount, new StatisticsData())
        {
        }

        public GameData(int currencyAmount, StatisticsData statistics)
        {
            CurrencyAmount = currencyAmount;
            Statistics = statistics ?? new StatisticsData();
        }

        public int GetCurrencyAmountOrDefault(int defaultValue)
        {
            if (!CurrencyAmount.HasValue || CurrencyAmount.Value < 0)
            {
                return defaultValue;
            }

            return CurrencyAmount.Value;
        }

        public StatisticsData GetStatisticsOrDefault()
        {
            Statistics ??= new StatisticsData();
            Statistics.EnsureInitialized();
            return Statistics;
        }
    }
}
