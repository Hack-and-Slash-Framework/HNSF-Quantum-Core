using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Debug/Log Message")]
    public unsafe partial class LogMessage : HNSFStateAction
    {
        public string msg = "";
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            Log.Debug(msg);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new LogMessage());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as LogMessage;
            t.msg = msg;
            return base.CopyTo(target);
        }
    }
}