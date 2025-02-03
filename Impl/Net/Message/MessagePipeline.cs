
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
            var stream = m_StreamPool.Get();
            m_ProtocalStage.Encode(protocal, stream);
            stream.Position = 0;

            var compressedStream = m_StreamPool.Get();
            m_CompressionStage.Compress(stream, compressedStream);
            m_StreamPool.Release(stream);
            stream = compressedStream;

            var encryptedStream = m_StreamPool.Get();
            m_EncryptionStage.Encrypt(stream, encryptedStream);
            m_StreamPool.Release(stream);

            var buffer = new byte[encryptedStream.Length];
            Buffer.BlockCopy(encryptedStream.Buffer, 0, buffer, 0, (int)encryptedStream.Length);
            m_StreamPool.Release(encryptedStream);
            return buffer;
        }

        public object Decode(byte[] data, int offset, int count)
        {
            try
            {
                var encryptedStream = m_StreamPool.Get();
                encryptedStream.Write(data, offset, count);

                var compressedStream = m_StreamPool.Get();
                m_EncryptionStage.Decrypt(encryptedStream, compressedStream);
                m_StreamPool.Release(encryptedStream);

                var protocalStream = compressedStream;
                if (m_CompressionStage != null)
                {
                    protocalStream = m_StreamPool.Get();
                    m_CompressionStage.Decompress(compressedStream, protocalStream);
                    m_StreamPool.Release(compressedStream);
                }

                var protocal = m_ProtocalStage.Decode(protocalStream);
                m_StreamPool.Release(protocalStream);

                return protocal;
            }
            catch (Exception e)
            {
                Log.Instance?.Error($"Decode failed: {e}");
            }

            return null;
        }

        private readonly IEncryptionStage m_EncryptionStage;
        private readonly ICompressionStage m_CompressionStage;
        private readonly IProtocalStage m_ProtocalStage;
        private readonly IArrayPool m_ArrayPool;
        private readonly ByteStreamPool m_StreamPool;
    }
}


