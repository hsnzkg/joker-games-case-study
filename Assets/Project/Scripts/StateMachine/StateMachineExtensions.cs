using UnityEngine;

namespace Project.Scripts.StateMachine
{
    public static class StateMachineExtensions
    {
        public static StateBuilder Builder(this StateBase state)
        {
            return new StateBuilder(state);
        }

        public static bool HasState(this StateBase parent, StateBase child)
        {
            if (parent.IsHierarchical) return parent.StateMachine.HasState(child);
            Debug.LogError("[FSM] State is not hierarchical !");
            return false;
        }

        public static void ChangeRuntimeMode(this StateBase state, IRuntimeMode mode)
        {
            state.StateMachine?.SetRuntimeMode(mode);
        }

        public static StateBuilder GetState<T>(this StateBase state)
        {
            return new StateBuilder(state.StateMachine.GetState<T>());
        }

        public static StateBase GetActiveState(this StateBase state)
        {
            return state.StateMachine.GetActiveState();
        }

        public static StateBase GetLeafState(this StateBase state)
        {
            return state.StateMachine?.GetLeafState();
        }
    }
}