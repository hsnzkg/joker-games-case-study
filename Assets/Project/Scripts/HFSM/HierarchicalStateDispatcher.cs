namespace Project.Scripts.HFSM
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