

using System;

namespace XDay
{
    public interface ITickTimer
    {
        static ITickTimer Create(bool queueMessage, int internalThreadTickInterval, Func<long> actionGetCurrentTime)
        {
            return new TickTimer(queueMessage, internalThreadTickInterval, actionGetCurrentTime);
        }

        void OnDestroy();
        int AddTask(int intervalInMs, Action action, int count = 0, Action actionOnCancel = null);
        bool CancelTask(int timerID);
        void Update();
        void ProcessCallbacks();
    }
}
