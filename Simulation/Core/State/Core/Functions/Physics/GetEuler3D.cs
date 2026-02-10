using System;
using System.Linq;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetEuler3D : StateFunctionFPVector3
    {
        public enum SourceType
        {
            Self,
            SoftTarget,
            HardTarget,
            ArticleOwner
        }

        public SourceType[] eulerSource = Array.Empty<SourceType>();
    
        public override FPVector3 Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            FPVector3 euler = FPVector3.Zero;

            foreach (var pt in eulerSource)
            {
                switch (pt)
                {
                    case SourceType.Self:
                        if (!frame.Unsafe.TryGetPointer<Transform3D>(entity, out var selfTransform)) continue;
                        euler = selfTransform->EulerAngles;
                        break;
                    case SourceType.SoftTarget:
                        if (!frame.Unsafe.TryGetPointer<CombatTargeter>(entity, out var selfCombatTargeterB)
                            || selfCombatTargeterB->softTarget == default
                            || !frame.Unsafe.TryGetPointer<Transform3D>(selfCombatTargeterB->softTarget, out var softTargetTransform)) continue;
                        euler = softTargetTransform->EulerAngles;
                        break;
                    case SourceType.HardTarget:
                        if (!frame.Unsafe.TryGetPointer<CombatTargeter>(entity, out var selfCombatTargeterHard)
                            || selfCombatTargeterHard->targetEntity == default
                            || !frame.Unsafe.TryGetPointer<Transform3D>(selfCombatTargeterHard->targetEntity, out var hardTargetTransform)) continue;
                        euler = hardTargetTransform->EulerAngles;
                        break;
                    case SourceType.ArticleOwner:
                        if (!frame.Unsafe.TryGetPointer<Article>(entity, out var article)
                            || !frame.Unsafe.TryGetPointer<Transform3D>(article->owner, out var ownerTransform)) continue;
                        euler = ownerTransform->EulerAngles;
                        break;
                }
            }
        
            return euler;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetEuler3D());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetEuler3D;
            t.eulerSource = eulerSource.ToArray();
            return base.CopyTo(target);
        }
    }
}