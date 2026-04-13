using System;
using UnityEngine;

namespace Project.Scripts.RouletteDesk
{
    [Serializable]
    public class DeskRotationSystem
    {
        private readonly Transform m_spinTransform;
        private float m_spinInitialSpeed;
        private float m_remainingSpeed;
        private readonly float m_drag;
        private readonly float m_tick;
        public bool IsEnabled { get; private set; }

        public DeskRotationSystem(Transform spinTransform, float drag)
        {
            m_spinTransform = spinTransform;
            m_drag = drag;
            m_tick = Time.fixedDeltaTime;
        }

        public void Tick()
        {
            if(!IsEnabled) return;
            m_spinTransform.Rotate(Vector3.up * (m_remainingSpeed * m_tick), Space.Self);
            m_remainingSpeed -= Time.fixedDeltaTime * m_drag;
            if (!(m_remainingSpeed <= 0)) return;
            m_remainingSpeed = 0;
            Disable();
        }
        
        public void Enable()
        {
            if(IsEnabled) return;
            IsEnabled = true;
        }

        public void Disable()
        {
            if(!IsEnabled) return;
            IsEnabled = false;
        }

        public void StartSpin(float initialSpeed)
        {
            m_spinInitialSpeed = initialSpeed;
            m_remainingSpeed = m_spinInitialSpeed;
            Enable();
        }

        public void Reset()
        {
            m_spinTransform.rotation = Quaternion.identity;
        }
    }
}