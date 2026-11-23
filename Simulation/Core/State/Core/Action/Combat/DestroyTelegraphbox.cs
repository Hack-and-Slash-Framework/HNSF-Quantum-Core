using Photon.Deterministic;
using System;
using Quantum;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace HnSF.core.state.actions
{
    [Serializable]
    [MovedFrom(autoUpdateAPI: false, sourceClassName: "DestroyWarningbox")]
    public unsafe partial class DestroyTelegraphbox : HNSFStateAction
    {
        [FormerlySerializedAs("warningboxIdentifier")] public int boxIdentifier;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<BoxCombatant>(entity, out var boxCombatant)) return false;
            boxCombatant->DeleteTelegraphboxByID(frame, boxIdentifier);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new DestroyTelegraphbox());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as DestroyTelegraphbox;
            t.boxIdentifier = boxIdentifier;
            return base.CopyTo(target);
        }
    }
}
