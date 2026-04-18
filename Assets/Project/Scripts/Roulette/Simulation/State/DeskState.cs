using UnityEngine;

namespace Project.Scripts.Roulette.Simulation.State
{
    public struct DeskState
    {
        public SerializableVector3 Position;
        public SerializableQuaternion Rotation;

        public DeskState(Vector3 pos, Quaternion rot)
        {
            Position = pos;
            Rotation = rot;
        }
    }
}
