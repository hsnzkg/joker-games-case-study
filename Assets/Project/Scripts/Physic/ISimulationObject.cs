namespace Project.Scripts.Physic
{
    public interface ISimulationObject
    {
        public void ChangeSimulationMode(SimulationMode mode);
        public void Tick(float delta);
        public void Start();
        public void Stop();
        public void ResetSimulationObject();
    }
}