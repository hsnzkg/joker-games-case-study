namespace Project.Scripts.HFSM.RuntimeMode
{
    public sealed class ManualMode : IRuntimeMode
    {
        public bool Tick(StateMachine fsm)
        {
            return false;
        }
    }
}