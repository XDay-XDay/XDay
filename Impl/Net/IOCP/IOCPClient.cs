using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System;

namespace XDay
{
    public class IOCPClient<Session> : IOCPPeer where Session : IOCPSession, new()
    {
        protected override void OnClose()
        {
            m_ClientSession?.Close();
            m_ClientSession = null;
        }

        public void Start(string ip, int port, Func<IMessageTranscoder> decoderCreator, bool queueMessage)
        {
            Log.Instance?.Info($"StartAsClient, connect to {ip}:{port}");

            m_DecoderCreator = decoderCreator;
            m_QueueMessage = queueMessage;
            var endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
            m_Socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            m_Args.RemoteEndPoint = endpoint;
            bool pending = m_Socket.ConnectAsync(m_Args);
            if (!pending)
            {
                OnConnectFinish();
            }
        }

        public void Send(object msg)
        {
            if (m_ClientSession == null)
            {
                Log.Instance?.Error($"client session not created!");
            }
            else
            {
                m_ClientSession.Send(msg);
            }
        }

        public void Update()
        {
            m_ClientSession?.Update();
        }

        protected override void OnIOCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    OnConnectFinish();
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }

        /// <summary>
        /// 只是TCP握手成功，链接建立，服务器如果没有Accept,则该链接无法收发数据!
        /// </summary>
        private void OnConnectFinish()
        {
            if (m_Args.SocketError == SocketError.Success)
            {
                m_ClientSession = new Session();
                m_ClientSession.Init(m_Socket, OnSessionClose, m_DecoderCreator(), m_QueueMessage);
                Log.Instance?.Info($"OnConnectSuccess, new session {m_ClientSession.ID}");
            }
            else
            {
                Log.Instance?.Error($"OnConnectFailed");
                Close();
            }
        }

        private void OnSessionClose(IOCPSession session)
        {
            Log.Instance?.Info($"OnSessionClose {session.ID}");
            if (m_ClientSession == session)
            {
                m_ClientSession = null;
            }
        }

        private IOCPSession m_ClientSession;
        private Socket m_Socket;
        private Func<IMessageTranscoder> m_DecoderCreator;
        private bool m_QueueMessage;
    }
}
