using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class DumpInputBuffer : HNSFStateAction
    {
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var inputs = frame.Unsafe.GetPointer<ActorInputBuffer>(entity);

            var str = "";
            for (int i = inputs->bufferPosition; i >= inputs->bufferPosition - Constants.INPUT_BUFFER_SIZE + 1; i--)
            {
                str += "\n";
            }
            
            Log.Debug(str);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new DumpInputBuffer());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            return base.CopyTo(target);
        }
    }
}