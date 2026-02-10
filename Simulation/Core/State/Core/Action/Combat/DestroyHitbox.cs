using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class DestroyHitbox : HNSFStateAction
    {
        public int hitboxIdentifier;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<BoxCombatant>(entity, out var boxCombatant)) return false;
            boxCombatant->DeleteHitboxByID(frame, hitboxIdentifier);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new DestroyHitbox());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as DestroyHitbox;
            t.hitboxIdentifier = hitboxIdentifier;
            return base.CopyTo(target);
        }
    }
}