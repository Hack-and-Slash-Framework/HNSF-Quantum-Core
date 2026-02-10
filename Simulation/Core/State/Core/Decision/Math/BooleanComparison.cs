using System;
using HnSF.core.state;
using HnSF.core.state.decisions;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class BooleanComparison : HNSFStateDecision
    {
        public enum CheckType
        {
            EQUAL,
            NOT_EQUAL,
        }

        public CheckType checkType;

        public HNSFParamBool paramA;
        public HNSFParamBool paramB;
    
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
            }
            return false;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new BooleanComparison());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as BooleanComparison;
            t.checkType = checkType;
            t.paramA = paramA.Clone() as HNSFParamBool;
            t.paramB = paramB.Clone() as HNSFParamBool;
            return base.CopyTo(target);
        }
    }
}