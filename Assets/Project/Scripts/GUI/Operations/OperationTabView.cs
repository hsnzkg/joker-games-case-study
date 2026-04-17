using System;
using Project.Scripts.GUI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.GUI.Operations
{
    public class OperationTabView : ViewBase
    {
        [SerializeField] private Button m_playButton;
        [SerializeField] private Button m_undoButton;
        [SerializeField] private Button m_resetButton;
        
        public event Action PlayPressed;
        public event Action UndoPressed;
        public event Action ResetPressed;
        
        protected override void OnEnable()
        {
            Register();
        }

        protected override void OnDisable()
        {
            Unregister();
            m_playButton.interactable = false;
            m_undoButton.interactable = false;
            m_resetButton.interactable = false;
        }

        private void Register()
        {
            m_playButton.onClick.AddListener(OnPlayPressed);
            m_undoButton.onClick.AddListener(OnUndoPressed);
            m_resetButton.onClick.AddListener(OnResetPressed);
        }
        
        private void Unregister()
        {
            m_playButton.onClick.RemoveListener(OnPlayPressed);
            m_undoButton.onClick.RemoveListener(OnUndoPressed);
            m_resetButton.onClick.RemoveListener(OnResetPressed);
        }

        private void OnPlayPressed()
        {
            PlayPressed?.Invoke();
        }

        private void OnUndoPressed()
        {
            UndoPressed?.Invoke();
        }

        private void OnResetPressed()
        {
            ResetPressed?.Invoke();
        }

        public void SetOperationInteractivity(bool value)
        {
            m_playButton.interactable = value;
            m_undoButton.interactable = value;
            m_resetButton.interactable = value;
        }
    }
}   