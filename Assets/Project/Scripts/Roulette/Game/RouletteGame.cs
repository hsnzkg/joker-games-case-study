using Project.Scripts.Roulette.Data;
using Project.Scripts.Roulette.RouletteBall;
using Project.Scripts.Roulette.RouletteDesk;
using Project.Scripts.Roulette.Simulation;
using Project.Scripts.Roulette.Simulation.State;
using Project.Scripts.Roulette.Utility;
using UnityEngine;
using Random = UnityEngine.Random;
using SimulationMode = Project.Scripts.Roulette.Simulation.SimulationMode;

namespace Project.Scripts.Roulette.Game
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
        [SerializeField] private DeskSettings m_predictionDeskSettings;
        [SerializeField] private BallSettings m_predictionBallSettings;
        [SerializeField] private int m_predictionMaxIterations = 5000;
        
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
                StartDeterministicGame(SlotColor.GREEN.GetRandomSlotInfoByColor().Index);
            }
            else
            {
                StartGame();
            }
        }

        private void OnDestroy()
        {
            m_simulator?.Dispose();
            m_simulator = null;
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

            m_simulator = new PhysicSimulator(m_predictionDeskSettings, m_predictionBallSettings.Prefab, m_predictionDeskSettings.Prefab, m_predictionMaxIterations);
        }

        public void StartGame()
        {
            GenerateRandomStart(out Vector3 ballDir, out float ballForce, out float spinSpeed, out float spinDrag, out float spinStartAngle);

            if (TrySimulate(ballDir, ballForce, spinSpeed, spinDrag, spinStartAngle, out SimulationState simulationState))
            {
                SetLastSimulationState(simulationState);
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

            SetLastSimulationState(simulationState);
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
            simulationState = desiredSlotIndex.HasValue 
                ? m_simulator.Simulate(ballDir, ballForce, spinSpeed, spinDrag, spinStartAngle, desiredSlotIndex.Value) 
                : m_simulator.Simulate(ballDir, ballForce, spinSpeed, spinDrag, spinStartAngle);
            return simulationState is { BallStates: not null, FrameCount: > 0 };
        }

        private void SetLastSimulationState(SimulationState simulationState)
        {
            m_lastSimulationState = simulationState;
            m_hasLastSimulationState = simulationState is { BallStates: not null, FrameCount: > 0 };
            int finalSlotIndex = GetFinalSlotIndex(simulationState);
            Debug.Log($"Simulation completed. FrameCount: [{simulationState.FrameCount}], final slot index: [{finalSlotIndex}].");
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
            ballForce = RandomRange(m_predictionBallSettings.ForceRange);
            spinSpeed = RandomRange(m_predictionDeskSettings.SpinSpeedRange);
            spinDrag = RandomRange(m_predictionDeskSettings.SpinDragRange);
            spinStartAngle = RandomRange(m_predictionDeskSettings.SpinStartAngleRange);
        }

        private Vector3 RandomDirection()
        {
            Vector3 directionMin = m_predictionBallSettings.DirectionMin;
            Vector3 directionMax = m_predictionBallSettings.DirectionMax;
            
            Vector3 v = new(Random.Range(directionMin.x, directionMax.x), Random.Range(directionMin.y, directionMax.y), Random.Range(directionMin.z, directionMax.z));

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
