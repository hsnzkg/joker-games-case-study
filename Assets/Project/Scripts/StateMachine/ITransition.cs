namespace Project.Scripts.StateMachine
{
    public interface ITransition
    {
        StateBase To { get;}
        IPredicate Condition { get;}
    }
}