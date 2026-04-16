using UnityEngine;

namespace Project.Scripts.Physic.State
{
    public struct BallState
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public readonly bool IsCollidingWithSlot;
        public readonly int SlotIndex;

        public BallState(Vector3 pos, Quaternion rot, bool isCollidingWithSlot = false, int slotIndex = -1)
        {
            Position = pos;
            Rotation = rot;
            IsCollidingWithSlot = isCollidingWithSlot;
            SlotIndex = -1;
        }
    }
}
