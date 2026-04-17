using Project.Scripts.Roulette.Data;
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
        [SerializeField] private DeskSettings m_deskSettings;
        [SerializeField] private Transform m_spinTransform;
        [SerializeField] private Transform m_launchTransform;
        private SimulationMode m_simulationMode;
        private SimulationReplayPlayer<DeskState> m_replayPlayer;
        private DeskPhysicSystem m_deskPhysicSystem;
        private DeskVisualSystem m_deskVisualSystem;
        public event System.Action<ISimulationObject> OnReplayStarted;
        public event System.Action<ISimulationObject> OnReplayEnded;
        public Transform LaunchTransform => m_launchTransform;
        public Transform SpinTransform => m_spinTransform;
        public bool IsSpinning => m_deskPhysicSystem.IsEnabled;
        public bool IsReplaying => m_replayPlayer != null && m_replayPlayer.IsPlaying;
        public SimulationMode SimulationMode => m_simulationMode;

        #region Unity Callbacks

        private void FixedUpdate()
        {
            if (m_simulationMode != SimulationMode.Simulation) return;
            Tick(m_deskSettings.Tick);
        }

        private void Update()
        {
            if (m_simulationMode != SimulationMode.Replay) return;
            Tick(Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            m_deskPhysicSystem?.DrawGizmos();
        }

        private void OnDestroy()
        {
            UnregisterReplayPlayerCallbacks();
            m_deskPhysicSystem.Dispose();
        }

        #endregion

        #region Simulation Object

        public void Initialize()
        {
            m_deskPhysicSystem = new DeskPhysicSystem(this, m_deskSettings, m_spinTransform);
            m_deskVisualSystem = new DeskVisualSystem(gameObject);
            m_replayPlayer = new SimulationReplayPlayer<DeskState>(new DeskReplayAdapter(m_spinTransform));
            RegisterReplayPlayerCallbacks();
        }

        public void Enable()
        {
            m_deskPhysicSystem.Start();
        }

        public void Disable()
        {
            m_deskPhysicSystem.Stop();
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

            if (mode == SimulationMode.Simulation)
            {
                m_deskVisualSystem.ChangeVisualState(mode == SimulationMode.Replay);
            }
        }

        public void Replay(SimulationState simulationState)
        {
            Replay(simulationState, simulationState.TickDuration);
        }

        public void Replay(SimulationState simulationState, float replayTickDuration)
        {
            ChangeSimulationMode(SimulationMode.Replay);
            m_replayPlayer.Play(simulationState.DeskStates, simulationState.FrameCount, replayTickDuration);
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

        #endregion

        public void StartSpin(float deskRotationSpeed, float deskDrag, float startAngle = 0f)
        {
            ChangeSimulationMode(SimulationMode.Simulation);
            m_deskPhysicSystem.StartSpin(deskRotationSpeed, deskDrag, startAngle);
        }

        private void RegisterReplayPlayerCallbacks()
        {
            if (m_replayPlayer == null)
            {
                return;
            }

            m_replayPlayer.OnReplayStarted += HandleReplayStarted;
            m_replayPlayer.OnReplayEnded += HandleReplayEnded;
        }

        private void UnregisterReplayPlayerCallbacks()
        {
            if (m_replayPlayer == null)
            {
                return;
            }

            m_replayPlayer.OnReplayStarted -= HandleReplayStarted;
            m_replayPlayer.OnReplayEnded -= HandleReplayEnded;
        }

        private void HandleReplayStarted()
        {
            OnReplayStarted?.Invoke(this);
        }

        private void HandleReplayEnded()
        {
            OnReplayEnded?.Invoke(this);
        }
    }
}