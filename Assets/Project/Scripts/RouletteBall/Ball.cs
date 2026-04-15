using Project.Scripts.Physic;
using UnityEngine;

namespace Project.Scripts.RouletteBall
{
    public class Ball : MonoBehaviour, ISimulationObject
    {
        public BallVisualSystem BallVisualSystem;
        public BallPhysicSystem BallPhysicSystem;
        private SimulationMode m_simulationMode;

        #region Unity Callbacks

        private void Awake()
        {
            Initialize();
        }

        #endregion
        
        private void Initialize()
        {
            BallVisualSystem = new BallVisualSystem(gameObject);
            BallPhysicSystem = new BallPhysicSystem(gameObject);
        }

        public SimulationMode SimulationMode
        {
            get => m_simulationMode;
            set => m_simulationMode = value;
        }

        public void ChangeSimulationMode(SimulationMode mode)
        {
            if (mode == SimulationMode.Script)
            {
                BallVisualSystem.ChangeVisualState(mode == SimulationMode.FixedUpdate);
            }
            m_simulationMode = mode;
        }
    }
}