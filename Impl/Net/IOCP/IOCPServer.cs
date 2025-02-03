
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System;

namespace XDay
{
    public class IOCPServer<Session> : IOCPPeer where Session : IOCPSession, new()
    {
        public int SessionCount => m_SessionCount;

        protected override void OnClose()
        {
            lock (m_ServerSessions)
            {
                foreach (var session in m_ServerSessions)
                {
                    session.Close();
                }
                m_ServerSessions.Clear();
            }

            m_Semaphore.Dispose();
        }

        public void Start(string ip, int port, int maxSessionCount, Func<IMessageTranscoder> decoderCreator, bool queueMessage)
        {
            Log.Instance?.Info($"StartAsServer, listen to {ip}:{port}");

            m_DecoderCreator = decoderCreator;
            m_Semaphore = new Semaphore(maxSessionCount, maxSessionCount);
            m_SessionPool = new IOCPSessionPool<Session>(maxSessionCount);
            m_QueueMessage = queueMessage;

            var endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
            m_Socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            m_Socket.Bind(endpoint);
            m_Socket.Listen(10);
            Accept();
        }

        public void Broadcast(object msg)
        {
            lock (m_ServerSessions)
            {
                foreach (var session in m_ServerSessions)
                {
                    session.Send(msg);
                }
            }
        }

        public void Update()
        {
            lock (m_ServerSessions)
            {
                foreach (var session in m_ServerSessions)
                {
                    session.Update();
                }
            }
        }

        private void Accept()
        {
            m_Args.AcceptSocket = null;
            m_Semaphore.WaitOne();

            Log.Instance?.Info($"Start accept");
            bool pending = m_Socket.AcceptAsync(m_Args);
            if (!pending)
            {
                OnAcceptFinish();
            }
        }

        protected override void OnIOCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    OnAcceptFinish();
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }

        private void OnAcceptFinish()
        {
            var session = m_SessionPool.Get();
            var decoder = m_DecoderCreator();
            session.Init(m_Args.AcceptSocket, OnSessionClose, decoder, m_QueueMessage);
            Interlocked.Increment(ref m_SessionCount);

            Log.Instance?.Info($"New session {session.ID}");

            lock (m_ServerSessions)
            {
                m_ServerSessions.Add(session);
            }

            Accept();
        }

        private void OnSessionClose(IOCPSession session)
        {
            Log.Instance?.Info($"OnSessionClose {session.ID}");

            lock (m_ServerSessions)
            {
                m_ServerSessions.Remove(session);
            }
            m_SessionPool.Release(session);

            Interlocked.Decrement(ref m_SessionCount);

            m_Semaphore?.Release();
        }

        private List<IOCPSession> m_ServerSessions = new List<IOCPSession>();
        private Semaphore m_Semaphore;
        private IOCPSessionPool<Session> m_SessionPool;
        private Socket m_Socket;
        private int m_SessionCount;
        private Func<IMessageTranscoder> m_DecoderCreator;
        private bool m_QueueMessage;
    }
}
