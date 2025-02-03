﻿

using System;
using System.Collections.Generic;

namespace XDay
{
    internal class EventSystem : IEventSystem
    {
        public void Register<Event>(object key, Action<Event> action) where Event : struct
        {
            m_Handlers.TryGetValue(key, out var handlers);
            if (handlers == null)
            {
                handlers = new();
                m_Handlers.Add(key, handlers);
            }
            foreach (var h in handlers)
            {
                if (h.eventType == typeof(Event))
                {
                    Log.Instance?.Error($"EventSystem register {typeof(Event)} failed");
                    return;
                }
            }

            var handler = new Handler() { eventType = typeof(Event), Action = action };
            if (m_IsBroadcasting)
            {
                m_RegisteringHandlers.Add(new RegisterInfo() { Key = key, Handler = handler });
            }
            else
            {
                handlers.Add(handler);
            }
        }

        public void Unregister<Event>(object key, Action<Event> action) where Event : struct
        {
            m_Handlers.TryGetValue(key, out var handlers);
            if (handlers == null)
            {
                Log.Instance?.Error($"{key} unregister event failed, no handler");
                return;
            }

            if (m_IsBroadcasting)
            {
                m_UnregisteringHandlers.Add(new UnregisterInfo() { Key = key, Action = action });
            }
            else
            {
                for (var idx = handlers.Count - 1; idx >= 0; --idx)
                {
                    if (ReferenceEquals(handlers[idx].Action, action))
                    {
                        handlers.RemoveAt(idx);
                        return;
                    }
                }
            }

            Log.Instance?.Error($"{key} unregister event failed");
        }

        public void Unregister(object key)
        {
            if (m_IsBroadcasting)
            {
                for (var idx = m_UnregisteringHandlers.Count - 1; idx >= 0; --idx)
                {
                    if (m_UnregisteringHandlers[idx].Key == key)
                    {
                        m_UnregisteringHandlers.RemoveAt(idx);
                    }
                }
                m_UnregisteringHandlers.Add(new UnregisterInfo() { Key = key, Action = null });
            }
            else
            {
                m_Handlers.Remove(key);
            }
        }

        public void Broadcast<Event>(Event e, object receiver = null) where Event : struct
        {
            m_IsBroadcasting = true;
            if (receiver == null)
            {
                bool processed = false;
                foreach (var handlers in m_Handlers.Values)
                {
                    foreach (var handler in handlers)
                    {
                        var act = handler.Action as Action<Event>;
                        act?.Invoke(e);
                        processed = true;
                    }
                }
                if (!processed)
                {
                    Log.Instance?.Error($"No handler for event {typeof(Event)}");
                }
            }
            else
            {
                m_Handlers.TryGetValue(receiver, out var handlers);
                if (handlers != null)
                {
                    foreach (var handler in handlers)
                    {
                        var act = handler.Action as Action<Event>;
                        act?.Invoke(e);
                    }
                }
                else
                {
                    Log.Instance?.Error($"No handler for event {typeof(Event)}");
                }
            }
            m_IsBroadcasting = false;

            UpdatePendingOperations();
        }

        private void UpdatePendingOperations()
        {
            //unregister
            foreach (var info in m_UnregisteringHandlers)
            {
                m_Handlers.TryGetValue(info.Key, out var handlers);
                Log.Instance?.Assert(handlers != null);
                for (var idx = handlers.Count - 1; idx >= 0; --idx)
                {
                    if (info.Action == null)
                    {
                        Log.Instance?.Assert(handlers.Count == 1);
                        m_Handlers.Remove(info.Key);
                    }
                    else
                    {
                        if (ReferenceEquals(handlers[idx].Action, info.Action))
                        {
                            handlers.RemoveAt(idx);
                            break;
                        }
                    }
                }
            }
            m_UnregisteringHandlers.Clear();

            //register
            foreach (var info in m_RegisteringHandlers)
            {
                m_Handlers.TryGetValue(info.Key, out var handlers);
                if (handlers == null)
                {
                    handlers = new();
                    m_Handlers.Add(info.Key, handlers);
                }
                handlers.Add(info.Handler);
            }
            m_RegisteringHandlers.Clear();
        }

        private bool m_IsBroadcasting = false;
        private readonly Dictionary<object, List<Handler>> m_Handlers = new();
        private readonly List<RegisterInfo> m_RegisteringHandlers = new();
        private readonly List<UnregisterInfo> m_UnregisteringHandlers = new();

        private class Handler
        {
            public object Action;
            public Type eventType;
        }

        private class UnregisterInfo
        {
            public object Key;
            public object Action;
        }

        private class RegisterInfo
        {
            public object Key;
            public Handler Handler;
        }
    }
}
