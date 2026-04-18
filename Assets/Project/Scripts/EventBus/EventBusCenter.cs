using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Project.Scripts.EventBus
{
    public static class EventBusCenter
    {
        private static List<Type> s_eventTypes;
        private static List<Type> s_registeredEventTypes;
        
        public static void Initialize()
        {
            Debug.Log("[Event Bus Center] : Initialize");
            s_eventTypes = typeof(IEvent).GetAssemblies();
            s_registeredEventTypes = InitializeAllBuses();
        }
        
        private static List<Type> InitializeAllBuses()
        {
            List<Type> eventBindTypes = new();

            Type typeDef = typeof(EventBus<>);
            foreach (Type eventType in s_eventTypes)
            {
                Type busType = typeDef.MakeGenericType(eventType);
                eventBindTypes.Add(busType);
                Debug.Log("[Event Bus] : " + $"Initialized EventBus<{eventType.Name}>");
            }

            return eventBindTypes;
        }
        
        public static void DisposeAllBuses()
        {
            Debug.Log("[Event Bus] : " + "Clearing all buses...");
            if (s_registeredEventTypes == null) return;
            for (int i = 0; i < s_registeredEventTypes.Count; i++)
            {
                Type busType = s_registeredEventTypes[i];
                MethodInfo disposeMethod = busType.GetMethod("Dispose", BindingFlags.Static | BindingFlags.NonPublic);
                if (disposeMethod != null)
                {
                    disposeMethod.Invoke(null, null);
                }
                else
                {
                    Debug.LogError("[Event Bus] : " + busType + " has no <Dispose> method !");
                }
            }
            s_eventTypes = null;
            s_registeredEventTypes = null;
        }
        
        public static void DisposeWithCenter(this Type eventType)
        {
            Type busType = typeof(EventBus<>).MakeGenericType(eventType);
            MethodInfo disposeMethod = busType.GetMethod("Dispose", BindingFlags.Static | BindingFlags.NonPublic);
            if (disposeMethod != null)
            {
                disposeMethod.Invoke(null, null);
            }
            else
            {
                Debug.LogError("[Event Bus] : " + busType + " has no <Dispose> method !");
            }
            s_registeredEventTypes.Remove(busType);
        }
    }
}