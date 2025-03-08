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
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace XDay
{
    public class KCPClient<Session> where Session : KCPSession, new()
    {
        public IPEndPoint LocalAddress => m_Udp.Client.LocalEndPoint as IPEndPoint;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="decoderCreator"></param>
        /// <param name="queueMessage">if message is queued, you have to call update to handle received messages</param>
        public void Start(string ip, int port, Func<IMessageTranscoder> decoderCreator, bool queueMessage)
        {
            Log.Instance?.Info($"Start KCPClient");

            m_DecoderCreator = decoderCreator;
            m_QueueMessage = queueMessage;
            m_Remote = new IPEndPoint(IPAddress.Parse(ip), port);
            m_Udp = new UdpClient();
            //bind
            m_Udp.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
            Log.Instance?.Info($"Local Endpoint: {m_Udp.Client.LocalEndPoint}", LogColor.Yellow);
            //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            //{
            //    m_Udp.Client.IOControl((IOControlCode)(-1744830452), new byte[] { 0, 0, 0, 0 }, null);
            //}
            m_TokenSource = new CancellationTokenSource();
            Task.Run(ReceiveLoop, m_TokenSource.Token);
        }

        public void Connect(int checkIntervalInMs)
        {
            Log.Instance?.Info($"Try Connect");

            if (m_Udp != null)
            {
                m_Udp.Send(new byte[4], 4, m_Remote);
            }
            Task.Run(async () => {
                var totalConnectionCheckTime = 0;
                while (m_Session == null)
                {
                    await Task.Delay(checkIntervalInMs);
                    totalConnectionCheckTime += checkIntervalInMs;

                    if (totalConnectionCheckTime >= m_MaxCheckTime)
                    {
                        break;
                    }
                }

                if (m_Session == null)
                {
                    Connect(checkIntervalInMs);
                }
            });
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

        public void ProcessMessage(object msg)
        {
            m_Session?.ProcessMessage(msg);
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
                        if (conv == 0 && ret.Buffer.Length > 4)
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
            m_Udp?.Send(data, data.Length, remote);
        }

        private void OnSessionClose(KCPSession session)
        {
            Debug.Assert(session == m_Session);
            m_Session = null;
            m_TokenSource.Cancel();
            m_Udp?.Close();
            m_Udp = null;
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
        private Session m_Session;
        private IPEndPoint m_Remote;
        private Func<IMessageTranscoder> m_DecoderCreator;
        private bool m_QueueMessage;
        private const int m_MaxCheckTime = 5000;
    }
}
