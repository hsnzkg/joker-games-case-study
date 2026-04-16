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
            Stop();
        }

        public void Start()
        {
            m_rigidbody.isKinematic = false;
            m_rigidbody.linearVelocity = Vector3.zero;
            m_rigidbody.angularVelocity = Vector3.zero;
        }

        public void Stop()
        {
            m_rigidbody.linearVelocity = Vector3.zero;
            m_rigidbody.angularVelocity = Vector3.zero;
            m_rigidbody.isKinematic = true;
        }

        public void Reset()
        {
            Stop();
            m_rigidbody.position = Vector3.zero;
            m_rigidbody.rotation = Quaternion.identity;
        }
        
        public void Launch(Vector3 fromPos,Quaternion fromRot, Vector3 dir, float force)
        {
            m_rigidbody.position = fromPos;
            m_rigidbody.rotation = fromRot;
            Start();
            m_rigidbody.AddForce(dir * force,ForceMode.Impulse);
        }
    }
}
