using System;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class HNSFStateDecision
    {
        public string Label;
        public StateActionTargetType decisionTargetType = StateActionTargetType.Self;
        
        public virtual Boolean Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return false;
        }

        public virtual Boolean DecideThreadSafe(FrameThreadSafe frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return Decide((Frame)frame, entity, ref stateContext);
        }

        public virtual HNSFStateDecision Copy()
        {
            return CopyTo(new HNSFStateDecision());
        }

        public virtual HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            target.Label = Label;
            return target;
        }
    }
}