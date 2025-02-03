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



using Cysharp.Text;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
#if PLATFORM_UNITY
using UnityEngine;
#endif

namespace XDay
{
#if !PLATFORM_UNITY

#endif

    public class LogSystem
    {
        public LogSystem(bool enableConsoleLog, Action onInit, Action onDestroy, string logDir)
        {
            onInit?.Invoke();

            m_OnDestroy = onDestroy;

            AddReceiver(new FileLogReceiver(), new Dictionary<string, object>()
            {
                {"OneFileMaxSize", 2*1024*1024L },
                {"Path", logDir },
            });

            if (enableConsoleLog)
            {
                if (onInit != null) 
                {
                    AddReceiver(new UnityConsoleLogReceiver());
                }
                else 
                {
                    AddReceiver(new ConsoleLogReceiver());
                }
            }
        }

        public void OnDestroy()
        {
            m_OnDestroy?.Invoke();

            foreach (var receiver in m_Receivers)
            {
                receiver.OnDestroy();
            }
        }

        public void Info(ZStringInterpolatedStringHandler message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            var builder = SetMessage(message.Builder, LogType.Log, callerMemberName, callerFilePath, callerLineNumber);
            Notify(builder, LogType.Log, false);
        }

        public void Warning(ZStringInterpolatedStringHandler message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            var builder = SetMessage(message.Builder, LogType.Warning, callerMemberName, callerFilePath, callerLineNumber);
            Notify(builder, LogType.Warning, false);
        }

        public void Error(ZStringInterpolatedStringHandler message,
                    [CallerMemberName] string callerMemberName = "",
                    [CallerFilePath] string callerFilePath = "",
                    [CallerLineNumber] int callerLineNumber = 0)
        {
            var builder = SetMessage(message.Builder, LogType.Error, callerMemberName, callerFilePath, callerLineNumber);
            Notify(builder, LogType.Error, false);
        }

        public void Exception(Exception e,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            var temp = ZString.CreateStringBuilder();
            temp.Append(e.Message);
            var builder = SetMessage(temp, LogType.Exception, callerMemberName, callerFilePath, callerLineNumber, e.StackTrace);
            Notify(builder, LogType.Exception, false);
        }

        public void Assert(bool condition,
            ZStringInterpolatedStringHandler message = default,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (!condition)
            {
                Error(message, callerMemberName, callerFilePath, callerLineNumber);
            }
        }

        public void LogMessage(string message, string stackTrace, LogType type)
        {
            if (message.StartsWith(LogHelper.LOG_IGNORE_KEY))
            {
                return;
            }

            var builder = ZString.CreateStringBuilder();
            builder.Append(message);
            builder = SetMessage(builder, type, "", "", 0, stackTrace);
            Notify(builder, type, true);
        }

        private void Notify(Utf16ValueStringBuilder message, LogType type, bool fromUnityDebug)
        {
            if (m_Receivers.Count == 1)
            {
                m_Receivers[0].OnLogReceived(message, type, fromUnityDebug);
            }
            else
            {
                foreach (var receiver in m_Receivers)
                {
                    //create message copy for every receiver
                    Utf16ValueStringBuilder temp = ZString.CreateStringBuilder();
                    temp.Append(message);
                    receiver.OnLogReceived(temp, type, fromUnityDebug);
                }
                message.Dispose();
            }
        }

        private void AddReceiver(LogReceiver receiver, Dictionary<string, object> keyValues = null)
        {
            receiver.Init(keyValues);
            m_Receivers.Add(receiver);
        }

        private Utf16ValueStringBuilder SetMessage(
            Utf16ValueStringBuilder message, 
            LogType type, 
            string callerMemberName, 
            string callerFileName, 
            int callerLineNumber, 
            string stackTrace = null)
        {
            var builder = ZString.CreateStringBuilder();
            builder.Append(LogHelper.LOG_IGNORE_KEY);

            builder.Append("[Thread ");
            builder.Append(Thread.CurrentThread.ManagedThreadId);
            builder.Append("]");

            builder.Append("[");
            if (type == LogType.Log)
            {
                builder.Append("Trace");
            }
            else if (type == LogType.Warning)
            {
                builder.Append("Warning");
            }
            else if (type == LogType.Error)
            {
                builder.Append("Error");
            }
            else if (type == LogType.Exception)
            {
                builder.Append("Exception");
            }
            builder.Append("]");
            if (!string.IsNullOrEmpty(callerFileName))
            {
                builder.Append(callerFileName);
            }
            if (callerLineNumber > 0)
            {
                builder.Append("@Line.");
                builder.Append(callerLineNumber);
            }
            if (!string.IsNullOrEmpty(callerMemberName))
            {
                builder.Append(" {");
                builder.Append(callerMemberName);
                builder.Append("} ");
            }
            builder.Append(message);
            if (stackTrace != null)
            {
                if (stackTrace.Length > LogHelper.MAX_STACK_TRACE_SIZE)
                {
                    stackTrace = stackTrace[..LogHelper.MAX_STACK_TRACE_SIZE];
                }
                builder.AppendLine(stackTrace);
            }
            message.Dispose();
            return builder;
        }

        private readonly List<LogReceiver> m_Receivers = new List<LogReceiver>();
        private Action m_OnDestroy;
    }

    internal abstract class LogReceiver
    {
        public virtual void Init(Dictionary<string, object> setting) { }
        public virtual void OnDestroy() { }
        public abstract void OnLogReceived(Utf16ValueStringBuilder builder, LogType type, bool fromUnityDebug);
    }
}
