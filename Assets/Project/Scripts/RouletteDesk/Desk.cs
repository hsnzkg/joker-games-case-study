using Project.Scripts.Physic;
using UnityEngine;

namespace Project.Scripts.RouletteDesk
{
    public class Desk : MonoBehaviour, ISimulationObject
    {
        [SerializeField] private DeskPhysicSettings m_deskPhysicSettings;
        [SerializeField] private Transform m_spinTransform;
        [SerializeField] private Transform m_launchTransform;
        public DeskPhysicSystem DeskPhysicSystem { get; private set; }
        public Transform LaunchTransform => m_launchTransform;
        private Renderer[] m_renderers;

        private SimulationMode m_simulationMode;

        public SimulationMode SimulationMode
        {
            get => m_simulationMode;
            set => ChangeSimulationMode(value);
        }

        #region Unity Callbacks

        private void Awake()
        {
            Initialize();
        }

        private void FixedUpdate()
        {
            DeskPhysicSystem.Tick();
        }

        private void OnDrawGizmos()
        {
            DeskPhysicSystem?.DrawGizmos();
        }

        #endregion

        private void Initialize()
        {
            DeskPhysicSystem = new DeskPhysicSystem(m_deskPhysicSettings,m_spinTransform);
            m_renderers = GetComponentsInChildren<Renderer>();
        }

        public void ChangeSimulationMode(SimulationMode mode)
        {
            foreach (Renderer r in m_renderers)
            {
                r.enabled = mode == SimulationMode.FixedUpdate;
            }
        }
    }
}