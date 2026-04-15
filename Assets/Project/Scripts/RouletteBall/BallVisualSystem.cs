using UnityEngine;

namespace Project.Scripts.RouletteBall
{
    public class BallVisualSystem
    {
        private readonly MeshRenderer m_ballRenderer;
        public BallVisualSystem(GameObject instance)
        {
            m_ballRenderer = instance.GetComponent<MeshRenderer>();
        }
        
        public void ChangeVisualState(bool active)
        {
            m_ballRenderer.enabled = active;
        }
    }
}