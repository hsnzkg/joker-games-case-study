using System;
using System.IO;
using Project.Scripts.Physic.State;
using Project.Scripts.RouletteBall;
using Project.Scripts.RouletteDesk;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Plastic.Newtonsoft.Json;

namespace Project.Scripts.Physic
{
    public sealed class PhysicSimulator : IDisposable
    {
        private static int s_simulationIndex;

        private readonly DeskPhysicSettings m_deskPhysicSettings;
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

        public PhysicSimulator(DeskPhysicSettings deskPhysicSettings, Ball ballPrefab, Desk deskPrefab, int maxIterations = 100)
        {
            m_deskPhysicSettings = deskPhysicSettings;
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
            if (desiredSlotIndex < 0 || desiredSlotIndex >= m_deskPhysicSettings.SlotCount)
            {
                Debug.LogError($"Desired slot index [{desiredSlotIndex}] is outside valid range [0, {m_deskPhysicSettings.SlotCount - 1}]. Falling back to regular simulation.");
                return RunSimulation(ballForceDirection, ballForce, deskRotationSpeed, deskDrag, deskStartAngle);
            }

            SimulationState initialState = RunSimulation(ballForceDirection, ballForce, deskRotationSpeed, deskDrag, deskStartAngle);
            SettledSlotInfo initialSettledSlotInfo = AnalyzeSettledSlot(initialState);

            if (!initialSettledSlotInfo.HasSettledSlot)
            {
                LogDeterministicFailure("Initial simulation did not finish inside any slot, so a deterministic correction could not be derived.", initialSettledSlotInfo.FinalSlotIndex, desiredSlotIndex, deskStartAngle, 0f, initialSettledSlotInfo.ContinuousStartFrame);
                return initialState;
            }

            if (initialSettledSlotInfo.FinalSlotIndex == desiredSlotIndex)
            {
                LogDeterministicSuccess("Initial simulation already matched the desired slot.", initialSettledSlotInfo.FinalSlotIndex, desiredSlotIndex, deskStartAngle, 0f, initialSettledSlotInfo.ContinuousStartFrame);
                return initialState;
            }

            float appliedOffset = GetSlotStartAngleCorrection(initialSettledSlotInfo, desiredSlotIndex);
            float correctedStartAngle = Mathf.Repeat(deskStartAngle + appliedOffset, 360f);

            Debug.Log(
                $"Deterministic simulate applying a single-pass rotation offset of [{appliedOffset:F3}] degrees " +
                $"from settled slot [{initialSettledSlotInfo.FinalSlotIndex}] at frame [{initialSettledSlotInfo.ContinuousStartFrame}].");

            SimulationState correctedState = RunSimulation(ballForceDirection, ballForce, deskRotationSpeed, deskDrag, correctedStartAngle);
            SettledSlotInfo correctedSettledSlotInfo = AnalyzeSettledSlot(correctedState);

            if (correctedSettledSlotInfo.HasSettledSlot && correctedSettledSlotInfo.FinalSlotIndex == desiredSlotIndex)
            {
                LogDeterministicSuccess("Single-pass angle correction matched the desired slot.", correctedSettledSlotInfo.FinalSlotIndex, desiredSlotIndex, correctedStartAngle, appliedOffset, correctedSettledSlotInfo.ContinuousStartFrame);
                return correctedState;
            }
            
            SaveDebugState(initialState);
            int correctedFinalSlotIndex = correctedSettledSlotInfo.HasSettledSlot ? correctedSettledSlotInfo.FinalSlotIndex : GetFinalSlotIndex(correctedState);
            
            LogDeterministicFailure("Single-pass angle correction did not reach the desired slot.", correctedFinalSlotIndex, desiredSlotIndex, correctedStartAngle, appliedOffset, correctedSettledSlotInfo.ContinuousStartFrame);
            return correctedState;
        }
        
        public void SaveDebugState(object initialState)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                string json = JsonConvert.SerializeObject(initialState, Formatting.Indented, settings);

                string path = Path.Combine(Application.dataPath, "debugState.json");

                File.WriteAllText(path, json);

                Debug.Log($"Saved JSON to: {path}");

#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
            }
            catch (System.Exception e)
            {
                Debug.LogError($"JSON save failed: {e}");
            }
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

                if (m_ballRb == null)
                {
                    Debug.LogError("PhysicSimulator requires a Rigidbody on the ball prefab.");
                    return simulationData;
                }

                ResetBall(ref simulationData);
                ResetDesk();

                Transform launchTransform = m_deskInstance.LaunchTransform;
                m_deskInstance.StartSpin(deskRotationSpeed, deskDrag, deskStartAngle);
                m_ballInstance.Launch(launchTransform.position, launchTransform.rotation, ballForceDirection, ballForce);

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

                m_ballInstance.Disable();
                m_deskInstance.Disable();
                return simulationData;
            }
            finally
            {
                if (m_ballInstance != null)
                {
                    m_ballInstance.Disable();
                }

                if (m_deskInstance != null)
                {
                    m_deskInstance.Disable();
                }

                Physics.simulationMode = previousMode;
            }
        }

        private float GetTickDuration()
        {
            if (m_deskPhysicSettings != null && m_deskPhysicSettings.Tick > 0f)
            {
                return m_deskPhysicSettings.Tick;
            }

            return Time.fixedDeltaTime;
        }

        private float GetSlotStartAngleCorrection(in SettledSlotInfo settledSlotInfo, int desiredSlotIndex)
        {
            Vector3 currentReferenceDirection = GetSlotReferenceDirection(settledSlotInfo.FinalSlotIndex, settledSlotInfo.SlotLocalBallPosition);
            Vector3 desiredReferenceDirection = GetSlotReferenceDirection(desiredSlotIndex, settledSlotInfo.SlotLocalBallPosition);

            if (currentReferenceDirection.sqrMagnitude < 0.0001f || desiredReferenceDirection.sqrMagnitude < 0.0001f)
            {
                return GetSlotStartAngleCorrection(settledSlotInfo.FinalSlotIndex, desiredSlotIndex);
            }

            return Vector3.SignedAngle(desiredReferenceDirection, currentReferenceDirection, Vector3.up);
        }

        private float GetSlotStartAngleCorrection(int currentSlotIndex, int desiredSlotIndex)
        {
            float currentSlotAngle = currentSlotIndex * GetSlotAngle();
            float desiredSlotAngle = desiredSlotIndex * GetSlotAngle();
            return Mathf.DeltaAngle(desiredSlotAngle, currentSlotAngle);
        }

        private Vector3 GetSlotReferenceDirection(int slotIndex, Vector3 slotLocalBallPosition)
        {
            Vector3 slotReference = Vector3.forward * m_deskPhysicSettings.DistanceFromOrigin + slotLocalBallPosition;
            slotReference.y = 0f;

            if (slotReference.sqrMagnitude < 0.0001f)
            {
                slotReference = Vector3.forward;
            }

            return GetSlotLocalRotation(slotIndex) * slotReference;
        }

        private Quaternion GetSlotLocalRotation(int slotIndex)
        {
            float slotAngle = slotIndex * GetSlotAngle();
            return Quaternion.Euler(0f, slotAngle, 0f) * Quaternion.Euler(m_deskPhysicSettings.SlotRotationOffset);
        }

        private void EnsureSimulationSceneCreated()
        {
            if (!m_simulationScene.IsValid())
            {
                CreateSceneParameters parameters = new(LocalPhysicsMode.Physics3D);
                string sceneName = $"Gameplay_Simulation_{++s_simulationIndex}";
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
            int bestSlotIndex = -1;
            float bestSlotScore = float.PositiveInfinity;

            for (int slotIndex = 0; slotIndex < m_deskPhysicSettings.SlotCount; slotIndex++)
            {
                Quaternion slotRotation = GetSlotWorldRotation(slotIndex, m_deskInstance.SpinTransform.rotation);
                Vector3 slotCenter = GetSlotCenter(deskCenter, slotRotation);
                int hitCount = m_physicsScene.OverlapBox(slotCenter, m_deskPhysicSettings.SlotBoxSize / 2f, m_overlapResults, slotRotation, m_ballLayerMask);

                if (hitCount <= 0)
                {
                    continue;
                }

                float slotScore = GetSlotMatchScore(m_ballRb.position, slotCenter, slotRotation);
                if (slotScore < bestSlotScore)
                {
                    bestSlotScore = slotScore;
                    bestSlotIndex = slotIndex;
                }
            }

            return bestSlotIndex;
        }

        private static int GetFinalSlotIndex(in SimulationState simulationState)
        {
            if (simulationState.FrameCount <= 0)
            {
                return -1;
            }

            return simulationState.BallStates[simulationState.FrameCount - 1].SlotIndex;
        }

        private SettledSlotInfo AnalyzeSettledSlot(in SimulationState simulationState)
        {
            //If simulation not valid
            if (simulationState.FrameCount <= 0 || simulationState.BallStates == null || simulationState.DeskStates == null)
            {
                return new SettledSlotInfo(false, -1, -1, Vector3.zero);
            }

            //If ball not slotted any slot due to an error collider geometry problem etc. visual models is not stable
            int finalSlotIndex = GetFinalSlotIndex(simulationState);
            if (finalSlotIndex < 0)
            {
                return new SettledSlotInfo(false, finalSlotIndex, -1, Vector3.zero);
            }

            int continuousStartFrame = simulationState.FrameCount - 1;
            while (continuousStartFrame > 0 && simulationState.BallStates[continuousStartFrame - 1].SlotIndex == finalSlotIndex)
            {
                continuousStartFrame--;
            }

            Vector3 slotLocalBallPosition = GetSlotLocalBallPosition(simulationState.BallStates[continuousStartFrame].Position, simulationState.DeskStates[continuousStartFrame], finalSlotIndex);

            return new SettledSlotInfo(true, finalSlotIndex, continuousStartFrame, slotLocalBallPosition);
        }

        private Vector3 GetSlotLocalBallPosition(Vector3 ballWorldPosition, in DeskState deskState, int slotIndex)
        {
            Vector3 deskCenter = GetDeskSlotOrigin(deskState.Position);
            Quaternion slotWorldRotation = GetSlotWorldRotation(slotIndex, deskState.Rotation);
            Vector3 slotCenter = GetSlotCenter(deskCenter, slotWorldRotation);
            return Quaternion.Inverse(slotWorldRotation) * (ballWorldPosition - slotCenter);
        }

        private Vector3 GetDeskSlotOrigin(Vector3 deskPosition)
        {
            return deskPosition + m_deskPhysicSettings.SlotOriginOffset;
        }

        private Quaternion GetSlotWorldRotation(int slotIndex, Quaternion deskRotation)
        {
            float slotAngle = slotIndex * GetSlotAngle();
            return Quaternion.Euler(0f, slotAngle, 0f) * Quaternion.Euler(m_deskPhysicSettings.SlotRotationOffset) * deskRotation;
        }

        private Vector3 GetSlotCenter(Vector3 deskCenter, Quaternion slotWorldRotation)
        {
            return deskCenter + slotWorldRotation * Vector3.forward * m_deskPhysicSettings.DistanceFromOrigin;
        }

        private float GetSlotAngle()
        {
            return 360f / m_deskPhysicSettings.SlotCount;
        }

        private float GetSlotMatchScore(Vector3 ballWorldPosition, Vector3 slotCenter, Quaternion slotWorldRotation)
        {
            Vector3 localBallPosition = Quaternion.Inverse(slotWorldRotation) * (ballWorldPosition - slotCenter);
            Vector3 halfExtents = m_deskPhysicSettings.SlotBoxSize / 2f;

            float x = halfExtents.x > Mathf.Epsilon ? localBallPosition.x / halfExtents.x : localBallPosition.x;
            float y = halfExtents.y > Mathf.Epsilon ? localBallPosition.y / halfExtents.y : localBallPosition.y;
            float z = halfExtents.z > Mathf.Epsilon ? localBallPosition.z / halfExtents.z : localBallPosition.z;

            return x * x + y * y + z * z;
        }

        private static void LogDeterministicSuccess(string reason, int finalSlotIndex, int desiredSlotIndex, float startAngle, float appliedOffset, int settledStartFrame)
        {
            Debug.Log($"Deterministic simulate <color=green>SUCCEEDED</color>: {reason} " + $"Final slot [{finalSlotIndex}], desired slot [{desiredSlotIndex}], start angle [{startAngle:F3}], " + $"applied offset [{appliedOffset:F3}], continuous slot start frame [{settledStartFrame}].");
        }

        private static void LogDeterministicFailure(string reason, int finalSlotIndex, int desiredSlotIndex, float startAngle, float appliedOffset, int settledStartFrame)
        {
            string settledStartFrameText = settledStartFrame >= 0 ? settledStartFrame.ToString() : "n/a";
            Debug.Log($"Deterministic simulate <color=red>FAILED</color>: {reason} " + $"Final slot [{finalSlotIndex}], desired slot [{desiredSlotIndex}], start angle [{startAngle:F3}], " + $"applied offset [{appliedOffset:F3}], continuous slot start frame [{settledStartFrameText}].");
        }
    }
}
