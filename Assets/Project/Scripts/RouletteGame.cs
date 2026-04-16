using System;
using Project.Scripts.Physic;
using Project.Scripts.Physic.State;
using Project.Scripts.RouletteBall;
using Project.Scripts.RouletteDesk;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Project.Scripts
{
    public partial class RouletteGame : MonoBehaviour
    {
        [Header("Mode")]
        [SerializeField] private GameMode m_gameMode = GameMode.Game;

        [Header("Runtime References")]
        [SerializeField] private Ball m_ball;
        [SerializeField] private Desk m_desk;

        [Header("Prediction (Simulation Scene)")]
        [SerializeField] private DeskPhysicSettings m_predictionDeskPhysicSettings;
        [SerializeField] private Ball m_predictionBallPrefab;
        [SerializeField] private Desk m_predictionDeskPrefab;
        [SerializeField] private int m_predictionMaxIterations = 1000;

        [Header("Random Ranges")]
        [SerializeField] private Vector3 m_ballDirectionMin = new(-1f, 0f, -1f);
        [SerializeField] private Vector3 m_ballDirectionMax = new(1f, 0.35f, 1f);
        [SerializeField] private Vector2 m_ballForceRange = new(2f, 6f);
        [SerializeField] private Vector2 m_spinSpeedRange = new(60f, 140f);
        [SerializeField] private Vector2 m_spinDragRange = new(4f, 14f);
        [SerializeField] private Vector2 m_spinStartAngleRange = new(0f, 360f);
        
        private PhysicSimulator m_simulator;

        private void Awake()
        {
            m_simulator = new PhysicSimulator(m_predictionDeskPhysicSettings, m_predictionBallPrefab, m_predictionDeskPrefab, m_predictionMaxIterations);
            m_simulator.Initialize();
        }

        private void Start()
        {
            if (m_gameMode == GameMode.Predicted)
            {
                StartPredictedGame();
            }
            else
            {
                StartGame();
            }
        }

        public void StartGame()
        {
            GenerateRandomStart(out Vector3 ballDir, out float ballForce, out float spinSpeed, out float spinDrag, out float spinStartAngle);
            m_ball.Launch(ballDir, ballForce);
            m_desk.StartSpin(spinSpeed, spinDrag, spinStartAngle);
        }

        public void StartPredictedGame()
        {
            GenerateRandomStart(out Vector3 ballDir, out float ballForce, out float spinSpeed, out float spinDrag, out float spinStartAngle);
            SimulationState simulationState = m_simulator.Simulate(ballDir, ballForce, spinSpeed, spinDrag, spinStartAngle);
            if (simulationState.FrameCount <= 0)
            {
                Debug.LogWarning("Predicted simulation produced no frames. Falling back to StartGame().");
                StartGame();
                return;
            }
            m_ball.Replay(simulationState);
            m_desk.Replay(simulationState);
        }

        private void GenerateRandomStart(out Vector3 ballDir, out float ballForce, out float spinSpeed, out float spinDrag, out float spinStartAngle)
        {
            ballDir = RandomDirection();
            ballForce = RandomRange(m_ballForceRange);
            spinSpeed = RandomRange(m_spinSpeedRange);
            spinDrag = RandomRange(m_spinDragRange);
            spinStartAngle = RandomRange(m_spinStartAngleRange);
        }

        private Vector3 RandomDirection()
        {
            Vector3 v = new(
                Random.Range(m_ballDirectionMin.x, m_ballDirectionMax.x),
                Random.Range(m_ballDirectionMin.y, m_ballDirectionMax.y),
                Random.Range(m_ballDirectionMin.z, m_ballDirectionMax.z)
            );

            if (v.sqrMagnitude < 0.0001f)
            {
                v = Vector3.forward;
            }

            return v.normalized;
        }

        private static float RandomRange(Vector2 range)
        {
            float min = Mathf.Min(range.x, range.y);
            float max = Mathf.Max(range.x, range.y);
            return Random.Range(min, max);
        }
    }
}
