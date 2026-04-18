using Project.Scripts.Roulette.Game.StateMachine.Core;
using Project.Scripts.Roulette.Simulation.State;
using Project.Scripts.SessionManagement.Data;

namespace Project.Scripts.Roulette.Game.StateMachine.States
{
    public class Simulation : GameStateBase
    {
        protected override GameSessionStateType StateType => GameSessionStateType.Simulation;
        protected override bool ShouldPersistSimulationData => true;

        public Simulation(GameStateContext context) : base(context)
        {
        }

        protected override void OnEnter()
        {
            if (TryLoadPostGameData())
            {
                RouletteGame.StateMachine.ChangeState<Prepare>();
                return;
            }

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