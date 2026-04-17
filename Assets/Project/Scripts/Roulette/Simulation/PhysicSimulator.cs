using System;
using Project.Scripts.Roulette.Data;
using Project.Scripts.Roulette.RouletteBall;
using Project.Scripts.Roulette.RouletteDesk;
using Project.Scripts.Roulette.Simulation.State;
using Project.Scripts.Roulette.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Scripts.Roulette.Simulation
{
    public sealed class PhysicSimulator : IDisposable
    {
        private int m_simulationIndex;

        private readonly DeskSettings m_deskSettings;
        private readonly Ball m_ballPrefab;
        private readonly Desk m_deskPrefab;
        private readonly int m_maxIterations;

        private Scene m_simulationScene;
        private PhysicsScene m_physicsScene;
        private Ball m_ballInstance;
        private Desk m_deskInstance;
        private Rigidbody m_ballRb;
        private readonly Collider[] m_overlapResults;
        private readonly int m_ballLayerMask;

        public PhysicSimulator(DeskSettings deskSettings, Ball ballPrefab, Desk deskPrefab, int maxIterations = 100)
        {
            m_deskSettings = deskSettings;
            m_ballPrefab = ballPrefab;
            m_deskPrefab = deskPrefab;
            m_maxIterations = Mathf.Max(2, maxIterations);

            m_overlapResults = new Collider[1];
            m_ballLayerMask = ballPrefab != null ? 1 << ballPrefab.gameObject.layer : 0;

            EnsureSimulationSceneCreated();
        }

        public SimulationState Simulate(Vector3 ballForceDirection, float ballForce, float deskRotationSpeed, float deskDrag, float deskStartAngle)
        {
            return RunSimulation(ballForceDirection, ballForce, deskRotationSpeed, deskDrag, deskStartAngle);
        }

        public SimulationState Simulate(Vector3 ballForceDirection, float ballForce, float deskRotationSpeed, float deskDrag, float deskStartAngle, int desiredSlotIndex)
        {
            if (desiredSlotIndex < 0 || desiredSlotIndex >= m_deskSettings.SlotCount)
            {
                Debug.LogError($"Desired slot index [{desiredSlotIndex}] is outside valid range [0, {m_deskSettings.SlotCount - 1}]. Falling back to regular simulation.");
                return RunSimulation(ballForceDirection, ballForce, deskRotationSpeed, deskDrag, deskStartAngle);
            }

            SlotInfo desiredSlotInfo = desiredSlotIndex.GetSlotInfo();
            SimulationState simulationState = RunSimulation(ballForceDirection, ballForce, deskRotationSpeed, deskDrag, deskStartAngle);
            SettledSlotInfo settledSlotInfo = AnalyzeSettledSlot(simulationState);

            if (!settledSlotInfo.HasSettledSlot)
            {
                simulationState.FinalSlotInfo = desiredSlotInfo;
                LogVisualRemapFailed("Physical simulation did not settle inside any slot. Replay stays unchanged.", settledSlotInfo.SlotInfo.Index, desiredSlotIndex, deskStartAngle, deskStartAngle, 0f, settledSlotInfo.ContinuousStartFrame);
                return simulationState;
            }

            int sourceSlotIndex = settledSlotInfo.SlotInfo.Index;
            if (sourceSlotIndex == desiredSlotIndex)
            {
                simulationState.FinalSlotInfo = desiredSlotInfo;
                LogVisualRemapSkipped(sourceSlotIndex, desiredSlotIndex, deskStartAngle, settledSlotInfo.ContinuousStartFrame);
                return simulationState;
            }

            int slotIndexDifference = desiredSlotIndex - sourceSlotIndex;
            float slotAngle = GetSlotAngle();
            float visualDeskOffset = GetVisualDeskAngleOffset(sourceSlotIndex, desiredSlotIndex);
            float visualStartAngle = Mathf.Repeat(deskStartAngle + visualDeskOffset, 360f);

            LogVisualRemapInitial(sourceSlotIndex, desiredSlotIndex, slotIndexDifference, slotAngle, visualDeskOffset, settledSlotInfo.ContinuousStartFrame, deskStartAngle, visualStartAngle);

            SimulationState visualReplayState = CreateVisualReplayState(simulationState, slotIndexDifference, visualDeskOffset);
            visualReplayState.FinalSlotInfo = desiredSlotInfo;
            LogVisualRemapApplied(sourceSlotIndex, desiredSlotIndex, deskStartAngle, visualStartAngle, visualDeskOffset, settledSlotInfo.ContinuousStartFrame);
            return visualReplayState;
        }

        public void Dispose()
        {
            if (m_ballInstance != null)
            {
                UnityEngine.Object.Destroy(m_ballInstance.gameObject);
                m_ballInstance = null;
            }

            if (m_deskInstance != null)
            {
                UnityEngine.Object.Destroy(m_deskInstance.gameObject);
                m_deskInstance = null;
            }

            m_ballRb = null;
            m_physicsScene = default;

            if (m_simulationScene.IsValid())
            {
                SceneManager.UnloadSceneAsync(m_simulationScene);
                m_simulationScene = default;
            }
        }

        private SimulationState RunSimulation(Vector3 ballForceDirection, float ballForce, float deskRotationSpeed, float deskDrag, float deskStartAngle)
        {
            float tick = GetTickDuration();
            SimulationState simulationData = new(m_maxIterations, tick);

            UnityEngine.SimulationMode previousMode = Physics.simulationMode;
            try
            {
                Physics.simulationMode = UnityEngine.SimulationMode.Script;
                EnsureSimulationSceneCreated();

                ResetBall(ref simulationData);
                ResetDesk();

                m_ballInstance.Enable();
                m_deskInstance.Enable();

                m_deskInstance.StartSpin(deskRotationSpeed, deskDrag, deskStartAngle);
                m_ballInstance.Launch(m_deskInstance.LaunchTransform.position, m_deskInstance.LaunchTransform.rotation, ballForceDirection, ballForce);

                int slotIndex = CheckSlots();
                RecordState(ref simulationData, 0, slotIndex);

                for (int iteration = 1; iteration < m_maxIterations; iteration++)
                {
                    m_deskInstance.Tick(tick);
                    Physics.SyncTransforms();
                    m_physicsScene.Simulate(tick);

                    slotIndex = CheckSlots();
                    RecordState(ref simulationData, iteration, slotIndex);

                    if (IsBallStopped() && IsDeskStopped())
                    {
                        break;
                    }
                }

                simulationData.FinalSlotInfo = ResolveFinalSlotInfo(simulationData);
                return simulationData;
            }
            finally
            {
                m_ballInstance?.Disable();
                m_deskInstance?.Disable();
                Physics.simulationMode = previousMode;
            }
        }

        private float GetTickDuration()
        {
            if (m_deskSettings != null && m_deskSettings.Tick > 0f)
            {
                return m_deskSettings.Tick;
            }

            return Time.fixedDeltaTime;
        }

        private float GetVisualDeskAngleOffset(int sourceSlotIndex, int desiredSlotIndex)
        {
            int slotIndexDifference = desiredSlotIndex - sourceSlotIndex;
            return -slotIndexDifference * GetSlotAngle();
        }

        private void EnsureSimulationSceneCreated()
        {
            if (!m_simulationScene.IsValid())
            {
                CreateSceneParameters parameters = new(LocalPhysicsMode.Physics3D);
                string sceneName = $"Gameplay_Simulation_{++m_simulationIndex}";
                m_simulationScene = SceneManager.CreateScene(sceneName, parameters);
                m_physicsScene = m_simulationScene.GetPhysicsScene();
            }

            if (m_deskInstance == null && m_deskPrefab != null)
            {
                CreateDesk();
            }

            if (m_ballInstance == null && m_ballPrefab != null)
            {
                CreateBall();
            }

            if (m_ballInstance != null && m_ballRb == null)
            {
                m_ballRb = m_ballInstance.GetComponent<Rigidbody>();
            }
        }

        private void CreateDesk()
        {
            m_deskInstance = UnityEngine.Object.Instantiate(m_deskPrefab);
            m_deskInstance.Initialize();
            m_deskInstance.enabled = false;
            m_deskInstance.ChangeSimulationMode(SimulationMode.Simulation);
            SetRenderersEnabled(m_deskInstance.gameObject, false);
            SceneManager.MoveGameObjectToScene(m_deskInstance.gameObject, m_simulationScene);
        }

        private void CreateBall()
        {
            m_ballInstance = UnityEngine.Object.Instantiate(m_ballPrefab);
            m_ballInstance.Initialize();
            m_ballInstance.enabled = false;
            m_ballInstance.ChangeSimulationMode(SimulationMode.Simulation);
            SetRenderersEnabled(m_ballInstance.gameObject, false);
            m_ballRb = m_ballInstance.GetComponent<Rigidbody>();
            SceneManager.MoveGameObjectToScene(m_ballInstance.gameObject, m_simulationScene);
        }

        private static void SetRenderersEnabled(GameObject root, bool isEnabled)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = isEnabled;
            }
        }

        private void ResetBall(ref SimulationState simulationState)
        {
            if (m_ballRb == null) return;
            m_ballRb.position = m_deskInstance.LaunchTransform.position;
            m_ballRb.rotation = Quaternion.identity;
            Array.Clear(simulationState.BallStates, 0, simulationState.Buffer);
            simulationState.FrameCount = 0;
        }

        private void ResetDesk()
        {
            m_deskInstance.ResetSimulationObject();
        }

        private bool IsBallStopped()
        {
            return m_ballRb != null && m_ballRb.IsSleeping();
        }

        private bool IsDeskStopped()
        {
            return m_deskInstance != null && !m_deskInstance.IsSpinning;
        }

        private void RecordState(ref SimulationState simulationState, int iteration, int slotIndex)
        {
            if (iteration >= simulationState.Buffer) return;

            simulationState.BallStates[iteration] = new BallState(m_ballRb.position, m_ballRb.rotation, slotIndex);
            simulationState.DeskStates[iteration] = new DeskState(m_deskInstance.SpinTransform.position, m_deskInstance.SpinTransform.rotation);
            simulationState.FrameCount = Mathf.Max(simulationState.FrameCount, iteration + 1);
        }

        private int CheckSlots()
        {
            Vector3 deskCenter = GetDeskSlotOrigin(m_deskInstance.SpinTransform.position);

            for (int slotIndex = 0; slotIndex < m_deskSettings.SlotCount; slotIndex++)
            {
                Quaternion slotRotation = GetSlotWorldRotation(slotIndex, m_deskInstance.SpinTransform.rotation);
                Vector3 slotCenter = GetSlotCenter(deskCenter, slotRotation);
                int hitCount = m_physicsScene.OverlapBox(slotCenter, m_deskSettings.SlotBoxSize / 2f, m_overlapResults, slotRotation, m_ballLayerMask);

                if (hitCount <= 0)
                {
                    continue;
                }

                return slotIndex;
            }

            return -1;
        }

        private static int GetFinalSlotIndex(in SimulationState simulationState)
        {
            if (simulationState.FrameCount <= 0)
            {
                return -1;
            }

            return simulationState.BallStates[simulationState.FrameCount - 1].SlotIndex;
        }

        private static SlotInfo ResolveFinalSlotInfo(in SimulationState simulationState)
        {
            int finalSlotIndex = GetFinalSlotIndex(simulationState);
            return finalSlotIndex.GetSlotInfo();
        }

        private SettledSlotInfo AnalyzeSettledSlot(in SimulationState simulationState)
        {
            //If simulation not valid
            if (simulationState.FrameCount <= 0 || simulationState.BallStates == null || simulationState.DeskStates == null)
            {
                return new SettledSlotInfo(false, (-1).GetSlotInfo(), -1);
            }

            //If ball not slotted any slot due to an error collider geometry problem etc. visual models is not stable
            int finalSlotIndex = GetFinalSlotIndex(simulationState);
            SlotInfo finalSlotInfo = finalSlotIndex.GetSlotInfo();
            if (finalSlotIndex < 0)
            {
                return new SettledSlotInfo(false, finalSlotInfo, -1);
            }

            int continuousStartFrame = simulationState.FrameCount - 1;
            while (continuousStartFrame > 0 && simulationState.BallStates[continuousStartFrame - 1].SlotIndex == finalSlotIndex)
            {
                continuousStartFrame--;
            }

            if (finalSlotIndex != simulationState.BallStates[continuousStartFrame].SlotIndex)
            {
                return new SettledSlotInfo(false, finalSlotInfo, -1);
            }

            return new SettledSlotInfo(true, finalSlotInfo, continuousStartFrame);
        }

        private SimulationState CreateVisualReplayState(in SimulationState physicalState, int slotIndexDifference, float visualDeskOffset)
        {
            SimulationState visualReplayState = new(physicalState.Buffer, physicalState.TickDuration)
            {
                FrameCount = physicalState.FrameCount,
                FinalSlotInfo = physicalState.FinalSlotInfo
            };

            Quaternion deskRotationOffset = Quaternion.Euler(0f, visualDeskOffset, 0f);

            for (int frame = 0; frame < physicalState.FrameCount; frame++)
            {
                BallState ballState = physicalState.BallStates[frame];
                DeskState deskState = physicalState.DeskStates[frame];

                visualReplayState.BallStates[frame] = new BallState(ballState.Position, ballState.Rotation, RemapSlotIndex(ballState.SlotIndex, slotIndexDifference));
                visualReplayState.DeskStates[frame] = new DeskState(deskState.Position, deskRotationOffset * deskState.Rotation);
            }

            return visualReplayState;
        }

        private int RemapSlotIndex(int sourceSlotIndex, int slotIndexDifference)
        {
            if (sourceSlotIndex < 0)
            {
                return -1;
            }

            int remappedSlotIndex = (sourceSlotIndex + slotIndexDifference) % m_deskSettings.SlotCount;
            return remappedSlotIndex < 0 ? remappedSlotIndex + m_deskSettings.SlotCount : remappedSlotIndex;
        }

        private Vector3 GetDeskSlotOrigin(Vector3 deskPosition)
        {
            return deskPosition + m_deskSettings.SlotOriginOffset;
        }

        private Quaternion GetSlotWorldRotation(int slotIndex, Quaternion deskRotation)
        {
            float slotAngle = slotIndex * GetSlotAngle();
            return Quaternion.Euler(0f, slotAngle, 0f) * Quaternion.Euler(m_deskSettings.SlotRotationOffset) * deskRotation;
        }

        private Vector3 GetSlotCenter(Vector3 deskCenter, Quaternion slotWorldRotation)
        {
            return deskCenter + slotWorldRotation * Vector3.forward * m_deskSettings.DistanceFromOrigin;
        }

        private float GetSlotAngle()
        {
            return 360f / m_deskSettings.SlotCount;
        }

        private static void LogVisualRemapInitial(int physicalSlotIndex, int desiredSlotIndex, int slotIndexDifference, float slotAngle, float visualDeskOffset, int sourceFrame, float physicalStartAngle, float visualStartAngle)
        {
            Debug.Log("Visual deterministic INITIAL: " + $"physical settled slot [{physicalSlotIndex}], desired visual slot [{desiredSlotIndex}], " + $"slot difference [{slotIndexDifference}], slot angle [{slotAngle:F3}], " + $"visual desk offset [{visualDeskOffset:F3}], source frame [{sourceFrame}], " + $"physical start angle [{physicalStartAngle:F3}], visual start angle [{visualStartAngle:F3}].");
        }

        private static void LogVisualRemapApplied(int physicalSlotIndex, int desiredSlotIndex, float physicalStartAngle, float visualStartAngle, float visualDeskOffset, int sourceFrame)
        {
            Debug.Log("Visual deterministic <color=green>APPLIED</color>: " + $"physics was not rerun. Replay keeps the same ball trajectory and rotates the desk by [{visualDeskOffset:F3}] " + $"from start angle [{physicalStartAngle:F3}] to [{visualStartAngle:F3}], remapping physical slot [{physicalSlotIndex}] " + $"to visual slot [{desiredSlotIndex}] from source frame [{sourceFrame}].");
        }

        private static void LogVisualRemapSkipped(int physicalSlotIndex, int desiredSlotIndex, float startAngle, int sourceFrame)
        {
            Debug.Log("Visual deterministic <color=green>SKIPPED</color>: " + $"physical settled slot [{physicalSlotIndex}] already matches desired visual slot [{desiredSlotIndex}]. " + $"Replay stays unchanged at start angle [{startAngle:F3}] from source frame [{sourceFrame}].");
        }

        private static void LogVisualRemapFailed(string reason, int physicalSlotIndex, int desiredSlotIndex, float physicalStartAngle, float visualStartAngle, float visualDeskOffset, int sourceFrame)
        {
            string sourceFrameText = sourceFrame >= 0 ? sourceFrame.ToString() : "n/a";
            Debug.Log("Visual deterministic <color=red>FAILED</color>: " + $"{reason} Physical slot [{physicalSlotIndex}], desired visual slot [{desiredSlotIndex}], " + $"visual desk offset [{visualDeskOffset:F3}], physical start angle [{physicalStartAngle:F3}], " + $"visual start angle [{visualStartAngle:F3}], source frame [{sourceFrameText}].");
        }
    }
}
