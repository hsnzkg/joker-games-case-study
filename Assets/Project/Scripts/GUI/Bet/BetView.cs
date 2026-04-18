using System;
using System.Collections.Generic;
using Project.Scripts.GUI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.GUI.Bet
{
    public class BetView : ViewBase
    {
        [SerializeField] private List<BetArea> m_betAreas = new();

        public event Action<BetArea> BetAreaPressed;

        protected override void Initialize()
        {
            base.Initialize();
            EnsureClickHandlers();
        }

        protected override void OnEnable()
        {
            Register();
        }

        protected override void OnDisable()
        {
            Unregister();
        }

        private void Register()
        {
            foreach (var betArea in m_betAreas)
            {
                if (betArea.Target == null)
                {
                    Debug.LogWarning("Bet area target is missing.", this);
                    continue;
                }

                var clickHandler = GetOrCreateClickHandler(betArea.Target);
                clickHandler.Initialize(betArea);
                clickHandler.Clicked -= OnBetAreaClicked;
                clickHandler.Clicked += OnBetAreaClicked;
            }
        }

        private void Unregister()
        {
            foreach (var betArea in m_betAreas)
            {
                if (betArea.Target == null || !betArea.Target.TryGetComponent<BetAreaClickHandler>(out var clickHandler))
                {
                    continue;
                }

                clickHandler.Clicked -= OnBetAreaClicked;
            }
        }

        private void EnsureClickHandlers()
        {
            foreach (var betArea in m_betAreas)
            {
                if (betArea.Target == null)
                {
                    continue;
                }

                var clickHandler = GetOrCreateClickHandler(betArea.Target);
                clickHandler.Initialize(betArea);
            }
        }

        private BetAreaClickHandler GetOrCreateClickHandler(RectTransform target)
        {
            if (!target.TryGetComponent<BetAreaClickHandler>(out var clickHandler))
            {
                clickHandler = target.gameObject.AddComponent<BetAreaClickHandler>();
            }

            EnsureRaycastTarget(target);
            return clickHandler;
        }

        private static void EnsureRaycastTarget(RectTransform target)
        {
            if (target.TryGetComponent<Graphic>(out _))
            {
                return;
            }

            var image = target.gameObject.AddComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = true;
        }

        private void OnBetAreaClicked(BetArea betArea)
        {
            BetAreaPressed?.Invoke(betArea);
        }
    }
}