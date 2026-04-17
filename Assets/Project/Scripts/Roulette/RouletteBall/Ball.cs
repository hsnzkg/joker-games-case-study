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

        #region Unity Callbacks
        
        private void Update()
        {
            Tick(Time.deltaTime);
        }

        #endregion

        public void Initialize()
        {
            m_ballVisualSystem = new BallVisualSystem(gameObject);
            m_ballPhysicSystem = new BallPhysicSystem(gameObject);
            m_replayPlayer = new SimulationReplayPlayer<BallState>(new BallReplayAdapter(this));
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

        public void Tick(float delta)
        {
            if (m_simulationMode == SimulationMode.Replay)
            {
                m_replayPlayer.Tick(delta);
            }
        }

        public void Enable()
        {
            m_ballPhysicSystem.Start();
        }

        public void Disable()
        {
            m_ballPhysicSystem.Stop();
        }

        public void ResetSimulationObject()
        {
            m_ballPhysicSystem.Reset();
        }

        public void Replay(SimulationState simulationState)
        {
            ChangeSimulationMode(SimulationMode.Replay);
            m_replayPlayer.Play(simulationState.BallStates, simulationState.FrameCount, simulationState.TickDuration);
        }

        public void Launch(Vector3 fromPos,Quaternion fromRot, Vector3 dir, float force)
        {
            ChangeSimulationMode(SimulationMode.Simulation);
            m_ballPhysicSystem.Launch(fromPos, fromRot, dir, force);
        }
    }
}
