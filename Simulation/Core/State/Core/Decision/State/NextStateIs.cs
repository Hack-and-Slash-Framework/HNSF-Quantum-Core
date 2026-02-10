using System;
using System.Linq;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class NextStateIs : HNSFStateDecision
    {
        public AssetRef<HNSFState>[] validStates;
    
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return stateContext.agentData->toStateRequested
                   && Array.IndexOf(validStates, stateContext.agentData->toState) >= 0;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new NextStateIs());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as NextStateIs;
            t.validStates = validStates.ToArray();
            return base.CopyTo(target);
        }
    }
}