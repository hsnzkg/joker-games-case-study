using System;
using Project.Scripts.Roulette.Data;
using UnityEngine;
using SimulationMode = Project.Scripts.Roulette.Simulation.SimulationMode;

namespace Project.Scripts.Roulette.RouletteDesk
{
    public class DeskPhysicSystem : IDisposable
    {
        private Desk m_desk;
        private Transform m_spinTransform;
        private float m_remainingSpeed;
        private float m_drag;
        private DeskSettings m_settings;
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
            m_spinTransform.Rotate(Vector3.up * (m_remainingSpeed * delta), Space.Self);
            m_remainingSpeed -= delta * m_drag;
            if (!(m_remainingSpeed <= 0)) return;
            m_remainingSpeed = 0;
            Stop();
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
            m_spinTransform.rotation = Quaternion.Euler(0, startAngle, 0);
            m_remainingSpeed = speed;
            m_drag = drag;
        }

        public void Reset()
        {
            m_spinTransform.rotation = Quaternion.identity;
        }

        public void DrawGizmos()
        {
            if (!IsEnabled) return;
            if (m_desk.SimulationMode == SimulationMode.Simulation) return;

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