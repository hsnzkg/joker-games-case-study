using UnityEngine;

namespace Project.Scripts.Physic
{
    public struct BallState
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public readonly bool IsCollidingWithSlot;
        public readonly Collider SlotCollider;

        public BallState(Vector3 pos, Quaternion rot, bool isCollidingWithSlot = false, Collider slotCollider = null)
        {
            Position = pos;
            Rotation = rot;
            IsCollidingWithSlot = isCollidingWithSlot;
            SlotCollider = slotCollider;
        }
    }
}
