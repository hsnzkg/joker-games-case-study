using Project.Scripts.Roulette.Game.StateMachine.Core;
using Project.Scripts.Roulette.Simulation.State;

namespace Project.Scripts.Roulette.Game.StateMachine.States
{
    public class Simulation : GameStateBase
    {
        public Simulation(GameStateContext context) : base(context)
        {
        }

        protected override void OnEnter()
        {
            if (Context.Game.TryCreateSimulationState(out SimulationState simulationState))
            {
                Context.Game.SetLastSimulationState(simulationState);
                RouletteGame.StateMachine.ChangeState<Prepare>();
            }
            else
            {
                Context.Game.ClearLastSimulationState();
                RouletteGame.StateMachine.ChangeState<Bet>();
            }
        }
    }
}
