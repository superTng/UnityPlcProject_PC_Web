using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using static ATF.MDataItem;
using Debug = UnityEngine.Debug;

namespace ATF
{
    /// <summary>
    /// 3个不同的plc程序--示例 这个地方可以后期更改
    /// </summary>
    public enum EPlcName
    {
        EPN_FenJian,
        EPN_JiXieShou,
        EPN_CangKu,
    }

    /// <summary>
    /// web端通讯的消息类型
    /// </summary>
    public enum DESType
    {
        DEST_PlcConnect,                    //Plc 连接
        DEST_DisPlcConnect,                 //Plc 取消连接
        DEST_AddListenerPLCValueChange,     //监听Plc数据变化
        DEST_RemoveListenerPLCValueChange,  //移除Plc数据监听
        DEST_WriteValue,                    //写入数值
        DEST_IsConnected,                   //是否连接
        DEST_ReadValue,                     //读取数值
    }

    /// <summary>
    /// Plc管理单例，所有Plc相关的操作 都从这调用
    /// </summary>
    public class PlcManager : UnitySingletonTemplate<PlcManager>
    {
        private int m_plcUpdateSpeed = 100;//Plc数据更新的速率： 100ms
        private Dictionary<EPlcName, PlcVariable> m_plcDic = new Dictionary<EPlcName, PlcVariable>();
        private Process m_webPlcProcess;

        //[DllImport("__Internal")]
        //private static extern void OpenExe(string path,string args);

        private async void Start()
        {
            m_plcDic.Add(EPlcName.EPN_FenJian, new PlcVariable(await ReadPLCData(PlcConstant.PlcConfigDataPath_FenJian), m_plcUpdateSpeed));
            m_plcDic.Add(EPlcName.EPN_JiXieShou, new PlcVariable(await ReadPLCData(PlcConstant.PlcConfigDataPath_JiXieShou), m_plcUpdateSpeed));
            m_plcDic.Add(EPlcName.EPN_CangKu, new PlcVariable(await ReadPLCData(PlcConstant.PlcConfigDataPath_CangKu), m_plcUpdateSpeed));

#if UNITY_WEBGL

#if UNITY_EDITOR
            m_webPlcProcess = new Process();
            m_webPlcProcess.StartInfo = new ProcessStartInfo(Application.streamingAssetsPath + PlcConstant.WebPlcServerPath, Application.streamingAssetsPath);
            m_webPlcProcess.Start();
#else
            //Debug.Log("开始调用js代码启动外部服务器...");
            //string streamingAssetsPath = Application.streamingAssetsPath.TrimEnd('/');
            //Debug.Log("streamingAssetsPath:  " + streamingAssetsPath);

            //string webPlcPath = streamingAssetsPath + "/" + PlcConstant.WebPlcServerPath.TrimStart('/');
            //Debug.Log("webPlcPath:  " + webPlcPath);
            //string args = Uri.EscapeDataString(Application.streamingAssetsPath);
            //string finalUrl = webPlcPath + "?args=" + args;
            //Debug.Log("finalUrl:  " +finalUrl);

            //OpenExe(webPlcPath, streamingAssetsPath);
#endif
            ThreadUtility.Ins.CreateThread(async () =>
            {
                while (true)
                {
                    MainThreadTaskQueue.EnqueueTask(async () => 
                    {
                        await StartUpdateReadPlcValeAsync();
                    });
                    await Task.Delay(m_plcUpdateSpeed);
                }
            });

#endif
        }

        private void OnDestroy()
        {
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                try
                {
                    if (m_webPlcProcess != null && !process.HasExited && process.ProcessName == m_webPlcProcess.ProcessName)
                    {
                        process.Kill();
                    }
                }
                catch (InvalidOperationException ex)
                {
                    UnityEngine.Debug.Log(ex);
                }
            }
        }


        #region PLC函数

        /// <summary>
        /// 开始连接PLC
        /// </summary>
        /// <param name="plcName"></param>
        /// <param name="plcIp"></param>
        /// <param name="succeedAction"></param>
        /// <param name="errorAction"></param>
        public void StartConnectPlc(EPlcName plcName, string plcIp = "", Action succeedAction = null, Action errorAction = null)
        {
            if (!m_plcDic.ContainsKey(plcName))
            {
                errorAction?.Invoke();
                return;
            }
#if UNITY_WEBGL
            MSG_PlcConnect requestData = new MSG_PlcConnect
            {
                plcName = plcName,
                plcIp = plcIp,
                message = $"S2C->  PLC{plcName} --- {plcIp} 正在连接.."
            };

            string jsonData = JsonConvert.SerializeObject(requestData);
            StartCoroutine(PlcWebMessage.Ins.C2S_StartSendMessage(DESType.DEST_PlcConnect, PlcConstant.PlcConnectServerPath, jsonData, succeedAction, errorAction));
#else
            m_plcDic[plcName].StartConnect(plcIp, succeedAction, errorAction);
            m_plcDic[plcName].StartUpdateWritePlc();
#endif
        }

        /// <summary>
        /// 开始更新读取的Plc数值
        /// </summary>
        /// <returns></returns>
        private async Task StartUpdateReadPlcValeAsync()
        {
            foreach (var plc in m_plcDic)
            {
                if (plc.Value != null && await IsConnected(plc.Key))
                {
                    foreach (var valName in plc.Value.m_plcOutputList)
                    {
                        if (plc.Value.m_plcDic[valName].Value != null && !plc.Value.m_plcDic[valName].SingleWrite)
                        {
                            WriteValue(plc.Key, valName, plc.Value.m_plcDic[valName].Value);
                        }
                    }
                    Dictionary<string, object> newDic = await StartReadValue(plc.Key);
                    //foreach (var val in newDic)
                    //{
                    //    Debug.Log($"{plc.Key}接受的信息:{val.Key},{val.Value}");
                    //}
                    foreach (var valName in plc.Value.m_plcInputList)
                    {
                        if (plc.Value.m_plcDic.ContainsKey(valName))
                        {
                            plc.Value.m_plcDic[valName].Value = newDic[valName];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 开始取消连接PLC
        /// </summary>
        /// <param name="plcName"></param>
        /// <param name="succeedAction"></param>
        /// <param name="errorAction"></param>
        public void StartDisConnectPlc(EPlcName plcName, Action succeedAction = null, Action errorAction = null)
        {
            if (!m_plcDic.ContainsKey(plcName))
            {
                errorAction?.Invoke();
                return;
            }

#if UNITY_WEBGL

            MSG_DisPlcConnect requestData = new MSG_DisPlcConnect
            {
                message = $"C2S->  PLC{plcName} --- 正在取消连接.."
            };
            string jsonData = JsonConvert.SerializeObject(requestData);
            StartCoroutine(PlcWebMessage.Ins.C2S_StartSendMessage(DESType.DEST_DisPlcConnect, PlcConstant.PlcConnectServerPath, jsonData, succeedAction, errorAction));
#else
            m_plcDic[plcName].StartDisConnect(succeedAction, errorAction);
#endif
        }

        /// <summary>
        /// 开始监听PLC值改变
        /// </summary>
        /// <param name="plcName"></param>
        /// <param name="valName"></param>
        /// <param name="action"></param>
        public void AddListenerPLCValueChange(EPlcName plcName, string valName, PlcValueChanged action)
        {
            if (!m_plcDic.ContainsKey(plcName))
            {
                return;
            }
            m_plcDic[plcName].AddListenerPLCValueChange(valName, action);
        }

        /// <summary>
        /// 移除PLC值改变
        /// </summary>
        /// <param name="plcName"></param>
        /// <param name="valName"></param>
        /// <param name="action"></param>
        public void RemoveListenerPLCValueChange(EPlcName plcName, string valName, PlcValueChanged action)
        {
            if (!m_plcDic.ContainsKey(plcName))
            {
                return;
            }
            m_plcDic[plcName].RemoveListenerPLCValueChange(valName, action);
        }

        /// <summary>
        /// 写入值
        /// </summary>
        /// <param name="valName"></param>
        /// <param name="val"></param>
        public void WriteValue(EPlcName plcName, string valName, object val, bool singleWrite = false)
        {
            if (!m_plcDic.ContainsKey(plcName))
            {
                return;
            }

#if UNITY_WEBGL
            MSG_WriteValue requestData = new MSG_WriteValue
            {
                plcName = plcName,
                valName = valName,
                val = val,
                singleWrite = singleWrite,
                message = $"C2S->  PLC:{plcName} ---Val:{valName}"
            };
            string jsonData = JsonConvert.SerializeObject(requestData);
            StartCoroutine(PlcWebMessage.Ins.C2S_StartSendMessage(DESType.DEST_WriteValue, PlcConstant.PlcConnectServerPath, jsonData));
#else
            m_plcDic[plcName].WriteValue(valName, val, singleWrite);
#endif
        }

        /// <summary>
        /// 开始读取数值
        /// </summary>
        /// <param name="plcName"></param>
        /// <returns></returns>
        private async Task<Dictionary<string, object>> StartReadValue(EPlcName plcName)
        {
#if UNITY_WEBGL
            MSG_ReadValue requestData = new MSG_ReadValue
            {
                plcName = plcName,
                message = $"C2S->  PLC{plcName} --- 正在读取数据.."
            };
            MSG_Type msg_Type = new MSG_Type
            {
                desType = DESType.DEST_ReadValue,
                content = JsonConvert.SerializeObject(requestData),
            };
            string jsonData = JsonConvert.SerializeObject(msg_Type);
            return await PlcWebMessage.Ins.C2S_ReadValue(PlcConstant.PlcConnectServerPath, jsonData);
#endif
            return null;
        }

        /// <summary>
        /// 是否连接
        /// </summary>
        /// <param name="plcName"></param>
        /// <returns></returns>
        public async Task<bool> IsConnected(EPlcName plcName)
        {
            if (!m_plcDic.ContainsKey(plcName))
            {
                return false;
            }

#if UNITY_WEBGL
            MSG_IsConnected requestData = new MSG_IsConnected
            {
                plcName = plcName,
                message = $"C2S->  PLC:{plcName}"
            };
            MSG_Type msg_Type = new MSG_Type
            {
                desType = DESType.DEST_IsConnected,
                content = JsonConvert.SerializeObject(requestData),
            };
            string jsonData = JsonConvert.SerializeObject(msg_Type);
            return await PlcWebMessage.Ins.C2S_IsConnected(DESType.DEST_IsConnected, PlcConstant.PlcConnectServerPath, jsonData);
#else
            return m_plcDic[plcName].IsConnected();
#endif
        }

        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <param name="csvPath"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, MDataItem>> ReadPLCData(string csvPath)
        {
            csvPath = Application.streamingAssetsPath + csvPath;
#if UNITY_WEBGL && !UNITY_EDITOR
    csvPath = csvPath.Replace("file://", ""); 
#endif
            return await PlcCSVUtility.ReadPLCData(csvPath);
        }
        #endregion

        private IEnumerator WaitForServer()
        {
            bool isServerRunning = false;

            while (!isServerRunning)
            {
                using (UnityWebRequest request = UnityWebRequest.Get("http://localhost:5001"))
                {
                    yield return request.SendWebRequest(); 

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        isServerRunning = true;
                        Debug.Log("Server is running. Proceeding with exe start.");
                        string exePath = Application.streamingAssetsPath + PlcConstant.WebPlcServerPath;  
                        string arg1 = Application.streamingAssetsPath + PlcConstant.WebPlcServerPath;
                        string arg2 = Application.streamingAssetsPath;
                        string[] exeArgs = new string[] { arg1, arg2 }; 
                        StartCoroutine(StartWebServerHelperExe(exePath, exeArgs));
                    }
                    else
                    {
                        Debug.Log("Waiting for server to start...");
                        yield return new WaitForSeconds(1);  
                    }
                }
            }
        }

        /// <summary>
        /// 开始webserver辅助程序
        /// </summary>
        /// <param name="exePath"></param>
        /// <returns></returns>
        private IEnumerator StartWebServerHelperExe(string exePath, string[] exeArgs)
        {
            string parametersJson = string.Join(",", exeArgs.Select(arg => "\"" + arg + "\""));
            string jsonBody = "{\"ExePath\":\"" + exePath + "\", \"Parameters\":[" + parametersJson + "]}";

            using (UnityWebRequest request = new UnityWebRequest(PlcConstant.WebPlcServerHelperUrl, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Exe started successfully with parameters!");
                }
                else
                {
                    Debug.LogError("Failed to start exe: " + request.error);
                }
            }
        }
    }
}