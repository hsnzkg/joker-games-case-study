namespace Project.Scripts.StateMachine
{
    public interface IRuntimeMode
    {
        bool Tick(StateMachine fsm);
    }
}