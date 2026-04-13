using UnityEngine;

namespace Project.Scripts.RouletteBall
{
    public class BallVisualSystem
    {
        private readonly MeshRenderer m_ballRenderer;
        private bool m_visualEnabled;
        public BallVisualSystem(GameObject instance)
        {
            m_ballRenderer = instance.GetComponent<MeshRenderer>();
            m_visualEnabled = m_ballRenderer.enabled;
        }

        public void SetType(BallInstanceType type)
        {
            if (type == BallInstanceType.Presentation)
            {
                if (m_visualEnabled) return;
                m_ballRenderer.enabled = true;
                m_visualEnabled = true;
            }
            else
            {
                if (!m_visualEnabled) return;
                m_ballRenderer.enabled = false;
                m_visualEnabled = false;
            }
        }
    }
}