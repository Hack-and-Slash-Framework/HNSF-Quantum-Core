using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class ClearHitEntitiesAfterHit : HNSFStateAction
    {
        public int clearEvery = 1;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<BoxCombatant>(entity, out var hitBoxCombatant)
                || !frame.Unsafe.TryGetPointer<LastHitWithInfo>(entity, out var lastHitWithInfo)) return false;

            if (hitBoxCombatant->GetCurrentEntityHitCount(frame) == 0) return false;
            if (lastHitWithInfo->data.Field != Quantum.LastHitWithData.HITINFODATA) return false;
            if((frame.Number - (lastHitWithInfo->lastHitEntityOnFrame+lastHitWithInfo->data.hitInfoData->lastHitstopAmount) ) >= clearEvery) hitBoxCombatant->ClearHitList(frame);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ClearHitEntitiesAfterHit());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ClearHitEntitiesAfterHit;
            t.clearEvery = clearEvery;
            return base.CopyTo(target);
        }
    }
}