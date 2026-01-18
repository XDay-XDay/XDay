/*
 * Copyright (c) 2024-2026 XDay
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

namespace XDay
{
    public partial class MessagePipeline : IMessagePipeline
    {
        public IEncryptionStage EncryptionStage => m_EncryptionStage;

        public MessagePipeline(
            IProtocalStage protocalStage,
            ICompressionStage compressionStage = null,
            IEncryptionStage encryptionStage = null)
        {
            Debug.Assert(protocalStage != null, "protocal stage is null");

            m_ArrayPool = IArrayPool.Create(1024);
            m_StreamPool = new ByteStreamPool(m_ArrayPool);
            m_ProtocalStage = protocalStage;

            compressionStage ??= new CompressionStageForward();
            m_CompressionStage = compressionStage;
            encryptionStage ??= new EncryptionStageForward();
            m_EncryptionStage = encryptionStage;
        }

        public void OnDestroy()
        {
            m_ProtocalStage?.OnDestroy();
        }

        public byte[] Encode(object protocal)
        {
            var stream = GetStream();
            m_ProtocalStage.Encode(protocal, stream);
            stream.Position = 0;

            var compressedStream = GetStream();
            m_CompressionStage.Compress(stream, compressedStream);
            ReleaseStream(stream);
            stream = compressedStream;

            var encryptedStream = GetStream();
            m_EncryptionStage.Encrypt(stream, encryptedStream);
            ReleaseStream(stream);

            var buffer = new byte[encryptedStream.Length];
            Buffer.BlockCopy(encryptedStream.Buffer, 0, buffer, 0, (int)encryptedStream.Length);
            ReleaseStream(encryptedStream);
            return buffer;
        }

        public object Decode(byte[] data, int offset, int count)
        {
            try
            {
                var encryptedStream = GetStream();
                encryptedStream.Write(data, offset, count);

                var compressedStream = GetStream();
                m_EncryptionStage.Decrypt(encryptedStream, compressedStream);
                ReleaseStream(encryptedStream);

                var protocalStream = compressedStream;
                if (m_CompressionStage != null)
                {
                    protocalStream = GetStream();
                    m_CompressionStage.Decompress(compressedStream, protocalStream);
                    ReleaseStream(compressedStream);
                }

                var protocal = m_ProtocalStage.Decode(protocalStream);
                ReleaseStream(protocalStream);

                return protocal;
            }
            catch (Exception e)
            {
                Log.Instance?.Error($"Decode failed: {e}");
            }

            return null;
        }

        private ByteStream GetStream()
        {
            return m_StreamPool.Get();
        }

        private void ReleaseStream(ByteStream stream)
        {
            m_StreamPool.Release(stream);
        }

        private readonly IEncryptionStage m_EncryptionStage;
        private readonly ICompressionStage m_CompressionStage;
        private readonly IProtocalStage m_ProtocalStage;
        private readonly IArrayPool m_ArrayPool;
        private readonly ByteStreamPool m_StreamPool;
    }
}


