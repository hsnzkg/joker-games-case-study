namespace Project.Scripts.HFSM
{
    public struct Transition : ITransition
    {
        public StateBase To { get; }
        public IPredicate Condition { get; }

        public Transition(StateBase to, IPredicate condition)
        {
            To = to;
            Condition = condition;
        }
    }
}