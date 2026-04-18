using Project.Scripts.Camera;
using Project.Scripts.Roulette.Game.StateMachine.Core;

namespace Project.Scripts.Roulette.Game.StateMachine.States
{
    public class Prepare : GameStateBase
    {
        public Prepare(GameStateContext context) : base(context)
        {
            Context.Camera.CameraFocusController.FocusComplete += OnFocusCompleted;
        }

        private void OnFocusCompleted(FocusType obj)
        {
            if (obj == FocusType.Roulette)
            {
            }
            else
            {
            }
        }

        protected override void OnEnter()
        {
            Context.Camera.CameraFocusController.FocusTo(FocusType.Roulette);
        }
    }
}