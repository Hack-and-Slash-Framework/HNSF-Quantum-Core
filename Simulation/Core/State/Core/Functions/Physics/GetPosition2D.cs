using System;
using System.Linq;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetPosition2D : StateFunctionFPVector2
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
        
        public override FPVector2 Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            FPVector2 pos = FPVector2.Zero;

            foreach (var pt in positionType)
            {
                switch (pt)
                {
                    case PositionType.Self:
                        if (!frame.Unsafe.TryGetPointer<Transform2D>(entity, out var selfTransform)) continue;
                        pos = selfTransform->Position;
                        break;
                    case PositionType.SoftTarget:
                        if (!frame.Unsafe.TryGetPointer<CombatTargeter>(entity, out var selfCombatTargeterB)
                            || selfCombatTargeterB->softTarget == default
                            || !frame.Unsafe.TryGetPointer<Transform2D>(selfCombatTargeterB->softTarget, out var softTargetTransform)) continue;
                        pos = softTargetTransform->Position;
                        break;
                    case PositionType.HardTarget:
                        if (!frame.Unsafe.TryGetPointer<CombatTargeter>(entity, out var selfCombatTargeterHard)
                            || selfCombatTargeterHard->targetEntity == default
                            || !frame.Unsafe.TryGetPointer<Transform2D>(selfCombatTargeterHard->targetEntity, out var hardTargetTransform)) continue;
                        pos = hardTargetTransform->Position;
                        break;
                    case PositionType.ArticleOwner:
                        if (!frame.Unsafe.TryGetPointer<Article>(entity, out var article)
                            || !frame.Unsafe.TryGetPointer<Transform2D>(article->owner, out var ownerTransform)) continue;
                        pos = ownerTransform->Position;
                        break;
                }
            }
            if (removeY) pos.Y = 0;
            return pos;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetPosition2D());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetPosition2D;
            t.positionType = positionType.ToArray();
            t.removeY = removeY;
            return base.CopyTo(target);
        }
    }
}