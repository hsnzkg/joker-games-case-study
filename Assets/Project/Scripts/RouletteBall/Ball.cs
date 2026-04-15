using Project.Scripts.Physic;
using UnityEngine;

namespace Project.Scripts.RouletteBall
{
    public class Ball : MonoBehaviour, ISimulationObject
    {
        public BallVisualSystem BallVisualSystem;
        public BallPhysicSystem BallPhysicSystem;
        private SimulationMode SimulationMode { get; set; }

        SimulationMode ISimulationObject.SimulationMode
        {
            get => SimulationMode;
            set => ChangeSimulationMode(value);
        }

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

        public void ChangeSimulationMode(SimulationMode mode)
        {
            if (mode == SimulationMode.Script)
            {
                BallVisualSystem.ChangeVisualState(mode == SimulationMode.FixedUpdate);
            }
            SimulationMode = mode;
        }
    }
}