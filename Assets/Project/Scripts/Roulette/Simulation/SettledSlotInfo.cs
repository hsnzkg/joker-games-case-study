namespace Project.Scripts.Roulette.Simulation
{
    public readonly struct SettledSlotInfo
    {
        public readonly bool HasSettledSlot;
        public readonly int FinalSlotIndex;
        public readonly int ContinuousStartFrame;

        public SettledSlotInfo(bool hasSettledSlot, int finalSlotIndex, int continuousStartFrame)
        {
            HasSettledSlot = hasSettledSlot;
            FinalSlotIndex = finalSlotIndex;
            ContinuousStartFrame = continuousStartFrame;
        }
    }
}