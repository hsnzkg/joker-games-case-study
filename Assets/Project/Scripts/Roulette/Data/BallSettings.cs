using UnityEngine;

namespace Project.Scripts.Roulette.Data
{
    [CreateAssetMenu(fileName = "BallSettings", menuName = "Project/BallSettings", order = 0)]
    public class BallSettings : ScriptableObject
    {
        [Header("Prefab Settings")]
        public Ball.Ball Prefab;
        
        [Header("Simulation Settings")]
        public Vector3 DirectionMin = new(-1f, 0f, -1f);
        public Vector3 DirectionMax = new(1f, 0.35f, 1f);
        public Vector2 ForceRange = new(2f, 6f);
    }
}
