using UnityEngine;

namespace Project.Scripts.RouletteDesk
{
    public class Desk : MonoBehaviour
    {
        [SerializeField] private Transform m_spinTransform;
        [SerializeField] private Transform m_launchTransform;
        public DeskPhysicSystem DeskPhysicSystem { get; private set; }
        public Transform LaunchTransform => m_launchTransform;

        #region Unity Callbacks

        private void Awake()
        {
            Initialize();
        }

        private void FixedUpdate()
        {
            DeskPhysicSystem.Tick();
        }

        #endregion

        private void Initialize()
        {
            DeskPhysicSystem = new DeskPhysicSystem(m_spinTransform);
        }
    }
}