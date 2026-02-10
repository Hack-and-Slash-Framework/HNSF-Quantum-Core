using System;
using Quantum;

namespace HnSF.core.state.functions
{
    [Serializable]
    public unsafe partial class StateFunctionAssetRefExternal : StateFunctionAssetRef
    {
        public AssetRef<HNSFStateFunctionExternal> externalFunction;
    
        public override AssetRef Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.TryFindAsset(externalFunction, out var ef)) return default;
            return (ef.function as StateFunctionAssetRef).Execute(frame, entity, ref stateContext);
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new StateFunctionAssetRefExternal());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as StateFunctionAssetRefExternal;
            t.externalFunction = externalFunction;
            return base.CopyTo(target);
        }
    }
}