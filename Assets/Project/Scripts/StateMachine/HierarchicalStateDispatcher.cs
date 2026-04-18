namespace Project.Scripts.StateMachine
{
    public class HierarchicalStateDispatcher : IDispatcher
    {
        public void Dispatch(StateBase state)
        {
            if (state.IsHierarchical)
            {
                state.StateMachine.Update();
            } 
        }
    }
}