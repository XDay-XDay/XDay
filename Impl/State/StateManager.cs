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
    internal class StateManager : IStateManager
    {
        public T CreateState<T>() where T : State, new()
        {
            if (GetState<T>() == null)
            {
                var state = new T();
                m_States.Add(state);
                return state;
            }
            Log.Instance?.Error($"{typeof(T).Name} is already created");
            return null;
        }

        public void PushState<T>() where T : State, new()
        {
            var state = GetState<T>();
            foreach (var activeState in m_ActivateStateStack)
            {
                if (activeState == state)
                {
                    Log.Instance?.Error($"{state} is active already");
                    return;
                }
            }

            if (m_ActivateStateStack.Count > 0)
            {
                m_ActivateStateStack[^1].OnPause();
            }
            m_ActivateStateStack.Add(state);
            state.OnEnter();
        }

        public void PopState(int n)
        {
            n = Math.Min(n, m_ActivateStateStack.Count);
            for (var i = 1; i <= n; ++i)
            {
                m_ActivateStateStack[^i].OnExit();
            }
            m_ActivateStateStack.RemoveRange(m_ActivateStateStack.Count - n, n);
            if (m_ActivateStateStack.Count > 0)
            {
                m_ActivateStateStack[^1].OnResume();
            }
        }

        public void ChangeState<T>() where T : State, new()
        {
            PopState(m_ActivateStateStack.Count);
            var state = GetState<T>();
            m_ActivateStateStack.Add(state);
            state.OnEnter();
        }

        public void Update(float dt)
        {
            for (var i = m_ActivateStateStack.Count - 1; i >= 0; --i)
            {
                m_ActivateStateStack[i].Update(dt);
            }
        }

        private T GetState<T>() where T : State, new()
        {
            foreach (var state in m_States)
            {
                if (state.GetType() == typeof(T))
                {
                    return state as T;
                }
            }
            return null;
        }

        private readonly List<State> m_States = new();
        private readonly List<State> m_ActivateStateStack = new();
    }
}
