using System;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class NextStateType : HNSFStateDecision
    {
        public bool inverse;
        public AssetRef<Tag>[] validStatesTypes;
    
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!stateContext.agentData->toStateRequested || !frame.TryFindAsset(stateContext.agentData->toState, out var toState)) return false;
            
            return inverse ? Array.IndexOf(validStatesTypes, toState.stateType) < 0
                : Array.IndexOf(validStatesTypes, toState.stateType) >= 0;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new NextStateType());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as NextStateType;
            t.validStatesTypes = validStatesTypes;
            return base.CopyTo(target);
        }
    }
}