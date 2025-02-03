using XDay;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace XDay
{
    public class KCPServer<Session> where Session : KCPSession, new()
    {
        public void Start(string ip, int port, Func<IMessageTranscoder> decoderCreator, bool queueMessage)
        {
            Log.Instance?.Info($"Start KCPServer");

            m_QueueMessage = queueMessage;

            var remote = new IPEndPoint(IPAddress.Parse(ip), port);
            m_Udp = new UdpClient(remote);

            m_DecoderCreator = decoderCreator;

            m_TokenSource = new CancellationTokenSource();
            Task.Run(ReceiveLoop, m_TokenSource.Token);
        }

        public void Close()
        {
            m_TokenSource.Cancel();
            m_Udp.Close();
            m_Udp = null;

            foreach (var kv in m_Sessions)
            {
                kv.Value.Close();
            }
            m_Sessions.Clear();
        }

        public void Broadcast(object msg)
        {
            foreach (var kv in m_Sessions)
            {
                kv.Value.Send(msg);
            }
        }

        public void Update()
        {
            foreach (var kv in m_Sessions)
            {
                kv.Value.UpdateMessage();
            }
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
                    var conv = BitConverter.ToUInt32(ret.Buffer);
                    if (conv == 0)
                    {
                        conv = GenerateConvID();
                        byte[] data = new byte[8];
                        var convData = BitConverter.GetBytes(conv);
                        Buffer.BlockCopy(convData, 0, data, 4, 4);
                        SendUDPData(data, ret.RemoteEndPoint);
                    }
                    else
                    {
                        m_Sessions.TryGetValue(conv, out var session);
                        if (session == null)
                        {
                            session = new Session();
                            session.Init(conv, SendUDPData, ret.RemoteEndPoint, OnSessionClose, m_DecoderCreator(), m_QueueMessage);
                            m_Sessions.TryAdd(conv, session);
                        }
                        
                        session.Receive(ret.Buffer);
                    }
                }
                catch (Exception ex)
                {
                    Log.Instance?.Error($"KCPServer: {ex}");
                    Close();
                }
            }
        }

        private uint GenerateConvID()
        {
            uint id = 1;
            while (m_Sessions.ContainsKey(id))
            {
                id++;
            }
            return id;
        }

        private void SendUDPData(byte[] data, IPEndPoint remote)
        {
            m_Udp.Send(data, data.Length, remote);
        }

        private void OnSessionClose(KCPSession session)
        {
            bool removed = m_Sessions.TryRemove(session.ID, out _);
            Debug.Assert(removed);
        }

        private UdpClient m_Udp;
        private CancellationTokenSource m_TokenSource;
        private readonly ConcurrentDictionary<uint, Session> m_Sessions = new();
        private Func<IMessageTranscoder> m_DecoderCreator;
        private bool m_QueueMessage;
    }
}
