using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class ClearInputBuffer : HNSFStateAction
    {
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var inputs = frame.Unsafe.GetPointer<ActorInputBuffer>(entity);

            InputHelper.DisableLastInput(frame, inputs);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ClearInputBuffer());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            return base.CopyTo(target);
        }
    }
}