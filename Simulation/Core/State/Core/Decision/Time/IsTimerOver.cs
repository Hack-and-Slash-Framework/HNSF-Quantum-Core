using System;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class IsTimerOver : HNSFStateDecision
    {
        public StateActionTargetType targetType;
        [DrawIf(nameof(targetType), (int)StateActionTargetType.FromFunction)]
        public HNSFParamEntityRef customTarget;
        
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            EntityRef targetEntity = ResolveEntity(frame, entity, ref stateContext);
            if (targetEntity == EntityRef.None) return false;
            if (!frame.Unsafe.TryGetPointer<GenericTimer>(targetEntity, out var genericTimer)) return true;
            return (genericTimer->countingType == TimerCountingType.CountDown && genericTimer->value <= 0);
        }

        private EntityRef ResolveEntity(Frame frame, EntityRef callingEntity, ref HNSFStateContext stateContext)
        {
            switch (targetType)
            {
                case StateActionTargetType.Self:
                    return callingEntity;
                case StateActionTargetType.Throwee:
                    break;
                case StateActionTargetType.FromFunction:
                    return customTarget.Resolve(frame, callingEntity, ref stateContext);
            }
            return EntityRef.None;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new IsTimerOver());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as IsTimerOver;
            t.targetType = targetType;
            t.customTarget = customTarget.Clone() as HNSFParamEntityRef;
            return base.CopyTo(target);
        }
    }
}