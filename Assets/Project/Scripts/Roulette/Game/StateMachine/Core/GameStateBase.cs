using Project.Scripts.HFSM;

namespace Project.Scripts.Roulette.Game.StateMachine.Core
{
    public class GameStateBase : StateBase
    {
        protected GameStateContext Context;

        public GameStateBase(GameStateContext context)
        {
            Context = context;
        }
    }
}