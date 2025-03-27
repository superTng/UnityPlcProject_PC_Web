using S7.Net;
using System;
using static WebPlc.Scripts.MDataItem;

namespace WebPlc.Scripts
{
    public class MDataItem
    {
        public string Id = "";
        public string Name = "";
        public DataType DataType = DataType.Input;
        public VarType VarType = VarType.Byte;
        public string LogicalAddress = "";
        public bool SingleWrite = false;
        private object? m_value = null;
        private readonly object m_lock = new object();
        public event PlcValueChanged? OnMyValueChanged = null;

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
        public void SpecialTrigger()
        {
            OnMyValueChanged?.Invoke(m_value);
        }
    }
    internal class PlcVariable
    {
        private Plc m_plc = null;
        private string m_plcIp = "192.168.0.1";
        private CpuType m_plcType = CpuType.S71200;
        private short m_plcJiJiaNumber = 0;
        private short m_plcJiCaoNumber = 1;
        private int m_plcUpdateSpeed = 100;
        private int m_plcWriteSpeed = 1000;
        private bool m_plcUpdateState = true;
        public readonly Dictionary<string, MDataItem> m_plcDic = new Dictionary<string, MDataItem>();
        public readonly List<string> m_plcInputList = new List<string>();
        public readonly List<string> m_plcOutputList = new List<string>();

        public PlcVariable(Dictionary<string, MDataItem> plcDic, int plcUpdateSpeed = 100)
        {
            m_plcDic.Clear();
            m_plcInputList.Clear();
            m_plcOutputList.Clear();

            m_plcUpdateSpeed = plcUpdateSpeed;

            m_plcDic = plcDic;
            InitDataItemList();
        }



        public async Task StartConnect(string ip, Action succeedAction = null, Action errorAction = null)
        {
            try
            {
                Console.WriteLine("PLC准备连接IP:   " + ip);
                if (!string.IsNullOrEmpty(ip))
                {
                    m_plcIp = ip;
                    Console.WriteLine($"当前PLC尝试开始连接PLC:  {m_plcType},    {m_plcIp},     {m_plcJiJiaNumber},    {m_plcJiCaoNumber}");
                    m_plc = new Plc(m_plcType, m_plcIp, m_plcJiJiaNumber, m_plcJiCaoNumber);
                }
                else
                {
                    Console.WriteLine($"当前输入IP为空，尝试开始连接默认IP，PLC:  {m_plcType},    {m_plcIp},     {m_plcJiJiaNumber},    {m_plcJiCaoNumber}");
                    m_plc = new Plc(m_plcType, m_plcIp, m_plcJiJiaNumber, m_plcJiCaoNumber);
                }

                await m_plc.OpenAsync().ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Task.Delay(100).ContinueWith(_ =>
                        {
                            errorAction?.Invoke();
                        });
                        Console.WriteLine("连接PLC失败");
                    }
                    else if (t.IsCompleted)
                    {
                        Task.Delay(100).ContinueWith(_ =>
                        {
                            succeedAction?.Invoke();
                        });
                        SetPlcUpdateState(true);
                        ThreadUtility.Ins.CreateThread(() =>
                        {
                            while (true)
                            {
                                UpdateReadeValue();
                                Thread.Sleep(m_plcUpdateSpeed);
                            }
                        });

                        Console.WriteLine("连接PLC成功");
                    }
                    Console.WriteLine("连接回调：  " + t.Exception);
                });
            }
            catch (System.Exception e)
            {
                errorAction?.Invoke();
                Console.WriteLine("PLC连接报错:" + e);
            }
        }

        /// <summary>
        /// 开始取消连接
        /// </summary>
        /// <param name="succeedAction"></param>
        /// <param name="errorAction"></param>
        public void StartDisConnect(Action succeedAction = null, Action errorAction = null)
        {
            try
            {
                if (m_plc != null && m_plc.IsConnected)
                {
                    m_plc.Close();
                    succeedAction?.Invoke();
                }
                else
                {
                    errorAction?.Invoke();
                }
            }
            catch (Exception e)
            {
                errorAction?.Invoke();
                throw e;
            }
        }

        /// <summary>
        /// 初始化DataItem列表
        /// </summary>
        private void InitDataItemList()
        {
            foreach (var item in m_plcDic.Values)
            {
                if (item.DataType == DataType.Input)
                {
                    m_plcInputList.Add(item.Id + "_" + item.Name);
                }
                else
                {
                    m_plcOutputList.Add(item.Id + "_" + item.Name);
                }
            }
        }

        /// <summary>
        /// 更新读取值
        /// </summary>
        private async void UpdateReadeValue()
        {
            if (m_plcUpdateState && m_plc != null && m_plc.IsConnected)
            {
                foreach (var plcName in m_plcInputList)
                {
                    if (m_plcDic.ContainsKey(plcName))
                    {
                        m_plcDic[plcName].Value = await m_plc.ReadAsync(m_plcDic[plcName].LogicalAddress);
                    }
                }
            }
        }

        /// <summary>
        /// 得到值
        /// </summary>
        /// <param name="valName"></param>
        /// <returns></returns>
        public object GetValue(string valName)
        {
            if (m_plc != null && m_plc.IsConnected && m_plcDic.ContainsKey(valName))
            {
                return m_plcDic[valName].Value;
            }
            return null;
        }

        /// <summary>
        /// 写入值
        /// </summary>
        /// <param name="valName"></param>
        /// <param name="val"></param>
        public void WriteValue(string valName, object val, bool singleWrite = false)
        {
            if (m_plcDic.ContainsKey(valName))
            {
                m_plcDic[valName].Value = val;
                m_plcDic[valName].SingleWrite = singleWrite;
                if (m_plc != null && m_plc.IsConnected)
                {
                    string address = m_plcDic[valName].LogicalAddress;

                    switch (m_plcDic[valName].VarType)
                    {
                        case VarType.Bit:
                            break;
                        case VarType.Byte:
                            //Console.WriteLine($"开始{address}  写  {valName}  值: {(bool)val}");
                            m_plc.WriteAsync(address, Convert.ToBoolean(val));
                            break;
                        case VarType.Word:
                            break;
                        case VarType.DWord:
                            break;
                        case VarType.Int:
                            m_plc.WriteAsync(address, Convert.ToInt16(val));
                            break;
                        case VarType.DInt:
                            break;
                        case VarType.Real:
                            //m_plc.WriteAsync(address, Conversion.ConvertToFloat((uint)val));
                            try
                            {
                                m_plc.WriteAsync(address, Convert.ToSingle(val));
                            }
                            catch (Exception)
                            {
                                Console.WriteLine($"报错：m_plc.WriteAsync(address, (float)val)：{val}");
                            }
                           
                            break;
                        case VarType.LReal:
                            break;
                        case VarType.String:
                            break;
                        case VarType.S7String:
                            break;
                        case VarType.S7WString:
                            break;
                        case VarType.Timer:
                            break;
                        case VarType.Counter:
                            break;
                        case VarType.DateTime:
                            break;
                        case VarType.DateTimeLong:
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 设置Plc更新
        /// </summary>
        /// <param name="state"></param>
        private void SetPlcUpdateState(bool state)
        {
            m_plcUpdateState = state;
        }

        /// <summary>
        /// 开始监听数据变化
        /// </summary>
        /// <param name="valName"></param>
        /// <param name="action"></param>
        public void AddListenerPLCValueChange(string valName, PlcValueChanged action)
        {
            if (m_plcDic.ContainsKey(valName))
            {
                m_plcDic[valName].OnMyValueChanged += action;
                //Console.WriteLine($"当前监听变量：{valName}");
            }
        }

        /// <summary>
        /// 结束监听数据变化
        /// </summary>
        /// <param name="valName"></param>
        /// <param name="action"></param>
        public void RemoveListenerPLCValueChange(string valName, PlcValueChanged action)
        {
            if (m_plcDic.ContainsKey(valName))
            {
                m_plcDic[valName].OnMyValueChanged -= action;
            }
        }

        /// <summary>
        /// 特殊触发只读数据
        /// </summary>
        public void TriggerAllReadValue()
        {
            if (m_plc != null && m_plc.IsConnected)
            {
                foreach (var plcName in m_plcInputList)
                {
                    m_plcDic[plcName].SpecialTrigger();
                }
            }
        }

        /// <summary>
        /// 开始更新输出数据
        /// </summary>
        public void StartUpdateWritePlc()
        {
            ThreadUtility.Ins.CreateThread(() => {
                while (true)
                {
                    if (m_plc != null && m_plc.IsConnected)
                    {
                        foreach (var plcName in m_plcOutputList)
                        {
                            if (m_plcDic[plcName].Value != null && !m_plcDic[plcName].SingleWrite)
                            {
                                WriteValue(plcName, m_plcDic[plcName].Value, m_plcDic[plcName].SingleWrite);
                                //Console.WriteLine($"持续写出值：{plcName}   {m_plcDic[plcName].Value}");
                            }
                        }
                    }
                    Thread.Sleep(m_plcWriteSpeed);
                }
            });

        }

        public bool IsConnected()
        {
            if (m_plc != null)
            {
                return m_plc.IsConnected;
            }
            return false;
        }

        public async Task<object?> ReadPlcDataAsync(string valName)
        {
            if (!m_plcDic.ContainsKey(valName))
            {
                return null;
            }

            return await m_plc.ReadAsync(m_plcDic[valName].LogicalAddress);
        }
    }
}
