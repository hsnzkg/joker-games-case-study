namespace Project.Scripts.Physic
{
    public struct SimulationState
    {
        public readonly int Buffer;
        public readonly BallState[] BallStates;
        public readonly DeskState[] DeskStates;

        public SimulationState(int buffer)
        {
            Buffer = buffer;
            BallStates = new BallState[Buffer];
            DeskStates = new DeskState[Buffer];
        }
    }
}