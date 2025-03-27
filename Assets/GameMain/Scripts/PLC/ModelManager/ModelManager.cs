using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ATF
{
    /// <summary>
    /// Plc模型实体管理，这个脚本的存在是很有意义的，因为会涉及到不同的模型身上的不同的plc，然后模型重新销毁和多次重复调用等
    /// </summary>
    public class ModelManager : UnitySingletonTemplate<ModelManager>
    {
        public Transform PlcModel;
        [HideInInspector]
        public PlcProcedureBase CurPlcProcedure;

        private void Start()
        {
            CurPlcProcedure = PlcModel.GetComponent<PlcProcedureBase>();
        }
    }
}