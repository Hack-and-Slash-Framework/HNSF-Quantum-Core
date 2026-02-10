using System;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class StateLastInAttackString : HNSFStateDecision
    {
        public int amountBeforeTrue = 1;
    
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<TrackingAttackString>(entity, out var trackingAttackString)) return false;
            return trackingAttackString->IsAttackLastInStringXTimes(frame, stateContext.workingState, amountBeforeTrue);
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new StateLastInAttackString());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as StateLastInAttackString;
            t.amountBeforeTrue = amountBeforeTrue;
            return base.CopyTo(target);
        }
    }
}