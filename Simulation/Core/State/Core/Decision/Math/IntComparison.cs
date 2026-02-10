using System;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class IntComparison : HNSFStateDecision
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
    
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var pA = paramA.Resolve(frame, entity, ref stateContext);
            var pB = paramB.Resolve(frame, entity, ref stateContext);
        
            switch (checkType)
            {
                case CheckType.EQUAL:
                    return pA == pB;
                case CheckType.NOT_EQUAL:
                    return pA != pB;
                case CheckType.GREATER_THAN:
                    return pA > pB;
                case CheckType.LESS_THAN:
                    return pA < pB;
                case CheckType.GREATER_OR_EQUAL:
                    return pA >= pB;
                case CheckType.LESS_OR_EQUAL:
                    return pA <= pB;
            }
            return false;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new IntComparison());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as IntComparison;
            t.checkType = checkType;
            t.paramA = paramA.Clone() as HNSFParamInt;
            t.paramB = paramB.Clone() as HNSFParamInt;
            return base.CopyTo(target);
        }
    }
}