using System.Net.Sockets;

namespace XDay
{
    public abstract class IOCPPeer
    {
        public IOCPPeer()
        {
            m_Args = new SocketAsyncEventArgs();
            m_Args.Completed += OnIOCompleted;
        }

        public void Close()
        {
            m_Args.SetBuffer(null);
            m_Args.AcceptSocket = null;
            m_Args.RemoteEndPoint = null;

            OnClose();
        }

        protected abstract void OnClose();
        protected abstract void OnIOCompleted(object sender, SocketAsyncEventArgs e);

        protected SocketAsyncEventArgs m_Args;
    }
}
