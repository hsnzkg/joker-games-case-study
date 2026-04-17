using Project.Scripts.GUI.Core;
using Project.Scripts.Observable;

namespace Project.Scripts.GUI.Operations
{
    public class OperationTabModel : IModel
    {
        public Observable<bool> IsGameRunning;

        public OperationTabModel()
        {
            IsGameRunning = new Observable<bool>();
        }
    }
}