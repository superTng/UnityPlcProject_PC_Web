using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ATF
{
    [Serializable]
    public class MSG_Type
    {
        public DESType desType = DESType.DEST_IsConnected;
        public string content;
    }

    [Serializable]
    public class MSG_PlcConnect
    {
        public EPlcName plcName = EPlcName.EPN_JiXieShou;
        public string plcIp = "";   //Plc IP
        public bool plcState;       //Plc状态
        public string message = ""; //返回信息
    }

    [Serializable]
    public class MSG_DisPlcConnect
    {
        public EPlcName plcName = EPlcName.EPN_JiXieShou;
        public bool connectState;       //连接状态
        public string message = ""; //返回信息
    }

    [Serializable]
    public class MSG_AddListenerPLCValueChange
    {
        public EPlcName plcName = EPlcName.EPN_JiXieShou;
        public string valName = "";
        public string message = ""; //返回信息
    }

    [Serializable]
    public class MSG_WriteValue
    {
        public EPlcName plcName = EPlcName.EPN_JiXieShou;
        public string valName = "";
        public object val = "";
        public bool singleWrite = false;
        public string message = ""; //返回信息
    }

    [Serializable]
    public class MSG_IsConnected
    {
        public EPlcName plcName = EPlcName.EPN_JiXieShou;
        public bool isConnected ;
        public string message = ""; //返回信息
    }

    [Serializable]
    public class MSG_ReadValue
    {
        public EPlcName plcName = EPlcName.EPN_JiXieShou;
        public Dictionary<string, object> dataDic = new Dictionary<string, object>(); //返回信息
        public string message = "";
    }

    public class PlcWebMessage : CommonSingletonTemplate<PlcWebMessage>
    {
        public IEnumerator C2S_StartSendMessage(DESType desType,string url, string jsonData, Action succeedAction = null, Action errorAction = null)
        {
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                MSG_Type msg_Type = new MSG_Type()
                {
                    desType = desType,
                    content = jsonData,
                };
                string msg = JsonConvert.SerializeObject(msg_Type);
                byte[] bodyRaw = Encoding.UTF8.GetBytes(msg);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                yield return request.SendWebRequest();
                if (request.ATResult())
                {
                    string responseJson = request.downloadHandler.text;
                    MSG_Type processRequestType = JsonConvert.DeserializeObject<MSG_Type>(responseJson);
                    if (processRequestType != null)
                    {
                        switch (processRequestType.desType)
                        {
                            case DESType.DEST_PlcConnect:
                                C2S_PlcConnect(processRequestType.content, succeedAction, errorAction);
                                break;
                            case DESType.DEST_DisPlcConnect:
                                C2S_DisPlcConnect(processRequestType.content, succeedAction, errorAction);
                                break;
                            case DESType.DEST_AddListenerPLCValueChange:
                                //DES_AddListenerPLCValueChange(processRequestType.content);
                                break;
                            case DESType.DEST_RemoveListenerPLCValueChange:
                                //DES_RemoveListenerPLCValueChange(processRequestType.content);
                                break;
                            case DESType.DEST_WriteValue:
                                //DES_WriteValue(processRequestType.content);
                                break;
                            case DESType.DEST_IsConnected:
                                //C2S_IsConnected(responseJson, succeedAction, errorAction);
                                break;
                            default:
                                break;
                        }
                    }
                }
                else
                {
                    Debug.LogError($"Request请求失败: {request.error}");
                    errorAction?.Invoke();
                }
            }
        }


        /// <summary>
        /// 是否建立了连接
        /// </summary>
        /// <param name="desType"></param>
        /// <param name="url"></param>
        /// <param name="jsonData"></param>
        /// <param name="succeedAction"></param>
        /// <param name="errorAction"></param>
        /// <returns></returns>
        public async Task<bool> C2S_IsConnected(DESType desType, string url, string jsonData, Action succeedAction = null, Action errorAction = null)
        {
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield(); // 等待请求完成
                }

                if (request.ATResult()) // 这里假设 ATResult() 是一个判断请求是否成功的扩展方法
                {
                    string responseJson = request.downloadHandler.text;
                    MSG_Type processRequestType = JsonConvert.DeserializeObject<MSG_Type>(responseJson);
                    try
                    {
                        if (processRequestType != null)
                        {
                            switch (processRequestType.desType)
                            {
                                case DESType.DEST_IsConnected:
                                    MSG_IsConnected responseData = JsonConvert.DeserializeObject<MSG_IsConnected>(processRequestType.content);
                                    return responseData.isConnected;
                                default:
                                    break;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"JSON 解析错误: {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogError($"请求失败: {request.error}");
                }
            }

            return false; // 失败时返回默认值
        }

        /// <summary>
        /// 读取值
        /// </summary>
        /// <param name="plcName"></param>
        /// <param name="url"></param>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, object>> C2S_ReadValue(string url, string jsonData)
        {
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield(); 
                }

                if (request.ATResult()) 
                {
                    string responseJson = request.downloadHandler.text;
                    MSG_Type processRequestType = JsonConvert.DeserializeObject<MSG_Type>(responseJson);
                    try
                    {
                        if (processRequestType != null)
                        {
                            switch (processRequestType.desType)
                            {
                                case DESType.DEST_ReadValue:
                                    MSG_ReadValue responseData = JsonConvert.DeserializeObject<MSG_ReadValue>(processRequestType.content);
                                    //Debug.Log($"C2S_ReadValue Dic Count->{responseData.dataDic.Count}");
                                    return responseData.dataDic;
                                default:
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"JSON 解析错误: {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogError($"请求失败: {request.error}");
                }
            }

            return null; // 失败时返回默认值
        }

        /// <summary>
        /// 消息_解析Plc连接
        /// </summary>
        /// <param name="request"></param>
        /// <param name="succeedAction"></param>
        /// <param name="errorAction"></param>
        private void C2S_PlcConnect(string content, Action succeedAction = null, Action errorAction = null)
        {
            MSG_PlcConnect responseData = JsonConvert.DeserializeObject<MSG_PlcConnect>(content);

            if (responseData.plcState)
            {
                Debug.Log($"服务器响应: {responseData.message}，状态：{responseData.plcState}”");
                succeedAction?.Invoke();
            }
            else
            {
                Debug.LogError($"请求失败: {responseData.plcState}");
                errorAction?.Invoke();
            }
        }

        /// <summary>
        /// 消息_取消连接
        /// </summary>
        /// <param name="request"></param>
        /// <param name="succeedAction"></param>
        /// <param name="errorAction"></param>
        private void C2S_DisPlcConnect(string request, Action succeedAction = null, Action errorAction = null)
        {
            MSG_DisPlcConnect responseData = JsonConvert.DeserializeObject<MSG_DisPlcConnect>(request);

            if (responseData.connectState)
            {
                Debug.Log($"服务器响应: {responseData.message}");
                succeedAction?.Invoke();
            }
            else
            {
                Debug.LogError($"请求失败: {responseData.connectState}");
                errorAction?.Invoke();
            }
        }

        /// <summary>
        /// 消息_开始监听PLC值变化
        /// </summary>
        /// <param name="request"></param>
        /// <param name="succeedAction"></param>
        /// <param name="errorAction"></param>
        private void C2S_AddListenerPLCValueChange(string request, Action succeedAction = null, Action errorAction = null)
        {
            MSG_AddListenerPLCValueChange responseData = JsonConvert.DeserializeObject<MSG_AddListenerPLCValueChange>(request);

            //if (responseData.plcValueChangedState)
            //{
            //    Debug.Log($"服务器响应: {responseData.message}");
            //    succeedAction?.Invoke();
            //}
            //else
            //{
            //    Debug.LogError($"请求失败: {request.error}");
            //    errorAction?.Invoke();
            //}
        }

        /// <summary>
        /// 消息_开始移除PLC值变化
        /// </summary>
        /// <param name="request"></param>
        /// <param name="succeedAction"></param>
        /// <param name="errorAction"></param>
        private void C2S_RemoveListenerPLCValueChange(string request, Action succeedAction = null, Action errorAction = null)
        {
            MSG_AddListenerPLCValueChange responseData = JsonConvert.DeserializeObject<MSG_AddListenerPLCValueChange>(request);

            //if (responseData.plcValueChangedState)
            //{
            //    Debug.Log($"服务器响应: {responseData.message}");
            //    succeedAction?.Invoke();
            //}
            //else
            //{
            //    Debug.LogError($"请求失败: {request.error}");
            //    errorAction?.Invoke();
            //}
        }

        /// <summary>
        /// 消息_开始移除PLC值变化
        /// </summary>
        /// <param name="request"></param>
        /// <param name="succeedAction"></param>
        /// <param name="errorAction"></param>
        private void C2S_WriteValue(string request, Action succeedAction = null, Action errorAction = null)
        {
            MSG_WriteValue responseData = JsonConvert.DeserializeObject<MSG_WriteValue>(request);

            //if (responseData.plcValueChangedState)
            //{
            //    Debug.Log($"服务器响应: {responseData.message}");
            //    succeedAction?.Invoke();
            //}
            //else
            //{
            //    Debug.LogError($"请求失败: {request.error}");
            //    errorAction?.Invoke();
            //}
        }
    }
}