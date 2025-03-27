using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using Newtonsoft.Json;


using S7.Net;
using S7.Net.Types;
using WebPlc.Scripts;
using static System.Net.Mime.MediaTypeNames;
using Constant = WebPlc.Scripts.Constant;

class Program
{
    private static Dictionary<EPlcName, PlcVariable> m_plcDic = new Dictionary<EPlcName, PlcVariable>();
    private static int m_plcUpdateSpeed = 100;//Plc数据更新的速率： 100ms
    static void Main()
    {
        //TODO:创建Plc

        string[] args = Environment.GetCommandLineArgs();
        string headPath = "";

        //foreach (string arg in args) 
        //{
        //    Console.WriteLine(arg);
        //}
        if (args.Length > 1)
        {
            headPath = args[1];
        }
        else
        {
            headPath = Path.Combine(Directory.GetCurrentDirectory());
        }
        m_plcDic.Add(EPlcName.EPN_FenJian, new PlcVariable(PlcMessage.ReadPLCData(headPath + Constant.PlcConfigDataPath_FenJian), m_plcUpdateSpeed));
        m_plcDic.Add(EPlcName.EPN_JiXieShou, new PlcVariable(PlcMessage.ReadPLCData(headPath + Constant.PlcConfigDataPath_JiXieShou), m_plcUpdateSpeed));
        m_plcDic.Add(EPlcName.EPN_CangKu, new PlcVariable(PlcMessage.ReadPLCData(headPath + Constant.PlcConfigDataPath_CangKu), m_plcUpdateSpeed));

        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/api/connectPLC/");
        listener.Start();
        Console.WriteLine("服务器启动，等待请求...");
        while (true)
        {
            var context = listener.GetContext();
            ProcessRequest(context);
        }
    }

    private static async void ProcessRequest(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

        if (request.HttpMethod == "OPTIONS")
        {
            response.StatusCode = 200;
            response.Close();
            return;
        }

        if (request.HttpMethod == "POST")
        {
            using (var reader = new StreamReader(request.InputStream, Encoding.UTF8))
            {
                string requestData = reader.ReadToEnd();
                MSG_Type plcRequest = JsonConvert.DeserializeObject<MSG_Type>(requestData);
                //Console.WriteLine($"接收到请求: PLC Type: {plcRequest.desType}, PLC Name: {plcRequest.content}");
                if (plcRequest != null)
                {
                    string sendMessage = "";
                    switch (plcRequest.desType)
                    {
                        case DESType.DEST_PlcConnect:
                            Console.WriteLine("开始连接...");
                            sendMessage = await S2C_PlcConnect(plcRequest.content);
                            break;
                        case DESType.DEST_DisPlcConnect:
                            S2C_DisPlcConnect(plcRequest.content);
                            break;
                        case DESType.DEST_AddListenerPLCValueChange:
                            break;
                        case DESType.DEST_RemoveListenerPLCValueChange:
                            break;
                        case DESType.DEST_WriteValue:
                            S2C_WriteValue(plcRequest.content);
                            break;
                        case DESType.DEST_IsConnected:
                            sendMessage = S2C_IsConnected(plcRequest.content);
                            break;
                        case DESType.DEST_ReadValue:
                            sendMessage = S2C_ReadValue(plcRequest.content);
                            break;
                        default:
                            break;
                    }
                    //Console.WriteLine("开始发送消息...");
                    plcRequest.content = sendMessage;
                    string responseJson = JsonConvert.SerializeObject(plcRequest);
                    byte[] buffer = Encoding.UTF8.GetBytes(responseJson);
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    //Console.WriteLine($"信息已发出：{responseJson}");
                }
            }
        }
        else
        {
            response.StatusCode = 405;
            byte[] buffer = Encoding.UTF8.GetBytes("仅支持 POST 请求");
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        response.Close();
    }

    /// <summary>
    /// 消息_解析Plc连接
    /// </summary>
    /// <param name="request"></param>
    /// <param name="succeedAction"></param>
    /// <param name="errorAction"></param>
    private static async Task<string> S2C_PlcConnect(string content)
    {
        MSG_PlcConnect responseData = JsonConvert.DeserializeObject<MSG_PlcConnect>(content);
        MSG_PlcConnect sendResponse = new MSG_PlcConnect
        {
            plcName = responseData.plcName,
            plcIp = responseData.plcIp,
            plcState = false,
            message = $"S2C->  PLC {responseData.plcIp} 连接失败，IP: {responseData.plcState}"
        };
        Action successAction = new Action(() => {
            sendResponse.plcState = true;
            sendResponse.message = $"S2C->  PLC {responseData.plcName} 连接成功";
        });
        Action failedAction = new Action(() => {
            sendResponse.plcState = false;
            sendResponse.message = $"S2C->  PLC {responseData.plcName} 连接失败";
        });

        switch (responseData.plcName)
        {
            case EPlcName.EPN_FenJian:
                await m_plcDic[EPlcName.EPN_FenJian].StartConnect(responseData.plcIp, successAction, failedAction);
                break;
            case EPlcName.EPN_JiXieShou:
                await m_plcDic[EPlcName.EPN_JiXieShou].StartConnect(responseData.plcIp, successAction, failedAction);
                break;
            case EPlcName.EPN_CangKu:
                await m_plcDic[EPlcName.EPN_CangKu].StartConnect(responseData.plcIp, successAction, failedAction);
                break;
            default:
                break;
        }
        await Task.Delay(1000);
        m_plcDic[responseData.plcName].StartUpdateWritePlc();
        return JsonConvert.SerializeObject(sendResponse);
    }

    /// <summary>
    /// 消息_取消连接
    /// </summary>
    /// <param name="request"></param>
    /// <param name="succeedAction"></param>
    /// <param name="errorAction"></param>
    private static void S2C_DisPlcConnect(string content)
    {
        MSG_DisPlcConnect responseData = JsonConvert.DeserializeObject<MSG_DisPlcConnect>(content);
        if (!m_plcDic.ContainsKey(responseData.plcName))
        {
            return;
        }
        m_plcDic[EPlcName.EPN_FenJian].StartDisConnect();
    }

    /// <summary>
    /// 消息_开始监听PLC值变化
    /// </summary>
    /// <param name="request"></param>
    /// <param name="succeedAction"></param>
    /// <param name="errorAction"></param>
    private static void S2C_AddListenerPLCValueChange(string request)
    {
        MSG_AddListenerPLCValueChange responseData = JsonConvert.DeserializeObject<MSG_AddListenerPLCValueChange>(request);
        if (!m_plcDic.ContainsKey(responseData.plcName))
        {
            return;
        }
    }

    /// <summary>
    /// 消息_开始写值
    /// </summary>
    /// <param name="request"></param>
    /// <param name="succeedAction"></param>
    /// <param name="errorAction"></param>
    private static void S2C_WriteValue(string request)
    {
        MSG_WriteValue responseData = JsonConvert.DeserializeObject<MSG_WriteValue>(request);

        if (!m_plcDic.ContainsKey(responseData.plcName))
        {
            return;
        }
        m_plcDic[responseData.plcName].WriteValue(responseData.valName, responseData.val, responseData.singleWrite);
    }

    /// <summary>
    /// 是否连接
    /// </summary>
    /// <param name="request"></param>
    /// <param name="succeedAction"></param>
    /// <param name="errorAction"></param>
    /// <returns></returns>
    private static string S2C_IsConnected(string request)
    {
        MSG_IsConnected responseData = JsonConvert.DeserializeObject<MSG_IsConnected>(request);
        MSG_IsConnected sendResponse = new MSG_IsConnected
        {
            plcName = responseData.plcName,
            //message = $"S2C->  PLC {responseData.plcName} 连接失败，IP: {responseData.plcState}"
        };

        if (!m_plcDic.ContainsKey(responseData.plcName))
        {
            sendResponse.isConnected = false;
        }
        else
        {
            sendResponse.isConnected = m_plcDic[responseData.plcName].IsConnected();
            sendResponse.message = $"S2C->  PLC{responseData.plcName} 连接状态: {sendResponse.isConnected}";
        }
        return JsonConvert.SerializeObject(sendResponse);
    }

    /// <summary>
    /// 读取数值
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private static string S2C_ReadValue(string request)
    {
        MSG_ReadValue responseData = JsonConvert.DeserializeObject<MSG_ReadValue>(request);
        Dictionary<string, object?> newDic = new Dictionary<string, object?>();
        MSG_ReadValue sendResponse = new MSG_ReadValue
        {
            plcName = responseData.plcName,
        };
        foreach (var item in m_plcDic[responseData.plcName].m_plcDic)
        {
            newDic.Add(item.Key, item.Value.Value);
        }
        //Console.WriteLine($"C2S_ReadValue Dic Count->{sendResponse.dataDic.Count}");
        sendResponse.dataDic = newDic;
        sendResponse.message = $"S2C->  PLC {responseData.plcName} 连读取值，数量: {newDic.Count}";
        return JsonConvert.SerializeObject(sendResponse);
    }
}
