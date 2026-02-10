using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class ClearAttackString : HNSFStateAction
    {
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            frame.Remove<TrackingAttackString>(entity);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ClearAttackString());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ClearAttackString;
            return base.CopyTo(target);
        }
    }
}