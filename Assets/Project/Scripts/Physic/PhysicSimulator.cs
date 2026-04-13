using System;
using Project.Scripts.RouletteBall;
using Project.Scripts.RouletteDesk;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Scripts.Physic
{
    public class PhysicSimulator : MonoBehaviour
    {
        [SerializeField] private float m_force;
        [SerializeField] private float m_deskRotationSpeed;
        [SerializeField] private Vector3 m_forceDirection;
        [SerializeField] private LayerMask m_simulationLayer;
        [SerializeField] private int m_maxIterations = 100;
        [SerializeField] private Transform m_ballLaunchTransform;
        [SerializeField] private Ball m_ballPrefab;
        [SerializeField] private Desk m_deskPrefab;
        private BallState[] m_states;
        private Scene m_simulationScene;
        private PhysicsScene m_physicsScene;
        private Ball m_ballInstance;
        private Desk m_deskInstance;
        private SphereCollider m_ballCollider;
        private Rigidbody m_ballRb;
        private float m_tick;
        private int m_currentIteration;
        private bool m_isReplaying;
        private int m_replayIndex;

        #region Unity Callbacks

        private void Awake()
        {
            Initialize();
            CreatePhysicsScene();
            CreateDesk();
            CreateBall();
            SpinDesk();
            LaunchBall(m_forceDirection, m_force);
        }

        private void OnDrawGizmos()
        {
            if (m_states == null) return;
            for (int i = 0; i < m_maxIterations; i++)
            {
                Color color = Color.Lerp(Color.green, Color.red, i / (float)m_maxIterations);
                Gizmos.color = color;
                Gizmos.DrawWireSphere(m_states[i].Position, m_ballCollider.radius * m_ballInstance.transform.localScale.x);
            }
        }

        #endregion


        private void Initialize()
        {
            Physics.simulationMode = SimulationMode.Script;
            m_tick = Time.fixedDeltaTime;
            m_states = new BallState[m_maxIterations];
        }

        private void CreatePhysicsScene()
        {
            CreateSceneParameters parameters = new(LocalPhysicsMode.Physics3D);
            m_simulationScene = SceneManager.CreateScene("Gameplay_Simulation", parameters);
            m_physicsScene = m_simulationScene.GetPhysicsScene();
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
            SceneManager.MoveGameObjectToScene(m_deskInstance.gameObject, m_simulationScene);
        }

        private void CreateBall()
        {
            if (m_ballInstance != null) return;
            m_ballInstance = Instantiate(m_ballPrefab);
            m_ballCollider = m_ballInstance.GetComponent<SphereCollider>();
            m_ballRb = m_ballInstance.GetComponent<Rigidbody>();
            m_ballInstance.BallVisualSystem.SetType(BallInstanceType.Simulation);
            SceneManager.MoveGameObjectToScene(m_ballInstance.gameObject, m_simulationScene);
        }

        public void LaunchBall(Vector3 initialDirection, float force)
        {
            m_ballRb.position = m_ballLaunchTransform.position;
            m_ballRb.rotation = Quaternion.identity;
            m_deskInstance.DeskRotationSystem.Reset();
            Array.Clear(m_states, 0, m_states.Length);
            m_currentIteration = 0;

            initialDirection.Normalize();
            RecordState();
            
            m_ballInstance.BallPhysicSystem.Enable();
            m_ballRb.AddForce(initialDirection * force, ForceMode.Impulse);
            m_deskInstance.DeskRotationSystem.StartSpin(m_deskRotationSpeed);
            
            SimulateUntilStop();
        }

        private void SpinDesk()
        {
            m_deskInstance.DeskRotationSystem.Reset();
            m_deskInstance.DeskRotationSystem.StartSpin(m_deskRotationSpeed);
        }

        private void SimulateUntilStop()
        {
            for (int i = 0; i < m_maxIterations - 1; i++)
            {
                m_physicsScene.Simulate(m_tick);
                m_deskInstance.DeskRotationSystem.Tick();
                RecordState();

                if (!IsBallStopped() || !IsDeskStopped()) continue;
                
                Debug.Log("Ball and Desk stopped at iteration: " + i);
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
            return !m_deskInstance.DeskRotationSystem.IsEnabled;
        }

        private void Stop()
        {
            m_ballInstance.BallPhysicSystem.Disable();
            m_deskInstance.DeskRotationSystem.Disable();
        }

        private void RecordState()
        {
            if (m_currentIteration >= m_states.Length) return;

            m_states[m_currentIteration] = new BallState { Position = m_ballRb.position, Rotation = m_ballRb.rotation, Velocity = m_ballRb.linearVelocity, AngularVelocity = m_ballRb.angularVelocity };

            m_currentIteration++;
        }
    }
}