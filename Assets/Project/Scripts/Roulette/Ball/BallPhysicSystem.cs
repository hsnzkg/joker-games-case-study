using System;
using UnityEngine;

namespace Project.Scripts.Roulette.Ball
{
    [Serializable]
    public class BallPhysicSystem
    {
        private readonly Rigidbody m_rigidbody;

        public BallPhysicSystem(GameObject instance)
        {
            m_rigidbody = instance.GetComponent<Rigidbody>();
        }

        public void Start()
        {
            m_rigidbody.isKinematic = false;
            m_rigidbody.linearVelocity = Vector3.zero;
            m_rigidbody.angularVelocity = Vector3.zero;
        }

        public void Stop()
        {
            if (m_rigidbody.isKinematic)
            {
                return;
            }

            m_rigidbody.linearVelocity = Vector3.zero;
            m_rigidbody.angularVelocity = Vector3.zero;
            m_rigidbody.isKinematic = true;
        }

        public void Reset()
        {
            Reset(Vector3.zero, Quaternion.identity);
        }

        public void Reset(Vector3 position, Quaternion rotation)
        {
            Stop();
            m_rigidbody.position = position;
            m_rigidbody.rotation = rotation;
        }

        public void Launch(Vector3 fromPos, Quaternion fromRot, Vector3 dir, float force)
        {
            m_rigidbody.position = fromPos;
            m_rigidbody.rotation = fromRot;
            m_rigidbody.AddForce(dir * force, ForceMode.Impulse);
        }
    }
}
