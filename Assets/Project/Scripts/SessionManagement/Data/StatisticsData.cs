using System;
using System.Collections.Generic;
using Project.Scripts.Roulette.Desk;

namespace Project.Scripts.SessionManagement.Data
{
    [Serializable]
    public sealed class StatisticsData
    {
        public int TotalSpinCount;
        public List<SlotInfo> SpinResults = new();
        public int TotalWinCount;
        public int TotalLoseCount;
        public int OverallProfit;

        public void RegisterSpin(SlotInfo slotInfo, bool isWin, int netProfit)
        {
            EnsureInitialized();
            TotalSpinCount++;
            SpinResults.Add(slotInfo);
            OverallProfit += netProfit;

            if (isWin)
            {
                TotalWinCount++;
                return;
            }

            TotalLoseCount++;
        }

        public void EnsureInitialized()
        {
            SpinResults ??= new List<SlotInfo>();
        }
    }
}
