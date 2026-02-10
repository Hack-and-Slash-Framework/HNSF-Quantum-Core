using System;
using Quantum;

namespace HnSF
{
    [Serializable]
    public struct AnimationCurveParam
    {
        [Serializable]
        public enum ReferenceType
        {
            Self,
            External
        }

        public ReferenceType referenceType;
        [DrawIf(nameof(referenceType), (int)ReferenceType.Self)]
        public FPAnimationCurve reference;
        [DrawIf(nameof(referenceType), (int)ReferenceType.External)]
        public AssetRef<AnimationCurveAsset> externalReference;

        public FPAnimationCurve Resolve(Frame frame)
        {
            switch (referenceType)
            {
                case ReferenceType.Self:
                    return reference;
                case ReferenceType.External:
                    return !frame.TryFindAsset(externalReference, out var eac) ? default : eac.animationCurve;
            }
            return default;
        }
        
        public bool TryResolve(Frame frame, out FPAnimationCurve animationCurve)
        {
            switch (referenceType)
            {
                case ReferenceType.Self:
                    animationCurve = reference;
                    return true;
                case ReferenceType.External:
                    if (frame.TryFindAsset(externalReference, out var eac))
                    {
                        animationCurve = eac.animationCurve;
                        return true;
                    }
                    break;
            }
            animationCurve = default;
            return false;
        }
    }
}
