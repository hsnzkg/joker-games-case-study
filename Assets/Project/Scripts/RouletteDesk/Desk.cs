using Project.Scripts.Physic;
using UnityEngine;

namespace Project.Scripts.RouletteDesk
{
    public class Desk : MonoBehaviour, ISimulationObject
    {
        [SerializeField] private DeskPhysicSettings m_deskPhysicSettings;
        [SerializeField] private Transform m_spinTransform;
        [SerializeField] private Transform m_launchTransform;
        private Renderer[] m_renderers;
        private SimulationMode m_simulationMode;
        public DeskPhysicSystem DeskPhysicSystem { get; private set; }
        public Transform LaunchTransform => m_launchTransform;

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
            DeskPhysicSystem.Tick(m_deskPhysicSettings.Tick);
        }

        private void OnDrawGizmos()
        {
            if (m_simulationMode == SimulationMode.FixedUpdate)
            {
                DeskPhysicSystem?.DrawGizmos();
            }
        }

        #endregion

        private void Initialize()
        {
            m_renderers = GetComponentsInChildren<Renderer>();
            DeskPhysicSystem = new DeskPhysicSystem(m_deskPhysicSettings, m_spinTransform);
        }

        public void ChangeSimulationMode(SimulationMode mode)
        {
            m_simulationMode = mode;
            foreach (Renderer r in m_renderers)
            {
                r.enabled = m_simulationMode == SimulationMode.FixedUpdate;
            }
        }
    }
}