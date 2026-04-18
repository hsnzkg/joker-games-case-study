using Project.Scripts.EventBus;
using Project.Scripts.EventBus.Events.GameState;
using Project.Scripts.EventBus.Events.GUI;
using Project.Scripts.Roulette.Game.StateMachine.Core;
using Project.Scripts.SessionManagement.Data;

namespace Project.Scripts.Roulette.Game.StateMachine.States
{
    public class Bet : GameStateBase
    {
        private readonly EventBind<EPlayPress> m_playPressedBind;
        protected override GameSessionStateType StateType => GameSessionStateType.Bet;

        public Bet(GameStateContext context) : base(context)
        {
            m_playPressedBind = new EventBind<EPlayPress>(OnPlayPressed);
        }

        protected override void OnEnter()
        {
            EventBus<EPlayPress>.Register(m_playPressedBind);

            if (Context.ShouldResumeFromPostGameData)
            {
                Context.ShouldResumeFromPostGameData = false;
                Context.Game.StartGame();
            }
            else
            {
                Context.Game.ClearSessionSimulationState(StateType);
            }
        }

        protected override void OnExit()
        {
            EventBus<EPlayPress>.Unregister(m_playPressedBind);
            EventBus<EBetExit>.Raise(new EBetExit());
        }

        private void OnPlayPressed()
        {
            Context.Game.StartGame();
        }
    }
}
