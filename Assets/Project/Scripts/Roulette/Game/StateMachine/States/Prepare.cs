using Project.Scripts.Camera;
using Project.Scripts.Roulette.Game.StateMachine.Core;

namespace Project.Scripts.Roulette.Game.StateMachine.States
{
    using Project.Scripts.StateManagement.Data;

    public class Prepare : GameStateBase
    {
        protected override PostGameState? PersistedState => PostGameState.Prepare;

        public Prepare(GameStateContext context) : base(context)
        {
        }

        protected override void OnEnter()
        {
            Load();
            Context.Game.StopReplayIfRunning();
            Context.Game.ResetBallToLaunchTransform();
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
