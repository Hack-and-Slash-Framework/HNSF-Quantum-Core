using System;
using System.Linq;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class ExecuteExternalStateActionList : HNSFStateAction
    {
        public StateActionListExternal[] externalActionLists = Array.Empty<StateActionListExternal>();
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent, ref HNSFStateContext stateContext)
        {
            foreach (var externalActionList in externalActionLists)
            {
                if (externalActionList.Execute(frame, entity)) break;
            }
            return false;
        }
        
        public override HNSFStateAction Copy()
        {
            return CopyTo(new ExecuteExternalStateActionList());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ExecuteExternalStateActionList;
            t.externalActionLists = externalActionLists.ToArray();
            return base.CopyTo(target);
        }
    }
}