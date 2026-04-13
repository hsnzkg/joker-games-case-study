using System;
using UnityEngine;

namespace Project.Scripts.RouletteBall
{
    [Serializable]
    public class BallPhysicSystem
    {
        private readonly Rigidbody m_rigidbody;
        
        public BallPhysicSystem(GameObject instance)
        {
            m_rigidbody = instance.GetComponent<Rigidbody>();
            Disable();
        }

        public void Enable()
        {
            m_rigidbody.isKinematic = false;
            m_rigidbody.linearVelocity = Vector3.zero;
            m_rigidbody.angularVelocity = Vector3.zero;
        }

        public void Disable()
        {
            m_rigidbody.linearVelocity = Vector3.zero;
            m_rigidbody.angularVelocity = Vector3.zero;
            m_rigidbody.isKinematic = true;
        }
    }
}