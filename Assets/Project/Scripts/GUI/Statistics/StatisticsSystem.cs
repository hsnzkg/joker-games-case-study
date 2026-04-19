using Project.Scripts.GUI.Core;

namespace Project.Scripts.GUI.Statistics
{
    public class StatisticsSystem : SystemBase<StatisticsModel, StatisticsView, StatisticsController>
    {
        protected override StatisticsModel CreateModel()
        {
            return new StatisticsModel();
        }

        protected override StatisticsController CreateController(StatisticsView view, StatisticsModel model)
        {
            return new StatisticsController(view, model);
        }
    }
}
