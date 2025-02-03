
using System.Collections.Generic;
using System.Diagnostics;

namespace XDay
{
    internal class ArrayPool : IArrayPool
    {
        public ArrayPool(int defaultLength)
        {
            m_DefaultLength = defaultLength;
        }

        public void Release(byte[] array)
        {
            lock (m_Lock)
            {
                if (m_Pools.TryGetValue(array.Length, out var pool))
                {
                    pool.Release(array);
                }
                else
                {
                    Log.Instance?.Info($"query pool of size {array.Length} failed!");
                }
            }
        }

        public byte[] Get(int length = 0)
        {
            lock (m_Lock)
            {
                length = length > 0 ? length : m_DefaultLength;
                m_Pools.TryGetValue(length, out var pool);
                if (pool == null)
                {
                    pool = IStructArrayPool<byte>.Create(10);
                    m_Pools.Add(length, pool);
                }
                var array = pool.Get(length);
                Debug.Assert(array.Length >= length);
                return array;
            }
        }

        private readonly Dictionary<int, IStructArrayPool<byte>> m_Pools = new Dictionary<int, IStructArrayPool<byte>>();
        private readonly object m_Lock = new object();
        private readonly int m_DefaultLength;
    }
}
