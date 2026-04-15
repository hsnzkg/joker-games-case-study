using System;

namespace Project.Scripts.Physic
{
    [Serializable]
    public struct SimulationState
    {
        public BallState[] BallStates;
        public DeskState[] DeskStates;
    }
}