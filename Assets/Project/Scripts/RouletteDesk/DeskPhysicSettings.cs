using UnityEngine;

namespace Project.Scripts.RouletteDesk
{
    [CreateAssetMenu(fileName = "DeskPhysicSettings", menuName = "Project/DeskPhysicSettings", order = 0)]
    public class DeskPhysicSettings : ScriptableObject
    {
        [Range(1,37)] public float SlotCount;
        public Vector3 SlotOriginOffset;
        public Vector3 SlotRotationOffset;
        public Vector3 SlotBoxSize;
        public float DistanceFromOrigin;
        public float Tick;
    }
}