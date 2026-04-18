using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Project.Scripts.GUI.Bet
{
    public class BetAreaClickHandler : MonoBehaviour, IPointerClickHandler
    {
        private BetArea m_betArea;

        public event Action<BetArea> Clicked;

        public void Initialize(BetArea betArea)
        {
            m_betArea = betArea;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke(m_betArea);
        }
    }
}