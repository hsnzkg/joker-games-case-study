using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Project.Scripts.GUI.Desk
{
    public class ClickableAreaHandler : MonoBehaviour, IPointerClickHandler
    {
        private string m_areaId;
        public event Action<string> Clicked;

        public void SetId(string areaId)
        {
            m_areaId = areaId;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!string.IsNullOrWhiteSpace(m_areaId))
            {
                Clicked?.Invoke(m_areaId);
            }
        }
    }
}
