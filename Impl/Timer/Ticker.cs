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



namespace XDay
{
    public class Ticker
	{
        public float NormalizedTime => m_Duration == 0 ? 1.0f : m_Timer / m_Duration;

        public void Reset()
        {
            m_Timer = 0;
            m_Running = false;
        }

        public void Start(float duration)
        {
            m_Duration = duration;
            Restart();
        }

        public void Restart()
        {
            m_Timer = 0;
            m_Running = true;
        }

        public bool Step(float dt)
        {
            if (m_Running)
            {
                m_Timer += dt;
                if (m_Timer >= m_Duration)
                {
                    m_Running = false;
                    m_Timer = m_Duration;
                    return true;
                }
            }
            return false;
        }

		private float m_Timer = 0;
        private float m_Duration = 0;
        private bool m_Running = false;
    }
}


//XDay