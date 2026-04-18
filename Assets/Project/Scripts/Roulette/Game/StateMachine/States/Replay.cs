using Project.Scripts.EventBus;
using Project.Scripts.EventBus.Events.Replay;
using Project.Scripts.Roulette.Game.StateMachine.Core;

namespace Project.Scripts.Roulette.Game.StateMachine.States
{
    using Project.Scripts.StateManagement.Data;

    public class Replay : GameStateBase
    {
        protected override PostGameState? PersistedState => PostGameState.Replay;

        public Replay(GameStateContext context) : base(context)
        {
        }

        protected override void OnEnter()
        {
            Load();
            Context.Game.StartReplay();
        }

        protected override void OnExit()
        {
            EventBus<EReplayEnd>.Raise(new EReplayEnd());
        }
    }
}
