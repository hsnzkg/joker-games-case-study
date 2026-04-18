namespace Project.Scripts.HFSM
{
    public interface ITransition
    {
        StateBase To { get;}
        IPredicate Condition { get;}
    }
}