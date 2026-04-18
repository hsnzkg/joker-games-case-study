using Project.Scripts.EventBus;
using Project.Scripts.EventBus.Events.GUI;
using Project.Scripts.HFSM;
using Project.Scripts.Roulette.Game.StateMachine.Core;

namespace Project.Scripts.Roulette.Game.StateMachine.States
{
    public class Bet : GameStateBase
    {
        public Bet(GameStateContext context) : base(context)
        {
            EventBind<EPlayPress> playPressedBind = new(OnPlayPressed);
            EventBus<EPlayPress>.Register(playPressedBind);
        }

        private void OnPlayPressed()
        {
            RouletteGame.StateMachine.ChangeState<Simulation>();
        }
    }
}