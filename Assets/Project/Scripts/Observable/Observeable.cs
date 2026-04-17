using System;
using System.Collections;
using System.Linq;

namespace Project.Scripts.Observable
{
    public class Observable<T>
    {
        private T m_value;

        public T Value
        {
            get => m_value;
            set => Set(value);
        }

        public event Action<T> OnChanged;

        public Observable(T initialValue = default)
        {
            m_value = initialValue;
        }

        private void Set(T newValue)
        {
            if (m_value is IEnumerable oldEnum && newValue is IEnumerable newEnum)
            {
                if (oldEnum.Cast<object>().SequenceEqual(newEnum.Cast<object>())) return;
            }
            else
            {
                if (Equals(m_value, newValue)) return;
            }

            m_value = newValue;
            OnChanged?.Invoke(m_value);
        }

        public void Subscribe(Action<T> listener, bool invokeImmediately = false)
        {
            OnChanged += listener;
            if (invokeImmediately)
            {
                listener(m_value);
            }
        }

        public void Unsubscribe(Action<T> listener)
        {
            OnChanged -= listener;
        }
    }
}