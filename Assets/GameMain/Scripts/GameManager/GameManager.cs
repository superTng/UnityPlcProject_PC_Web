using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ATF
{
    [DefaultExecutionOrder(-10)]
    public class GameManager : UnitySingletonTemplate<GameManager>
    {
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this);
        }
        private void Start()
        {
            
        }
    }
}