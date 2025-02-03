

using System.Collections.Generic;


namespace XDay
{
    internal class StructListPool<T> : IStructListPool<T> where T : struct
    {
        public StructListPool(int capacity)
        {
            m_Cache = new List<List<T>>(capacity);
        }

        public void Release(List<T> list)
        {
            list.Clear();
            m_Cache.Add(list);
        }

        public List<T> Get()
        {
            if (m_Cache.Count > 0)
            {
                var list = m_Cache[^1];
                m_Cache.RemoveAt(m_Cache.Count - 1);
                return list;
            }

            return new List<T>();
        }

        private List<List<T>> m_Cache;
    }

    internal class StructArrayPool<T> : IStructArrayPool<T> where T : struct
    {
        public int Count => m_Cache.Count;

        public StructArrayPool(int capacity = 0)
        {
            m_Cache = new List<T[]>(capacity);
        }

        public void Release(T[] array)
        {
            m_Cache.Add(array);
        }

        public T[] Get(int length)
        {
            T[] array = null;

            var n = m_Cache.Count;
            if (n > 0)
            {
                array = m_Cache[n - 1];
                m_Cache.RemoveAt(n - 1);
            }

            array ??= new T[length];

            return array;
        }

        private List<T[]> m_Cache;
    }

}

//XDay