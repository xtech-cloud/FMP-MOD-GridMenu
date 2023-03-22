
using System;
using LibMVCS = XTC.FMP.LIB.MVCS;
using XTC.FMP.MOD.GridMenu.LIB.Bridge;
using XTC.FMP.MOD.GridMenu.LIB.MVCS;

namespace XTC.FMP.MOD.GridMenu.LIB.Unity
{
    /// <summary>
    /// 虚拟视图，用于处理消息订阅
    /// </summary>
    public class DummyView :  DummyViewBase
    {
        public DummyView(string _uid) : base(_uid)
        {
        }

        protected override void setup()
        {
            base.setup();
        }
    }
}

