using UnityEngine;

namespace Project.Scripts.Physic
{
    public interface ISimulationObject
    {
        public SimulationMode SimulationMode { get; set; }
        public void ChangeSimulationMode(SimulationMode mode);
    }
}