using Project.Scripts.Roulette.Simulation.State;

namespace Project.Scripts.Event.Events.Replay
{
    public readonly struct EReplayStart
    {
        public readonly SimulationState SimulationState;

        public EReplayStart(SimulationState simulationState)
        {
            SimulationState = simulationState;
        }
    }
}