using Project.Scripts.GUI.Core;
using Project.Scripts.Observable;

namespace Project.Scripts.GUI.Operations
{
    public class OperationTabModel : IModel
    {
        public readonly Observable<string> DeterministicNumberText;

        public OperationTabModel()
        {
            DeterministicNumberText = new Observable<string>(string.Empty);
        }
    }
}