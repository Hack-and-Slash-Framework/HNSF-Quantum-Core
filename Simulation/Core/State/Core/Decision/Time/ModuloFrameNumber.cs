using System;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class ModuloFrameNumber : HNSFStateDecision
    {
        public int modulo = 0;
        
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return frame.Number % modulo == 0;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new ModuloFrameNumber());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as ModuloFrameNumber;
            t.modulo = modulo;
            return base.CopyTo(target);
        }
    }
}