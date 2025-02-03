﻿
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace XDay
{
    internal class TickTimer : ITickTimer
    {
        public TickTimer(bool queueMessage, int internalThreadTickInterval, Func<long> actionGetCurrentTime)
        {
            if (queueMessage)
            {
                m_TaskInvokeQueue = new();
            }

            if (internalThreadTickInterval > 0)
            {
                var threadStart = new ThreadStart(() => {
                    while (true)
                    {
                        try
                        {
                            Update();
                            Thread.Sleep(internalThreadTickInterval);
                        }
                        catch (Exception ex)
                        {
                            {
                                Log.Instance?.Exception(ex);
                            }
                        }
                    };
                });
                
                m_TickThread = new Thread(threadStart);
                m_TickThread.Start();
            }

            m_ActionGetCurrentTime = actionGetCurrentTime;
        }

        public void OnDestroy()
        {
            if (m_TaskInvokeQueue != null &&
                m_TaskInvokeQueue.Count > 0)
            {
                Log.Instance?.Warning($"Has {m_TaskInvokeQueue.Count} pending task to invoke");
            }

            m_TickThread?.Abort();
        }

        public int AddTask(int interval, Action action, int count, Action actionOnCancel)
        {
            var task = new TimerTask()
            {
                ID = GetUniqueID(),
                TotalCount = count,
                InvokedCount = 0,
                Action = action,
                ActionOnCancel = actionOnCancel,
                StartTime = m_ActionGetCurrentTime(),
                Interval = interval,
            };
            task.NextInvokeTime = task.StartTime + task.Interval;

            if (!m_Tasks.TryAdd(task.ID, task))
            {
                Log.Instance?.Error($"TickTimer: add task failed!");
            }
            return task.ID;
        }

        public bool CancelTask(int timerID)
        {
            if (m_Tasks.TryRemove(timerID, out var task))
            {
                InvokeCallback(task.ActionOnCancel);
                return true;
            }
            Log.Instance?.Error($"TickTimer: cancel task {timerID} failed!");
            return false;
        }

        public void Update()
        {
            var now = m_ActionGetCurrentTime();
            foreach (var kv in m_Tasks)
            {
                var task = kv.Value;
                if (now >= task.NextInvokeTime)
                {
                    ++task.InvokedCount;
                    InvokeCallback(task.Action);
                    task.NextInvokeTime = task.StartTime + task.InvokedCount * task.Interval;

                    if (task.TotalCount > 0 && 
                        task.InvokedCount == task.TotalCount)
                    {
                        CancelTask(task.ID);
                    }
                }
            }
        }

        public void ProcessCallbacks()
        {
            if (m_TaskInvokeQueue != null)
            {
                while (m_TaskInvokeQueue.TryDequeue(out var invoke))
                {
                    invoke();
                }
            }
        }

        private void InvokeCallback(Action action)
        {
            if (action == null)
            {
                return;
            }

            if (m_TaskInvokeQueue != null)
            {
                m_TaskInvokeQueue.Enqueue(action);
            }
            else
            {
                action.Invoke();
            }
        }

        private int GetUniqueID()
        {
            lock (m_IDLock)
            {
                while (true)
                {
                    ++m_NextID;
                    if (m_NextID == int.MaxValue)
                    {
                        m_NextID = 0;
                    }
                    if (!m_Tasks.ContainsKey(m_NextID))
                    {
                        return m_NextID;
                    }
                }
            }
        }

        private Func<long> m_ActionGetCurrentTime;
        private ConcurrentDictionary<int, TimerTask> m_Tasks = new();
        private ConcurrentQueue<Action> m_TaskInvokeQueue;
        private int m_NextID = 0;
        private object m_IDLock = new();
        private Thread m_TickThread;
        private class TimerTask 
        {
            public int ID;
            public int InvokedCount;
            public int TotalCount;
            public long StartTime;
            public long Interval;
            public long NextInvokeTime;
            public Action Action;
            public Action ActionOnCancel;
        }
    }
}
