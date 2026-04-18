using System;
using Project.Scripts.Roulette.Simulation.State;

namespace Project.Scripts.Roulette.Simulation
{
    public interface ISimulationObject
    {
        event Action<ISimulationObject> OnReplayStarted;
        event Action<ISimulationObject> OnReplayEnded;
        public void Initialize();
        public void ChangeSimulationMode(SimulationMode mode);
        public void Tick(float delta);
        public void Replay(SimulationState simulationState,float replayTickDuration, float replayInterpolationFactor);
        public void Enable();
        public void Disable();
        public void ResetSimulationObject();
    }
}
