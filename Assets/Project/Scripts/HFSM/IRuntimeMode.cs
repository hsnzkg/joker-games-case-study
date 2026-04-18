namespace Project.Scripts.HFSM
{
    public interface IRuntimeMode
    {
        bool Tick(StateMachine fsm);
    }
}