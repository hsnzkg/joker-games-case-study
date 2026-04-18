using Project.Scripts.Camera;
using Project.Scripts.Roulette.Game.StateMachine.Core;

namespace Project.Scripts.Roulette.Game.StateMachine.States
{
    public class Prepare : GameStateBase
    {
        public Prepare(GameStateContext context) : base(context)
        {
        }

        protected override void OnEnter()
        {
            Context.Camera.CameraFocusController.FocusComplete += OnFocusCompleted;
            Context.Camera.CameraFocusController.FocusTo(FocusType.Roulette);
        }

        protected override void OnExit()
        {
            Context.Camera.CameraFocusController.FocusComplete -= OnFocusCompleted;
            Context.Game.StopDeskReplayAlignmentRoutine();
        }

        private void OnFocusCompleted(FocusType focusType)
        {
            if (focusType != FocusType.Roulette) return;
            Context.Game.StopDeskReplayAlignmentRoutine();
            Context.Game.ResetReplayLifecycleTracking();
            Context.Game.StartAlignToReplay();
        }
    }
}
