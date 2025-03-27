using S7.Net;

namespace ATF
{
    public class MDataItem
    {
        public string Id;
        public string Name;
        public DataType DataType;
        public VarType VarType;
        public string LogicalAddress;
        public bool SingleWrite;
        private object m_value;
        private readonly object m_lock = new object();

        public MDataItem() { }

        public object Value
        {
            get
            {
                lock (m_lock)
                {
                    return m_value;
                }
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                lock (m_lock)
                {
                    if (!value.Equals(m_value))
                    {
                        m_value = value;
                        OnMyValueChanged?.Invoke(m_value);
                    }
                }
            }
        }

        public delegate void PlcValueChanged(object sender);
        public event PlcValueChanged OnMyValueChanged;
        public void SpecialTrigger()
        {
            OnMyValueChanged?.Invoke(m_value);
        }
    }
}