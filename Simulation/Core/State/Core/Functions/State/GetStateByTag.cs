using Quantum;
#if QUANTUM_UNITY
using UnityEngine.Scripting.APIUpdating;
#endif

namespace HnSF.core.state.functions
{
#if QUANTUM_UNITY
    [MovedFrom(autoUpdateAPI: true, sourceClassName: "GetStateByTagFunction")]
#endif
    [System.Serializable]
    public unsafe partial class GetStateByTag : StateFunctionAssetRef
    {
        public AssetRef<Tag> stateTag;

        public override AssetRef Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<GenericStateMachine>(entity, out var csm) ||
                !frame.TryFindAsset(csm->stateAgent.stateSet, out var stateSet)) return default;
            return stateSet.AttemptGetStateByTag(csm->stateAgent.stateData.moveset, stateTag, out var stateReference) ? stateReference.Id : default;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetStateByTag());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetStateByTag;
            t.stateTag = stateTag;
            return base.CopyTo(target);
        }
    }
}