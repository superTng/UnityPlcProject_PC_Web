using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Program;

namespace WebPlc.Scripts
{
    public enum EPlcName
    {
        EPN_FenJian,
        EPN_JiXieShou,
        EPN_CangKu,
    }

    public enum DESType
    {
        DEST_PlcConnect,        //Plc 连接
        DEST_DisPlcConnect,     //Plc 取消连接
        DEST_AddListenerPLCValueChange,
        DEST_RemoveListenerPLCValueChange,
        DEST_WriteValue,
        DEST_IsConnected,
        DEST_ReadValue,
    }

    [Serializable]
    public class MSG_Type
    {
        public DESType desType = DESType.DEST_IsConnected;
        public string content ="";
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
        public object val= "";
        public bool singleWrite = false;
        public string message = ""; //返回信息
    }

    [Serializable]
    public class MSG_IsConnected
    {
        public EPlcName plcName = EPlcName.EPN_JiXieShou;
        public bool isConnected;
        public string message = ""; //返回信息
    }

    [Serializable]
    public class MSG_ReadValue
    {
        public EPlcName plcName = EPlcName.EPN_JiXieShou;
        public Dictionary<string ,object?> dataDic = new Dictionary<string, object?>(); //返回信息
        public string message = "";
    }

    public static class PlcMessage
    {
        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <param name="csvPath"></param>
        /// <returns></returns>
        public static Dictionary<string, MDataItem> ReadPLCData(string csvPath)
        {
            //string filePath = Path.Combine(Directory.GetCurrentDirectory()+csvPath);
            return CSVUtility.ReadPLCData(csvPath); 
        }
    }
}
