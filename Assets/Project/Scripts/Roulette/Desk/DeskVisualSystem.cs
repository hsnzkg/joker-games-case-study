using UnityEngine;

namespace Project.Scripts.Roulette.Desk
{
    public class DeskVisualSystem
    {
        private readonly Renderer[] m_renderers;
        public DeskVisualSystem(GameObject instance)
        {
            m_renderers = instance.GetComponentsInChildren<Renderer>();
        }
        
        public void ChangeVisualState(bool active)
        {
            foreach (Renderer renderer in m_renderers)
            {
                renderer.enabled = active;
            }
        }
    }
}