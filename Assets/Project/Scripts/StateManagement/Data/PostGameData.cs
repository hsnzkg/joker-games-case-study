using System;
using Project.Scripts.Roulette.Simulation.State;

namespace Project.Scripts.StateManagement.Data
{
    [Serializable]
    public struct PostGameData
    {
        public PostGameState State;
        public SimulationState SimulationData;

        public static PostGameData Empty => new() { State = PostGameState.None };

        public bool HasSimulationData => State != PostGameState.None && SimulationData.Buffer > 0 && SimulationData.FrameCount > 0 && SimulationData.BallStates != null && SimulationData.DeskStates != null;

        public bool Matches(PostGameState state)
        {
            return State == state && HasSimulationData;
        }

        public static PostGameData Create(PostGameState state, in SimulationState simulationState)
        {
            return new PostGameData { State = state, SimulationData = simulationState };
        }

        public bool TryGetSimulationState(out SimulationState simulationState)
        {
            simulationState = SimulationData;
            return HasSimulationData;
        }
    }
}