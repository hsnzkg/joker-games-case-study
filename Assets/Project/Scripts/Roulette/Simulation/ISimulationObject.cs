using Project.Scripts.Roulette.Simulation.State;

namespace Project.Scripts.Roulette.Simulation
{
    public interface ISimulationObject
    {
        public void Initialize();
        public void ChangeSimulationMode(SimulationMode mode);
        public void Tick(float delta);
        public void Replay(SimulationState simulationState);
        public void Enable();
        public void Disable();
        public void ResetSimulationObject();
    }
}