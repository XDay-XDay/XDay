/*
 * Copyright (c) 2024-2026 XDay
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

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
