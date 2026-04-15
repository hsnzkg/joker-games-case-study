using UnityEngine;

namespace Project.Scripts.Physic
{
    public struct DeskState
    {
        public Quaternion Rotation;
        
        public DeskState(Quaternion rotation)
        {
            Rotation = rotation;
        }
    }
}