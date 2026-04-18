using Project.Scripts.EventBus;
using Project.Scripts.EventBus.Events.Replay;
using Project.Scripts.Roulette.Game.StateMachine.Core;

namespace Project.Scripts.Roulette.Game.StateMachine.States
{
    public class Replay : GameStateBase
    {
        public Replay(GameStateContext context) : base(context)
        {
        }

        protected override void OnEnter()
        {
            Context.Game.StartReplay();
        }

        protected override void OnExit()
        {
            EventBus<EReplayEnd>.Raise(new EReplayEnd());
        }
    }
}
