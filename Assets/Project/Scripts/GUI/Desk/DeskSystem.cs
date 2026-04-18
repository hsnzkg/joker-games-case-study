using Project.Scripts.GUI.Core;

namespace Project.Scripts.GUI.Desk
{
    public class DeskSystem : SystemBase<DeskModel, DeskView, DeskController>
    {
        protected override DeskModel CreateModel()
        {
            return new DeskModel();
        }

        protected override DeskController CreateController(DeskView view, DeskModel model)
        {
            return new DeskController(view, model);
        }
    }
}
