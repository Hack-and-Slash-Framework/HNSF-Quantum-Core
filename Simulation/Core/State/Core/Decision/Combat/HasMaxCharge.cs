using System;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class HasMaxCharge : HNSFStateDecision
    {
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return frame.Unsafe.TryGetPointer<IsChargingAttack>(entity, out var isCharging)
                   && isCharging->currentCharge >= isCharging->maxCharge;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new HasMaxCharge());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as HasMaxCharge;
            return base.CopyTo(target);
        }
    }
}