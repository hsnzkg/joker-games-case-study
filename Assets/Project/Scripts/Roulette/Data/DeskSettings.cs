using Project.Scripts.Roulette.RouletteDesk;
using UnityEngine;

namespace Project.Scripts.Roulette.Data
{
    [CreateAssetMenu(fileName = "DeskSettings", menuName = "Project/DeskSettings", order = 0)]
    public class DeskSettings : ScriptableObject
    {
        [Header("Prefab Settings")]
        public Desk Prefab;
        
        [Header("Configuration")]
        [Range(1,37)] public int SlotCount;
        public Vector3 SlotOriginOffset;
        public Vector3 SlotRotationOffset;
        public Vector3 SlotBoxSize;
        public float DistanceFromOrigin;
        
        [Header("Simulation Settings")]
        public Vector2 SpinSpeedRange = new(60f, 140f);
        public Vector2 SpinDragRange = new(4f, 14f);
        public Vector2 SpinStartAngleRange = new(0f, 360f);
        public float Tick;
    }
}