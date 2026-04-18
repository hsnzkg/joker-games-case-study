using System;
using Project.Scripts.Roulette.Simulation.State;

namespace Project.Scripts.StateManagement.Data
{
    [Serializable]
    public struct PostGameData
    {
        public GameSessionStateType StateType;
        public SimulationState SimulationData;

        public PostGameData(GameSessionStateType stateType)
        {
            StateType = stateType;
            SimulationData = default;
        }

        public PostGameData(GameSessionStateType stateType, SimulationState simulationState)
        {
            StateType = stateType;
            SimulationData = simulationState;
        }

        public bool HasSimulationData => SimulationData is { Buffer: > 0, FrameCount: > 0, BallStates: not null, DeskStates: not null };
        public bool CanResumeSession => HasSimulationData && StateType is GameSessionStateType.Simulation or GameSessionStateType.Prepare or GameSessionStateType.Replay;

        public bool Matches(GameSessionStateType stateType)
        {
            return StateType == stateType;
        }

        public PostGameData WithoutSimulationData(GameSessionStateType stateType)
        {
            PostGameData postGameData = this;
            postGameData.StateType = stateType;
            postGameData.SimulationData = default;
            return postGameData;
        }

        public bool TryGetSimulationState(out SimulationState simulationState)
        {
            simulationState = SimulationData;
            return HasSimulationData;
        }
    }
}