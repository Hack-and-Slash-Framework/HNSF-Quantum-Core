using System;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class IsCharging : HNSFStateDecision
    {
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return frame.Has<IsChargingAttack>(entity);
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new IsCharging());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as IsCharging;
            return base.CopyTo(target);
        }
    }
}