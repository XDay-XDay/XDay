﻿/*
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

namespace XDay
{
    public interface ITaskSystem
    {
        static ITaskSystem Create(TaskSystemCreateInfo createInfo)
        {
            return new TaskSystem(createInfo);
        }

        /// <summary>
        /// enqueue task
        /// </summary>
        /// <param name="task"></param>
        void ScheduleTask(ITask task);

        /// <summary>
        /// get task object from pool
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetTask<T>() where T : class, ITask, new();

        /// <summary>
        /// release task to pool
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        void ReleaseTask<T>(T task) where T : class, ITask;

        /// <summary>
        /// running tasks in worker thread
        /// </summary>
        void Update();
    }

    public interface ITask
    {
        bool RequestCancel { get; }
        int Layer { get; }

        void OnDestroy();
        void OnGetFromPool();
        void OnReleaseToPool();
        //called in main thread
        void OnTaskCompleted(ITaskOutput output);
        void OnTaskCancelled();

        ITaskOutput Run();
    }

    public interface ITaskOutput
    {
    }

    public struct TaskLayerInfo
    {
        public TaskLayerInfo(int layer, int threadCount)
        {
            Layer = layer;
            ThreadCount = threadCount;
        }

        public int Layer { get; set; }
        public int ThreadCount { get; set; }
    }

    public class TaskSystemCreateInfo
    {
        public List<TaskLayerInfo> LayerInfo { get; set; } = new();
    }
}
