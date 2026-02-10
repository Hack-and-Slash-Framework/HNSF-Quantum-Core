using System;
using System.Linq;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class CurrentStatesetIs : HNSFStateDecision
    {
        public AssetRef<HNSFStateSet>[] wantedStateSet = Array.Empty<AssetRef<HNSFStateSet>>();
    
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<GenericStateMachine>(entity, out var charaStateMachines)) return false;
            return Array.IndexOf(wantedStateSet, charaStateMachines->stateAgent.stateSet) >= 0;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new CurrentStatesetIs());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as CurrentStatesetIs;
            t.wantedStateSet = wantedStateSet.ToArray();
            return base.CopyTo(target);
        }
    }
}