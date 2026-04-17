namespace Project.Scripts.Roulette.Simulation.State
{
    public struct SimulationState
    {
        public readonly int Buffer;
        public readonly float TickDuration;
        public readonly BallState[] BallStates;
        public readonly DeskState[] DeskStates;
        public int FrameCount;
        public Project.Scripts.Roulette.RouletteDesk.SlotInfo FinalSlotInfo;

        public SimulationState(int buffer, float tickDuration)
        {
            Buffer = buffer;
            TickDuration = tickDuration;
            BallStates = new BallState[Buffer];
            DeskStates = new DeskState[Buffer];
            FrameCount = 0;
            FinalSlotInfo = new Project.Scripts.Roulette.RouletteDesk.SlotInfo(-1, -1, Project.Scripts.Roulette.RouletteDesk.SlotColor.UNKNOWN);
        }
    }
}
