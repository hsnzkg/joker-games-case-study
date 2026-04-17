using Project.Scripts.GUI.Core;

namespace Project.Scripts.GUI.Operations
{
    public class OperationTabSystem : SystemBase<OperationTabModel,OperationTabView,OperationTabController>
    {
        protected override OperationTabModel CreateModel()
        {
            return new OperationTabModel();
        }

        protected override OperationTabController CreateController(OperationTabView view, OperationTabModel model)
        {
            return new OperationTabController(view, model);
        }
    }
}