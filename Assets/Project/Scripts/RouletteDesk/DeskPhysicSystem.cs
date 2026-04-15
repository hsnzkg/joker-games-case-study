using System;
using UnityEngine;

namespace Project.Scripts.RouletteDesk
{
    [Serializable]
    public class DeskPhysicSystem
    {
        private readonly Transform m_spinTransform;
        private float m_spinInitialSpeed;
        private float m_remainingSpeed;
        private float m_drag;
        private readonly float m_tick;
        public bool IsEnabled { get; private set; }

        public DeskPhysicSystem(Transform spinTransform)
        {
            m_spinTransform = spinTransform;
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

        public void StartSpin(float speed,float drag)
        {
            Enable();
            m_spinInitialSpeed = speed;
            m_remainingSpeed = speed;
            m_drag = drag;
        }

        public void Reset()
        {
            m_spinTransform.rotation = Quaternion.identity;
        }
    }
}