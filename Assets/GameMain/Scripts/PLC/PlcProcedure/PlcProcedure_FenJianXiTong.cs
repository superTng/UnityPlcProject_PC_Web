using System;
using UnityEngine;

namespace ATF
{
    public class PlcProcedure_FenJianXiTong : PlcProcedureBase
    {
        public MenuForm UI_MenuForm;
        public override void StartConnectPlc(EPlcName plcName, string plcIp = "", Action succeedAction = null, Action errorAction = null)
        {
            PlcManager.Ins.StartConnectPlc(EPlcName.EPN_FenJian, plcIp, succeedAction, errorAction);
            StartInitListener();
        }

        public override void StartDisConnectPlc(EPlcName plcName, Action succeedAction = null, Action errorAction = null)
        {
            base.StartDisConnectPlc(plcName, succeedAction, errorAction);
            PlcManager.Ins.StartDisConnectPlc(EPlcName.EPN_FenJian, succeedAction, errorAction);
        }

        public override void StartInitListener()
        {
            PlcManager.Ins.AddListenerPLCValueChange(EPlcName.EPN_FenJian, "1_改变图片颜色", OnChangeImageColor);
        }

        private void OnChangeImageColor(object sender)
        {
            if (sender != null)
            {
                bool state = (bool)sender;
                Debug.Log("1_改变图片颜色:  " + state);
                MainThreadTaskQueue.EnqueueTask(() =>
                {
                    UI_MenuForm.OnChangeImageColor();
                });
            }
        }
    }
}