namespace Project.Scripts.HFSM
{
    public abstract partial class StateBase
    {
        protected StateBase()
        {
        }

        #region Internal Methods

        internal void OnEnterInternal()
        {
            OnEnter();
        }

        internal void OnExitInternal()
        {
            OnExit();
        }
        
        internal void Handle(IDispatcher visitor)
        {
            visitor.Dispatch(this);
        }
        
        #endregion
        
        #region Virtual Methods

        protected virtual void OnEnter()
        {
        }

        protected virtual void OnExit()
        {
        }
        
        #endregion
    }
}