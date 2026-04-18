namespace Project.Scripts.HFSM
{
    public interface IDispatcher
    {
        void Dispatch(StateBase state);
    }
    
    public interface IValueDispatcher<in TPayload>
    {
        void Dispatch(StateBase state, TPayload payload);
    }

    public interface IRefDispatcher<TPayload>
    {
        void Dispatch(StateBase state, ref TPayload payload);
    }
}