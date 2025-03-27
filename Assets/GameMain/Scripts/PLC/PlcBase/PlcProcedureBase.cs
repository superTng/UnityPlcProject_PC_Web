using System;using UnityEngine;

namespace ATF
{
    public class PlcProcedureBase : MonoBehaviour
    {
        public virtual void StartConnectPlc(EPlcName plcName, string plcIp = "", Action succeedAction = null, Action errorAction = null)
        {

        }
        public virtual void StartDisConnectPlc(EPlcName plcName,  Action succeedAction = null, Action errorAction = null)
        {

        }

        public virtual void StartInitListener()
        { 
        
        }

        public virtual void StartResetScene()
        {

        }
        public virtual void StartResetPlcModel()
        {

        }
    }
}