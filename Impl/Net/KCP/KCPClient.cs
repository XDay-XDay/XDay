using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace XDay
{
    public class KCPClient<Session> where Session : KCPSession, new()
    {
        public void Start(string ip, int port, Func<IMessageTranscoder> decoderCreator, bool queueMessage)
        {
            Log.Instance?.Info($"Start KCPClient");

            m_DecoderCreator = decoderCreator;
            m_QueueMessage = queueMessage;
            m_Remote = new IPEndPoint(IPAddress.Parse(ip), port);
            m_Udp = new UdpClient();
            m_Udp.Send(new byte[4], 4, m_Remote);

            m_TokenSource = new CancellationTokenSource();
            Task.Run(ReceiveLoop, m_TokenSource.Token);
        }

        public void Close()
        {
            m_TokenSource.Cancel();
            m_Udp?.Close();
            m_Udp = null;
            m_Session?.Close();
            m_Session = null;
        }

        public void Send(object msg)
        {
            if (m_Session != null && 
                m_Session.IsConnected)
            {
                m_Session.Send(msg);
            }
            else
            {
                Log.Instance?.Error($"client can't send, not connected");
            }
        }

        public void Update()
        {
            m_Session?.UpdateMessage();
        }

        private async void ReceiveLoop()
        {
            while (true)
            {
                try
                {
                    if (m_TokenSource.IsCancellationRequested)
                    {
                        break;
                    }

                    var ret = await m_Udp.ReceiveAsync();
                    if (Equals(ret.RemoteEndPoint,m_Remote))
                    {
                        var conv = BitConverter.ToUInt32(ret.Buffer);
                        if (conv == 0)
                        {
                            if (m_Session == null)
                            {
                                conv = BitConverter.ToUInt32(ret.Buffer, 4);
                                Debug.Assert(conv != 0);
                                m_Session = new Session();
                                m_Session.Init(conv, SendUDPData, ret.RemoteEndPoint, OnSessionClose, m_DecoderCreator(), m_QueueMessage);
                            }
                            else
                            {
                                Log.Instance?.Warning($"Session already connected!");
                            }
                        }
                        else
                        {
                            m_Session?.Receive(ret.Buffer);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Instance?.Error($"{ex}");
                    Close();
                }
            }
        }

        private void SendUDPData(byte[] data, IPEndPoint remote)
        {
            m_Udp.Send(data, data.Length, remote);
        }

        private void OnSessionClose(KCPSession session)
        {
            Debug.Assert(session == m_Session);
            m_Session = null;
            m_TokenSource.Cancel();
            m_Udp?.Close();
            m_Udp = null;
        }

        private UdpClient m_Udp;
        private CancellationTokenSource m_TokenSource;
        private Session m_Session;
        private IPEndPoint m_Remote;
        private Func<IMessageTranscoder> m_DecoderCreator;
        private bool m_QueueMessage;
    }
}
