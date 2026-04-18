using UnityEngine;

namespace Project.Scripts.Roulette.Simulation.State
{
    public struct BallState
    {
        public SerializableVector3 Position;
        public SerializableQuaternion Rotation;
        public int SlotIndex;

        public BallState(Vector3 pos, Quaternion rot, int slotIndex = -1)
        {
            Position = pos;
            Rotation = rot;
            SlotIndex = slotIndex;
        }
    }

    public struct SerializableVector3
    {
        public float X;
        public float Y;
        public float Z;

        public SerializableVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static implicit operator SerializableVector3(Vector3 vector)
        {
            return new SerializableVector3(vector.x, vector.y, vector.z);
        }

        public static implicit operator Vector3(SerializableVector3 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }
    }

    public struct SerializableQuaternion
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public SerializableQuaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static implicit operator SerializableQuaternion(Quaternion quaternion)
        {
            return new SerializableQuaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        }

        public static implicit operator Quaternion(SerializableQuaternion quaternion)
        {
            return new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }
    }
}
