using Quantum;

namespace HnSF
{
    [System.Serializable]
    public struct TaggedModAssetSoftReference
    {
        public AssetRef<Tag> tag;
        public ModAssetSoftReferenceParam referenceParam;
    }
}