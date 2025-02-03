using System.Collections.Concurrent;

namespace XDay
{
    class IOCPSessionPool<Session> where Session : IOCPSession, new()
    {
        public IOCPSessionPool(int count)
        {
            m_Sessions = new ConcurrentStack<IOCPSession>();
            for (var i = 0; i < count; i++)
            {
                var session = new Session();
                session.ID = i + 1;
                m_Sessions.Push(session);
            }
        }

        public IOCPSession Get()
        {
            m_Sessions.TryPop(out var session);
            return session;
        }

        public void Release(IOCPSession session)
        {
            m_Sessions.Push(session);
        }

        private ConcurrentStack<IOCPSession> m_Sessions;
    }
}
