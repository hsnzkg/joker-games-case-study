namespace Project.Scripts.Physic
{
    public interface ISimulationObject
    {
        public void Initialize();
        public void ChangeSimulationMode(SimulationMode mode);
        public void Tick(float delta);
        public void Enable();
        public void Disable();
        public void ResetSimulationObject();
    }
}