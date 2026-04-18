using Project.Scripts.EventBus;
using Project.Scripts.EventBus.Events.Application;
using Project.Scripts.HFSM;

namespace Project.Scripts.Roulette.Game.StateMachine.Core
{
    public class GameStateBase : StateBase
    {
        protected readonly GameStateContext Context;
        private readonly EventBind<EStart> m_applicationStartBind;
        private readonly EventBind<EQuit> m_applicationQuitBind;
        
        protected GameStateBase(GameStateContext context)
        {
            Context = context;
            
            m_applicationStartBind = new EventBind<EStart>(OnApplicationStarted);
            m_applicationQuitBind = new EventBind<EQuit>(OnApplicationQuit);
            
            EventBus<EStart>.Register(m_applicationStartBind);
            EventBus<EQuit>.Register(m_applicationQuitBind);
        }

        public virtual void Save(){}

        public virtual void Load(){}

        protected virtual void OnApplicationStarted(EStart obj)
        {
        }

        protected void OnApplicationQuit(EQuit obj)
        {
        }
    }
}