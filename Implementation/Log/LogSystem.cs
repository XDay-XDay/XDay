/*
 * Copyright (c) 2024-2026 XDay
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
    public enum LogColor
    {
        None, Red, Green, Blue, Cyan, Yellow, Magenta, White,
    }

    public class LogSystem
    {
        public LogSystem(LogSetting setting)
        {
            m_LogSetting = setting;

            setting.ActionOnInit?.Invoke();

            AddReceiver(new FileLogReceiver(), new Dictionary<string, object>()
            {
                {"OneFileMaxSize", 2*1024*1024L },
                {"Path", setting.LogFileDirectory },
                {"Name", setting.LogName },
                {"CleanOldLogs", setting.ClearOldLogs},
            });

            if (setting.EnableConsoleLog)
            {
                if (setting.ActionOnInit != null) 
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
            m_LogSetting.ActionOnDestroy?.Invoke();

            foreach (var receiver in m_Receivers)
            {
                receiver.OnDestroy();
            }
        }

        public void Info(ZStringInterpolatedStringHandler message,
            LogColor color = LogColor.None,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            var builder = SetMessage(message.Builder, LogType.Log, callerMemberName, callerFilePath, callerLineNumber);
            Notify(builder, LogType.Log, color, false);
        }

        public void Warning(ZStringInterpolatedStringHandler message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            var builder = SetMessage(message.Builder, LogType.Warning, callerMemberName, callerFilePath, callerLineNumber);
            Notify(builder, LogType.Warning, LogColor.Yellow, false);
        }

        public void Error(ZStringInterpolatedStringHandler message,
                    [CallerMemberName] string callerMemberName = "",
                    [CallerFilePath] string callerFilePath = "",
                    [CallerLineNumber] int callerLineNumber = 0)
        {
            var builder = SetMessage(message.Builder, LogType.Error, callerMemberName, callerFilePath, callerLineNumber);
            Notify(builder, LogType.Error, LogColor.Red, false);
        }

        public void Exception(Exception e,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            var temp = ZString.CreateStringBuilder();
            temp.Append(e.Message);
            var builder = SetMessage(temp, LogType.Exception, callerMemberName, callerFilePath, callerLineNumber, e.StackTrace);
            Notify(builder, LogType.Exception, LogColor.Red, false);
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
            Notify(builder, type, LogColor.None, true);
        }

        private void Notify(Utf16ValueStringBuilder message, LogType type, LogColor color, bool fromUnityDebug)
        {
            if (m_Receivers.Count == 1)
            {
                m_Receivers[0].OnLogReceived(message, type, color, fromUnityDebug);
            }
            else
            {
                foreach (var receiver in m_Receivers)
                {
                    //create message copy for every receiver
                    Utf16ValueStringBuilder temp = ZString.CreateStringBuilder();
                    temp.Append(message);
                    receiver.OnLogReceived(temp, type, color, fromUnityDebug);
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

            if (m_LogSetting.ShowThreadID)
            {
                builder.Append("[Thread ");
                builder.Append(Thread.CurrentThread.ManagedThreadId);
                builder.Append("]");
            }

            if (m_LogSetting.ShowTime)
            {
                builder.Append("[");
                builder.Append(DateTime.Now);
                builder.Append("]");
            }

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
            if (m_LogSetting.ShowFileName)
            {
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
        private readonly LogSetting m_LogSetting;
    }

    internal abstract class LogReceiver
    {
        public virtual void Init(Dictionary<string, object> setting) { }
        public virtual void OnDestroy() { }
        public abstract void OnLogReceived(Utf16ValueStringBuilder builder, LogType type, LogColor color, bool fromUnityDebug);
    }
}
