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
