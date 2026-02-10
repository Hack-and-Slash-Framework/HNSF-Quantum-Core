using System;
using System.Linq;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class CurrentStateIs : HNSFStateDecision
    {
        public AssetRef<HNSFState>[] validStates;
    
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return Array.IndexOf(validStates, stateContext.agentData->state) >= 0;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new CurrentStateIs());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as CurrentStateIs;
            t.validStates = validStates.ToArray();
            return base.CopyTo(target);
        }
    }
}