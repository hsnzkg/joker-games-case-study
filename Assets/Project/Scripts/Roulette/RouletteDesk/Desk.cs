using Project.Scripts.Roulette.Simulation;
using Project.Scripts.Roulette.Simulation.Replay;
using Project.Scripts.Roulette.Simulation.Replay.Core;
using Project.Scripts.Roulette.Simulation.State;
using UnityEngine;
using SimulationMode = Project.Scripts.Roulette.Simulation.SimulationMode;

namespace Project.Scripts.Roulette.RouletteDesk
{
    public class Desk : MonoBehaviour, ISimulationObject
    {
        [SerializeField] private DeskPhysicSettings m_deskPhysicSettings;
        [SerializeField] private Transform m_spinTransform;
        [SerializeField] private Transform m_launchTransform;
        private Renderer[] m_renderers;
        private SimulationMode m_simulationMode;
        private SimulationReplayPlayer<DeskState> m_replayPlayer;
        private DeskPhysicSystem m_deskPhysicSystem;
        private DeskVisualSystem m_deskVisualSystem;
        public Transform LaunchTransform => m_launchTransform;
        public Transform SpinTransform => m_spinTransform;
        public bool IsSpinning => m_deskPhysicSystem.IsEnabled;
        public SimulationMode SimulationMode => m_simulationMode;

        #region Unity Callbacks

        private void FixedUpdate()
        {
            if (m_simulationMode != SimulationMode.Simulation)
            {
                return;
            }
            Tick(m_deskPhysicSettings.Tick);
        }

        private void Update()
        {
            if (m_simulationMode != SimulationMode.Replay)
            {
                return;
            }
            Tick(Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            m_deskPhysicSystem?.DrawGizmos();
        }

        #endregion

        public void Initialize()
        {
            m_renderers = GetComponentsInChildren<Renderer>();
            m_deskPhysicSystem = new DeskPhysicSystem(this,m_deskPhysicSettings, m_spinTransform);
            m_deskVisualSystem = new DeskVisualSystem(gameObject);
            m_replayPlayer = new SimulationReplayPlayer<DeskState>(new DeskReplayAdapter(m_spinTransform));
        }

        public void ChangeSimulationMode(SimulationMode mode)
        {
            m_simulationMode = mode;
            
            if (m_simulationMode == SimulationMode.Replay)
            {
                m_deskPhysicSystem.Stop();
            }
            else
            {
                m_replayPlayer.Stop();
            }
            m_deskVisualSystem.ChangeVisualState(mode == SimulationMode.Replay);
        }

        public void Replay(SimulationState simulationState)
        {
            ChangeSimulationMode(SimulationMode.Replay);
            m_replayPlayer.Play(simulationState.DeskStates, simulationState.FrameCount, simulationState.TickDuration);
        }

        public void StartSpin(float deskRotationSpeed, float deskDrag, float startAngle = 0f)
        {
            ChangeSimulationMode(SimulationMode.Simulation);
            m_deskPhysicSystem.StartSpin(deskRotationSpeed, deskDrag, startAngle);
        }

        public void ResetSimulationObject()
        {
            m_deskPhysicSystem.Stop();
            m_deskPhysicSystem.Reset();
        }

        public void Tick(float delta)
        {
            if (m_simulationMode == SimulationMode.Simulation)
            {
                m_deskPhysicSystem.Tick(delta);
            }
            else
            {
                m_replayPlayer.Tick(delta);
            }
        }

        public void Enable()
        {
            m_deskPhysicSystem.Start();
        }

        public void Disable()
        {
            m_deskPhysicSystem.Stop();
        }
    }
}
