using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class DestroyWarningbox : HNSFStateAction
    {
        public int warningboxIdentifier;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<BoxCombatant>(entity, out var boxCombatant)) return false;
            boxCombatant->DeleteWarningboxByID(frame, warningboxIdentifier);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new DestroyWarningbox());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as DestroyWarningbox;
            t.warningboxIdentifier = warningboxIdentifier;
            return base.CopyTo(target);
        }
    }
}
