using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPlc.Scripts
{
    internal class ThreadUtility : CommonSingletonTemplate<ThreadUtility>
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
        public void DestroyThread(Thread thread, float time = 0)
        {
            Task.Delay((int)(time * 1000)).ContinueWith(_ =>
            {
                if (thread != null && thread.IsAlive)
                {
                    try
                    {
                        thread.Abort(); // 强制终止线程（不推荐）
                        Console.WriteLine($"已终止线程：{thread.ManagedThreadId}");
                    }
                    catch (ThreadAbortException e)
                    {
                        Console.WriteLine($"Thread aborted: {e.Message}");
                    }
                    finally
                    {
                        lock (lockObj)
                        {
                            Console.WriteLine($"已删除线程：{thread.ManagedThreadId}");
                            activeThreads.Remove(thread);
                        }
                    }
                }
                Console.WriteLine($"已停止线程：{thread?.ManagedThreadId}");
            });
        }

        /// <summary>
        /// 销毁所有活动线程
        /// </summary>
        public void DestroyAllThreads()
        {
            lock (lockObj)
            {
                //Debug.Log($" 在应用程序退出时，终止所有线程: {activeThreads.Count}");
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
                            Console.WriteLine($"Thread aborted: {e.Message}");
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
