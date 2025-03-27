using System;
using System.Collections.Generic;
using UnityEngine;

namespace ATF
{
    /// <summary>
    /// 可监听值类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObservableValue<T>
    {
        private T m_value;
        public event Action<T> OnValueChanged;

        public T Value
        {
            get => m_value;
            set
            {
                if (!m_value.Equals(value))
                {
                    m_value = value;
                    OnValueChanged?.Invoke(m_value);
                }
            }
        }

        public ObservableValue(T initialValue)
        {
            m_value = initialValue;
        }
    }

    public class ObservableValue<T1, T2>
    {
        private T1 m_value1;
        private T2 m_value2;

        public event Action<T1, T2> OnValueChanged;
        public T1 Value1 => m_value1;
        public T2 Value2 => m_value2;
        private readonly object _lock = new object();
        public ObservableValue(T1 initialValue1, T2 initialValue2)
        {
            m_value1 = initialValue1;
            m_value2 = initialValue2;
        }

        public void SetValue(T1 value1, T2 value2)
        {
            lock (_lock)
            {
                if (!EqualityComparer<T1>.Default.Equals(m_value1, value1) ||
              !EqualityComparer<T2>.Default.Equals(m_value2, value2))
                {
                    m_value1 = value1;
                    m_value2 = value2;
                    OnValueChanged?.Invoke(m_value1, m_value2);
                }
            }
        }
    }
    public class ObservableValue<T1, T2, T3>
    {
        private T1 m_value1;
        private T2 m_value2;
        private T3 m_value3;
        public event Action<T1, T2, T3> OnValueChanged;
        public T1 Value1 => m_value1;
        public T2 Value2 => m_value2;
        public T3 Value3 => m_value3;
        private readonly object _lock = new object();
        public ObservableValue(T1 initialValue1, T2 initialValue2, T3 initialValue3)
        {
            m_value1 = initialValue1;
            m_value2 = initialValue2;
            m_value3 = initialValue3;
        }

        public void SetValue(T1 value1, T2 value2, T3 value3)
        {
            lock (_lock)
            {
                if (!EqualityComparer<T1>.Default.Equals(m_value1, value1) ||
                    !EqualityComparer<T2>.Default.Equals(m_value2, value2) ||
                    !EqualityComparer<T3>.Default.Equals(m_value3, value3))
                {
                    m_value1 = value1;
                    m_value2 = value2;
                    m_value3 = value3;
                    OnValueChanged?.Invoke(m_value1, m_value2, m_value3);
                }
            }
        }
    }
}