using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class DestroyCollisionbox : HNSFStateAction
    {
        public int collisionboxIdentifier;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<BoxCombatant>(entity, out var boxCombatant)) return false;
            boxCombatant->DeleteCollisionboxByID(frame, collisionboxIdentifier);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new DestroyCollisionbox());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as DestroyCollisionbox;
            t.collisionboxIdentifier = collisionboxIdentifier;
            return base.CopyTo(target);
        }
    }
}