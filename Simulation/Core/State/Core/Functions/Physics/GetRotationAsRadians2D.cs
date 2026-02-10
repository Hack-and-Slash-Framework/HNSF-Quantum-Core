using System;
using System.Linq;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetRotationAsRadians2D : StateFunctionFP
    {
        public enum SourceType
        {
            Self,
            SoftTarget,
            HardTarget,
            ArticleOwner
        }

        public SourceType[] sourceType = Array.Empty<SourceType>();
    
        public override FP Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            FP radians = 0;
            
            foreach (var pt in sourceType)
            {
                switch (pt)
                {
                    case SourceType.Self:
                        if (!frame.Unsafe.TryGetPointer<Transform2D>(entity, out var selfTransform)) continue;
                        radians = selfTransform->Rotation;
                        break;
                    case SourceType.SoftTarget:
                        if (!frame.Unsafe.TryGetPointer<CombatTargeter>(entity, out var selfCombatTargeterB)
                            || selfCombatTargeterB->softTarget == default
                            || !frame.Unsafe.TryGetPointer<Transform2D>(selfCombatTargeterB->softTarget, out var softTargetTransform)) continue;
                        radians = softTargetTransform->Rotation;
                        break;
                    case SourceType.HardTarget:
                        if (!frame.Unsafe.TryGetPointer<CombatTargeter>(entity, out var selfCombatTargeterHard)
                            || selfCombatTargeterHard->targetEntity == default
                            || !frame.Unsafe.TryGetPointer<Transform2D>(selfCombatTargeterHard->targetEntity, out var hardTargetTransform)) continue;
                        radians = hardTargetTransform->Rotation;
                        break;
                    case SourceType.ArticleOwner:
                        if (!frame.Unsafe.TryGetPointer<Article>(entity, out var article)
                            || !frame.Unsafe.TryGetPointer<Transform2D>(article->owner, out var ownerTransform)) continue;
                        radians = ownerTransform->Rotation;
                        break;
                }
            }
        
            return radians;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetRotationAsRadians2D());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetRotationAsRadians2D;
            t.sourceType = sourceType.ToArray();
            return base.CopyTo(target);
        }
    }
}