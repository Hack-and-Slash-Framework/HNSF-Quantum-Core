using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class DestroyThrowbox : HNSFStateAction
    {
        public int throwboxIdentifier;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<BoxCombatant>(entity, out var boxCombatant)) return false;
            boxCombatant->DeleteThrowboxByID(frame, throwboxIdentifier);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new DestroyThrowbox());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as DestroyThrowbox;
            t.throwboxIdentifier = throwboxIdentifier;
            return base.CopyTo(target);
        }
    }
}