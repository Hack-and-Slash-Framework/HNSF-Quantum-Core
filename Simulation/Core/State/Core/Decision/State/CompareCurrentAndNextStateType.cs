using System;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class CompareCurrentAndNextStateType : HNSFStateDecision
    {
        public enum CheckType
        {
            EQUAL,
            NOT_EQUAL,
        }

        public CheckType checkType;
    
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!stateContext.agentData->toStateRequested 
                || !frame.TryFindAsset(stateContext.agentData->state, out var currentState)
                || !frame.TryFindAsset(stateContext.agentData->toState, out var toState)) return false;

            switch (checkType)
            {
                case CheckType.EQUAL:
                    return currentState.stateType == toState.stateType;
                case CheckType.NOT_EQUAL:
                    return currentState.stateType != toState.stateType;
            }
            return false;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new CompareCurrentAndNextStateType());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as CompareCurrentAndNextStateType;
            t.checkType = checkType;
            return base.CopyTo(target);
        }
    }
}