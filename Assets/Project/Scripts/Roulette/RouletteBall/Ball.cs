using Project.Scripts.Roulette.Simulation;
using Project.Scripts.Roulette.Simulation.Replay;
using Project.Scripts.Roulette.Simulation.Replay.Core;
using Project.Scripts.Roulette.Simulation.State;
using UnityEngine;
using SimulationMode = Project.Scripts.Roulette.Simulation.SimulationMode;

namespace Project.Scripts.Roulette.RouletteBall
{
    public class Ball : MonoBehaviour, ISimulationObject
    {
        private BallVisualSystem m_ballVisualSystem;
        private BallPhysicSystem m_ballPhysicSystem;
        private SimulationReplayPlayer<BallState> m_replayPlayer;
        private SimulationMode m_simulationMode;
        public event System.Action<ISimulationObject> OnReplayStarted;
        public event System.Action<ISimulationObject> OnReplayEnded;
        public bool IsReplaying => m_replayPlayer != null && m_replayPlayer.IsPlaying;

        #region Unity Callbacks

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            UnregisterReplayPlayerCallbacks();
        }

        #endregion

        #region Simulation Object

        public void Initialize()
        {
            m_ballVisualSystem = new BallVisualSystem(gameObject);
            m_ballPhysicSystem = new BallPhysicSystem(gameObject);
            m_replayPlayer = new SimulationReplayPlayer<BallState>(new BallReplayAdapter(this));
            RegisterReplayPlayerCallbacks();
        }

        public void Enable()
        {
            m_ballPhysicSystem.Start();
        }

        public void Disable()
        {
            m_ballPhysicSystem.Stop();
        }

        public void Tick(float delta)
        {
            if (m_simulationMode == SimulationMode.Replay)
            {
                m_replayPlayer.Tick(delta);
            }
        }

        public void ChangeSimulationMode(SimulationMode mode)
        {
            m_simulationMode = mode;
            if (m_simulationMode == SimulationMode.Replay)
            {
                m_ballPhysicSystem.Stop();
            }
            else
            {
                m_replayPlayer.Stop();
            }

            m_ballVisualSystem.ChangeVisualState(mode == SimulationMode.Replay);
        }
        
        public void ResetSimulationObject()
        {
            m_ballPhysicSystem.Reset();
        }

        public void Replay(SimulationState simulationState, float replayTickDuration, float replayInterpolationFactor)
        {
            ChangeSimulationMode(SimulationMode.Replay);
            m_replayPlayer.Play(simulationState.BallStates, simulationState.FrameCount, replayTickDuration, replayInterpolationFactor);
        }

        #endregion

        public void Launch(Vector3 fromPos, Quaternion fromRot, Vector3 dir, float force)
        {
            ChangeSimulationMode(SimulationMode.Simulation);
            m_ballPhysicSystem.Launch(fromPos, fromRot, dir, force);
        }

        private void RegisterReplayPlayerCallbacks()
        {
            m_replayPlayer.OnReplayStarted += HandleReplayStarted;
            m_replayPlayer.OnReplayEnded += HandleReplayEnded;
        }

        private void UnregisterReplayPlayerCallbacks()
        {
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
