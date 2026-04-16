using System;
using Project.Scripts.Physic.State;
using Project.Scripts.RouletteBall;
using Project.Scripts.RouletteDesk;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            
            if (!TryGetSettledSlotInfo(initialState, out int finalSlotIndex, out int settledStartFrame))
            {
                Debug.LogWarning("Deterministic simulate could not find a settled final slot from the initial simulation. Returning initial state.");
                return initialState;
            }

            if (finalSlotIndex == desiredSlotIndex)
            {
                Debug.Log($"Deterministic simulate already matched desired slot [{desiredSlotIndex}] in the initial run. Settled from frame [{settledStartFrame}].");
                return initialState;
            }

            float slotAngle = GetSlotAngle();
            float currentSlotAngle = finalSlotIndex * slotAngle;
            float desiredSlotAngle = desiredSlotIndex * slotAngle;
            float startAngleCorrection = Mathf.DeltaAngle(desiredSlotAngle, currentSlotAngle);
            float correctedStartAngle = Mathf.Repeat(deskStartAngle + startAngleCorrection, 360f);

            Debug.Log($"Deterministic simulate adjusting start angle. Desired slot: [{desiredSlotIndex}], initial final slot: [{finalSlotIndex}], " + $"settled from frame: [{settledStartFrame}], angle correction: [{startAngleCorrection:F3}], corrected start angle: [{correctedStartAngle:F3}].");

            SimulationState correctedState = RunSimulation(ballForceDirection, ballForce, deskRotationSpeed, deskDrag, correctedStartAngle);
            int correctedFinalSlotIndex = GetFinalSlotIndex(correctedState);

            if (correctedFinalSlotIndex == desiredSlotIndex)
            {
                Debug.Log($"Deterministic simulate succeeded. Final slot [{correctedFinalSlotIndex}] matched desired slot [{desiredSlotIndex}].");
            }
            else
            {
                Debug.LogWarning($"Deterministic simulate recalculated start angle but final slot [{correctedFinalSlotIndex}] did not match desired slot [{desiredSlotIndex}]. Returning recalculated state.");
            }

            return correctedState;
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
                CreatePhysicsScene();

                if (m_ballRb == null)
                {
                    Debug.LogError("PhysicSimulator requires a Rigidbody on the ball prefab.");
                    return simulationData;
                }

                ResetBall(ref simulationData);
                ResetDesk();

                Transform launchTransform = m_deskInstance.LaunchTransform;
                m_deskInstance.StartSpin(deskRotationSpeed, deskDrag, deskStartAngle);
                m_ballInstance.Launch(launchTransform.position,launchTransform.rotation,ballForceDirection, ballForce);

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

                m_ballInstance.Stop();
                m_deskInstance.Stop();
                return simulationData;
            }
            finally
            {
                Physics.simulationMode = previousMode;
                Dispose();
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

        private float GetSlotAngle()
        {
            return 360f / m_deskPhysicSettings.SlotCount;
        }

        private void CreatePhysicsScene()
        {
            CreateSceneParameters parameters = new(LocalPhysicsMode.Physics3D);
            string sceneName = $"Gameplay_Simulation_{++s_simulationIndex}";
            m_simulationScene = SceneManager.CreateScene(sceneName, parameters);
            m_physicsScene = m_simulationScene.GetPhysicsScene();

            CreateDesk();
            CreateBall();
        }

        private void CreateDesk()
        {
            m_deskInstance = UnityEngine.Object.Instantiate(m_deskPrefab);
            m_deskInstance.enabled = false;
            m_deskInstance.ChangeSimulationMode(SimulationMode.Simulation);
            SetRenderersEnabled(m_deskInstance.gameObject, false);
            SceneManager.MoveGameObjectToScene(m_deskInstance.gameObject, m_simulationScene);
        }

        private void CreateBall()
        {
            m_ballInstance = UnityEngine.Object.Instantiate(m_ballPrefab);
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

            simulationState.BallStates[iteration] = new BallState(m_ballRb.position, m_ballRb.rotation, slotIndex >= 0, slotIndex);
            simulationState.DeskStates[iteration] = new DeskState(m_deskInstance.SpinTransform.position, m_deskInstance.SpinTransform.rotation);
            simulationState.FrameCount = Mathf.Max(simulationState.FrameCount, iteration + 1);
        }

        private int CheckSlots()
        {
            Vector3 center = m_deskInstance.SpinTransform.position + m_deskPhysicSettings.SlotOriginOffset;
            float slotPerAngle = GetSlotAngle();

            for (int slotIndex = 0; slotIndex < m_deskPhysicSettings.SlotCount; slotIndex++)
            {
                Quaternion rot = Quaternion.Euler(0f, slotIndex * slotPerAngle, 0f) * Quaternion.Euler(m_deskPhysicSettings.SlotRotationOffset) * m_deskInstance.SpinTransform.rotation;

                Vector3 dir = rot * Vector3.forward;
                Vector3 pointB = center + dir * m_deskPhysicSettings.DistanceFromOrigin;

                int hitCount = m_physicsScene.OverlapBox(pointB, m_deskPhysicSettings.SlotBoxSize / 2f, m_overlapResults, rot, m_ballLayerMask);
                if (hitCount > 0)
                {
                    return slotIndex;
                }
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

        private static bool TryGetSettledSlotInfo(in SimulationState simulationState, out int finalSlotIndex, out int settledStartFrame)
        {
            finalSlotIndex = GetFinalSlotIndex(simulationState);
            settledStartFrame = -1;

            if (finalSlotIndex < 0 || simulationState.FrameCount <= 0)
            {
                return false;
            }

            settledStartFrame = simulationState.FrameCount - 1;
            while (settledStartFrame > 0 && simulationState.BallStates[settledStartFrame - 1].SlotIndex == finalSlotIndex)
            {
                settledStartFrame--;
            }

            return true;
        }
    }
}