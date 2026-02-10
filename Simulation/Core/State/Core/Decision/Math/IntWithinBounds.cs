using System;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class IntWithinBounds : HNSFStateDecision
    {
        public enum CheckType
        {
            MORE_THAN_LIMIT_LOW,
            IN_BETWEEN,
            LESS_THAN_LIMIT_LOW
        }
    
        public CheckType checkType;
        public HNSFParamInt value;
        public int limitLow;
        public int limitHigh;

        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var val = value.Resolve(frame, entity, ref stateContext);
            switch (checkType)
            {
                case CheckType.MORE_THAN_LIMIT_LOW:
                    return val > limitLow;
                case CheckType.LESS_THAN_LIMIT_LOW:
                    return val < limitLow;
                case CheckType.IN_BETWEEN:
                    return val >= limitLow && val <= limitHigh;
            }

            return false;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new IntWithinBounds());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as IntWithinBounds;
            t.checkType = checkType;
            t.value = value.Clone() as HNSFParamInt;
            t.limitLow = limitLow;
            t.limitHigh = limitHigh;
            return base.CopyTo(target);
        }
    }
}