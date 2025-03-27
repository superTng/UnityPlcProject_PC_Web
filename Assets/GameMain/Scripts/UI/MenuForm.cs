using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace ATF
{
    public class MenuForm : MonoBehaviour
    {
        public TMP_InputField Input_ip;
        public Toggle Tog_isConnected;
        public Image Img_isConnected;
        public Button Btn_StartConnect;
        public Button Btn_DisConnect;

        public TMP_InputField Input_WriteIntValue;
        public Button Btn_StartIntWrite;

        public TMP_InputField Input_WriteFloatValue;
        public Button Btn_StartFloatWrite;

        public Image Img_AddListenerValueChange;

        private void Start()
        {
            Input_ip.text = "192.168.0.1";
            Btn_StartConnect.onClick.AddListener(OnStartConnectBtnClick);
            Btn_DisConnect.onClick.AddListener(OnStopConnectBtnClick);
           

            //给Plc写入一个整数
            Btn_StartIntWrite.onClick.AddListener(OnStartIntWriteClick);

            //给Plc写入一个小数
            Btn_StartFloatWrite.onClick.AddListener(OnStartFloatWriteClick);
        }


        /// <summary>
        /// 开始连接按钮按下
        /// </summary>
        private void OnStartConnectBtnClick()
        { 
            string ip = Input_ip.text;
            Action successAction = () => { Tog_isConnected.isOn = true; Img_isConnected.color = Color.green; };
            Action faildAction = () => { Tog_isConnected.isOn = false; Img_isConnected.color = Color.red; };
            ModelManager.Ins.CurPlcProcedure.StartConnectPlc(EPlcName.EPN_FenJian, ip, successAction, faildAction);
        }

        /// <summary>
        /// 结束连接按钮按下
        /// </summary>
        private void OnStopConnectBtnClick()
        {
            Action successAction = () => { Tog_isConnected.isOn = false; Img_isConnected.color = Color.red; };
            Action faildAction = () => { Tog_isConnected.isOn = true; Img_isConnected.color = Color.green; };
            ModelManager.Ins.CurPlcProcedure.StartDisConnectPlc(EPlcName.EPN_FenJian, successAction, faildAction);
        }

        /// <summary>
        /// 当image颜色改变时的事件
        /// </summary>
        /// <param name="sender"></param>
        public void OnChangeImageColor()
        {
            Img_AddListenerValueChange.color = Random.ColorHSV();
        }

        /// <summary>
        /// 写入数值按钮按下
        /// </summary>
        private void OnStartIntWriteClick()
        {
            PlcManager.Ins.WriteValue(EPlcName.EPN_FenJian, "2_写个整数测试", short.Parse(Input_WriteIntValue.text));
        }

        /// <summary>
        /// 写入数值按钮按下
        /// </summary>
        private void OnStartFloatWriteClick()
        {
            PlcManager.Ins.WriteValue(EPlcName.EPN_FenJian, "3_写个小数测试", float.Parse(Input_WriteFloatValue.text));
        }
    }
}