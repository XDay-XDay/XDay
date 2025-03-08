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

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                //https://blog.csdn.net/sunzhen6251/article/details/124168805
                m_Udp.Client.IOControl((IOControlCode)(-1744830452), new byte[] { 0, 0, 0, 0 }, null);
            }

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
                        Log.Instance?.Info($"conv = 0, udp receive: {ToString(ret.Buffer)}, addr: {ret.RemoteEndPoint} to {m_Udp.Client.LocalEndPoint}", LogColor.Red);
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
                            Log.Instance?.Info($"创建Session: {conv}, addr: {ret.RemoteEndPoint} to {m_Udp.Client.LocalEndPoint}", LogColor.Red);
                        }
                        
                        session.Receive(ret.Buffer);
                    }
                }
                catch (Exception ex)
                {
                    Log.Instance?.Error($"KCPServer: {ex}");
                }
            }
        }

        private uint GenerateConvID()
        {
            Interlocked.Add(ref m_SessionID, 1);
            return (uint)m_SessionID;
        }

        private void SendUDPData(byte[] data, IPEndPoint remote)
        {
            m_Udp?.Send(data, data.Length, remote);
        }

        private void OnSessionClose(KCPSession session)
        {
            bool removed = m_Sessions.TryRemove(session.ID, out _);
            Debug.Assert(removed);
            Log.Instance?.Info($"删除Session: {session.ID}", LogColor.Green);
        }

        private string ToString(byte[] bytes)
        {
            string str = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                str += string.Format("\n      [{0}]:{1}", i, bytes[i]);
            }
            return str;
        }

        private UdpClient m_Udp;
        private CancellationTokenSource m_TokenSource;
        private readonly ConcurrentDictionary<uint, Session> m_Sessions = new();
        private Func<IMessageTranscoder> m_DecoderCreator;
        private bool m_QueueMessage;
        private int m_SessionID;
    }
}
