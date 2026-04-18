using System;

namespace Project.Scripts.StateMachine
{
    public class FunctionPredicate : IPredicate
    {
        private readonly Func<bool> m_func;

        public FunctionPredicate(Func<bool> func)
        {
            m_func = func;
        }
        
        public bool Evaluate() => m_func.Invoke();
    }
}   