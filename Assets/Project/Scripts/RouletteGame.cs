using Project.Scripts.RouletteBall;
using Project.Scripts.RouletteDesk;
using UnityEngine;

namespace Project.Scripts
{
    public class RouletteGame : MonoBehaviour
    {
        [SerializeField] private Vector3 m_ballDir;
        [SerializeField] private float m_ballForce;
        [SerializeField] private float m_rotateSpeed;
        [SerializeField] private float m_drag;
        [SerializeField] private Ball m_ball;
        [SerializeField] private Desk m_desk;
        private void Start()
        {
            m_ball.BallPhysicSystem.Launch(m_ballDir, m_ballForce);
            m_desk.DeskPhysicSystem.StartSpin(m_rotateSpeed,m_drag);
        }
    }
}