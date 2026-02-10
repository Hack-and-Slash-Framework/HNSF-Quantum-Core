using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class RegisterStateToAttackString : HNSFStateAction
    {
        public bool printString;
    
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            frame.AddOrGet(entity, out TrackingAttackString* trackedAttackString);
            trackedAttackString->RegisterAttackToString(frame, stateContext.workingState);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new RegisterStateToAttackString());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as RegisterStateToAttackString;
            t.printString = printString;
            return base.CopyTo(target);
        }
    }
}