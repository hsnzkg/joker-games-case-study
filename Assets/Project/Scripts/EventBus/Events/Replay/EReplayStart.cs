using Project.Scripts.Roulette.Simulation.State;

namespace Project.Scripts.EventBus.Events.Replay
{
    public struct EReplayStart : IEvent
    {
        public readonly SimulationState SimulationState;

        public EReplayStart(SimulationState simulationState)
        {
            SimulationState = simulationState;
        }
    }
}