using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPlc.Scripts
{
    internal class CommonSingletonTemplate<T> where T : class, new()
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
