namespace Project.Scripts.StateMachine.RuntimeMode
{
    public sealed class AutoMode : IRuntimeMode
    {
        public bool Tick(StateMachine fsm)
        {
            bool hasTransition = fsm.HasValidTransition(out ITransition transition);
            if (hasTransition) fsm.ChangeStateInternal(transition.To);
            return hasTransition;
        }
    }
}