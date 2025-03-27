using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ATF
{
    public class ThreadUtility : UnitySingletonTemplate<ThreadUtility>
    {
        private static readonly List<Thread> activeThreads = new List<Thread>();
        private static readonly object lockObj = new object();

        /// <summary>
        /// 创建并启动新线程
        /// </summary>
        /// <param name="action"> </param>
        /// <returns></returns>
        public Thread CreateThread(Action action)
        {
            Thread thread = new Thread(new ThreadStart(action));
            lock (lockObj)
            {
                activeThreads.Add(thread);
            }
            thread.Start();
            return thread;
        }

        /// <summary>
        /// 安全销毁线程
        /// </summary>
        /// <param name="thread"></param>
        public void DestroyThread(Thread thread)
        {
            if (thread != null && thread.IsAlive)
            {
                try
                {
                    thread.Abort(); // 强制终止线程
                }
                catch (ThreadAbortException e)
                {
                    Debug.Log($"Thread aborted: {e.Message}");
                }
                finally
                {
                    lock (lockObj)
                    {
                        activeThreads.Remove(thread);
                    }
                }
            }
        }

        /// <summary>
        /// 销毁所有活动线程
        /// </summary>
        public void DestroyAllThreads()
        {
            lock (lockObj)
            {
                foreach (Thread thread in activeThreads)
                {
                    if (thread.IsAlive)
                    {
                        try
                        {
                            thread.Abort();
                        }
                        catch (ThreadAbortException e)
                        {
                            Debug.Log($"Thread aborted: {e.Message}");
                        }
                    }
                }
                activeThreads.Clear();
            }
        }

        /// <summary>
        /// 在应用程序退出时，终止所有线程
        /// </summary>
        private void OnApplicationQuit()
        {
            DestroyAllThreads();
        }

        /// <summary>
        /// 在场景中的对象被销毁时，自动终止所有线程
        /// </summary>
        private void OnDestroy()
        {
            DestroyAllThreads();
        }
    }
}