namespace Project.Scripts.EventBus.Events.GUI
{
    public readonly struct EPlayPress : IEvent
    {
        public int? DeterministicNumber { get; }

        public EPlayPress(int? deterministicNumber = null)
        {
            DeterministicNumber = deterministicNumber;
        }
    }
}
