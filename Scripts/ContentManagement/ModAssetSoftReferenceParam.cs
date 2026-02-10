using System;
using Quantum;

namespace HnSF
{
    [System.Serializable]
    public struct ModAssetSoftReferenceParam : IEquatable<ModAssetSoftReferenceParam>
    {
        [System.Serializable]
        public enum ReferenceType
        {
            Self,
            External
        }

        public ReferenceType referenceType;
        [DrawIf(nameof(referenceType), (int)ReferenceType.Self)]
        public ModAssetSoftReference reference;
        [DrawIf(nameof(referenceType), (int)ReferenceType.External)]
        public ExternalModAssetSoftReference externalReference;

        public ModAssetSoftReference Get()
        {
            switch (referenceType)
            {
                case ReferenceType.Self:
                    return reference;
                case ReferenceType.External:
                    return (externalReference) ? externalReference.reference : reference;
            }
            return reference;
        }

        public bool Equals(ModAssetSoftReferenceParam other)
        {
            return referenceType == other.referenceType && reference.Equals(other.reference) && Equals(externalReference, other.externalReference);
        }

        public override bool Equals(object obj)
        {
            return obj is ModAssetSoftReferenceParam other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)referenceType, reference, externalReference);
        }
    }
}