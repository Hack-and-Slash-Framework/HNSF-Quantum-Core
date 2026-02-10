using System;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class CurrentStateType : HNSFStateDecision
    {
        public bool inverse;
        public AssetRef<Tag>[] validStatesTypes;
    
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.TryFindAsset(stateContext.agentData->state, out var currentState)) return false;
            
            return inverse ? Array.IndexOf(validStatesTypes, currentState.stateType) < 0
                : Array.IndexOf(validStatesTypes, currentState.stateType) >= 0;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new CurrentStateType());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as CurrentStateType;
            t.validStatesTypes = validStatesTypes;
            return base.CopyTo(target);
        }
    }
}