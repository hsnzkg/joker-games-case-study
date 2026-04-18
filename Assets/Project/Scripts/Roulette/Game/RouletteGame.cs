using System;
using Project.Scripts.Roulette.Data;
using Project.Scripts.Roulette.RouletteBall;
using Project.Scripts.Roulette.RouletteDesk;
using Project.Scripts.Roulette.Simulation;
using Project.Scripts.Roulette.Simulation.State;
using Project.Scripts.Utility.Easing;
using System.Collections;
using Project.Scripts.Camera;
using Project.Scripts.HFSM.RuntimeMode;
using Project.Scripts.Roulette.Game.StateMachine.Core;
using Project.Scripts.Roulette.Game.StateMachine.States;
using UnityEngine;
using Random = UnityEngine.Random;
using SimulationMode = Project.Scripts.Roulette.Simulation.SimulationMode;

namespace Project.Scripts.Roulette.Game
{
    public partial class RouletteGame : MonoBehaviour, IDisposable
    {
        [Header("Game")] [Range(0f, 1f)] 
        [SerializeField] private float m_replayInterpolationFactor = 1f;
        [SerializeField] private float m_deskStartAlignmentDuration = 0.35f;

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
        private PhysicSimulator m_simulator;
        private bool m_hasLastSimulationState;
        private bool m_isReplayRunning;
        private bool m_ballReplayStarted;
        private bool m_ballReplayEnded;
        private bool m_deskReplayStarted;
        private bool m_deskReplayEnded;
        private int? m_pendingDesiredSlotIndex;
        private Coroutine m_deskReplayAlignmentRoutine;
        private GameCamera m_camera;

        private GameStateContext m_context;

        public static HFSM.StateMachine StateMachine;

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
            StopDeskReplayAlignmentRoutine();
            Unregister();
            Dispose();
        }

        private void OnDestroy()
        {
            StopDeskReplayAlignmentRoutine();
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
            m_camera = FindFirstObjectByType<GameCamera>();
            m_camera.Initialize();

            CreateStateMachine();

            m_ball.Initialize();
            m_desk.Initialize();

            m_ball.ChangeSimulationMode(SimulationMode.Replay);
            m_desk.ChangeSimulationMode(SimulationMode.Replay);

            m_simulator = new PhysicSimulator(m_predictionDeskSettings, m_predictionBallSettings.Prefab, m_predictionDeskSettings.Prefab, m_predictionMaxIterations);
        }

        private void CreateStateMachine()
        {
            StateMachine = new HFSM.StateMachine(new ManualMode(), true);
            m_context = new GameStateContext(this, m_camera);
            Bet bet = new(m_context);
            StateMachine.States.Simulation simulation = new(m_context);
            Prepare prepare = new(m_context);
            Replay replay = new(m_context);
            Result result = new(m_context);

            StateMachine.AddState(bet);
            StateMachine.AddState(simulation);
            StateMachine.AddState(prepare);
            StateMachine.AddState(replay);
            StateMachine.AddState(result);

            StateMachine.SetDefaultState(bet);
            StateMachine.ChangeState<Bet>();
        }

        private void Register()
        {
            SubscribeReplayCallbacks(m_ball);
            SubscribeReplayCallbacks(m_desk);
        }

        private void Unregister()
        {
            UnsubscribeReplayCallbacks(m_ball);
            UnsubscribeReplayCallbacks(m_desk);
        }

        public void StartGame()
        {
            m_pendingDesiredSlotIndex = null;
            StateMachine.ChangeState<Project.Scripts.Roulette.Game.StateMachine.States.Simulation>();
        }

        public void StartDeterministicGame(int slotIndex)
        {
            m_pendingDesiredSlotIndex = slotIndex;
            StateMachine.ChangeState<Project.Scripts.Roulette.Game.StateMachine.States.Simulation>();
        }

        public bool TryCreateSimulationState(out SimulationState simulationState)
        {
            int? desiredSlotIndex = m_pendingDesiredSlotIndex;
            m_pendingDesiredSlotIndex = null;

            GenerateRandomStart(out Vector3 ballDir, out float ballForce, out float spinSpeed, out float spinDrag, out float spinStartAngle);

            if (!desiredSlotIndex.HasValue)
            {
                return TrySimulate(ballDir, ballForce, spinSpeed, spinDrag, spinStartAngle, out simulationState);
            }

            if (TrySimulate(ballDir, ballForce, spinSpeed, spinDrag, spinStartAngle, out simulationState, desiredSlotIndex.Value))
            {
                return true;
            }

            Debug.LogWarning("Game failed to calculate a simulation state. Falling back to StartGame().");
            GenerateRandomStart(out ballDir, out ballForce, out spinSpeed, out spinDrag, out spinStartAngle);
            return TrySimulate(ballDir, ballForce, spinSpeed, spinDrag, spinStartAngle, out simulationState);
        }

        private bool TrySimulate(Vector3 ballDir, float ballForce, float spinSpeed, float spinDrag, float spinStartAngle, out SimulationState simulationState, int? desiredSlotIndex = null)
        {
            simulationState = default;
            simulationState = desiredSlotIndex.HasValue ? m_simulator.Simulate(ballDir, ballForce, spinSpeed, spinDrag, spinStartAngle, desiredSlotIndex.Value) : m_simulator.Simulate(ballDir, ballForce, spinSpeed, spinDrag, spinStartAngle);
            return simulationState is { BallStates: not null, FrameCount: > 0 };
        }

        public void SetLastSimulationState(SimulationState simulationState)
        {
            m_lastSimulationState = simulationState;
            m_hasLastSimulationState = simulationState is { BallStates: not null, FrameCount: > 0 };
        }

        public void ClearLastSimulationState()
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

        private void SubscribeReplayCallbacks(ISimulationObject simulationObject)
        {
            simulationObject.OnReplayEnded += OnSimulationObjectReplayEnded;
        }

        private void UnsubscribeReplayCallbacks(ISimulationObject simulationObject)
        {
            simulationObject.OnReplayEnded -= OnSimulationObjectReplayEnded;
        }

        public void ResetReplayLifecycleTracking()
        {
            m_ballReplayStarted = false;
            m_ballReplayEnded = false;

            m_deskReplayStarted = false;
            m_deskReplayEnded = false;

            m_isReplayRunning = false;
        }

        private void OnSimulationObjectReplayEnded(ISimulationObject simulationObject)
        {
            if (!m_ball.IsReplaying && !m_desk.IsReplaying)
            {
                OnReplayEnded();
            }
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

        private void OnReplayEnded()
        {
            if (!m_isReplayRunning) return;
            m_isReplayRunning = false;
            m_context.GameData.LastSlotInfo = m_lastSimulationState.FinalSlotInfo;
            StateMachine.ChangeState<Result>();
        }

        public void StartAlignToReplay()
        {
            m_deskReplayAlignmentRoutine = StartCoroutine(AlignDeskToReplayStart(StateMachine.ChangeState<Replay>));
        }

        private IEnumerator AlignDeskToReplayStart(Action onCompleted)
        {
            Transform deskTransform = m_desk.SpinTransform;
            DeskState replayStartDeskState = m_lastSimulationState.DeskStates[0];
            Vector3 startPosition = deskTransform.position;
            Vector3 startEuler = deskTransform.rotation.eulerAngles;
            Vector3 targetEuler = replayStartDeskState.Rotation.eulerAngles;
            float leftRotationDelta = GetLeftRotationDelta(startEuler.y, targetEuler.y);
            float elapsedTime = 0f;

            while (elapsedTime < m_deskStartAlignmentDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / m_deskStartAlignmentDuration);
                float easedT = EaseUtility.EaseInOutCirc(t);
                float currentYAngle = Mathf.Repeat(startEuler.y + (leftRotationDelta * easedT), 360f);
                Quaternion currentRotation = Quaternion.Euler(targetEuler.x, currentYAngle, targetEuler.z);
                deskTransform.SetPositionAndRotation(Vector3.Lerp(startPosition, replayStartDeskState.Position, easedT), currentRotation);
                yield return null;
            }

            deskTransform.SetPositionAndRotation(replayStartDeskState.Position, replayStartDeskState.Rotation);
            m_deskReplayAlignmentRoutine = null;
            onCompleted?.Invoke();
        }

        private static float GetLeftRotationDelta(float startAngle, float targetAngle)
        {
            return -Mathf.Repeat(startAngle - targetAngle, 360f);
        }

        public void StartReplay()
        {
            float tickDuration = m_lastSimulationState.TickDuration;

            m_ballReplayStarted = true;
            m_deskReplayStarted = true;
            m_isReplayRunning = true;

            m_context.GameData.LastSlotInfo = default;

            m_desk.Replay(m_lastSimulationState, tickDuration, m_replayInterpolationFactor);
            m_ball.Replay(m_lastSimulationState, tickDuration, m_replayInterpolationFactor);
        }

        public void StopDeskReplayAlignmentRoutine()
        {
            if (m_deskReplayAlignmentRoutine == null) return;
            StopCoroutine(m_deskReplayAlignmentRoutine);
            m_deskReplayAlignmentRoutine = null;
        }

        public void Dispose()
        {
            StateMachine = null;
        }
    }
}