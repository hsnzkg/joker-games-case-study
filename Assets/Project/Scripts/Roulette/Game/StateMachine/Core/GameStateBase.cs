using Project.Scripts.HFSM;
using Project.Scripts.Roulette.Simulation.State;
using Project.Scripts.SessionManagement;
using Project.Scripts.SessionManagement.Data;

namespace Project.Scripts.Roulette.Game.StateMachine.Core
{
    public class GameStateBase : StateBase
    {
        protected readonly GameStateContext Context;
        protected virtual GameSessionStateType StateType => GameSessionStateType.None;
        protected virtual bool ShouldPersistSimulationData => false;
        
        protected GameStateBase(GameStateContext context)
        {
            Context = context;
        }


        public virtual void Save()
        {
            PostGameData postGameData = new(StateType);

            if (ShouldPersistSimulationData && Context.Game.TryGetLastSimulationState(out SimulationState simulationState))
            {
                postGameData = new PostGameData(StateType, simulationState);
            }

            Context.CurrentPostGameData = postGameData;
            DataSerializer.SavePostGameData(postGameData);
        }

        protected virtual void Load()
        {
            TryLoadPostGameData();
        }

        protected bool TryLoadPostGameData()
        {
            if (!Context.CurrentPostGameData.CanResumeSession)
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
