using Project.Scripts.StateMachine.RuntimeMode;

namespace Project.Scripts.StateMachine
{
    public partial class StateBase
    {
        internal StateMachine StateMachine { get; private set; }
        internal StateBase Parent { get; set; }
        internal bool IsHierarchical => StateMachine != null;
        protected bool Debug;
        
        protected StateBase(IRuntimeMode runtimeMode, bool debug = false)
        {
            if (runtimeMode == null)
            {
                UnityEngine.Debug.LogWarning("[FSM] State Config.RuntimeMode is null creating a default one...");
                StateMachine = new StateMachine(new AutoMode(),debug);
            }
            else
            {
                StateMachine = new StateMachine(runtimeMode,debug);
            }
        }
        
        internal void ConvertHierarchical()
        {
            StateMachine = new StateMachine(new AutoMode(),Debug);
        }
        
        internal void OnChildStateChangedInternal(StateBase from, StateBase to)
        {
            OnChildStateChanged(from, to);
        }
        
        internal void OnChildExitedInternal(StateBase state)
        {
            if (StateMachine.Debug)
            {
                UnityEngine.Debug.Log($"[FSM] State Machine Inner Child Exit: {state.GetType().Name}");
            }
            
            OnChildExited(state);
        }

        internal void OnChildEnteredInternal(StateBase state)
        {

            if (StateMachine.Debug)
            {
                UnityEngine.Debug.Log($"[FSM] State Machine Inner Child Entry: {state.GetType().Name}");
            }

            OnChildEntered(state);
        }

        protected virtual void OnChildEntered(StateBase state)
        {
        }

        protected virtual void OnChildExited(StateBase state)
        {
        }
        
        protected virtual void OnChildStateChanged(StateBase from , StateBase to)
        {
        }
    }
}