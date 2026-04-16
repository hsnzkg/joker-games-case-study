using Project.Scripts.Physic;
using UnityEngine;
using SimulationMode = Project.Scripts.Physic.SimulationMode;

namespace Project.Scripts.RouletteBall
{
    public class Ball : MonoBehaviour, ISimulationObject
    {
        private BallVisualSystem m_ballVisualSystem;
        private BallPhysicSystem m_ballPhysicSystem;
        private SimulationReplayPlayer<BallState> m_replayPlayer;
        private SimulationMode SimulationMode { get; set; }

        #region Unity Callbacks

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        #endregion

        private void Initialize()
        {
            m_ballVisualSystem = new BallVisualSystem(gameObject);
            m_ballPhysicSystem = new BallPhysicSystem(gameObject);
            m_replayPlayer = new SimulationReplayPlayer<BallState>(new BallReplayAdapter(this));
        }

        public void ChangeSimulationMode(SimulationMode mode)
        {
            m_ballVisualSystem.ChangeVisualState(mode == SimulationMode.Replay);
            if (mode == SimulationMode.Replay)
            {
                m_ballPhysicSystem.Stop();
            }
            else
            {
                m_replayPlayer.Stop();
            }

            SimulationMode = mode;
        }

        public void Tick(float delta)
        {
            if (SimulationMode == SimulationMode.Replay)
            {
                m_replayPlayer.Tick(delta);
            }
        }

        public void Start()
        {
            m_ballPhysicSystem.Start();
        }

        public void Stop()
        {
            m_ballPhysicSystem.Stop();
        }

        public void Replay(SimulationState simulationState)
        {
            ChangeSimulationMode(SimulationMode.Replay);
            m_replayPlayer.Play(simulationState.BallStates, simulationState.FrameCount, simulationState.TickDuration);
        }

        public void Launch(Vector3 dir, float force)
        {
            m_ballPhysicSystem.Launch(dir, force);
        }
    }
}
