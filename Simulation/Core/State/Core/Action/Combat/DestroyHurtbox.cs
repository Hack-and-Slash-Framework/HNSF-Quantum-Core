using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class DestroyHurtbox : HNSFStateAction
    {
        public int hurtboxIdentifier;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<BoxCombatant>(entity, out var boxCombatant)) return false;
            boxCombatant->DeleteHurtboxByID(frame, hurtboxIdentifier);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new DestroyHurtbox());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as DestroyHurtbox;
            t.hurtboxIdentifier = hurtboxIdentifier;
            return base.CopyTo(target);
        }
    }
}