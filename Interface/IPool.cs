

using System;
using System.Collections.Generic;

namespace XDay
{
    public interface IStructListPool<T> where T : struct
    {
        static IStructListPool<T> Create(int capacity)
        {
            return new StructListPool<T>(capacity);
        }

        List<T> Get();
        void Release(List<T> list);
    }

    public interface IStructArrayPool<T> where T : struct
    {
        public static IStructArrayPool<T> Create(int initialCapacity = 0)
        {
            return new StructArrayPool<T>(initialCapacity);
        }

        T[] Get(int arrayLength);
        void Release(T[] array);
    }

    public interface IObjectPool<T> where T : class
    {
        public static IObjectPool<T> Create(
            Func<T> createFunc = null,
            Action<T> actionOnDestroy = null,
            Action<T, bool> actionOnGet = null,
            Action<T> actionOnRelease = null,
            int capacity = 0)
        {
            return new ObjectPool<T>(createFunc, actionOnDestroy, actionOnGet, actionOnRelease, capacity);
        }

        void OnDestroy();

        T Get();

        void Release(T instance);

        void Clear();

        int CachedObjectCount { get; }
    }

    public interface IArrayPool
    {
        static IArrayPool Create(int defaultLength)
        {
            return new ArrayPool(defaultLength);
        }

        void Release(byte[] array);
        byte[] Get(int length);
    }

    public interface IConcurrentObjectPool<T> where T : class
    {
        static IConcurrentObjectPool<T> Create(
            Func<T> createFunc,
            int capacity = 10,
            Action<T> actionOnDestroy = null,
            Action<T, bool> actionOnGet = null,
            Action<T> actionOnRelease = null)
        {
            return new ConcurrentObjectPool<T>(createFunc, capacity, actionOnDestroy, actionOnGet, actionOnRelease);
        }

        void OnDestroy();
        void Clear();
        T Get();
        void Release(T obj);
    }

    public interface IConcurrentStructListPool<T> where T : struct
    {
        static IConcurrentStructListPool<T> Create(int capacity = 100)
        {
            return new ConcurrentStructListPool<T>(capacity);
        }

        List<T> Get();
        void Release(List<T> list);
    }
}
