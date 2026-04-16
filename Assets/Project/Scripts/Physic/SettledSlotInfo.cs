using UnityEngine;

namespace Project.Scripts.Physic
{
    public readonly struct SettledSlotInfo
    {
        public readonly bool HasSettledSlot;
        public readonly int FinalSlotIndex;
        public readonly int ContinuousStartFrame;
        public readonly Vector3 SlotLocalBallPosition;

        public SettledSlotInfo(bool hasSettledSlot, int finalSlotIndex, int continuousStartFrame, Vector3 slotLocalBallPosition)
        {
            HasSettledSlot = hasSettledSlot;
            FinalSlotIndex = finalSlotIndex;
            ContinuousStartFrame = continuousStartFrame;
            SlotLocalBallPosition = slotLocalBallPosition;
        }
    }
}