using UnityEngine;

namespace Project.Scripts.StateMachine
{
    public class StateBuilder
    {
        private readonly StateBase m_state;

        public StateBuilder(StateBase state)
        {
            m_state = state;
        }

        public StateBuilder Add(StateBase state)
        {
            state.Parent = m_state;
            if (!m_state.IsHierarchical)
            {
                m_state.ConvertHierarchical();
            }

            m_state.StateMachine.AddState(state);
            return this;
        }

        public StateBuilder SetDefault(StateBase state)
        {
            if (!m_state.HasState(state))
            {
                Debug.LogWarning($"[FSM] does not contain {state.GetType().Name} adding automatically...");
                Add(state);
            }

            m_state.StateMachine.SetDefaultState(state);
            return this;
        }

        public StateBuilder SetDefault<T>()
        {
            m_state.StateMachine.SetDefaultState<T>();
            return this;
        }

        public StateBuilder AddTransition<TFrom, TTo>(IPredicate condition) where TFrom : StateBase where TTo : StateBase
        {
            m_state.StateMachine.AddTransition<TFrom, TTo>(condition);
            return this;
        }

        public StateBuilder AddAnyTransition<TTo>(IPredicate condition) where TTo : StateBase
        {
            m_state.StateMachine.AddAnyTransition<TTo>(condition);
            return this;
        }

        public StateBuilder AddTransition(StateBase from, StateBase to, IPredicate condition)
        {
            m_state.StateMachine.AddTransition(from, to, condition);
            return this;
        }
 
        public StateBuilder AddAnyTransition(StateBase to, IPredicate condition)
        {
            m_state.StateMachine.AddAnyTransition(to, condition);
            return this;
        }
    }
}