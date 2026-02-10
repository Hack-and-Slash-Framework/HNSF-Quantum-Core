using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class ClearCharge : HNSFStateAction
    {
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            frame.Remove<IsChargingAttack>(entity);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ClearCharge());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ClearCharge;
            return base.CopyTo(target);
        }
    }
}