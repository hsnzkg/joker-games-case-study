using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.EventBus
{
    public static class EventBus<T> where T : IEvent
    {
        private static readonly List<EventBind<T>> s_bindings = new();
        private static readonly HashSet<EventBind<T>> s_pendingAdds = new();
        private static readonly HashSet<EventBind<T>> s_pendingRemovals = new();

        private static PendingFlags s_pendingFlags;
        private static int s_raiseDepth;
        private static int s_count;

        public static void Register(EventBind<T> bind)
        {
            if (bind == null) return;

            if (s_raiseDepth > 0)
            {
                s_pendingAdds.Add(bind);
                s_pendingRemovals.Remove(bind);
                s_pendingFlags |= PendingFlags.HAS_ADDS;
                return;
            }

            RegisterImmediate(bind);
        }

        public static void Unregister(EventBind<T> bind)
        {
            if (bind == null) return;

            if (s_raiseDepth > 0)
            {
                s_pendingRemovals.Add(bind);
                s_pendingAdds.Remove(bind);
                s_pendingFlags |= PendingFlags.HAS_REMOVALS;
                return;
            }

            UnregisterImmediate(bind);
        }

        private static void RegisterImmediate(EventBind<T> bind)
        {
            if (s_bindings.Contains(bind))
            {
                Debug.LogWarning("[EventBus] Same bind registered twice.");
                return;
            }

            int index = s_bindings.FindIndex(b => b.Priority.CompareTo(bind.Priority) > 0);

            if (index >= 0)
            {
                s_bindings.Insert(index, bind);
            }
            else
            {
                s_bindings.Add(bind);
            }

            s_count = s_bindings.Count;
        }

        private static void UnregisterImmediate(EventBind<T> bind)
        {
            if (!s_bindings.Remove(bind))
            {
                return;
            }

            s_count = s_bindings.Count;
        }

        public static void Raise(T @event)
        {
            if (@event == null)
            {
                Debug.LogError($"[EventBus] Null event : {typeof(T).Name}");
                return;
            }

            s_raiseDepth++;
            int bindingCount = s_bindings.Count;
            for (int i = 0; i < bindingCount; i++)
            {
                EventBind<T> bind = s_bindings[i];
                try
                {
                    bind.Invoke(@event);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error invoking event handler -> Bind : {bind.GetType().Name}\n{e}");
                }
            }

            s_raiseDepth--;

            if (s_raiseDepth == 0)
            {
                FlushPending();
            }
        }

        private static void FlushPending()
        {
            if ((s_pendingFlags & PendingFlags.HAS_REMOVALS) != 0)
            {
                foreach (EventBind<T> bind in s_pendingRemovals)
                {
                    UnregisterImmediate(bind);
                }

                s_pendingRemovals.Clear();
                s_pendingFlags &= ~PendingFlags.HAS_REMOVALS;
            }

            if ((s_pendingFlags & PendingFlags.HAS_ADDS) == 0) return;
            {
                foreach (EventBind<T> bind in s_pendingAdds)
                {
                    RegisterImmediate(bind);
                }

                s_pendingAdds.Clear();
                s_pendingFlags &= ~PendingFlags.HAS_ADDS;
            }
        }

        private static void Dispose()
        {
            s_bindings.Clear();
            s_pendingAdds.Clear();
            s_pendingRemovals.Clear();
            s_count = 0;
            s_raiseDepth = 0;
            s_pendingFlags = PendingFlags.NONE;

            Debug.Log($"[EventBus] Cleared bindings for {typeof(T).Name}");
        }

        public static void DisposeWithCenter()
        {
            typeof(T).DisposeWithCenter();
        }

        [Flags]
        private enum PendingFlags : byte
        {
            NONE = 0,
            HAS_ADDS = 1 << 0,
            HAS_REMOVALS = 1 << 1
        }
    }
}
