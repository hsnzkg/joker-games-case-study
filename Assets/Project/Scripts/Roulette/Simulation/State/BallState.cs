using UnityEngine;

namespace Project.Scripts.Roulette.Simulation.State
{
    public struct BallState
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public readonly int SlotIndex;

        public BallState(Vector3 pos, Quaternion rot, int slotIndex = -1)
        {
            Position = pos;
            Rotation = rot;
            SlotIndex = slotIndex;
        }
    }
}
