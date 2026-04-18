using UnityEngine;

namespace Project.Scripts.Roulette.Ball
{
    public class BallVisualSystem
    {
        private readonly MeshRenderer m_renderer;
        public BallVisualSystem(GameObject instance)
        {
            m_renderer = instance.GetComponent<MeshRenderer>();
        }
        
        public void ChangeVisualState(bool active)
        {
            m_renderer.enabled = active;
        }
    }
}