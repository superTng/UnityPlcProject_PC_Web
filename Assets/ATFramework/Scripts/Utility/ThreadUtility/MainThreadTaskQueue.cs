using System;
using System.Collections.Generic;
using UnityEngine;

namespace ATF
{
    public class MainThreadTaskQueue : MonoBehaviour
    {
        private static readonly Queue<Action> tasks = new Queue<Action>();
        private static readonly object queueLock = new object();

        private void Start()
        {
            tasks.Clear();
        }
        public static void EnqueueTask(Action action)
        {
            lock (queueLock)
            {
                tasks.Enqueue(action);
            }
        }
        private void Update()
        {
            ExecuteTasks();
        }

        public static void ExecuteTasks()
        {
            lock (queueLock)
            {
                while (tasks.Count > 0)
                {
                    var task = tasks.Dequeue();
                    task?.Invoke();
                }
            }
        }
    }
}