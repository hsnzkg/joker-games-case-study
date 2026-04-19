using System;
using Project.Scripts.GUI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.GUI.Operations
{
    public class OperationTabView : ViewBase
    {
        [SerializeField] private Button m_playButton;
        [SerializeField] private Button m_undoButton;
        [SerializeField] private Button m_resetButton;
        [SerializeField] private TMP_InputField m_deterministicNumberInput;

        public event Action PlayPressed;
        public event Action UndoPressed;
        public event Action ResetPressed;
        public event Action<string> DeterministicNumberChanged;

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
            m_deterministicNumberInput.interactable = false;
        }

        private void Register()
        {
            m_playButton.onClick.AddListener(OnPlayPressed);
            m_undoButton.onClick.AddListener(OnUndoPressed);
            m_resetButton.onClick.AddListener(OnResetPressed);
            m_deterministicNumberInput.onValueChanged.AddListener(OnDeterministicNumberChanged);
        }

        private void Unregister()
        {
            m_playButton.onClick.RemoveListener(OnPlayPressed);
            m_undoButton.onClick.RemoveListener(OnUndoPressed);
            m_resetButton.onClick.RemoveListener(OnResetPressed);
            m_deterministicNumberInput.onValueChanged.RemoveListener(OnDeterministicNumberChanged);
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

        private void OnDeterministicNumberChanged(string value)
        {
            DeterministicNumberChanged?.Invoke(value);
        }

        public void SetDeterministicNumberText(string value)
        {
            if (m_deterministicNumberInput == null)
            {
                return;
            }

            string safeValue = value ?? string.Empty;
            if (m_deterministicNumberInput.text == safeValue)
            {
                return;
            }

            m_deterministicNumberInput.SetTextWithoutNotify(safeValue);
        }

        public void ClearDeterministicNumberText()
        {
            SetDeterministicNumberText(string.Empty);
        }

        public void SetOperationInteractivity(bool value)
        {
            m_playButton.interactable = value;
            m_undoButton.interactable = value;
            m_resetButton.interactable = value;

            if (m_deterministicNumberInput != null)
            {
                m_deterministicNumberInput.interactable = value;
            }
        }
    }
}