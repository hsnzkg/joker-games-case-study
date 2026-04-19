using Project.Scripts.SessionManagement.Data;

namespace Project.Scripts.EventBus.Events.GameState
{
    public readonly struct EStatisticsChanged : IEvent
    {
        public StatisticsData Statistics { get; }

        public EStatisticsChanged(StatisticsData statistics)
        {
            Statistics = statistics;
        }
    }
}
