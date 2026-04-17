using Project.Scripts.Event;
using Project.Scripts.Event.Events.GUI;
using Project.Scripts.Event.Events.Replay;
using Project.Scripts.GUI.Core;

namespace Project.Scripts.GUI.Operations
{
    public class OperationTabController : ControllerBase<OperationTabView,OperationTabModel> 
    {
        public OperationTabController(OperationTabView view, OperationTabModel model) : base(view, model)
        {
        }

        public override void Enable()
        {
            View.PlayPressed += OnPlayPressed;
            View.UndoPressed += OnUndoPressed;
            View.ResetPressed += OnResetPressed;
            EventBus.Subscribe<EReplayEnd>(OnReplayEnded);
        }

        public override void Disable()
        {
            View.PlayPressed -= OnPlayPressed;
            View.UndoPressed -= OnUndoPressed;
            View.ResetPressed -= OnResetPressed;
            EventBus.Unsubscribe<EReplayEnd>(OnReplayEnded);
        }

        private void OnPlayPressed()
        {
            View.SetOperationInteractivity(false);
            EventBus.Publish<EPlayPress>();
        }

        private void OnUndoPressed()
        {
            EventBus.Publish<EUndoPressed>();
        }

        private void OnResetPressed()
        {
            EventBus.Publish<EResetPress>();
        }

        private void OnReplayEnded()
        {
            View.SetOperationInteractivity(true);
        }
    }
}