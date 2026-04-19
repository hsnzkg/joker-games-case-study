using Project.Scripts.GUI.Core;
using Project.Scripts.Observable;
using Project.Scripts.SessionManagement.Data;

namespace Project.Scripts.GUI.Statistics
{
    public class StatisticsModel : IModel
    {
        public readonly Observable<int> TotalSpinCount;
        public readonly Observable<int> TotalWinCount;
        public readonly Observable<int> TotalLoseCount;
        public readonly Observable<int> OverallProfit;
        public readonly Observable<int> Currency;

        public StatisticsModel()
        {
            TotalSpinCount = new Observable<int>(0);
            TotalWinCount = new Observable<int>(0);
            TotalLoseCount = new Observable<int>(0);
            OverallProfit = new Observable<int>(0);
            Currency = new Observable<int>(0);
        }

        public void SetStatistics(StatisticsData statisticsData)
        {
            StatisticsData safeStatistics = statisticsData ?? new StatisticsData();
            safeStatistics.EnsureInitialized();
            TotalSpinCount.Value = safeStatistics.TotalSpinCount;
            TotalWinCount.Value = safeStatistics.TotalWinCount;
            TotalLoseCount.Value = safeStatistics.TotalLoseCount;
            OverallProfit.Value = safeStatistics.OverallProfit;
        }
    }
}
