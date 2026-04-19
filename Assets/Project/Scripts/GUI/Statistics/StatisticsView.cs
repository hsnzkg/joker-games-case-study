using Project.Scripts.GUI.Core;
using TMPro;
using UnityEngine;

namespace Project.Scripts.GUI.Statistics
{
    public class StatisticsView : ViewBase
    {
        [SerializeField] private TextMeshProUGUI m_totalSpinCountText;
        [SerializeField] private TextMeshProUGUI m_totalWinText;
        [SerializeField] private TextMeshProUGUI m_overallProfitText;
        [SerializeField] private TextMeshProUGUI m_currencyText;

        public void SetTotalSpinCount(int value)
        {
            if (m_totalSpinCountText == null)
            {
                return;
            }

            m_totalSpinCountText.text = $"Total Spin : {value}";
        }

        public void SetTotalWinLose(int winCount, int loseCount)
        {
            if (m_totalWinText == null)
            {
                return;
            }

            m_totalWinText.text = $"Win/Lose : {winCount} / {loseCount}";
        }

        public void SetOverallProfit(int value)
        {
            if (m_overallProfitText == null)
            {
                return;
            }

            m_overallProfitText.text = $"Profit : {value}";
        }

        public void SetCurrency(int value)
        {
            if (m_currencyText == null)
            {
                return;
            }

            m_currencyText.text = $"Currency : {value}";
        }
    }
}
