using Project.Scripts.Currency;
using Project.Scripts.EventBus;
using Project.Scripts.EventBus.Events.Currency;
using Project.Scripts.EventBus.Events.GameState;
using Project.Scripts.GUI.Core;
using Project.Scripts.SessionManagement;
using Project.Scripts.SessionManagement.Data;

namespace Project.Scripts.GUI.Statistics
{
    public class StatisticsController : ControllerBase<StatisticsView, StatisticsModel>
    {
        private readonly EventBind<ECurrencyChange> m_currencyChangedBind;
        private readonly EventBind<EStatisticsChanged> m_statisticsChangedBind;

        public StatisticsController(StatisticsView view, StatisticsModel model) : base(view, model)
        {
            m_currencyChangedBind = new EventBind<ECurrencyChange>(OnCurrencyChanged);
            m_statisticsChangedBind = new EventBind<EStatisticsChanged>(OnStatisticsChanged);
        }

        public override void Enable()
        {
            RefreshModel();
            SubscribeModel();
            EventBus<ECurrencyChange>.Register(m_currencyChangedBind);
            EventBus<EStatisticsChanged>.Register(m_statisticsChangedBind);
        }

        public override void Disable()
        {
            Model.TotalSpinCount.Unsubscribe(OnTotalSpinCountChanged);
            Model.TotalWinCount.Unsubscribe(OnWinLoseChanged);
            Model.TotalLoseCount.Unsubscribe(OnWinLoseChanged);
            Model.OverallProfit.Unsubscribe(OnOverallProfitChanged);
            Model.Currency.Unsubscribe(OnCurrencyModelChanged);
            EventBus<ECurrencyChange>.Unregister(m_currencyChangedBind);
            EventBus<EStatisticsChanged>.Unregister(m_statisticsChangedBind);
        }

        private void SubscribeModel()
        {
            Model.TotalSpinCount.Subscribe(OnTotalSpinCountChanged, true);
            Model.TotalWinCount.Subscribe(OnWinLoseChanged, true);
            Model.TotalLoseCount.Subscribe(OnWinLoseChanged, true);
            Model.OverallProfit.Subscribe(OnOverallProfitChanged, true);
            Model.Currency.Subscribe(OnCurrencyModelChanged, true);
        }

        private void RefreshModel()
        {
            if (DataSerializer.TryLoadGameData(out GameData gameData) && gameData != null)
            {
                Model.SetStatistics(gameData.GetStatisticsOrDefault());
            }
            else
            {
                Model.SetStatistics(new StatisticsData());
            }

            Model.Currency.Value = CurrencyManager.Instance.GetAmount();
        }

        private void OnCurrencyChanged(ECurrencyChange currencyChange)
        {
            Model.Currency.Value = currencyChange.NewAmount;
        }

        private void OnStatisticsChanged(EStatisticsChanged statisticsChanged)
        {
            Model.SetStatistics(statisticsChanged.Statistics);
        }

        private void OnTotalSpinCountChanged(int totalSpinCount)
        {
            View.SetTotalSpinCount(totalSpinCount);
        }

        private void OnWinLoseChanged(int _)
        {
            View.SetTotalWinLose(Model.TotalWinCount.Value, Model.TotalLoseCount.Value);
        }

        private void OnOverallProfitChanged(int overallProfit)
        {
            View.SetOverallProfit(overallProfit);
        }

        private void OnCurrencyModelChanged(int currency)
        {
            View.SetCurrency(currency);
        }
    }
}
