using System;

namespace Project.Scripts.EventBus
{
    public class EventBind<T> where T : IEvent
    {
        private Action m_handlerNoArgs;
        private Action<T> m_handlerWithArgs;
        internal int Channel { get; }
        internal EventPriority Priority { get; }

        public EventBind(Action handler, int channel = 0, EventPriority priority = EventPriority.MONITOR)
        {
            m_handlerNoArgs = handler;
            m_handlerWithArgs = null;
            Channel = channel;
            Priority = priority;
        }

        public EventBind(Action<T> handler, int channel = 0, EventPriority priority = EventPriority.MONITOR)
        {
            m_handlerWithArgs = handler;
            m_handlerNoArgs = null;
            Channel = channel;
            Priority = priority;
        }

        public void Invoke(T @event)
        {
            m_handlerNoArgs?.Invoke();
            m_handlerWithArgs?.Invoke(@event);
        }

        public void Add(Action handler)
        {
            if(handler == null) return;
            m_handlerNoArgs += handler;
        }

        public void Remove(Action handler)
        {
            if(handler == null) return;
            m_handlerNoArgs -= handler;
        }

        public void Add(Action<T> handler)
        {
            if(handler == null) return;
            m_handlerWithArgs += handler;
        }

        public void Remove(Action<T> handler)
        {
            if(handler == null) return;
            m_handlerWithArgs -= handler;
        }
    }
}