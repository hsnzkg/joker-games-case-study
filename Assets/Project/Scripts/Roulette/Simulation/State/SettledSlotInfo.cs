using Project.Scripts.Roulette.Desk;

namespace Project.Scripts.Roulette.Simulation.State
{
    public readonly struct SettledSlotInfo
    {
        public readonly bool HasSettledSlot;
        public readonly SlotInfo SlotInfo;
        public readonly int ContinuousStartFrame;

        public SettledSlotInfo(bool hasSettledSlot, SlotInfo slotInfo, int continuousStartFrame)
        {
            HasSettledSlot = hasSettledSlot;
            SlotInfo = slotInfo;
            ContinuousStartFrame = continuousStartFrame;
        }
    }
}
