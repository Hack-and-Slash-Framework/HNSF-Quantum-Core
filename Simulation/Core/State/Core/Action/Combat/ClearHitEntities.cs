using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class ClearHitEntities : HNSFStateAction
    {
        public int clearEvery = 1;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<BoxCombatant>(entity, out var hitBoxCombatant)) return false;
            
            if(stateContext.stateFrame % clearEvery == 0) hitBoxCombatant->ClearHitList(frame);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ClearHitEntities());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ClearHitEntities;
            t.clearEvery = clearEvery;
            return base.CopyTo(target);
        }
    }
}