using Project.Scripts.GUI.Core;

namespace Project.Scripts.GUI.Bet
{
    public class BetSystem : SystemBase<BetModel, BetView, BetController>
    {
        protected override BetModel CreateModel()
        {
            return new BetModel();
        }

        protected override BetController CreateController(BetView view, BetModel model)
        {
            return new BetController(view, model);
        }
    }
}