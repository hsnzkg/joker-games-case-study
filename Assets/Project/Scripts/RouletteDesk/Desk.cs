using UnityEngine;

namespace Project.Scripts.RouletteDesk
{
    public class Desk : MonoBehaviour
    {
        [SerializeField] private Transform m_spinTransform;
        [SerializeField] private float m_drag;
        public DeskRotationSystem DeskRotationSystem { get; private set; }

        #region Unity Callbacks

        private void Awake()
        {
            Initialize();
        }

        private void FixedUpdate()
        {
            DeskRotationSystem.Tick();
        }

        #endregion

        private void Initialize()
        {
            DeskRotationSystem = new DeskRotationSystem(m_spinTransform,m_drag);
        }
    }
}