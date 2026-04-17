using Project.Scripts.Event;
using Project.Scripts.Event.Events.Camera;
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
        private bool m_ballReplayStarted;
        private bool m_ballReplayEnded;
        private bool m_deskReplayStarted;
        private bool m_deskReplayEnded;
        private SimulationState m_pendingReplayStartState;
        private bool m_hasPendingReplayStartState;

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
            EventBus.Subscribe<ECameraFocusEnd>(StartGame);
            SubscribeReplayCallbacks(m_ball);
            SubscribeReplayCallbacks(m_desk);
        }

        private void Unregister()
        {
            EventBus.Unsubscribe<ECameraFocusEnd>(StartGame);
            UnsubscribeReplayCallbacks(m_ball);
            UnsubscribeReplayCallbacks(m_desk);
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
            StopActiveReplayIfNeeded();
            ResetReplayLifecycleTracking(simulationState);
            float replayTickDuration = GetReplayTickDuration(simulationState);

            m_ball.Disable();
            m_desk.Disable();

            m_desk.ResetSimulationObject();
            m_ball.ResetSimulationObject();

            m_desk.Replay(simulationState, replayTickDuration);
            m_ball.Replay(simulationState, replayTickDuration);
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

        private void SubscribeReplayCallbacks(ISimulationObject simulationObject)
        {
            simulationObject.OnReplayStarted += OnSimulationObjectReplayStarted;
            simulationObject.OnReplayEnded += OnSimulationObjectReplayEnded;
        }

        private void UnsubscribeReplayCallbacks(ISimulationObject simulationObject)
        {
            simulationObject.OnReplayStarted -= OnSimulationObjectReplayStarted;
            simulationObject.OnReplayEnded -= OnSimulationObjectReplayEnded;
        }

        private void ResetReplayLifecycleTracking(in SimulationState simulationState)
        {
            m_ballReplayStarted = false;
            m_ballReplayEnded = false;
            m_deskReplayStarted = false;
            m_deskReplayEnded = false;
            m_pendingReplayStartState = simulationState;
            m_hasPendingReplayStartState = true;
            m_isReplayRunning = false;
        }

        private void StopActiveReplayIfNeeded()
        {
            if (!m_isReplayRunning)
            {
                return;
            }

            m_ball?.ChangeSimulationMode(SimulationMode.Simulation);
            m_desk?.ChangeSimulationMode(SimulationMode.Simulation);
        }

        private void OnSimulationObjectReplayStarted(ISimulationObject simulationObject)
        {
            if (simulationObject == m_ball)
            {
                m_ballReplayStarted = true;
            }
            else if (simulationObject == m_desk)
            {
                m_deskReplayStarted = true;
            }

            if (!m_hasPendingReplayStartState || m_isReplayRunning || !HaveAllReplayObjectsStarted())
            {
                return;
            }

            NotifyReplayStarted(m_pendingReplayStartState);
        }

        private void OnSimulationObjectReplayEnded(ISimulationObject simulationObject)
        {
            if (simulationObject == m_ball)
            {
                m_ballReplayEnded = true;
            }
            else if (simulationObject == m_desk)
            {
                m_deskReplayEnded = true;
            }

            if (!m_isReplayRunning || !HaveAllReplayObjectsEnded())
            {
                return;
            }

            NotifyReplayEnded();
        }

        private bool HaveAllReplayObjectsStarted()
        {
            bool ballStarted = m_ball == null || m_ballReplayStarted;
            bool deskStarted = m_desk == null || m_deskReplayStarted;
            return ballStarted && deskStarted;
        }

        private bool HaveAllReplayObjectsEnded()
        {
            bool ballEnded = m_ball == null || m_ballReplayEnded;
            bool deskEnded = m_desk == null || m_deskReplayEnded;
            return ballEnded && deskEnded;
        }

        private void NotifyReplayStarted(in SimulationState simulationState)
        {
            m_isReplayRunning = true;
            m_hasPendingReplayStartState = false;
            EventBus.Publish(new EReplayStart(simulationState));
        }

        private void NotifyReplayEnded()
        {
            if (!m_isReplayRunning) return;
            m_isReplayRunning = false;
            m_hasPendingReplayStartState = false;
            EventBus.Publish<EReplayEnd>();
        }
    }
}