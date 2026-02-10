using System;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class NewChargeLevel : HNSFStateDecision
    {
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return frame.Unsafe.TryGetPointer<IsChargingAttack>(entity, out var isCharging)
                   && isCharging->newChargeLevel;
        }
        
        public override HNSFStateDecision Copy()
        {
            return CopyTo(new NewChargeLevel());
        }
        
        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as NewChargeLevel;
            return base.CopyTo(target);
        }
    }
}