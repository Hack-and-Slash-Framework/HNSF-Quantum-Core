using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class ModifyLocalDeltaTime : HNSFStateAction
    {
        public FP multiplier = 1;
    
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var targetEntityRef = GetActionTargetEntityRef(frame, entity);
            if (targetEntityRef == EntityRef.None) return false;
            
            if (!frame.Unsafe.TryGetPointer<LocalDeltaTime>(targetEntityRef, out var ldt)) return false;
            ldt->multiplier = multiplier;
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ModifyLocalDeltaTime());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ModifyLocalDeltaTime;
            t.multiplier = multiplier;
            return base.CopyTo(target);
        }
    }
}