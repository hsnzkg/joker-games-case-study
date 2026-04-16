using System;
using Project.Scripts.RouletteBall;
using Project.Scripts.RouletteDesk;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Scripts.Physic
{
    public class PhysicSimulator : MonoBehaviour
    {
        [SerializeField] private DeskPhysicSettings m_deskPhysicSettings;
        [SerializeField] private float m_ballForce;
        [SerializeField] private Vector3 m_ballForceDirection;
        [SerializeField] private float m_deskRotationSpeed;
        [SerializeField] private float m_deskDrag;
        [SerializeField] private Ball m_ballPrefab;
        [SerializeField] private Desk m_deskPrefab;
        [SerializeField] private int m_maxIterations = 100;

        private SimulationState m_simulationData;
        private Scene m_simulationScene;
        private PhysicsScene m_physicsScene;
        private Ball m_ballInstance;
        private Desk m_deskInstance;
        private SphereCollider m_ballCollider;
        private Rigidbody m_ballRb;
        private float m_tick;
        private int m_currentIteration;
        private int m_replayIndex;
        private bool m_isPhysicSceneCreated;
        private Collider[] m_overlapResults;
        private int m_ballLayerMask;
        private bool m_isBallCollidingWithSlot;

        public SimulationState SimulationState => m_simulationData;


        #region Unity Callbacks

        private void Awake()
        {
            Initialize();
            CreatePhysicsScene();

            m_currentIteration = 0;
            ResetBall();
            ResetDesk();
            RecordState();

            m_ballInstance.Launch(m_ballForceDirection, m_ballForce);
            m_deskInstance.StartSpin(m_deskRotationSpeed, m_deskDrag);

            SimulateUntilStop();
        }

        private void OnDrawGizmos()
        {
            if (!m_isPhysicSceneCreated) return;
            for (int i = 0; i < m_simulationData.BallStates.Length; i++)
            {
                Color color = Color.Lerp(Color.green, Color.red, i / (float)m_maxIterations);
                Gizmos.color = color;
                Gizmos.DrawWireSphere(m_simulationData.BallStates[i].Position, m_ballCollider.radius * m_ballInstance.transform.localScale.x);
            }
        }

        #endregion


        private void Initialize()
        {
            Physics.simulationMode = UnityEngine.SimulationMode.Script;
            m_tick = Time.fixedDeltaTime;
            m_overlapResults = new Collider[1];
            m_simulationData = new SimulationState(m_maxIterations, m_tick);
            m_ballLayerMask = 1 << m_ballPrefab.gameObject.layer;
        }

        private void CreatePhysicsScene()
        {
            CreateSceneParameters parameters = new(LocalPhysicsMode.Physics3D);
            m_simulationScene = SceneManager.CreateScene("Gameplay_Simulation", parameters);
            m_physicsScene = m_simulationScene.GetPhysicsScene();

            CreateDesk();
            CreateBall();

            m_isPhysicSceneCreated = true;
        }

        private void CopyCollidersToSimulation(GameObject obj)
        {
            Collider[] colliders = obj.GetComponents<Collider>();

            if (colliders.Length == 0) return;

            GameObject clone = new(obj.name + "_SIM") { transform = { position = obj.transform.position, rotation = obj.transform.rotation, localScale = obj.transform.localScale } };

            foreach (Collider col in colliders)
            {
                Type type = col.GetType();

                if (type == typeof(BoxCollider))
                {
                    BoxCollider src = (BoxCollider)col;
                    BoxCollider dst = clone.AddComponent<BoxCollider>();

                    dst.center = src.center;
                    dst.size = src.size;
                    dst.isTrigger = src.isTrigger;
                }
                else if (type == typeof(SphereCollider))
                {
                    SphereCollider src = (SphereCollider)col;
                    SphereCollider dst = clone.AddComponent<SphereCollider>();

                    dst.center = src.center;
                    dst.radius = src.radius;
                    dst.isTrigger = src.isTrigger;
                }
                else if (type == typeof(MeshCollider))
                {
                    MeshCollider src = (MeshCollider)col;
                    MeshCollider dst = clone.AddComponent<MeshCollider>();

                    dst.sharedMesh = src.sharedMesh;
                    dst.convex = src.convex;
                    dst.isTrigger = src.isTrigger;
                }
            }

            SceneManager.MoveGameObjectToScene(clone, m_simulationScene);
        }

        private void CreateDesk()
        {
            if (m_deskInstance != null) return;
            m_deskInstance = Instantiate(m_deskPrefab);
            m_deskInstance.ChangeSimulationMode(SimulationMode.Simulation);
            SceneManager.MoveGameObjectToScene(m_deskInstance.gameObject, m_simulationScene);
        }

        private void CreateBall()
        {
            if (m_ballInstance != null) return;
            m_ballInstance = Instantiate(m_ballPrefab);
            m_ballCollider = m_ballInstance.GetComponent<SphereCollider>();
            m_ballRb = m_ballInstance.GetComponent<Rigidbody>();
            m_ballInstance.ChangeSimulationMode(SimulationMode.Simulation);
            SceneManager.MoveGameObjectToScene(m_ballInstance.gameObject, m_simulationScene);
        }

        private void ResetBall()
        {
            m_ballRb.position = m_deskInstance.LaunchTransform.position;
            m_ballRb.rotation = Quaternion.identity;
            Array.Clear(m_simulationData.BallStates, 0, m_maxIterations);
            m_simulationData.FrameCount = 0;
        }

        private void ResetDesk()
        {
            m_deskInstance.Reset();
        }

        private void SimulateUntilStop()
        {
            for (int i = 1; i < m_maxIterations - 1; i++)
            {
                m_deskInstance.Tick(m_deskPhysicSettings.Tick);
                Physics.SyncTransforms();
                m_physicsScene.Simulate(m_tick);

                m_currentIteration = i;
                m_isBallCollidingWithSlot = CheckSlots();
                RecordState();

                if (!IsBallStopped() || !IsDeskStopped()) continue;

                Debug.Log("Ball & Desk stopped at iteration : " + i);
                Stop();
                break;
            }
        }

        private bool IsBallStopped()
        {
            return m_ballRb.IsSleeping();
        }

        private bool IsDeskStopped()
        {
            return !m_deskInstance.IsSpinning;
        }

        private void Stop()
        {
            m_ballInstance.Stop();
            m_deskInstance.Stop();
            Physics.simulationMode = UnityEngine.SimulationMode.FixedUpdate;
        }

        private void RecordState()
        {
            if (m_currentIteration >= m_simulationData.Buffer) return;

            m_simulationData.BallStates[m_currentIteration] = new BallState(m_ballRb.position, m_ballRb.rotation, m_isBallCollidingWithSlot);
            m_simulationData.DeskStates[m_currentIteration] = new DeskState(m_deskInstance.SpinTransform.position, m_deskInstance.SpinTransform.rotation);
            m_simulationData.FrameCount = Mathf.Max(m_simulationData.FrameCount, m_currentIteration + 1);
        }

        private bool CheckSlots()
        {
            Vector3 center = m_deskInstance.SpinTransform.position + m_deskPhysicSettings.SlotOriginOffset;
            float slotPerAngle = 360f / m_deskPhysicSettings.SlotCount;

            for (int i = 0; i < m_deskPhysicSettings.SlotCount; i++)
            {
                Quaternion rot = Quaternion.Euler(0f, i * slotPerAngle, 0f) * Quaternion.Euler(m_deskPhysicSettings.SlotRotationOffset) * m_deskInstance.SpinTransform.rotation;

                Vector3 dir = rot * Vector3.forward;
                Vector3 pointB = center + dir * m_deskPhysicSettings.DistanceFromOrigin;

                int hitCount = m_physicsScene.OverlapBox(pointB, m_deskPhysicSettings.SlotBoxSize / 2f, m_overlapResults, rot, m_ballLayerMask);
                if (hitCount > 0)
                {
                    Debug.Log($"Ball in slot [{i}] | Iteration: {m_currentIteration}");
                    return true;
                }
            }

            return false;
        }
    }
}
