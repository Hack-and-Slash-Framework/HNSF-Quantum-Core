using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class ClearSoftTarget : HNSFStateAction
    {
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<CombatTargeter>(entity, out var targeter)) return false;
            targeter->softTarget = EntityRef.None;
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ClearSoftTarget());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ClearSoftTarget;
            return base.CopyTo(target);
        }
    }
}