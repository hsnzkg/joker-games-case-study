using UnityEngine;

namespace Project.Scripts.Physic
{
    public struct SimulationObjectState
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public SimulationObjectState(Vector3 pos, Quaternion rot)
        {
            Position = pos;
            Rotation = rot;
        }
    }
}