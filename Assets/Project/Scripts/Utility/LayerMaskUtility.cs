using UnityEngine;

namespace Project.Scripts.Utility
{
    public static class LayerMaskUtility
    {
        public static bool Contains(this LayerMask mask, int layer)
        {
            return (mask.value & (1 << layer)) != 0;
        }
    }
}