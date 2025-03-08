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

using System.Collections.Generic;
using System;

namespace XDay
{
    internal class ObjectPool<T> : IObjectPool<T> where T : class
    {
        public int CachedObjectCount => m_CachedObjects.Count;

        public ObjectPool(
            Func<T> createFunc = null,
            Action<T> actionOnDestroy = null,
            Action<T, bool> actionOnGet = null,
            Action<T> actionOnRelease = null,
            int capacity = 0)
        {
            m_CreateFunc = createFunc ?? CreateFunc;
            m_ActionOnDestroy = actionOnDestroy ?? ActionOnDestroy;
            m_ActionOnGet = actionOnGet ?? ActionOnGet;
            m_ActionOnRelease = actionOnRelease ?? ActionOnRelease;

            m_CachedObjects = new List<T>(capacity);
        }

        public void OnDestroy()
        {
            Clear();
        }

        public T Get()
        {
            T instance = null;

            var n = m_CachedObjects.Count;
            if (n > 0)
            {
                instance = m_CachedObjects[n - 1];
                m_CachedObjects.RemoveAt(n - 1);
            }

            var newCreatedInstance = false;
            if (instance is null)
            {
                instance = m_CreateFunc();
                newCreatedInstance = true;
            }

            m_ActionOnGet(instance, newCreatedInstance);

            return instance;
        }

        public void Release(T obj)
        {
            m_ActionOnRelease(obj);
            m_CachedObjects.Add(obj);
        }

        public void Remove(T obj)
        {
            m_ActionOnDestroy(obj);
            m_CachedObjects.Remove(obj);
        }

        public void Clear()
        {
            foreach (var instance in m_CachedObjects)
            {
                m_ActionOnDestroy(instance);
            }
            m_CachedObjects.Clear();
        }

        private static T CreateFunc()
        {
            return Activator.CreateInstance<T>();
        }

        private void ActionOnDestroy(T instance)
        {
        }

        private void ActionOnRelease(T instance)
        {
        }

        private void ActionOnGet(T instance, bool newCreatedInstance)
        {
        }

        private readonly List<T> m_CachedObjects;
        private readonly Func<T> m_CreateFunc;
        private readonly Action<T> m_ActionOnDestroy;
        private readonly Action<T, bool> m_ActionOnGet;
        private readonly Action<T> m_ActionOnRelease;
    }
}
