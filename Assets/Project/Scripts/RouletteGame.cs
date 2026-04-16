using System;
using Project.Scripts.Physic;
using Project.Scripts.Physic.State;
using Project.Scripts.RouletteBall;
using Project.Scripts.RouletteDesk;
using UnityEngine;
using Random = UnityEngine.Random;
using SimulationMode = Project.Scripts.Physic.SimulationMode;

namespace Project.Scripts
{
    public partial class RouletteGame : MonoBehaviour
    {
        [Header("Mode")] 
        [SerializeField] private GameMode m_gameMode = GameMode.Game;
        [SerializeField] private int m_startDesiredSlotIndex;

        [Header("Runtime References")] 
        [SerializeField] private Ball m_ball;
        [SerializeField] private Desk m_desk;

        [Header("Simulation")] 
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

        private SimulationState m_lastSimulationState;
        private bool m_hasLastSimulationState;
        private PhysicSimulator m_simulator;


        #region Unity Callbacks

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            if (m_gameMode == GameMode.Deterministic)
            {
                StartDeterministicGame(m_startDesiredSlotIndex);
            }
            else
            {
                StartGame();
            }
        }

        private void OnDrawGizmos()
        {
            if (!m_hasLastSimulationState || m_lastSimulationState.BallStates == null || m_lastSimulationState.FrameCount <= 0)
            {
                return;
            }

            SphereCollider ballCollider = m_ball != null ? m_ball.GetComponent<SphereCollider>() : null;
            float radius = ballCollider != null ? ballCollider.radius * m_ball.transform.lossyScale.x : 0.05f;

            for (int i = 0; i < m_lastSimulationState.FrameCount; i++)
            {
                float t = m_lastSimulationState.FrameCount > 1 ? i / (float)(m_lastSimulationState.FrameCount - 1) : 0f;
                Gizmos.color = Color.Lerp(Color.green, Color.red, t);
                Gizmos.DrawWireSphere(m_lastSimulationState.BallStates[i].Position, radius);
            }
        }

        #endregion

        private void Initialize()
        {
            m_ball.Initialize();
            m_desk.Initialize();
            
            m_ball.ChangeSimulationMode(SimulationMode.Replay);
            m_desk.ChangeSimulationMode(SimulationMode.Replay);
            
            m_simulator = new PhysicSimulator(m_predictionDeskPhysicSettings, m_predictionBallPrefab, m_predictionDeskPrefab, m_predictionMaxIterations);
        }

        public void StartGame()
        {
            GenerateRandomStart(out Vector3 ballDir, out float ballForce, out float spinSpeed, out float spinDrag, out float spinStartAngle);

            if (TrySimulate(ballDir, ballForce, spinSpeed, spinDrag, spinStartAngle, out SimulationState simulationState))
            {
                SetLastSimulationState(simulationState, "StartGame");
            }
            else
            {
                ClearLastSimulationState();
                return;
            }

            PlaySimulation(simulationState);
        }

        public void StartDeterministicGame(int desiredSlotIndex)
        {
            GenerateRandomStart(out Vector3 ballDir, out float ballForce, out float spinSpeed, out float spinDrag, out float spinStartAngle);

            if (!TrySimulate(ballDir, ballForce, spinSpeed, spinDrag, spinStartAngle, out SimulationState simulationState, desiredSlotIndex))
            {
                Debug.LogWarning("StartDeterministicGame failed to calculate a simulation state. Falling back to StartGame().");
                StartGame();
                return;
            }

            SetLastSimulationState(simulationState, "StartDeterministicGame");
            PlaySimulation(simulationState);
        }

        private void PlaySimulation(in SimulationState simulationState)
        {
            m_ball.Disable();
            m_desk.Disable();

            m_desk.ResetSimulationObject();
            m_ball.ResetSimulationObject();

            m_desk.Replay(simulationState);
            m_ball.Replay(simulationState);
        }

        private bool TrySimulate(Vector3 ballDir, float ballForce, float spinSpeed, float spinDrag, float spinStartAngle, out SimulationState simulationState, int? desiredSlotIndex = null)
        {
            simulationState = default;
            simulationState = desiredSlotIndex.HasValue ? m_simulator.Simulate(ballDir, ballForce, spinSpeed, spinDrag, spinStartAngle, desiredSlotIndex.Value) : m_simulator.Simulate(ballDir, ballForce, spinSpeed, spinDrag, spinStartAngle);
            return simulationState is { BallStates: not null, FrameCount: > 0 };
        }

        private void SetLastSimulationState(SimulationState simulationState, string context)
        {
            m_lastSimulationState = simulationState;
            m_hasLastSimulationState = simulationState is { BallStates: not null, FrameCount: > 0 };
            int finalSlotIndex = GetFinalSlotIndex(simulationState);
            Debug.Log($"{context} completed. FrameCount: [{simulationState.FrameCount}], final slot index: [{finalSlotIndex}].");
        }

        private void ClearLastSimulationState()
        {
            m_lastSimulationState = default;
            m_hasLastSimulationState = false;
        }

        private static int GetFinalSlotIndex(in SimulationState simulationState)
        {
            if (simulationState.BallStates == null || simulationState.FrameCount <= 0)
            {
                return -1;
            }

            return simulationState.BallStates[simulationState.FrameCount - 1].SlotIndex;
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
            Vector3 v = new(Random.Range(m_ballDirectionMin.x, m_ballDirectionMax.x), Random.Range(m_ballDirectionMin.y, m_ballDirectionMax.y), Random.Range(m_ballDirectionMin.z, m_ballDirectionMax.z));

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
