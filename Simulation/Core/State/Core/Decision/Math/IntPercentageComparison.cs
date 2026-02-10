using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class IntPercentageComparison : HNSFStateDecision
    {
        public enum CheckType
        {
            EQUAL,
            NOT_EQUAL,
            GREATER_THAN,
            LESS_THAN,
            GREATER_OR_EQUAL,
            LESS_OR_EQUAL,
        }

        public CheckType checkType;

        public HNSFParamInt paramA;
        public HNSFParamInt paramB;
        public HNSFParamFP percentage;
    
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var val = (FP)paramA.Resolve(frame, entity, ref stateContext) /  (FP)paramB.Resolve(frame, entity, ref stateContext);
            var p = percentage.Resolve(frame, entity, ref stateContext);
        
            switch (checkType)
            {
                case CheckType.EQUAL:
                    return val == p;
                case CheckType.NOT_EQUAL:
                    return val == p;
                case CheckType.GREATER_THAN:
                    return val > p;
                case CheckType.LESS_THAN:
                    return val < p;
                case CheckType.GREATER_OR_EQUAL:
                    return val >= p;
                case CheckType.LESS_OR_EQUAL:
                    return val <= p;
            }
            return false;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new IntPercentageComparison());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as IntPercentageComparison;
            t.checkType = checkType;
            t.paramA = paramA.Clone() as HNSFParamInt;
            t.paramB = paramB.Clone() as HNSFParamInt;
            t.percentage = percentage.Clone() as HNSFParamFP;
            return base.CopyTo(target);
        }
    }
}