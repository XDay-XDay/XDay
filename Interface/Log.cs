using System;

namespace XDay
{
    public static class Log
    {
        public static LogSystem Instance => m_LogSystem;

        public static void Init(bool enableConsoleLog, Action onInit, Action onDestroy, string logDir)
        {   
            m_LogSystem ??= new LogSystem(enableConsoleLog, onInit, onDestroy, logDir);
        }

        public static void Uninit()
        {
            m_LogSystem?.OnDestroy();
            m_LogSystem = null;
        }

        private static LogSystem m_LogSystem;
    }
}
