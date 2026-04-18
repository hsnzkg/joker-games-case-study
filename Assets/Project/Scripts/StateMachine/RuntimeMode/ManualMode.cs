namespace Project.Scripts.StateMachine.RuntimeMode
{
    public sealed class ManualMode : IRuntimeMode
    {
        public bool Tick(StateMachine fsm)
        {
            return false;
        }
    }
}