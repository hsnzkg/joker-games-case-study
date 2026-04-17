using System;
using Project.Scripts.Roulette.Data;
using Project.Scripts.Utility.Easing;
using UnityEngine;
using SimulationMode = Project.Scripts.Roulette.Simulation.SimulationMode;

namespace Project.Scripts.Roulette.RouletteDesk
{
    public class DeskPhysicSystem : IDisposable
    {
        private Desk m_desk;
        private Transform m_spinTransform;
        private float m_currentSpeed;
        private float m_targetSpeed;
        private float m_drag;
        private float m_spinEaseDuration;
        private float m_spinEaseElapsed;
        private DeskSettings m_settings;
        private bool m_isSpinEasingIn;
        public event Action OnSpinEaseInCompleted;
        public bool IsEnabled { get; private set; }

        public DeskPhysicSystem(Desk desk, DeskSettings deskSettings, Transform spinTransform)
        {
            m_desk = desk;
            m_settings = deskSettings;
            m_spinTransform = spinTransform;
        }

        public void Tick(float delta)
        {
            if (!IsEnabled) return;

            if (m_isSpinEasingIn)
            {
                m_spinEaseElapsed += delta;
                float t = Mathf.Clamp01(m_spinEaseElapsed / m_spinEaseDuration);
                m_currentSpeed = Mathf.Lerp(0f, m_targetSpeed, EaseUtility.EaseOutCirc(t));

                if (t >= 1f)
                {
                    m_currentSpeed = m_targetSpeed;
                    m_isSpinEasingIn = false;
                    OnSpinEaseInCompleted?.Invoke();
                }
            }
            else
            {
                m_currentSpeed -= delta * m_drag;
                if (m_currentSpeed <= 0f)
                {
                    m_currentSpeed = 0f;
                    Stop();
                    return;
                }
            }

            m_spinTransform.Rotate(Vector3.up * (m_currentSpeed * delta), Space.Self);
        }

        public void Start()
        {
            if (IsEnabled) return;
            IsEnabled = true;
        }

        public void Stop()
        {
            if (!IsEnabled) return;
            IsEnabled = false;
        }

        public void StartSpin(float speed, float drag, float startAngle = 0f)
        {
            m_spinTransform.rotation = Quaternion.Euler(0f, startAngle, 0f);
            m_targetSpeed = speed;
            m_currentSpeed = 0f;
            m_drag = drag;
            m_spinEaseElapsed = 0f;
            m_spinEaseDuration = m_settings != null ? Mathf.Max(0f, m_settings.SpinEaseInDuration) : 0f;
            m_isSpinEasingIn = m_spinEaseDuration > 0f;

            if (!m_isSpinEasingIn)
            {
                m_currentSpeed = m_targetSpeed;
                OnSpinEaseInCompleted?.Invoke();
            }
        }

        public void Reset()
        {
            m_currentSpeed = 0f;
            m_targetSpeed = 0f;
            m_drag = 0f;
            m_spinEaseDuration = 0f;
            m_spinEaseElapsed = 0f;
            m_isSpinEasingIn = false;
            m_spinTransform.rotation = Quaternion.identity;
        }

        public void DrawGizmos()
        {
            if (m_desk.SimulationMode == SimulationMode.Simulation) return;
            if (!m_spinTransform) return;
            if (!m_settings) return;
            Gizmos.color = Color.green;
            Vector3 center = m_spinTransform.position + m_settings.SlotOriginOffset;

            Matrix4x4 oldMatrix = Gizmos.matrix;

            float slotPerAngle = 360f / m_settings.SlotCount;

            for (int i = 0; i < m_settings.SlotCount; i++)
            {
                float percentage = i / 360f;
                float angle = i * slotPerAngle;

                Quaternion rot = Quaternion.Euler(0f, angle, 0f) * Quaternion.Euler(m_settings.SlotRotationOffset) * m_spinTransform.rotation;

                Vector3 dir = rot * Vector3.forward;
                Vector3 pointB = center + dir * m_settings.DistanceFromOrigin;

                Gizmos.matrix = oldMatrix;
                Gizmos.color = Color.aquamarine;
                Gizmos.DrawLine(center, pointB);

                Color color = Color.Lerp(Color.purple, Color.blue, percentage);
                Gizmos.color = color;
                Gizmos.matrix = Matrix4x4.TRS(pointB, rot, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, m_settings.SlotBoxSize);

#if UNITY_EDITOR
                Gizmos.matrix = oldMatrix;
                UnityEditor.Handles.Label(pointB, i.ToString());
#endif
            }

            Gizmos.matrix = oldMatrix;
        }

        public void Dispose()
        {
            m_desk = null;
            m_settings = null;
            m_spinTransform = null;
        }
    }
}
