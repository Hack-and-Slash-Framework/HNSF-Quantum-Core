using Quantum;
#if QUANTUM_UNITY
using UnityEngine.Scripting.APIUpdating;
#endif

namespace HnSF.core.state.functions
{
#if QUANTUM_UNITY
    [MovedFrom(autoUpdateAPI: true, sourceClassName: "GetAssetReferenceFunction")]
#endif
    [System.Serializable]
    public unsafe partial class GetAssetReference : StateFunctionAssetRef
    {
        public AssetRef assetRef;

        public override AssetRef Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return assetRef;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetAssetReference());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetAssetReference;
            t.assetRef = assetRef;
            return base.CopyTo(target);
        }
    }
}