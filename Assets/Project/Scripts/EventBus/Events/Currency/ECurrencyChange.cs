using Project.Scripts.EventBus;

namespace Project.Scripts.EventBus.Events.Currency
{
    public readonly struct ECurrencyChange : IEvent
    {
        public int PreviousAmount { get; }
        public int NewAmount { get; }
        public int Delta => NewAmount - PreviousAmount;

        public ECurrencyChange(int previousAmount, int newAmount)
        {
            PreviousAmount = previousAmount;
            NewAmount = newAmount;
        }
    }
}
