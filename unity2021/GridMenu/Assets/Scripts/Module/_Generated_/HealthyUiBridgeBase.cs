
//*************************************************************************************
//   !!! Generated by the fmp-cli 1.83.0.  DO NOT EDIT!
//*************************************************************************************

using System;
using System.Threading;
using LibMVCS = XTC.FMP.LIB.MVCS;
using XTC.FMP.MOD.GridMenu.LIB.Bridge;

namespace XTC.FMP.MOD.GridMenu.LIB.Unity
{

    public class HealthyUiBridgeBase : IHealthyUiBridge
    {
        public LibMVCS.Logger logger { get; set; }

        public virtual void Alert(string _code, string _message, object _context)
        {
            throw new NotImplementedException();
        }


        public virtual void RefreshEcho(IDTO _dto, object _context)
        {
            throw new NotImplementedException();
        }

    }
}