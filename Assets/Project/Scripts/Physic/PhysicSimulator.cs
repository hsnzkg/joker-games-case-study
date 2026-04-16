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

        public void Initialize()
        {
            CreatePhysicsScene();
        }

        public SimulationState Simulate(Vector3 ballForceDirection, float ballForce, float deskRotationSpeed, float deskDrag, float deskStartAngle)
        {
            float tick = m_deskPhysicSettings != null && m_deskPhysicSettings.Tick > 0f ? m_deskPhysicSettings.Tick : Time.fixedDeltaTime;
            SimulationState simulationData = new(m_maxIterations, tick);

            UnityEngine.SimulationMode previousMode = Physics.simulationMode;
            try
            {
                Physics.simulationMode = UnityEngine.SimulationMode.Script;
                if (m_ballRb == null)
                {
                    Debug.LogError("PhysicSimulator requires a Rigidbody on the ball prefab.");
                    return simulationData;
                }
                ResetBall(ref simulationData);
                ResetDesk();

                m_deskInstance.StartSpin(deskRotationSpeed, deskDrag, deskStartAngle);
                m_ballInstance.Launch(ballForceDirection, ballForce);

                int currentIteration = 0;
                bool isBallCollidingWithSlot = false;
                RecordState(ref simulationData, currentIteration, isBallCollidingWithSlot);

                for (int i = 1; i < m_maxIterations; i++)
                {
                    m_deskInstance.Tick(tick);
                    Physics.SyncTransforms();
                    m_physicsScene.Simulate(tick);

                    currentIteration = i;
                    isBallCollidingWithSlot = CheckSlots();
                    RecordState(ref simulationData, currentIteration, isBallCollidingWithSlot);

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

            if (m_simulationScene.IsValid())
            {
                SceneManager.UnloadSceneAsync(m_simulationScene);
                m_simulationScene = default;
            }
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
            if (m_deskInstance != null) return;
            m_deskInstance = UnityEngine.Object.Instantiate(m_deskPrefab);
            m_deskInstance.enabled = false;
            m_deskInstance.ChangeSimulationMode(SimulationMode.Simulation);
            SceneManager.MoveGameObjectToScene(m_deskInstance.gameObject, m_simulationScene);
        }

        private void CreateBall()
        {
            if (m_ballInstance != null) return;
            m_ballInstance = UnityEngine.Object.Instantiate(m_ballPrefab);
            m_ballInstance.enabled = false;
            m_ballRb = m_ballInstance.GetComponent<Rigidbody>();
            m_ballInstance.ChangeSimulationMode(SimulationMode.Simulation);
            SceneManager.MoveGameObjectToScene(m_ballInstance.gameObject, m_simulationScene);
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
            m_deskInstance.Reset();
        }

        private bool IsBallStopped()
        {
            return m_ballRb != null && m_ballRb.IsSleeping();
        }

        private bool IsDeskStopped()
        {
            return m_deskInstance != null && !m_deskInstance.IsSpinning;
        }

        private void RecordState(ref SimulationState simulationState, int iteration, bool isBallCollidingWithSlot)
        {
            if (iteration >= simulationState.Buffer) return;
            simulationState.BallStates[iteration] = new BallState(m_ballRb.position, m_ballRb.rotation, isBallCollidingWithSlot);
            simulationState.DeskStates[iteration] = new DeskState(m_deskInstance.SpinTransform.position, m_deskInstance.SpinTransform.rotation);
            simulationState.FrameCount = Mathf.Max(simulationState.FrameCount, iteration + 1);
        }

        private bool CheckSlots()
        {
            Vector3 center = m_deskInstance.SpinTransform.position + m_deskPhysicSettings.SlotOriginOffset;
            float slotPerAngle = 360f / m_deskPhysicSettings.SlotCount;

            for (int i = 0; i < m_deskPhysicSettings.SlotCount; i++)
            {
                Quaternion rot = Quaternion.Euler(0f, i * slotPerAngle, 0f) *
                                 Quaternion.Euler(m_deskPhysicSettings.SlotRotationOffset) *
                                 m_deskInstance.SpinTransform.rotation;

                Vector3 dir = rot * Vector3.forward;
                Vector3 pointB = center + dir * m_deskPhysicSettings.DistanceFromOrigin;

                int hitCount = m_physicsScene.OverlapBox(pointB, m_deskPhysicSettings.SlotBoxSize / 2f, m_overlapResults, rot, m_ballLayerMask);
                if (hitCount > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
