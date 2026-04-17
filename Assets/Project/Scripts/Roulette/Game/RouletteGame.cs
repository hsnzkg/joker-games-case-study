using Project.Scripts.Event;
using Project.Scripts.Event.Events.GUI;
using Project.Scripts.Event.Events.Replay;
using Project.Scripts.Roulette.Data;
using Project.Scripts.Roulette.RouletteBall;
using Project.Scripts.Roulette.RouletteDesk;
using Project.Scripts.Roulette.Simulation;
using Project.Scripts.Roulette.Simulation.State;
using UnityEngine;
using Random = UnityEngine.Random;
using SimulationMode = Project.Scripts.Roulette.Simulation.SimulationMode;

namespace Project.Scripts.Roulette.Game
{
    public partial class RouletteGame : MonoBehaviour
    {
        [Header("Game")] 
        [SerializeField] private float m_replayDuration;
        
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
        private bool m_isReplayRunning;
        
        #region Unity Callbacks

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Register();
        }

        private void OnDisable()
        {
            Unregister();
        }

        private void LateUpdate()
        {
            if (!m_isReplayRunning || m_ball == null || m_desk == null)
            {
                return;
            }

            if (m_ball.IsReplaying || m_desk.IsReplaying)
            {
                return;
            }

            NotifyReplayEnded();
        }

        private void OnDestroy()
        {
            m_isReplayRunning = false;
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
        
        private void Register()
        {
            EventBus.Subscribe<EPlayPress>(StartGame);
        }
        
        private void Unregister()
        {
            EventBus.Unsubscribe<EPlayPress>(StartGame);
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

        public void StartDeterministicGame(int slotIndex)
        {
            GenerateRandomStart(out Vector3 ballDir, out float ballForce, out float spinSpeed, out float spinDrag, out float spinStartAngle);

            if (!TrySimulate(ballDir, ballForce, spinSpeed, spinDrag, spinStartAngle, out SimulationState simulationState, slotIndex))
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
            NotifyReplayEnded();
            float replayTickDuration = GetReplayTickDuration(simulationState);

            m_ball.Disable();
            m_desk.Disable();

            m_desk.ResetSimulationObject();
            m_ball.ResetSimulationObject();

            m_desk.Replay(simulationState, replayTickDuration);
            m_ball.Replay(simulationState, replayTickDuration);

            NotifyReplayStarted(simulationState);
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
            SlotInfo finalSlotInfo = simulationState.FinalSlotInfo;
            Debug.Log($"Simulation completed. FrameCount: [{simulationState.FrameCount}], final slot index: [{finalSlotInfo.Index}], final slot number: [{finalSlotInfo.Number}], final slot color: [{finalSlotInfo.Color}].");
        }

        private void ClearLastSimulationState()
        {
            m_lastSimulationState = default;
            m_hasLastSimulationState = false;
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

        private float GetReplayTickDuration(in SimulationState simulationState)
        {
            if (simulationState.FrameCount <= 1 || m_replayDuration <= 0f)
            {
                return simulationState.TickDuration;
            }

            float replayStepCount = simulationState.FrameCount - 1;
            return Mathf.Max(m_replayDuration / replayStepCount, Mathf.Epsilon);
        }

        private void NotifyReplayStarted(in SimulationState simulationState)
        {
            m_isReplayRunning = true;
            EventBus.Publish(new EReplayStart(simulationState));
        }

        private void NotifyReplayEnded()
        {
            if (!m_isReplayRunning) return;
            m_isReplayRunning = false;
            EventBus.Publish<EReplayEnd>();
        }
    }
}
