using Project.Scripts.RouletteBall;
using Project.Scripts.RouletteDesk;
using UnityEngine;

namespace Project.Scripts
{
    public class RouletteGame : MonoBehaviour
    {
        [SerializeField] private Project.Scripts.Physic.SimulationMode m_simulationMode = Physic.SimulationMode.Simulation;
        [SerializeField] private Vector3 m_ballDir;
        [SerializeField] private float m_ballForce;
        [SerializeField] private float m_rotateSpeed;
        [SerializeField] private float m_drag;
        [SerializeField] private Physic.PhysicSimulator m_physicSimulator;
        [SerializeField] private Ball m_ball;
        [SerializeField] private Desk m_desk;
        
        private void Start()
        {
            if (m_simulationMode == Physic.SimulationMode.Replay)
            {
                if (m_physicSimulator == null)
                {
                    Debug.LogWarning("Replay mode selected but PhysicSimulator is null. Falling back to Simulation.");
                    StartSimulation();
                    return;
                }

                Physic.SimulationState simulationState = m_physicSimulator.SimulationState;
                if (simulationState.FrameCount <= 0)
                {
                    Debug.LogWarning("Replay mode selected but SimulationState has no frames. Falling back to Simulation.");
                    StartSimulation();
                    return;
                }

                m_ball.Replay(simulationState);
                m_desk.Replay(simulationState);
                return;
            }

            StartSimulation();
        }

        private void StartSimulation()
        {
            m_ball.Launch(m_ballDir, m_ballForce);
            m_desk.StartSpin(m_rotateSpeed, m_drag);
        }
    }
}
