﻿using System;
using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets.Kcp;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace XDay
{
    public abstract class KCPSession : IKcpCallback
    {
        public uint ID => m_ID;
        public bool IsConnected => m_Connected;
        public IMessageTranscoder MessageTranscoder => m_MessageTranscoder;

        public void Init(
            uint id, 
            Action<byte[],
            IPEndPoint> sender, 
            IPEndPoint remote, 
            Action<KCPSession> onClose, 
            IMessageTranscoder transcoder,
            bool queueMessage)
        {
            m_ID = id;
            m_Remote = remote;
            m_Sender = sender;
            m_Connected = true;
            m_OnClose = onClose;
            m_MessageTranscoder = transcoder;
            if (queueMessage)
            {
                m_ReceiveQueue = new();
            }

            m_Kcp = new SimpleSegManager.Kcp(id, this);
            m_Kcp.NoDelay(1, 10, 2, 1);
            m_Kcp.WndSize(64, 64);
            m_Kcp.SetMtu(512);

            m_TokenSource = new CancellationTokenSource();
            Task.Run(Update, m_TokenSource.Token);

            OnConnected();
        }

        public void Close()
        {
            OnClose();
            m_OnClose.Invoke(this);
            m_TokenSource.Cancel();
            m_Kcp?.Dispose();
            m_Kcp = null;
            m_Connected = false;
        }

        public async void Update()
        {
            while (true)
            {
                if (m_TokenSource.IsCancellationRequested)
                {
                    break;
                }

                m_Kcp.Update(DateTime.UtcNow);
                int len;
                while ((len = m_Kcp.PeekSize()) > 0)
                {
                    Debug.Assert(len <= m_Buffer.Length, "Overflow!");
                    if (m_Kcp.Recv(m_Buffer) >= 0)
                    {
                        Log.Instance?.Info($"Received: {len} bytes");

                        m_MessageTranscoder.Input(m_Buffer, Math.Min(m_Buffer.Length, len));
                        while (true)
                        {
                            object msg = m_MessageTranscoder.Decode();
                            if (msg != null)
                            {
                                if (m_ReceiveQueue != null)
                                {
                                    m_ReceiveQueue.Enqueue(msg);
                                }
                                else
                                {
                                    HandleMessage(msg);
                                }

                                OnMessageReceived();
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                await Task.Delay(10);
            }
        }

        //如果使用queueMessage,则需要手动调用
        public void UpdateMessage()
        {
            while (m_ReceiveQueue.TryDequeue(out var msg))
            {
                HandleMessage(msg);
            }
        }

        public void Send(object msg)
        {
            if (IsConnected)
            {
                var bytes = m_MessageTranscoder.Encode(msg);
                m_Kcp.Send(bytes);
            }
        }

        public void Receive(byte[] data)
        {
            if (IsConnected)
            {
                m_Kcp.Input(data);
            }
            else
            {
                Log.Instance?.Error($"session not connected, can't receive!");
            }
        }

        public void Output(IMemoryOwner<byte> buffer, int validLength)
        {
            m_Sender.Invoke(buffer.Memory.Slice(0, validLength).ToArray(), m_Remote);
        }

        public void RegisterMessageHandler(Type type, Action<object> handler)
        {
            m_MessageHandler.AddHandler(type, handler);
        }

        private void HandleMessage(object msg)
        {
            m_MessageHandler.HandleMessage(msg);
        }

        protected abstract void OnConnected();
        protected abstract void OnClose();
        protected virtual void OnMessageReceived() { }

        private uint m_ID;
        private Kcp<KcpSegment> m_Kcp;
        private byte[] m_Buffer = new byte[2048];
        private CancellationTokenSource m_TokenSource;
        private IPEndPoint m_Remote;
        private Action<byte[], IPEndPoint> m_Sender;
        private bool m_Connected = false;
        private Action<KCPSession> m_OnClose;
        private IMessageTranscoder m_MessageTranscoder;
        private MessageHandler m_MessageHandler = new();
        private ConcurrentQueue<object> m_ReceiveQueue;
    }
}
