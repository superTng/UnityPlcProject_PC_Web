using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ATF
{
    public static partial class PlcConstant
    {
        public const string PlcConfigDataPath_FenJian = "/PlcConfig/Plc程序A.csv";
        public const string PlcConfigDataPath_JiXieShou = "/PlcConfig/Plc程序B.csv";
        public const string PlcConfigDataPath_CangKu = "/PlcConfig/Plc程序C.csv";
        public const string GameConfig = "/GameConfig/GameConfig.csv";
        public const string PlcConnectServerPath = "http://localhost:8080/api/connectPLC/";
        public const string WebPlcServerPath = "/WebPlc/WebPlc.exe";
        public const string WebPlcServerHelperPath = "/WebServerHelper/WebPlcHelper.exe";
        public const string WebPlcServerHelperUrl = "http://localhost:5000/startExe";
    }
}