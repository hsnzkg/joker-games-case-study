using Project.Scripts.EventBus;
using Project.Scripts.EventBus.Events.GUI;
using Project.Scripts.Roulette.Game.StateMachine.Core;

namespace Project.Scripts.Roulette.Game.StateMachine.States
{
    public class Bet : GameStateBase
    {
        private readonly EventBind<EPlayPress> m_playPressedBind;

        public Bet(GameStateContext context) : base(context)
        {
            m_playPressedBind = new EventBind<EPlayPress>(OnPlayPressed);
        }

        protected override void OnEnter()
        {
            EventBus<EPlayPress>.Register(m_playPressedBind);
        }

        protected override void OnExit()
        {
            EventBus<EPlayPress>.Unregister(m_playPressedBind);
        }

        private void OnPlayPressed()
        {
            Context.Game.StartGame();
        }
    }
}
