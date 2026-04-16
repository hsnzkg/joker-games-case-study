using Project.Scripts.Physic.State;

namespace Project.Scripts.Physic
{
    public struct DeterministicCandidate
    {
        public bool HasValue;
        public float StartAngle;
        public float MatchScore;
        public SimulationState State;
        public SettledSlotInfo SettledSlotInfo;
    }
}