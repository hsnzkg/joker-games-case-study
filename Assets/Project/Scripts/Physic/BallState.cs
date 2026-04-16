using UnityEngine;

namespace Project.Scripts.Physic
{
    public struct BallState
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public bool IsCollidingWithSlot;
        public Collider SlotCollider;

        public BallState(Vector3 pos, Quaternion rot)
        {
            Position = pos;
            Rotation = rot;
            IsCollidingWithSlot = false;
            SlotCollider = null;
        }
    }
}