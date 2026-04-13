using UnityEngine;

namespace Project.Scripts.RouletteBall
{
    public class Ball : MonoBehaviour
    {
        [SerializeField] private Rigidbody m_rb;
        public BallVisualSystem BallVisualSystem { get; private set; }
        public BallPhysicSystem BallPhysicSystem { get; private set; }

        #region Unity Callbacks

        private void Awake()
        {
            Initialize();
        }

        #endregion
        
        private void Initialize()
        {
            BallVisualSystem = new BallVisualSystem(gameObject);
            BallPhysicSystem = new BallPhysicSystem(gameObject);
        }
    }
}