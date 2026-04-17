using UnityEngine;

namespace Project.Scripts.Utility.Easing
{
    public static class EaseUtility
    {
        public static float EaseInCirc(float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            return 1f - Mathf.Sqrt(1f - (clampedValue * clampedValue));
        }
    }
}
