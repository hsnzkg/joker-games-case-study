using Project.Scripts.EventBus;
using Project.Scripts.EventBus.Events.GUI;
using Project.Scripts.EventBus.Events.Replay;
using Project.Scripts.GUI.Core;

namespace Project.Scripts.GUI.Operations
{
    public class OperationTabController : ControllerBase<OperationTabView, OperationTabModel>
    {
        private readonly EventBind<EReplayEnd> m_replayEndedBind;

        public OperationTabController(OperationTabView view, OperationTabModel model) : base(view, model)
        {
            m_replayEndedBind = new EventBind<EReplayEnd>(OnReplayEnded);
        }

        public override void Enable()
        {
            View.PlayPressed += OnPlayPressed;
            View.UndoPressed += OnUndoPressed;
            View.ResetPressed += OnResetPressed;
            EventBus<EReplayEnd>.Register(m_replayEndedBind);
        }

        public override void Disable()
        {
            View.PlayPressed -= OnPlayPressed;
            View.UndoPressed -= OnUndoPressed;
            View.ResetPressed -= OnResetPressed;
            EventBus<EReplayEnd>.Unregister(m_replayEndedBind);
        }

        private void OnPlayPressed()
        {
            View.SetOperationInteractivity(false);
            EventBus<EPlayPress>.Raise(new EPlayPress());
        }

        private void OnUndoPressed()
        {
            EventBus<EUndoPressed>.Raise(new EUndoPressed());
        }

        private void OnResetPressed()
        {
            EventBus<EResetPress>.Raise(new EResetPress());
        }

        private void OnReplayEnded()
        {
            View.SetOperationInteractivity(true);
        }
    }
}