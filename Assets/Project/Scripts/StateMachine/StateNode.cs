using System.Collections.Generic;

namespace Project.Scripts.StateMachine
{
    public class StateNode
    {
        public StateBase StateBase { get; }
        public HashSet<ITransition> Transitions { get; }

        public StateNode(StateBase stateBase)
        {
            StateBase = stateBase;
            Transitions = new HashSet<ITransition>();
        }

        public void AddTransition(StateBase to, IPredicate condition)
        {
            Transitions.Add(new Transition(to, condition));
        }
    }
}