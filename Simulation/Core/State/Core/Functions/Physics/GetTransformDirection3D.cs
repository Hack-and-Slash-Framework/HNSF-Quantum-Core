using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetTransformDirection3D : StateFunctionFPVector3
    {
        public enum SourceType
        {
            Self,
            SoftTarget,
            HardTarget,
            ArticleOwner
        }

        public enum DirectionType
        {
            Forward,
            Backward,
            Left,
            Right,
            Up,
            Down
        }

        public SourceType[] eulerSource = Array.Empty<SourceType>();
        public DirectionType directionType = DirectionType.Forward;

        public bool flattenOnMovementPlane = false;
        
        public override FPVector3 Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            FPVector3 dir = FPVector3.Zero;
            
            foreach (var pt in eulerSource)
            {
                switch (pt)
                {
                    case SourceType.Self:
                        if (!frame.Unsafe.TryGetPointer<Transform3D>(entity, out var selfTransform)) continue;
                        dir = GetDirection(selfTransform);
                        break;
                    case SourceType.SoftTarget:
                        if (!frame.Unsafe.TryGetPointer<CombatTargeter>(entity, out var selfCombatTargeterB)
                            || selfCombatTargeterB->softTarget == default
                            || !frame.Unsafe.TryGetPointer<Transform3D>(selfCombatTargeterB->softTarget, out var softTargetTransform)) continue;
                        dir = GetDirection(softTargetTransform);
                        break;
                    case SourceType.HardTarget:
                        if (!frame.Unsafe.TryGetPointer<CombatTargeter>(entity, out var selfCombatTargeterHard)
                            || selfCombatTargeterHard->targetEntity == default
                            || !frame.Unsafe.TryGetPointer<Transform3D>(selfCombatTargeterHard->targetEntity, out var hardTargetTransform)) continue;
                        dir = GetDirection(hardTargetTransform);
                        break;
                    case SourceType.ArticleOwner:
                        if (!frame.Unsafe.TryGetPointer<Article>(entity, out var article)
                            || !frame.Unsafe.TryGetPointer<Transform3D>(article->owner, out var ownerTransform)) continue;
                        dir = GetDirection(ownerTransform);
                        break;
                }
                if(dir != FPVector3.Zero) break;
            }

            if (flattenOnMovementPlane)
            {
                dir.Y = 0;
                dir = dir.Normalized;
            }

            return dir;
        }

        public FPVector3 GetDirection(Transform3D* transform)
        {
            switch (directionType)
            {
                case DirectionType.Forward:
                    return transform->Forward;
                case DirectionType.Backward:
                    return transform->Back;
                case DirectionType.Left:
                    return transform->Left;
                case DirectionType.Right:
                    return transform->Right;
                case DirectionType.Up:
                    return transform->Up;
                case DirectionType.Down:
                    return transform->Down;
                default:
                    return transform->Forward;
            }
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetTransformDirection3D());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetTransformDirection3D;
            t.eulerSource = new SourceType[eulerSource.Length];
            Array.Copy(eulerSource, t.eulerSource, eulerSource.Length);
            t.directionType = directionType;
            return base.CopyTo(target);
        }
    }
}