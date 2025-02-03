using System;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace XDay
{
    public abstract class IOCPSession
    {
        public int ID { internal set => m_ID = value; get => m_ID; }
        public IMessageTranscoder MessageTranscoder => m_MessageTranscoder;

        public IOCPSession()
        {
            m_SendArgs = new SocketAsyncEventArgs();
            m_SendArgs.Completed += OnIOCompleted;
            m_ReceiveArgs = new SocketAsyncEventArgs();
            m_ReceiveArgs.Completed += OnIOCompleted;
            m_ReceiveArgs.SetBuffer(new byte[2048], 0, 2048);
        }

        //queueMessage后需要在Update里触发Message处理
        public void Init(Socket socket, Action<IOCPSession> onClose, IMessageTranscoder transcoder, bool queueMessage)
        {
            m_Socket = socket;
            m_OnClose = onClose;
            m_MessageTranscoder = transcoder;

            if (queueMessage)
            {
                m_ReceiveQueue = new();
            }

            OnConnected();
            Receive();
        }

        public void Close()
        {
            OnClose();
            m_OnClose?.Invoke(this);
            m_SendQueue.Clear();
            m_Socket = null;
            m_IsSending = false;
        }

        public void Send(object msg)
        {
            if (m_IsSending)
            {
                m_SendQueue.Enqueue(msg);
                return;
            }

            var bytes = m_MessageTranscoder.Encode(msg);

            m_IsSending = true;
            m_SendArgs.SetBuffer(bytes);
            bool pending = m_Socket.SendAsync(m_SendArgs);
            if (!pending)
            {
                OnSendFinish();
            }
        }

        public void Receive()
        {
            bool pending = m_Socket.ReceiveAsync(m_ReceiveArgs);
            if (!pending)
            {
                OnReceiveFinish();
            }
        }

        public void Update()
        {
            if (m_ReceiveQueue != null) 
            {
                while (m_ReceiveQueue.TryDequeue(out var msg))
                {
                    HandleMessage(msg);
                }
            }
        }

        public void RegisterHandler(Type type, Action<object> handler)
        {
            m_MessageHandler.AddHandler(type, handler);
        }

        private void OnIOCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Send:
                    OnSendFinish();
                    break;
                case SocketAsyncOperation.Receive:
                    OnReceiveFinish();
                    break;
                default:
                    Log.Instance?.Error($"Invalid op {e.LastOperation}");
                    break;
            }
        }

        private void OnSendFinish()
        {
            if (m_SendArgs.SocketError == SocketError.Success)
            {
                m_IsSending = false;
                if (m_SendQueue.TryDequeue(out var bytes))
                {
                    Send(bytes);
                }
            }
            else
            {
                Log.Instance?.Error($"Send error: {m_SendArgs.SocketError}");
                Close();
            }
        }

        private void OnReceiveFinish()
        {
            if (m_ReceiveArgs.SocketError == SocketError.Success &&
                m_ReceiveArgs.BytesTransferred > 0)
            {
                Log.Instance?.Info($"Received {m_ReceiveArgs.BytesTransferred} bytes, {System.Text.Encoding.ASCII.GetString(m_ReceiveArgs.Buffer, 0, m_ReceiveArgs.BytesTransferred)}");

                m_MessageTranscoder.Input(m_ReceiveArgs.Buffer, m_ReceiveArgs.BytesTransferred);

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
                    }
                    else
                    {
                        break;
                    }
                }

                Receive();
            }
            else
            {
                Log.Instance?.Error($"Receive error: {m_ReceiveArgs.SocketError}");
                Close();
            }
        }

        private void HandleMessage(object msg)
        {
            m_MessageHandler.HandleMessage(msg);
        }

        protected abstract void OnConnected();
        protected abstract void OnClose();

        private Socket m_Socket;
        private SocketAsyncEventArgs m_SendArgs;
        private SocketAsyncEventArgs m_ReceiveArgs;
        private ConcurrentQueue<object> m_SendQueue = new ConcurrentQueue<object>();
        private ConcurrentQueue<object> m_ReceiveQueue;
        private bool m_IsSending = false;
        private Action<IOCPSession> m_OnClose;
        private int m_ID;
        private IMessageTranscoder m_MessageTranscoder;
        private MessageHandler m_MessageHandler = new();
    }
}
