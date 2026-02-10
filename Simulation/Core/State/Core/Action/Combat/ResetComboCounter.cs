using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class ResetComboCounter : HNSFStateAction
    {
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (frame.Unsafe.TryGetPointer(entity, out ComboCounting* comboCounting))
            {
                comboCounting->comboCounter = 0;
                comboCounting->realComboCounter = 0;
            }
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ResetComboCounter());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ResetComboCounter;
            return base.CopyTo(target);
        }
    }
}