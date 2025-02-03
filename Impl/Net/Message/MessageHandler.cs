using System;
using System.Collections.Generic;

namespace XDay
{
    public class MessageHandler
    {
        public void AddHandler(Type type, Action<object> handler)
        {
            m_MessageHandlers.Add(type, handler);
        }

        public void HandleMessage(object msg)
        {
            m_MessageHandlers.TryGetValue(msg.GetType(), out var handler);
            handler?.Invoke(msg);
        }

        private readonly Dictionary<Type, Action<object>> m_MessageHandlers = new Dictionary<Type, Action<object>>();
    }
}
