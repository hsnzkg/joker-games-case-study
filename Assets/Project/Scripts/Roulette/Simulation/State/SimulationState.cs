using Project.Scripts.Roulette.Desk;

namespace Project.Scripts.Roulette.Simulation.State
{
    public struct SimulationState
    {
        public int Buffer;
        public float TickDuration;
        public BallState[] BallStates;
        public DeskState[] DeskStates;
        public int FrameCount;
        public SlotInfo FinalSlotInfo;

        public SimulationState(int buffer, float tickDuration)
        {
            Buffer = buffer;
            TickDuration = tickDuration;
            BallStates = new BallState[Buffer];
            DeskStates = new DeskState[Buffer];
            FrameCount = 0;
            FinalSlotInfo = new SlotInfo(-1, -1, SlotColor.UNKNOWN);
        }
    }
}
