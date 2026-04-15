namespace Project.Scripts.Physic
{
    public struct SimulationState
    {
        public readonly int Buffer;
        public readonly SimulationObjectState[] BallStates;
        public readonly SimulationObjectState[] DeskStates;

        public SimulationState(int buffer)
        {
            Buffer = buffer;
            BallStates = new SimulationObjectState[Buffer];
            DeskStates = new SimulationObjectState[Buffer];
        }
    }
}