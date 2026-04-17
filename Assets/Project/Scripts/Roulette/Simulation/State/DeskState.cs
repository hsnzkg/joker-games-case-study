using UnityEngine;

namespace Project.Scripts.Roulette.Simulation.State
{
    public struct DeskState
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public DeskState(Vector3 pos, Quaternion rot)
        {
            Position = pos;
            Rotation = rot;
        }
    }
}