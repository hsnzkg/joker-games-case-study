using Project.Scripts.HFSM;
using Project.Scripts.Roulette.Simulation.State;
using Project.Scripts.StateManagement;
using Project.Scripts.StateManagement.Data;

namespace Project.Scripts.Roulette.Game.StateMachine.Core
{
    public class GameStateBase : StateBase
    {
        protected readonly GameStateContext Context;
        
        protected GameStateBase(GameStateContext context)
        {
            Context = context;
        }

        protected virtual PostGameState? PersistedState => null;
        public bool CanPersistPostGameData => PersistedState.HasValue;

        public virtual void Save()
        {
            if (!PersistedState.HasValue)
            {
                return;
            }

            if (!Context.Game.TryGetLastSimulationState(out SimulationState simulationState))
            {
                return;
            }

            PostGameData postGameData = PostGameData.Create(PersistedState.Value, simulationState);
            Context.CurrentPostGameData = postGameData;
            DataSerializer.SavePostGameData(postGameData);
        }

        protected virtual void Load()
        {
            TryLoadPostGameData();
        }

        protected bool TryLoadPostGameData()
        {
            if (!Context.CurrentPostGameData.HasSimulationData)
            {
                return false;
            }

            if (!Context.CurrentPostGameData.TryGetSimulationState(out SimulationState simulationState))
            {
                return false;
            }

            Context.Game.SetLastSimulationState(simulationState);
            return true;
        }
    }
}
