using System;
using System.Linq;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetPosition3D : StateFunctionFPVector3
    {
        public enum PositionType
        {
            Self,
            SoftTarget,
            HardTarget,
            ArticleOwner
        }

        public PositionType[] positionType = Array.Empty<PositionType>();
        public bool removeY = false;
        
        public override FPVector3 Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            FPVector3 pos = FPVector3.Zero;

            foreach (var pt in positionType)
            {
                switch (pt)
                {
                    case PositionType.Self:
                        if (!frame.Unsafe.TryGetPointer<Transform3D>(entity, out var selfTransform)) continue;
                        pos = selfTransform->Position;
                        break;
                    case PositionType.SoftTarget:
                        if (!frame.Unsafe.TryGetPointer<CombatTargeter>(entity, out var selfCombatTargeterB)
                            || selfCombatTargeterB->softTarget == default
                            || !frame.Unsafe.TryGetPointer<Transform3D>(selfCombatTargeterB->softTarget, out var softTargetTransform)) continue;
                        pos = softTargetTransform->Position;
                        break;
                    case PositionType.HardTarget:
                        if (!frame.Unsafe.TryGetPointer<CombatTargeter>(entity, out var selfCombatTargeterHard)
                            || selfCombatTargeterHard->targetEntity == default
                            || !frame.Unsafe.TryGetPointer<Transform3D>(selfCombatTargeterHard->targetEntity, out var hardTargetTransform)) continue;
                        pos = hardTargetTransform->Position;
                        break;
                    case PositionType.ArticleOwner:
                        if (!frame.Unsafe.TryGetPointer<Article>(entity, out var article)
                            || !frame.Unsafe.TryGetPointer<Transform3D>(article->owner, out var ownerTransform)) continue;
                        pos = ownerTransform->Position;
                        break;
                }

                if (pos != FPVector3.Zero) break;
            }
            if (removeY) pos.Y = 0;
            return pos;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetPosition3D());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetPosition3D;
            t.positionType = positionType.ToArray();
            t.removeY = removeY;
            return base.CopyTo(target);
        }
    }
}