using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Event
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> s_payloadSubscribers = new();
        private static readonly Dictionary<Type, Delegate> s_parameterlessSubscribers = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            s_payloadSubscribers.Clear();
            s_parameterlessSubscribers.Clear();
        }

        public static void Subscribe<TEvent>(Action<TEvent> listener)
        {
            if (listener == null)
            {
                return;
            }

            Type eventType = typeof(TEvent);

            if (s_payloadSubscribers.TryGetValue(eventType, out Delegate existingDelegate))
            {
                s_payloadSubscribers[eventType] = Delegate.Combine(existingDelegate, listener);
                return;
            }

            s_payloadSubscribers.Add(eventType, listener);
        }

        public static void Subscribe<TEvent>(Action listener)
        {
            if (listener == null)
            {
                return;
            }

            Type eventType = typeof(TEvent);

            if (s_parameterlessSubscribers.TryGetValue(eventType, out Delegate existingDelegate))
            {
                s_parameterlessSubscribers[eventType] = Delegate.Combine(existingDelegate, listener);
                return;
            }

            s_parameterlessSubscribers.Add(eventType, listener);
        }

        public static void Unsubscribe<TEvent>(Action<TEvent> listener)
        {
            if (listener == null)
            {
                return;
            }

            Type eventType = typeof(TEvent);

            if (!s_payloadSubscribers.TryGetValue(eventType, out Delegate existingDelegate))
            {
                return;
            }

            Delegate updatedDelegate = Delegate.Remove(existingDelegate, listener);

            if (updatedDelegate == null)
            {
                s_payloadSubscribers.Remove(eventType);
                return;
            }

            s_payloadSubscribers[eventType] = updatedDelegate;
        }

        public static void Unsubscribe<TEvent>(Action listener)
        {
            if (listener == null)
            {
                return;
            }

            Type eventType = typeof(TEvent);

            if (!s_parameterlessSubscribers.TryGetValue(eventType, out Delegate existingDelegate))
            {
                return;
            }

            Delegate updatedDelegate = Delegate.Remove(existingDelegate, listener);

            if (updatedDelegate == null)
            {
                s_parameterlessSubscribers.Remove(eventType);
                return;
            }

            s_parameterlessSubscribers[eventType] = updatedDelegate;
        }

        public static void Publish<TEvent>(TEvent eventData)
        {
            Type eventType = typeof(TEvent);

            if (s_payloadSubscribers.TryGetValue(eventType, out Delegate payloadDelegate) && payloadDelegate is Action<TEvent> payloadCallback)
            {
                payloadCallback.Invoke(eventData);
            }

            if (s_parameterlessSubscribers.TryGetValue(eventType, out Delegate parameterlessDelegate) && parameterlessDelegate is Action parameterlessCallback)
            {
                parameterlessCallback.Invoke();
            }
        }

        public static void Publish<TEvent>() where TEvent : struct
        {
            Publish(default(TEvent));
        }
    }
}
