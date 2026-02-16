using System;
using System.Linq;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class NextStateGrounded : HNSFStateDecision
    {
        public bool inverse;
        public StateGroundedType[] validGroundedTypes;
    
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!stateContext.agentData->toStateRequested || !frame.TryFindAsset(stateContext.agentData->toState, out var toState)) return false;
            
            return inverse ? Array.IndexOf(validGroundedTypes, toState.initialGroundedState) < 0
                : Array.IndexOf(validGroundedTypes, toState.initialGroundedState) >= 0;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new NextStateGrounded());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as NextStateGrounded;
            t.inverse = inverse;
            t.validGroundedTypes = validGroundedTypes.ToArray();
            return base.CopyTo(target);
        }
    }
}