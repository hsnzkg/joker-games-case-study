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
        private DeskPhysicSettings m_settings;
        public bool IsEnabled { get; private set; }
        public Transform SpinTransform => m_spinTransform;

        public DeskPhysicSystem(DeskPhysicSettings deskPhysicSettings, Transform spinTransform)
        {
            m_settings = deskPhysicSettings;
            m_spinTransform = spinTransform;
            m_tick = m_settings.Tick;
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

        public void StartSpin(float speed,float drag,float startAngle = 0f)
        {
            Enable();
            m_spinTransform.rotation = Quaternion.Euler(0, startAngle, 0);
            m_spinInitialSpeed = speed;
            m_remainingSpeed = speed;
            m_drag = drag;
        }

        public void Reset()
        {
            m_spinTransform.rotation = Quaternion.identity;
        }

        public void DrawGizmos()
        {
            Gizmos.color = Color.green;

            Vector3 center = m_spinTransform.position + m_settings.SlotOriginOffset;

            Matrix4x4 oldMatrix = Gizmos.matrix;

            float slotPerAngle = 360f / m_settings.SlotCount;
            for (float i = 0f; i < 360f; i += slotPerAngle)
            {
                float percentage = i / 360f;
  
                
                Quaternion rot =
                    Quaternion.Euler(0f, i, 0f) *
                    Quaternion.Euler(m_settings.SlotRotationOffset);

                Vector3 dir = rot * Vector3.forward;
                Vector3 pointB = center + dir * m_settings.DistanceFromOrigin;

                Gizmos.matrix = oldMatrix;
                Gizmos.color = Color.aquamarine;
                Gizmos.DrawLine(center, pointB);
                
                Color color = Color.Lerp(Color.purple, Color.blue, percentage);
                Gizmos.color = color;
                Gizmos.matrix = Matrix4x4.TRS(pointB, rot, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, m_settings.SlotBoxSize);
            }

            Gizmos.matrix = oldMatrix;
        }
    }
}