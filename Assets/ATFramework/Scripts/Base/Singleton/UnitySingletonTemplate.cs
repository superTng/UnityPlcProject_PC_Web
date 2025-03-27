using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ATF
{
    public class UnitySingletonTemplate<T> : MonoBehaviour where T : Component
    {
        private static T instance;
        private static readonly object lockObj = new object();
        private static bool isShuttingDown = false;

        public static T Ins
        {
            get
            {
                if (isShuttingDown)
                {
                    Debug.LogWarning($"[Singleton] Instance of '{typeof(T)}' already destroyed. Returning null.");
                    return null;
                }

                lock (lockObj)
                {
                    if (instance == null)
                    {
                        T[] instances = FindObjectsOfType<T>();
                        if (instances.Length > 1)
                        {
                            Debug.LogError($"[Singleton] More than one instance of '{typeof(T)}' found! Returning the first instance found.");
                        }

                        instance = instances.Length > 0 ? instances[0] : null;

                        if (instance == null)
                        {
                            GameObject singletonObj = new GameObject($"{typeof(T).Name} (Singleton)");
                            instance = singletonObj.AddComponent<T>();
                        }
                    }

                    return instance;
                }
            }
        }

        protected virtual void Awake()
        {
            lock (lockObj)
            {
                if (instance == null)
                {
                    instance = this as T;
                }
                else if (instance != this)
                {
                    Debug.LogWarning($"[Singleton] Another instance of '{typeof(T)}' already exists! Destroying this instance.");
                    Destroy(gameObject);
                }
            }
        }

        //private void OnApplicationQuit()
        //{
        //    isShuttingDown = true;
        //}

        //private void OnDestroy()
        //{
        //    if (instance == this)
        //    {
        //        isShuttingDown = true;
        //    }
        //}
    }

    public class CommonSingletonTemplate<T> where T : class, new()
    {
        private static readonly object sysLock = new object();
        private static T instance;

        public static T Ins
        {
            get
            {
                if (instance == null)
                {
                    lock (sysLock)
                    {
                        if (instance == null)
                        {
                            instance = new T();
                        }
                    }
                }
                return instance;
            }
        }
    }
}
