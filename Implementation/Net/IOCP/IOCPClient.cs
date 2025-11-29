/*
 * Copyright (c) 2024-2025 XDay
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
